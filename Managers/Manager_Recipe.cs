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
            requiredProgress: 10,
            requiredIngredients: new List<Item>(),
            requiredStation: StationName.Tree,
            requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Logger, 0) },
            recipeProducts: new List<Item> { (Manager_Item.GetItem(1100, 1)) },
            possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
            ));
    }

    void _processedMaterialRecipes()
    {
        AllRecipes.Add(RecipeName.Plank, new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            requiredProgress: 10,
            requiredIngredients: new List<Item> { (Manager_Item.GetItem(1100, 2)) },
            requiredStation: StationName.Sawmill,
            requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Sawyer, 0) },
            recipeProducts: new List<Item> { (Manager_Item.GetItem(2300, 1)) },
            possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
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

[Serializable]
public class Recipe
{
    public RecipeName RecipeName;
    public string RecipeDescription;

    public int RequiredProgress;
    public List<Item> RequiredIngredients;
    public StationName RequiredStation;
    public List<VocationRequirement> RequiredVocations;

    public List<Item> RecipeProducts;
    public List<CraftingQuality> PossibleQualities;

    public Recipe(RecipeName recipeName, string recipeDescription,
        int requiredProgress, List<Item> requiredIngredients, StationName requiredStation, List<VocationRequirement> requiredVocations, 
        List<Item> recipeProducts, List<CraftingQuality> possibleQualities)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        RequiredProgress = requiredProgress;
        RequiredIngredients = requiredIngredients;
        RequiredStation = requiredStation;
        RequiredVocations = requiredVocations;

        RecipeProducts = recipeProducts;
        PossibleQualities = possibleQualities;
    }
}

[Serializable]
public class CraftingQuality
{
    public int QualityLevel;
    public ItemQualityName QualityName;

    public CraftingQuality(int qualityLevel, ItemQualityName qualityName)
    {
        QualityLevel = qualityLevel;
        QualityName = qualityName;
    }

    public CraftingQuality(CraftingQuality other)
    {
        QualityLevel = other.QualityLevel;
        QualityName = other.QualityName;
    }
}

[Serializable]
public class VocationRequirement
{
    public VocationName VocationName;
    public int MinimumVocationExperience;
    public int ExpectedVocationExperience;

    public VocationRequirement(VocationName vocationName, int expectedVocationExperience, int minimumVocationExperience = 0)
    {
        VocationName = vocationName;
        MinimumVocationExperience = minimumVocationExperience;
        ExpectedVocationExperience = expectedVocationExperience;
    }
}