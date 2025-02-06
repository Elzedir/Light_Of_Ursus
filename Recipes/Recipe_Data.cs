using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Station;
using Tools;

namespace Recipes
{
    [Serializable]
    public class Recipe_Data : Data_Class
    {
        public RecipeName RecipeName;
        public string RecipeDescription;

        public int RequiredProgress;
        public StationName RequiredStation;
        
        public Dictionary<ulong, ulong> RequiredIngredients;
        public Dictionary<ulong, ulong> RecipeProducts;
        
        public Dictionary<ulong, CraftingQuality> PossibleQualities;
        public Dictionary<ulong, VocationRequirement> RequiredVocations;
        
        public List<Item> RequiredIngredientList => Item.GetListItemFromDictionary(RequiredIngredients);
        public List<Item> RecipeProductList => Item.GetListItemFromDictionary(RecipeProducts);

        public Recipe_Data(
            RecipeName recipeName, string recipeDescription, int requiredProgress, StationName requiredStation,
            Dictionary<ulong, ulong> requiredIngredients,
            Dictionary<ulong, VocationRequirement> requiredVocations,
            Dictionary<ulong, ulong> recipeProducts,
            Dictionary<ulong, CraftingQuality> possibleQualities)
        {
            RecipeName = recipeName;
            RecipeDescription = recipeDescription;

            RequiredProgress = requiredProgress;
            RequiredStation = requiredStation;
            RequiredIngredients = requiredIngredients;
            RequiredVocations = requiredVocations;

            RecipeProducts = recipeProducts;
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
                    itemID => $"Item: {itemID.Key}:",
                    itemAmount => $"Amount: {itemAmount.Value}"));

            _updateDataDisplay(DataToDisplay,
                title: "Required Vocations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: RequiredVocations?.ToDictionary(
                    vocation => $"{(VocationName)vocation.Key}:",
                    vocation =>
                        $"Min: {vocation.Value.MinimumVocationExperience} " +
                        $"Expected: {vocation.Value.ExpectedVocationExperience}"));

            _updateDataDisplay(DataToDisplay,
                title: "Recipe Products",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: RecipeProducts?.ToDictionary(
                    item => $"Item: {item.Key}:",
                    item => $"Amount: {item.Value}"));

            _updateDataDisplay(DataToDisplay,
                title: "Possible Qualities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: PossibleQualities?.ToDictionary(
                    qualityName => $"{(ItemQualityName)qualityName.Key}:",
                    qualityLevel => $"{qualityLevel.Value.QualityLevel}"));

            return DataToDisplay;
        }
    }

    [Serializable]
    public class Recipe
    {
        public readonly RecipeName RecipeName;
        public int CurrentProgress;

        // This is still only references, change it so that it creates new instances of the objects.

        public string RecipeDescription => RecipeData.RecipeDescription;
        public int RequiredProgress => RecipeData.RequiredProgress;
        Dictionary<ulong, ulong> RequiredIngredients => RecipeData.RequiredIngredients;
        public StationName RequiredStation => RecipeData.RequiredStation;
        Dictionary<ulong, VocationRequirement> RequiredVocations => RecipeData.RequiredVocations;
        Dictionary<ulong, ulong> RecipeProducts => RecipeData.RecipeProducts;
        Dictionary<ulong, CraftingQuality> PossibleQualities => RecipeData.PossibleQualities;

        Recipe_Data _recipeData;
        public Recipe_Data RecipeData => _recipeData ??= Recipe_Manager.GetRecipe_Data(RecipeName);

        public Recipe(RecipeName recipeName)
        {
            RecipeName = recipeName;
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