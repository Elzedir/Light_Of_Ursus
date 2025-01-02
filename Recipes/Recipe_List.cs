using System.Collections.Generic;
using Items;
using Station;

namespace Recipes
{
    public abstract class Recipe_List
    {
        public static readonly Dictionary<uint, Recipe_Data> DefaultRecipes = new()
        {
            {
                (uint)RecipeName.None, new Recipe_Data(
                    recipeName: RecipeName.None,
                    recipeDescription: "Select a recipe",
                    requiredProgress: 0,
                    requiredIngredients: new List<Item>(),
                    requiredStation: StationName.None,
                    requiredVocations: new List<VocationRequirement>(),
                    recipeProducts: new List<Item>(),
                    possibleQualities: new List<CraftingQuality>())
            },
            {
                (uint)RecipeName.Log, new Recipe_Data(
                    recipeName: RecipeName.Log,
                    recipeDescription: "Chop a log",
                    requiredProgress: 10,
                    requiredIngredients: new List<Item>(),
                    requiredStation: StationName.Tree,
                    requiredVocations: new List<VocationRequirement> { new(VocationName.Logging, 0) },
                    recipeProducts: new List<Item> { new(1100, 1) },
                    possibleQualities: new List<CraftingQuality> { new(1, ItemQualityName.Common) })
            },
            {
                (uint)RecipeName.Plank, new Recipe_Data(
                    recipeName: RecipeName.Plank,
                    recipeDescription: "Craft a plank",
                    requiredProgress: 10,
                    requiredIngredients: new List<Item> { new(1100, 2) },
                    requiredStation: StationName.Sawmill,
                    requiredVocations: new List<VocationRequirement> { new(VocationName.Sawying, 0) },
                    recipeProducts: new List<Item> { new(2300, 1) },
                    possibleQualities: new List<CraftingQuality> { new(1, ItemQualityName.Common) })
            }
        };
    }
}