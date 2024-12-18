using System;
using Items;
using UnityEngine;

namespace Recipe
{
    public abstract class Recipe_Manager
    {
        const string _recipe_SOPath = "ScriptableObjects/Recipe_SO";

        static Recipe_SO _allRecipes;
        static Recipe_SO AllRecipes => _allRecipes ??= _getRecipe_SO();

        public static Recipe_Data GetRecipe_Master(RecipeName recipeName) => AllRecipes.GetRecipe_Master(recipeName).DataObject;

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
        
        public static void ClearSOData()
        {
            AllRecipes.ClearSOData();
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