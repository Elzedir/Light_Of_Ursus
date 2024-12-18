using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Station;
using Tools;
using UnityEngine;

namespace Recipe
{
    [Serializable]
    public class Recipe_Data : Data_Class
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

        public Recipe_Data(RecipeName recipeName, string recipeDescription,
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Recipe Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Recipe Name: {RecipeName}",
                        $"Recipe Description: {RecipeDescription}",
                        $"Required Progress: {RequiredProgress}",
                        $"Required Station: {RequiredStation}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Recipe Data not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Required Ingredients",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: RequiredIngredients.Select(item => $"{item.ItemID}: {item.ItemName} - Qty: {item.ItemAmount}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Required Ingredients not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Required Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: RequiredVocations.Select(vocation =>
                                               $"{vocation.VocationName}: "                  +
                                               $"Min: {vocation.MinimumVocationExperience} " +
                                               $"Expected: {vocation.ExpectedVocationExperience}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Required Vocations not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Recipe Products",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: RecipeProducts.Select(item => $"{item.ItemID}: {item.ItemName}: Qty - {item.ItemAmount}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Recipe Products not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Possible Qualities",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: PossibleQualities.Select(quality => $"{quality.QualityName}: {quality.QualityLevel}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Possible Qualities not found.");
            }

            return new Data_Display(
                title: "Base Recipe Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
        }
    }
    
    [Serializable]
    public class Recipe
    {
        public readonly RecipeName RecipeName;
        public          int        CurrentProgress;

        public string                    RecipeDescription   => RecipeData.RecipeDescription;
        public int                       RequiredProgress    => RecipeData.RequiredProgress;
        public List<Item>                RequiredIngredients => RecipeData.RequiredIngredients;
        public StationName               RequiredStation     => RecipeData.RequiredStation;
        public List<VocationRequirement> RequiredVocations   => RecipeData.RequiredVocations;
        public List<Item>                RecipeProducts      => RecipeData.RecipeProducts;

        Recipe_Data _recipeData;
        Recipe_Data RecipeData => _recipeData ??= Recipe_Manager.GetRecipe_Master(RecipeName);
            
        public Recipe(RecipeName recipeName)
        {
            RecipeName      = recipeName;
            CurrentProgress = 0;
        }
    }
    
    public enum RecipeName
    {
        None,
        Plank,
        Log,
        Iron_Ingot,
    }
}