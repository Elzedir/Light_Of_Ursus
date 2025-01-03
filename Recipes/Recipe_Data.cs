using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Station;
using Tools;
using UnityEngine;

namespace Recipes
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Base Recipe Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    subData: new Dictionary<string, Data_Display>(),
                    firstData: true);

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Base Recipe Data", out var baseRecipeData))
                {
                    dataSO_Object.SubData["Base Recipe Data"] = new Data_Display(
                        title: "Base Recipe Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (baseRecipeData is not null)
                {
                    baseRecipeData.Data = new Dictionary<string, string>
                    {
                        { "Recipe Name", RecipeName.ToString() },
                        { "Recipe Description", RecipeDescription },
                        { "Required Progress", $"{RequiredProgress}" },
                        { "Required Station", RequiredStation.ToString() }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error: Recipe Data not found.");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Required Ingredients", out var requiredIngredients))
                {
                    dataSO_Object.SubData["Required Ingredients"] = new Data_Display(
                        title: "Required Ingredients",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (requiredIngredients is not null)
                {
                    requiredIngredients.Data = RequiredIngredients.ToDictionary(item => $"{item.ItemID}:", item => $"Qty: {item.ItemAmount}");
                }
            }
            catch
            {
                Debug.LogError("Error: Required Ingredients not found.");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Required Vocations", out var requiredVocations))
                {
                    dataSO_Object.SubData["Required Vocations"] = new Data_Display(
                        title: "Required Vocations",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (requiredVocations is not null)
                {
                    requiredVocations.Data = RequiredVocations.ToDictionary(vocation =>
                            $"{vocation.VocationName}:",
                        vocation => $"Min: {vocation.MinimumVocationExperience} " +
                                    $"Expected: {vocation.ExpectedVocationExperience}");
                }
            }
            catch
            {
                Debug.LogError("Error: Required Vocations not found.");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Recipe Products", out var recipeProducts))
                {
                    dataSO_Object.SubData["Recipe Products"] = new Data_Display(
                        title: "Recipe Products",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (recipeProducts is not null)
                {
                    recipeProducts.Data = RecipeProducts.ToDictionary(item => $"{item.ItemID}:", item => $"Qty: {item.ItemAmount}");
                }
            }
            catch
            {
                Debug.LogError("Error: Recipe Products not found.");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Possible Qualities", out var possibleQualities))
                {
                    dataSO_Object.SubData["Possible Qualities"] = new Data_Display(
                        title: "Possible Qualities",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (possibleQualities is not null)
                {
                    possibleQualities.Data = PossibleQualities.ToDictionary(quality => $"{quality.QualityName}:", quality => $"{quality.QualityLevel}");
                }
            }
            catch
            {
                Debug.LogError("Error: Possible Qualities not found.");
            }

            return dataSO_Object;
        }
    }
    
    [Serializable]
    public class Recipe
    {
        public readonly RecipeName RecipeName;
        public          int        CurrentProgress;

        // This is still only references, change it so that it creates new instances of the objects.
        
        public string                    RecipeDescription   => RecipeData.RecipeDescription;
        public int                       RequiredProgress    => RecipeData.RequiredProgress;
        public List<Item>                RequiredIngredients => RecipeData.RequiredIngredients;
        public StationName               RequiredStation     => RecipeData.RequiredStation;
        public List<VocationRequirement> RequiredVocations   => RecipeData.RequiredVocations;
        public List<Item>                RecipeProducts      => RecipeData.RecipeProducts;

        Recipe_Data _recipeData;
        public Recipe_Data RecipeData => _recipeData ??= Recipe_Manager.GetRecipe_Master(RecipeName);
            
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