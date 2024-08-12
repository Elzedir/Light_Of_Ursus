using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public enum StationName
{
    None,

    Iron_Node,

    Anvil,

    Tree,
    Sawmill,

    Fishing_Spot,
    Farming_Plot,

    Campfire,

    Tanning_Station,
}

[Serializable]
public class StationData : IStationInventory
{
    public int StationID;
    StationName _stationName;
    public StationName StationName { get { return _stationName; } private set { _stationName = value; } }
    public int JobsiteID;

    public bool StationIsActive = true;

    public bool OverwriteDataInStationFromEditor = false;

    public string StationDescription;
    public GameObject GameObject {  get; private set; }

    public InventoryData InventoryData { get; private set; }

    public void InitialiseInventoryComponent()
    {
        GameObject = Manager_Station.GetStation(StationID).gameObject;
        InventoryData = new InventoryData(this, new List<Item>());
    }

    public void InitialiseStationData(int jobsiteID)
    {
        Manager_Station.GetStation(StationID).Initialise();
    }

    public List<Item> GetStationYield(Actor_Base actor)
    {
        throw new ArgumentException("Base class cannot be used.");

        //return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 7) }; For tree
    }

    public Vector3 GetOperatingPosition()
    {
        throw new ArgumentException("Base class cannot be used.");

        // return Manager_Station.GetStation(StationID).StationArea.bounds.center; Use a list of vector3 from possible operating positions
    }

    public void SetStationIsActive(bool stationIsActive)
    {
        StationIsActive = stationIsActive;
    }

    public virtual List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.InventoryData.Inventory.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }
}

[CustomPropertyDrawer(typeof(StationData))]
public class StationData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stationNameProp = property.FindPropertyRelative("StationName");
        string stationName = ((StationName)stationNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}

public class GatheringComponent
{

    public static IResourceStation GetNearestResource(ResourceStationName resourceStationName, Vector3 currentPosition)
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<IResourceStation>()
            .Where(station => station.GetResourceStationName() == resourceStationName)
            .OrderBy(station => Vector3.Distance(currentPosition, station.GameObject.transform.position))
            .FirstOrDefault();
    }
}

public class CraftingComponent
{
    public Actor_Base Actor;
    public ICraftingStation CraftingStation;
    public List<Recipe> KnownRecipes = new();

    public CraftingComponent(Actor_Base actor, List<Recipe> knownRecipes)
    {
        Actor = actor;
        KnownRecipes = knownRecipes;
    }

    public bool AddRecipe(RecipeName recipeName)
    {
        if (!KnownRecipes.Any(r => r.RecipeName == recipeName)) return false;

        KnownRecipes.Add(Manager_Recipe.GetRecipe(recipeName));

        return true;
    }

    public List<Item> ConvertFromRecipeToIngredientItemList(Recipe recipe)
    {
        return recipe.RecipeIngredients.Select(ingredient => { return Manager_Item.GetItem(itemID: ingredient.Item1.CommonStats_Item.ItemID, itemQuantity: ingredient.Item2); }).ToList();
    }

    public List<Item> ConvertFromRecipeToProductItemList(Recipe recipe)
    {
        return recipe.RecipeProducts.Select(product => { return Manager_Item.GetItem(itemID: product.Item1.CommonStats_Item.ItemID, itemQuantity: product.Item2); }).ToList();
    }

    public IEnumerator CraftItemAll(RecipeName recipeName, IStationInventory craftingStation)
    {
        var recipe = Manager_Recipe.GetRecipe(recipeName);
        var ingredients = ConvertFromRecipeToIngredientItemList(recipe);

        while (inventoryContainsAllIngredients(ingredients))
        {
            yield return Actor.StartCoroutine(CraftItem(recipeName, craftingStation));
        }

        bool inventoryContainsAllIngredients(List<Item> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                var inventoryItem = Actor.Inventory.ItemInInventory(ingredient.CommonStats_Item.ItemID);

                if (inventoryItem == null || inventoryItem.CommonStats_Item.CurrentStackSize < ingredient.CommonStats_Item.CurrentStackSize)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public IEnumerator CraftItem(RecipeName recipeName, IStationInventory craftingStation)
    {
        CraftingStation = craftingStation;

        if (!KnownRecipes.Any(r => r.RecipeName == recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); yield break; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        if (!removedIngredientsFromInventory()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }
        if (!addedIngredientsToCraftingStation()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }

        yield return Actor.StartCoroutine(recipe.GetAction("Craft plank", Actor, ));

        if (!addedProductsToInventory()) { Debug.Log($"Cannot add products back into inventory"); yield break; }

        bool addedIngredientsToCraftingStation()
        {
            var sortedIngredients = ConvertFromRecipeToIngredientItemList(recipe);

            if (CraftingStation.InventoryComponent.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.Inventory.AddToInventory(sortedIngredients);

            return false;
        }

        bool removedIngredientsFromInventory()
        {
            return Actor.Inventory.RemoveFromInventory(ConvertFromRecipeToIngredientItemList(recipe));
        }

        bool addedProductsToInventory()
        {
            var sortedIngredients = ConvertFromRecipeToProductItemList(recipe);

            if (Actor.Inventory.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.Inventory.AddToInventory(sortedIngredients);

            return false;
        }
    }

    public static Collider GetTaskArea(Actor_Base actor, string taskObjectName)
    {
        float radius = 100; // Change the distance to depend on the area somehow, later.
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;

        Collider[] colliders = Physics.OverlapSphere(actor.transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.name.Contains(taskObjectName))
            {
                float distance = Vector3.Distance(actor.transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = collider;
                }
            }
        }

        return closestCollider;
    }
}