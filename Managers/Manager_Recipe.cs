using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Recipe : MonoBehaviour
{
    public static Dictionary<RecipeName, Recipe> AllRecipes = new();

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
        AllRecipes.Add(RecipeName.Plank, new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            recipeIngredients: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Log"), 2) },
            stationName: StationName.Sawmill,
            recipeProducts: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Plank"), 1) }
            ));
    }

    public static Recipe GetRecipe(RecipeName recipeName)
    {
        if (!AllRecipes.ContainsKey(recipeName)) throw new ArgumentException($"RecipeName: {recipeName} is not in recipe list.");

        return AllRecipes[recipeName];
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

    public StationName StationName;
    public List<(Item, int)> RecipeIngredients;
    public List<(Item, int)> RecipeProducts = new();

    public Recipe(RecipeName recipeName, string recipeDescription, 
        List<(Item, int)> recipeIngredients, StationName stationName, List<(Item, int)> recipeProducts)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        StationName = stationName;
        RecipeIngredients = recipeIngredients;
        RecipeProducts = recipeProducts;
    }
}

public class CraftingComponent
{
    public Actor_Base Actor;
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
            yield return CraftItem(recipeName, craftingStation);
        }

        bool inventoryContainsAllIngredients(List<Item> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                var inventoryItem = Actor.ActorData.InventoryAndEquipment.Inventory.ItemInInventory(ingredient.CommonStats_Item.ItemID);

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
        if (!KnownRecipes.Any(r => r.RecipeName == recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); yield break; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        if (!removedIngredientsFromInventory()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }
        if (!addedIngredientsToCraftingStation()) { Debug.Log($"Inventory does not have all required ingredients"); yield break; }

        // Use the stationComponent instead.

        if (!addedProductsToInventory()) { Debug.Log($"Cannot add products back into inventory"); yield break; }

        bool addedIngredientsToCraftingStation()
        {
            var sortedIngredients = ConvertFromRecipeToIngredientItemList(recipe);

            if (craftingStation.InventoryData.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.ActorData.InventoryAndEquipment.Inventory.AddToInventory(sortedIngredients);

            return false;
        }

        bool removedIngredientsFromInventory()
        {
            return Actor.ActorData.InventoryAndEquipment.Inventory.RemoveFromInventory(ConvertFromRecipeToIngredientItemList(recipe));
        }

        bool addedProductsToInventory()
        {
            var sortedIngredients = ConvertFromRecipeToProductItemList(recipe);

            if (Actor.ActorData.InventoryAndEquipment.Inventory.AddToInventory(sortedIngredients))
            {
                return true;
            }

            Actor.ActorData.InventoryAndEquipment.Inventory.AddToInventory(sortedIngredients);

            return false;
        }
    }
}