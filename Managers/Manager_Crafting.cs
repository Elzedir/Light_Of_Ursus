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
    static Dictionary<int, (Actor_Base Actor, bool Trigger)> _craftingEntities = new();

    public static void SetCharacter(int ID, (Actor_Base, bool) data)
    {
        if (_craftingEntities.ContainsKey(ID))
        {
            _craftingEntities[ID] = data;
        }
        else
        {
            _craftingEntities.Add(ID, data);
        }
    }

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
        IEnumerator craftPlank(int id)
        {
            Actor_Base actor = _craftingEntities[id].Actor;
            actor.BasicMove(GetTaskArea(actor, "Sawmill").bounds.center);



            // Play some animation.
            yield return null;
        }

        AllRecipes.Add(new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            recipeIngredients: new List<(Item, int)>
            {
                (Manager_Item.GetItem(itemName: "Log"), 2)
            },
            craftingStation: CraftingStationName.None,
            recipeOutcomes: new List<(Item, int)>
            {
                (Manager_Item.GetItem(itemName: "Plank"), 1)
            },
            recipeAction: (int ID) => StartCoroutine(craftPlank(ID))
            ));
    }
}

public class CharacterCraftingManager
{
    public List<Recipe> KnownRecipes = new();

    public bool AddRecipe(RecipeName recipeName)
    {
        if (!KnownRecipes.Any(r => r.RecipeName == recipeName)) return false;

        KnownRecipes.Add(Manager_Crafting.GetRecipe(recipeName));

        return true;
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
    public List<(Item, int)> RecipeOutcomes = new();

    public Action<int> RecipeAction;

    public Recipe(RecipeName recipeName, string recipeDescription, List<(Item, int)> recipeIngredients, CraftingStationName craftingStation, List<(Item, int)> recipeOutcomes, Action<int> recipeAction)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        RecipeIngredients = recipeIngredients;
        CraftingStation = craftingStation;
        RecipeOutcomes = recipeOutcomes;

        RecipeAction = recipeAction;
    }
}