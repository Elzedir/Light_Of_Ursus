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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Recipe Name", RecipeName.ToString() },
                { "Recipe Description", RecipeDescription },
                { "Required Progress", $"{RequiredProgress}" },
                { "Required Station", RequiredStation.ToString() }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Recipe Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Required Ingredients",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: RequiredIngredients?.ToDictionary(
                    item => $"{item.ItemID}:",
                    item => $"Qty: {item.ItemAmount}"));

            _updateDataDisplay(DataToDisplay,
                title: "Required Vocations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: RequiredVocations?.ToDictionary(
                    vocation => $"{vocation.VocationName}:",
                    vocation => $"Min: {vocation.MinimumVocationExperience} " +
                                $"Expected: {vocation.ExpectedVocationExperience}"));

            _updateDataDisplay(DataToDisplay,
                title: "Recipe Products",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: RecipeProducts?.ToDictionary(item => $"{item.ItemID}:", item => $"Qty: {item.ItemAmount}"));
            
            _updateDataDisplay(DataToDisplay,
                title: "Possible Qualities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: PossibleQualities?.ToDictionary(quality => $"{quality.QualityName}:", quality => $"{quality.QualityLevel}"));

            return DataToDisplay;
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
        
        No_Recipe,
        Plank,
        Log,
        Iron_Ingot,
    }
}