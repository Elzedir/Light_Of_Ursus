using System.Collections.Generic;
using Items;
using Station;

namespace Recipe
{
    public abstract class Recipe_List
    {
        // Don't use initialised lists since it relies on Item, which needs Item_Master to be initialised first.
        
        public static Dictionary<uint, Recipe_Data> GetAllDefaultRecipes()
        {
            var allRecipes = new Dictionary<uint, Recipe_Data>();

            foreach (var none in _defaultNone())
            {
                allRecipes.Add((uint)none.Key, none.Value);
            }
            
            foreach (var rawMaterial in _defaultRawMaterials())
            {
                allRecipes.Add((uint)rawMaterial.Key, rawMaterial.Value);
            }

            foreach (var processedMaterial in _defaultProcessedMaterials())
            {
                allRecipes.Add((uint)processedMaterial.Key, processedMaterial.Value);
            }
            
            return allRecipes;
        }

        static Dictionary<RecipeName, Recipe_Data> _defaultNone()
        {
            return new Dictionary<RecipeName, Recipe_Data>
            {
                {
                    RecipeName.None, new Recipe_Data(
                        recipeName: RecipeName.None,
                        recipeDescription: "Select a recipe",
                        requiredProgress: 0,
                        requiredIngredients: new List<Item>(),
                        requiredStation: StationName.None,
                        requiredVocations: new List<VocationRequirement>(),
                        recipeProducts: new List<Item>(),
                        possibleQualities: new List<CraftingQuality>())
                }
            };
        }

        static Dictionary<RecipeName, Recipe_Data> _defaultRawMaterials()
        {
            return new Dictionary<RecipeName, Recipe_Data>
            {
                {
                    RecipeName.Log, new Recipe_Data(
                        recipeName: RecipeName.Log,
                        recipeDescription: "Chop a log",
                        requiredProgress: 10,
                        requiredIngredients: new List<Item>(),
                        requiredStation: StationName.Tree,
                        requiredVocations: new List<VocationRequirement> { new(VocationName.Logging, 0) },
                        recipeProducts: new List<Item> { new(1100, 1) },
                        possibleQualities: new List<CraftingQuality> { new(1, ItemQualityName.Common) })
                }
            };
        }

        static Dictionary<RecipeName, Recipe_Data> _defaultProcessedMaterials()
        {
            return new Dictionary<RecipeName, Recipe_Data>
            {
                {
                    RecipeName.Plank, new Recipe_Data(
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
}
