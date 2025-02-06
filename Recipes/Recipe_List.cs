using System.Collections.Generic;
using Items;
using Station;

namespace Recipes
{
    public abstract class Recipe_List
    {
        static Dictionary<ulong, Recipe_Data> s_defaultRecipes;

        public static Dictionary<ulong, Recipe_Data> S_DefaultRecipes =>
            s_defaultRecipes ??= _initialiseDefaultRecipes();

        static Dictionary<ulong, Recipe_Data> _initialiseDefaultRecipes()
        {
            return new Dictionary<ulong, Recipe_Data>
            {
                {
                    (ulong)RecipeName.None, new Recipe_Data(
                        recipeName: RecipeName.None,
                        recipeDescription: "Select a recipe",
                        requiredProgress: 0,
                        requiredIngredients: new Dictionary<ulong, ulong>(),
                        requiredStation: StationName.None,
                        requiredVocations: new Dictionary<ulong, VocationRequirement>(),
                        recipeProducts: new Dictionary<ulong, ulong>(),
                        possibleQualities: new Dictionary<ulong, CraftingQuality>())
                },
                {
                    (ulong)RecipeName.Log, new Recipe_Data(
                        recipeName: RecipeName.Log,
                        recipeDescription: "Chop a log",
                        requiredProgress: 10,
                        requiredStation: StationName.Tree,
                        requiredIngredients: new Dictionary<ulong, ulong>(),
                        recipeProducts: new Dictionary<ulong, ulong> { { 1100, 1 } },
                        requiredVocations: new Dictionary<ulong, VocationRequirement>
                        {
                            {
                                (ulong)VocationName.Logging, new VocationRequirement(VocationName.Logging, 0)
                            }
                        },
                        possibleQualities: new Dictionary<ulong, CraftingQuality>
                        {
                            {
                                (ulong)ItemQualityName.Common, new CraftingQuality(1, ItemQualityName.Common)   
                            }
                        })
                },
                {
                    (ulong)RecipeName.Plank, new Recipe_Data(
                        recipeName: RecipeName.Plank,
                        recipeDescription: "Craft a plank",
                        requiredProgress: 10,
                        requiredStation: StationName.Sawmill,
                        requiredIngredients: new Dictionary<ulong, ulong> { { 1100, 2 } },
                        recipeProducts: new Dictionary<ulong, ulong> { { 2300, 1 } },
                        requiredVocations: new Dictionary<ulong, VocationRequirement>
                        {
                            {
                                (ulong)VocationName.Sawying, new VocationRequirement(VocationName.Sawying, 0)
                            }
                        },
                        possibleQualities: new Dictionary<ulong, CraftingQuality>
                        {
                            {
                                (ulong)ItemQualityName.Common, new CraftingQuality(1, ItemQualityName.Common)
                            }
                        })
                }
            };
        }
    }
}