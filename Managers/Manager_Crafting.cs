using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CraftingStationName 
{
    None,
    Anvil,
    Campfire,
    Sawmill,
    Tanning_Station,
}

public class Manager_Crafting : MonoBehaviour
{
    public static HashSet<Recipe> AllRecipes = new();

    public static Recipe GetRecipe(RecipeName recipeName)
    {
        if (!AllRecipes.Any(r => r.RecipeName == recipeName)) throw new ArgumentException($"RecipeName: {recipeName} is not in recipe list.");

        return AllRecipes.FirstOrDefault(r => r.RecipeName == recipeName);
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

    public static ICraftingStation GetNearestCraftingStation(CraftingStationName craftingStationName, Vector3 currentPosition)
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ICraftingStation>()
            .Where(station => station.GetCraftingStationName() == craftingStationName)
            .OrderBy(station => Vector3.Distance(currentPosition, station.GameObject.transform.position))
            .FirstOrDefault();
    }

    public void OnSceneLoaded()
    {
        _initialiseCrafting();
    }

    void _initialiseCrafting()
    {
        _consumableRecipes();
        _processedMaterialRecipes();
        _weaponRecipes();
    }

    void _weaponRecipes()
    {

    }

    void _consumableRecipes()
    {

    }

    void _processedMaterialRecipes()
    {
        IEnumerator craftPlank(Actor_Base actor = null)
        {
            if (actor == null) throw new ArgumentException("Actor is null.");

            yield return actor.BasicMove(GetTaskArea(actor, "Sawmill").bounds.center);
            yield return new WaitForSeconds(0.5f);
            // Play some animation.
            yield return null;
        }

        AllRecipes.Add(new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            recipeIngredients: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Log"), 2) },
            craftingStation: CraftingStationName.None,
            recipeProducts: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Plank"), 1) },
            recipeActions: new List<(string Name, Func<Actor_Base, IEnumerator>)> { ("Craft plank", craftPlank) }
            ));
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

        KnownRecipes.Add(Manager_Crafting.GetRecipe(recipeName));

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

    public IEnumerator CraftItemAll(RecipeName recipeName, ICraftingStation craftingStation)
    {
        var recipe = Manager_Crafting.GetRecipe(recipeName);
        var ingredients = ConvertFromRecipeToIngredientItemList(recipe);

        while (inventoryContainsAllIngredients(ingredients))
        {
            yield return Actor.StartCoroutine(CraftItem(recipeName, craftingStation));
        }

        bool inventoryContainsAllIngredients(List<Item> ingredients)
        {
            foreach(var ingredient in ingredients)
            {
                var inventoryItem = Actor.InventoryComponent.ItemInInventory(ingredient.CommonStats_Item.ItemID);

                if (inventoryItem == null || inventoryItem.CommonStats_Item.CurrentStackSize < ingredient.CommonStats_Item.CurrentStackSize)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public IEnumerator CraftItem(RecipeName recipeName, ICraftingStation craftingStation)
    {
        CraftingStation = craftingStation;

        if (!KnownRecipes.Any(r => r.RecipeName == recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); yield break; }

        Recipe recipe = Manager_Crafting.GetRecipe(recipeName);

        if (!removedIngredientsFromInventory()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }
        if (!addedIngredientsToCraftingStation()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }

        yield return Actor.StartCoroutine(recipe.GetAction("Craft plank", Actor));

        if (!addedProductsToInventory()) { Debug.Log($"Cannot add products back into inventory"); yield break; }

        bool addedIngredientsToCraftingStation()
        {
            var sortedIngredients = ConvertFromRecipeToIngredientItemList(recipe);

            if (CraftingStation.InventoryComponent.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.InventoryComponent.AddToInventory(sortedIngredients);

            return false;
        }

        bool removedIngredientsFromInventory()
        {
            return Actor.InventoryComponent.RemoveFromInventory(ConvertFromRecipeToIngredientItemList(recipe));
        }

        bool addedProductsToInventory()
        {
            var sortedIngredients = ConvertFromRecipeToProductItemList(recipe);

            if (Actor.InventoryComponent.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.InventoryComponent.AddToInventory(sortedIngredients);

            return false;
        }
    }
}

public enum RecipeName 
{
    Plank
}

public class Recipe
{
    public RecipeName RecipeName;
    public string RecipeDescription;

    public List<(Item, int)> RecipeIngredients;
    public CraftingStationName CraftingStation;
    public List<(Item, int)> RecipeProducts = new();

    public List<(string Name, Func<Actor_Base, IEnumerator> Action)> RecipeActions;

    public Recipe(RecipeName recipeName, string recipeDescription, List<(Item, int)> recipeIngredients, CraftingStationName craftingStation, List<(Item, int)> recipeProducts, List<(string Name, Func<Actor_Base, IEnumerator>)> recipeActions)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        RecipeIngredients = recipeIngredients;
        CraftingStation = craftingStation;
        RecipeProducts = recipeProducts;

        RecipeActions = recipeActions;
    }

    public IEnumerator GetAction(string actionName, Actor_Base actor)
    {
        if (!RecipeActions.Any(a => a.Name == actionName)) throw new ArgumentException($"RecipeActions does not contain ActionName: {actionName}");

        return RecipeActions.FirstOrDefault(a => a.Name == actionName).Action(actor);
    }
}