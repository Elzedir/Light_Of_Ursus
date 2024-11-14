using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class Manager_Recipe : MonoBehaviour
    {
        static readonly Dictionary<RecipeName, Recipe> _allRecipes = new();

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
            _allRecipes.Add(RecipeName.Log, new Recipe(
                recipeName: RecipeName.Log,
                recipeDescription: "Chop a log",
                requiredProgress: 10,
                requiredIngredients: new List<Item>(),
                requiredStation: StationName.Tree,
                requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Logging, 0) },
                recipeProducts: new List<Item> { new Item(1100, 1) },
                possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
            ));
        }

        void _processedMaterialRecipes()
        {
            _allRecipes.Add(RecipeName.Plank, new Recipe(
                recipeName: RecipeName.Plank,
                recipeDescription: "Craft a plank",
                requiredProgress: 10,
                requiredIngredients: new List<Item> { new Item(1100, 2) },
                requiredStation: StationName.Sawmill,
                requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Sawying, 0) },
                recipeProducts: new List<Item> { new Item(2300, 1) },
                possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
            ));
        }

        public static Recipe GetRecipe(RecipeName recipeName)
        {
            if (!_allRecipes.TryGetValue(recipeName, out var recipe)) throw new ArgumentException($"RecipeName: {recipeName} is not in recipe list.");

            return recipe;
        }
    }

    public enum RecipeName 
    {
        None,
        Plank,
        Log
    }

    [Serializable]
    public class Recipe
    {
        public RecipeName RecipeName;
        public string     RecipeDescription;

        public int                       RequiredProgress;
        List<Item> _requiredIngredients;
        public List<Item> RequiredIngredients =>
            (_requiredIngredients ??= new List<Item>()).Count is 0
                ? new List<Item>()
                : _requiredIngredients.Select(item => new Item(item)).ToList();   
            
        
        public StationName               RequiredStation;
        public List<VocationRequirement> RequiredVocations;

        List<Item> _recipeProducts;
        public List<Item> RecipeProducts =>
            (_recipeProducts ??= new List<Item>()).Count is 0
                ? new List<Item>()
                : _recipeProducts.Select(item => new Item(item)).ToList();
            
        
        public List<CraftingQuality> PossibleQualities;

        public Recipe(RecipeName recipeName,       string                recipeDescription,
                      int        requiredProgress, List<Item>            requiredIngredients, StationName requiredStation, List<VocationRequirement> requiredVocations, 
                      List<Item> recipeProducts,   List<CraftingQuality> possibleQualities)
        {
            RecipeName        = recipeName;
            RecipeDescription = recipeDescription;

            RequiredProgress     = requiredProgress;
            _requiredIngredients = new List<Item>(requiredIngredients);
            RequiredStation      = requiredStation;
            RequiredVocations    = requiredVocations;

            _recipeProducts   = new List<Item>(recipeProducts);
            PossibleQualities = possibleQualities;
        }
    }

    [Serializable]
    public class CraftingQuality
    {
        public int             QualityLevel;
        public ItemQualityName QualityName;

        public CraftingQuality(int qualityLevel, ItemQualityName qualityName)
        {
            QualityLevel = qualityLevel;
            QualityName  = qualityName;
        }

        public CraftingQuality(CraftingQuality other)
        {
            QualityLevel = other.QualityLevel;
            QualityName  = other.QualityName;
        }
    }

    [Serializable]
    public class VocationRequirement
    {
        public VocationName VocationName;
        public int          MinimumVocationExperience;
        public int          ExpectedVocationExperience;

        public VocationRequirement(VocationName vocationName, int expectedVocationExperience, int minimumVocationExperience = 0)
        {
            VocationName               = vocationName;
            MinimumVocationExperience  = minimumVocationExperience;
            ExpectedVocationExperience = expectedVocationExperience;
        }
    }
}