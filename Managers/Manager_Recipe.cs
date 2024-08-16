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
        _rawMaterialRecipes();
        _processedMaterialRecipes();
        _weaponRecipes();
    }

    void _weaponRecipes()
    {

    }

    void _consumableRecipes()
    {

    }

    void _rawMaterialRecipes()
    {
        AllRecipes.Add(RecipeName.Log, new Recipe(
            recipeName: RecipeName.Log,
            recipeDescription: "Chop a log",
            recipeIngredients: new List<Item>(),
            stationName: StationName.Tree,
            recipeProducts: new List<Item> { (Manager_Item.GetItem(1100, 1)) }
            ));
    }

    void _processedMaterialRecipes()
    {
        AllRecipes.Add(RecipeName.Plank, new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            recipeIngredients: new List<Item> { (Manager_Item.GetItem(1100, 2)) },
            stationName: StationName.Sawmill,
            recipeProducts: new List<Item> { (Manager_Item.GetItem(2300, 1)) }
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
    Plank,
    Log
}

public class Recipe
{
    public RecipeName RecipeName;
    public string RecipeDescription;

    public StationName StationName;
    public List<Item> RecipeIngredients;
    public List<Item> RecipeProducts;

    public Recipe(RecipeName recipeName, string recipeDescription, 
        List<Item> recipeIngredients, StationName stationName, List<Item> recipeProducts)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        StationName = stationName;
        RecipeIngredients = recipeIngredients;
        RecipeProducts = recipeProducts;
    }
}