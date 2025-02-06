using System;
using Items;
using UnityEngine;

namespace Recipes
{
    public abstract class Recipe_Manager
    {
        const string _recipe_SOPath = "ScriptableObjects/Recipe_SO";

        static Recipe_SO s_allRecipes;
        public static Recipe_SO S_AllRecipes => s_allRecipes ??= _getRecipe_SO();

        public static Recipe_Data GetRecipe_Data(RecipeName recipeName) => S_AllRecipes.GetRecipe_Master(recipeName).Data_Object;

        static Recipe_SO _getRecipe_SO()
        {
            var recipe_SO = Resources.Load<Recipe_SO>(_recipe_SOPath);

            if (recipe_SO is not null) return recipe_SO;

            Debug.LogError("Recipe_SO not found. Creating temporary Recipe_SO.");
            recipe_SO = ScriptableObject.CreateInstance<Recipe_SO>();

            return recipe_SO;
        }
        
        public static void ClearSOData()
        {
            S_AllRecipes.ClearSOData();
        }
    }

    [Serializable]
    public class CraftingQuality
    {
        public ItemQualityName QualityName;
        public int QualityLevel;

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