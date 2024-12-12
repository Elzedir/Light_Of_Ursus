using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Station;
using UnityEngine;

namespace Recipes
{
    public abstract class Recipe_Manager
    {
        const string _recipe_SOPath = "ScriptableObjects/Recipe_SO";

        static Recipe_SO _allRecipes;
        static Recipe_SO AllRecipes => _allRecipes ??= _getRecipe_SO();

        public static Recipe_Master GetRecipe_Master(RecipeName recipeName) => AllRecipes.GetRecipe_Master(recipeName);

        static Recipe_SO _getRecipe_SO()
        {
            var recipe_SO = Resources.Load<Recipe_SO>(_recipe_SOPath);

            if (recipe_SO is not null) return recipe_SO;

            Debug.LogError("Recipe_SO not found. Creating temporary Recipe_SO.");
            recipe_SO = ScriptableObject.CreateInstance<Recipe_SO>();

            return recipe_SO;
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
        Recipe_Master RecipeMaster => _recipeMaster ??= Recipe_Manager.GetRecipe_Master(RecipeName);

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