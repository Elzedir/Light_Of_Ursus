using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using Station;
using UnityEditor;
using UnityEngine;

namespace Recipes
{
    public abstract class Manager_Recipe
    {
        const string _allRecipesSOPath = "ScriptableObjects/AllRecipes_SO";

        static AllRecipes_SO _allRecipes;
        static AllRecipes_SO AllRecipes => _allRecipes ??= _getOrCreateAllRecipesSO();

        public static Recipe_Master GetRecipe_Master(RecipeName recipeName) => AllRecipes.GetRecipe_Master(recipeName);

        static AllRecipes_SO _getOrCreateAllRecipesSO()
        {
            var allRecipesSO = Resources.Load<AllRecipes_SO>(_allRecipesSOPath);

            if (allRecipesSO is not null) return allRecipesSO;

            allRecipesSO = ScriptableObject.CreateInstance<AllRecipes_SO>();
            AssetDatabase.CreateAsset(allRecipesSO, $"Assets/Resources/{_allRecipesSOPath}");
            AssetDatabase.SaveAssets();

            return allRecipesSO;
        }

        public static void PopulateAllRecipes()
        {
            AllRecipes.PopulateDefaultRecipes();
            // Then populate custom recipes.
        }
    }

    public enum RecipeName
    {
        None,
        Plank,
        Log,
        Iron_Ingot,
    }

    public class Recipe
    {
        public readonly RecipeName RecipeName;
        public          int        CurrentProgress;

        public string                    RecipeDescription   => RecipeMaster.RecipeDescription;
        public int                       RequiredProgress    => RecipeMaster.RequiredProgress;
        public List<Item>                RequiredIngredients => RecipeMaster.RequiredIngredients;
        public StationName               RequiredStation     => RecipeMaster.RequiredStation;
        public List<VocationRequirement> RequiredVocations   => RecipeMaster.RequiredVocations;
        public List<Item>                RecipeProducts      => RecipeMaster.RecipeProducts;

        Recipe_Master _recipeMaster;
        Recipe_Master RecipeMaster => _recipeMaster ??= Manager_Recipe.GetRecipe_Master(RecipeName);

        public Recipe(RecipeName recipeName)
        {
            RecipeName      = recipeName;
            CurrentProgress = 0;
        }
    }

    [Serializable]
    public class Recipe_Master
    {
        public RecipeName RecipeName;
        public string     RecipeDescription;

        public int RequiredProgress;
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

        public Recipe_Master(RecipeName recipeName, string recipeDescription,
                             int requiredProgress, List<Item> requiredIngredients, StationName requiredStation,
                             List<VocationRequirement> requiredVocations,
                             List<Item> recipeProducts, List<CraftingQuality> possibleQualities)
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

        public VocationRequirement(VocationName vocationName, int expectedVocationExperience,
                                   int          minimumVocationExperience = 0)
        {
            VocationName               = vocationName;
            MinimumVocationExperience  = minimumVocationExperience;
            ExpectedVocationExperience = expectedVocationExperience;
        }
    }
}