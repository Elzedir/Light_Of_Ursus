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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);

            try
            {
                dataObjects["Base Recipe Data"] = new Data_Display(
                    title: "Base Recipe Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Recipe Name", RecipeName.ToString() },
                        { "Recipe Description", RecipeDescription },
                        { "Required Progress", $"{RequiredProgress}" },
                        { "Required Station", RequiredStation.ToString() }
                    });
            }
            catch
            {
                Debug.LogError("Error: Recipe Data not found.");
            }

            try
            {
                dataObjects["Required Ingredients"] = new Data_Display(
                    title: "Required Ingredients",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: RequiredIngredients.ToDictionary(item => $"{item.ItemID}:", item => $"Qty: {item.ItemAmount}"));
            }
            catch
            {
                Debug.LogError("Error: Required Ingredients not found.");
            }

            try
            {
                dataObjects["Required Vocations"] = new Data_Display(
                    title: "Required Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: RequiredVocations.ToDictionary(vocation =>
                            $"{vocation.VocationName}:",
                        vocation => $"Min: {vocation.MinimumVocationExperience} " +
                                    $"Expected: {vocation.ExpectedVocationExperience}"));
            }
            catch
            {
                Debug.LogError("Error: Required Vocations not found.");
            }

            try
            {
                dataObjects["Recipe Products"] = new Data_Display(
                    title: "Recipe Products",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: RecipeProducts.ToDictionary(item => $"{item.ItemID}:", item => $"Qty - {item.ItemAmount}"));
            }
            catch
            {
                Debug.LogError("Error: Recipe Products not found.");
            }

            try
            {
                dataObjects["Possible Qualities"] = new Data_Display(
                    title: "Possible Qualities",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: PossibleQualities.ToDictionary(quality => $"{quality.QualityName}:", quality => $"{quality.QualityLevel}"));
            }
            catch
            {
                Debug.LogError("Error: Possible Qualities not found.");
            }

            return dataSO_Object = new Data_Display(
                title: "Base Recipe Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
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