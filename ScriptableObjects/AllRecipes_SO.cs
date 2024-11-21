using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AllRecipes_SO", menuName = "SOList/AllRecipes_SO")]
    [Serializable]
    public class AllRecipes_SO : ScriptableObject
    {
        [SerializeField] Recipe_Master[]   _recipes;
        public           Recipe_Master[]   Recipes => _recipes ??= InitialiseAllRecipes();
        Dictionary<RecipeName, int>        _recipeIndexLookup;
        public Dictionary<RecipeName, int> RecipeIndexLookup => _recipeIndexLookup ??= _buildIndexLookup();
        int                                _currentIndex;
        
        public Recipe_Master[] InitialiseAllRecipes()
        {
            _recipes = new Recipe_Master[_defaultRecipes.Count * 2];
            Array.Copy(_defaultRecipes.Values.ToArray(), Recipes, _defaultRecipes.Count);
            _currentIndex = _defaultRecipes.Count;
            _buildIndexLookup();
            return Recipes ?? throw new NullReferenceException("Recipes is null.");
        }

        Dictionary<RecipeName, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<RecipeName, int>();
            
            for (var i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i] != null)
                {
                    newIndexLookup[Recipes[i].RecipeName] = i;
                }
            }
            
            return newIndexLookup;
        }

        public Recipe_Master GetRecipe_Master(RecipeName recipeName)
        {
            if (Recipes == null || Recipes.Length is 0) InitialiseAllRecipes();
                
            if (RecipeIndexLookup.TryGetValue(recipeName, out var index))
            {
                return Recipes?[index];
            }

            Debug.LogWarning($"Recipe {recipeName} does not exist in Recipes.");
            return null;
        }

        public void AddRecipe(RecipeName recipeName, Recipe_Master recipe)
        {
            if (RecipeIndexLookup.ContainsKey(recipeName))
            {
                Debug.LogWarning($"Recipe {recipeName} already exists in Recipes.");
                return;
            }

            if (_currentIndex >= Recipes.Length)
            {
                _compactAndResizeArray();
            }

            Recipes[_currentIndex]       = recipe;
            RecipeIndexLookup[recipeName] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveRecipe(RecipeName recipeName)
        {
            if (!RecipeIndexLookup.TryGetValue(recipeName, out var index))
            {
                Debug.LogWarning($"Recipe {recipeName} does not exist in Recipes.");
                return;
            }

            Recipes[index] = null;
            RecipeIndexLookup.Remove(recipeName);
            
            if (RecipeIndexLookup.Count < Recipes.Length / 4)
            {
                _compactAndResizeArray();
            }
        }
        
        void _compactAndResizeArray()
        {
            var newSize = 0;
            
            for (var i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i] == null) continue;
                
                Recipes[newSize]                         = Recipes[i];
                RecipeIndexLookup[Recipes[i].RecipeName] = newSize;
                newSize++;
            }

            Array.Resize(ref _recipes, Math.Max(newSize * 2, Recipes.Length));
            _currentIndex = newSize;
        }

        public void UpdateRecipe(RecipeName recipeName, Recipe_Master recipe)
        {
            if (RecipeIndexLookup.TryGetValue(recipeName, out var index))
            {
                Recipes[index] = recipe;
            }
            else
            {
                AddRecipe(recipeName, recipe);
            }
        }

        public void ClearRecipeData()
        {
            _recipes = Array.Empty<Recipe_Master>();
            RecipeIndexLookup.Clear();
            _currentIndex = 0;
        }
        
        static readonly Dictionary<RecipeName, Recipe_Master> _defaultRecipes = new()
        {
            { RecipeName.None, new Recipe_Master(
                recipeName: RecipeName.None,
                recipeDescription: "Select a recipe",
                requiredProgress: 0,
                requiredIngredients: new List<Item>(),
                requiredStation: StationName.None,
                requiredVocations: new List<VocationRequirement>(),
                recipeProducts: new List<Item>(),
                possibleQualities: new List<CraftingQuality>()
            )},
            { RecipeName.Log, new Recipe_Master(
                recipeName: RecipeName.Log,
                recipeDescription: "Chop a log",
                requiredProgress: 10,
                requiredIngredients: new List<Item>(),
                requiredStation: StationName.Tree,
                requiredVocations: new List<VocationRequirement> { new(VocationName.Logging, 0) },
                recipeProducts: new List<Item> { new(1100, 1) },
                possibleQualities: new List<CraftingQuality> { new(1, ItemQualityName.Common) }
            )},
            { RecipeName.Plank, new Recipe_Master(
                recipeName: RecipeName.Plank,
                recipeDescription: "Craft a plank",
                requiredProgress: 10,
                requiredIngredients: new List<Item> { new(1100, 2) },
                requiredStation: StationName.Sawmill,
                requiredVocations: new List<VocationRequirement> { new(VocationName.Sawying, 0) },
                recipeProducts: new List<Item> { new(2300, 1) },
                possibleQualities: new List<CraftingQuality> { new(1, ItemQualityName.Common) }
            )},
        };
    }

    [CustomEditor(typeof(AllRecipes_SO))]
    public class AllRecipes_SOEditor : Editor
    {
        int _selectedRecipeIndex = -1;

        Vector2 _recipeScrollPos;

        bool _showIngredients;
        bool _showProducts;
        bool _showVocations;

        void _unselectAll()
        {
            _showIngredients = false;
            _showProducts    = false;
            _showVocations   = false;
        }

        public override void OnInspectorGUI()
        {
            var allRecipeSO = (AllRecipes_SO)target;

            if (allRecipeSO?.Recipes is null || allRecipeSO.Recipes.Length is 0)
            {
                EditorGUILayout.LabelField("No Recipes Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear Recipe Data"))
            {
                allRecipeSO.ClearRecipeData();
                EditorUtility.SetDirty(allRecipeSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Recipes", EditorStyles.boldLabel);

            var nonNullRecipes = allRecipeSO.Recipes.Where(recipe =>
                recipe            != null &&
                recipe.RecipeName != RecipeName.None).ToArray();

            _recipeScrollPos = EditorGUILayout.BeginScrollView(_recipeScrollPos,
                GUILayout.Height(Math.Min(200, nonNullRecipes.Length * 20)));
            _selectedRecipeIndex = GUILayout.SelectionGrid(_selectedRecipeIndex, _getRecipeNames(nonNullRecipes), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedRecipeIndex >= 0 && _selectedRecipeIndex < nonNullRecipes.Length)
            {
                _drawRecipeData(nonNullRecipes[_selectedRecipeIndex]);
            }
        }

        string[] _getRecipeNames(Recipe_Master[] recipes) =>
            recipes.Select(recipe => recipe.RecipeName.ToString()).ToArray();


        void _drawRecipeData(Recipe_Master recipe)
        {
            EditorGUILayout.LabelField("Recipe Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Recipe Name",        $"{recipe.RecipeName}");
            EditorGUILayout.LabelField("Recipe Description", recipe.RecipeDescription);
            EditorGUILayout.LabelField("Required Progress",  $"{recipe.RequiredProgress}");
            EditorGUILayout.LabelField("Required Station",   $"{recipe.RequiredStation}");

            if (recipe.RequiredIngredients != null)
            {
                EditorGUILayout.LabelField("RequiredIngredients", EditorStyles.boldLabel);

                var requiredIngredients = recipe.RequiredIngredients;

                _showIngredients = EditorGUILayout.Toggle("Ingredients", _showIngredients);

                if (_showIngredients)
                {
                    _drawIngredients(requiredIngredients);
                }
            }

            if (recipe.RecipeProducts != null)
            {
                EditorGUILayout.LabelField("Recipe Products", EditorStyles.boldLabel);

                var recipeProducts = recipe.RecipeProducts;

                _showProducts = EditorGUILayout.Toggle("Products", _showProducts);

                if (_showProducts)
                {
                    _drawProducts(recipeProducts);
                }
            }

            if (recipe.RequiredVocations != null)
            {
                EditorGUILayout.LabelField("Required Vocations", EditorStyles.boldLabel);

                var requiredVocations = recipe.RequiredVocations;

                _showVocations = EditorGUILayout.Toggle("Vocations", _showVocations);

                if (_showVocations)
                {
                    _drawVocations(requiredVocations);
                }
            }
        }

        void _drawIngredients(List<Item> requiredIngredients)
        {
            if (requiredIngredients.Count == 1)
            {
                EditorGUILayout.LabelField($"{requiredIngredients[0].ItemName}: {requiredIngredients[0].ItemAmount}");
            }
            else
            {
                _recipeScrollPos = EditorGUILayout.BeginScrollView(_recipeScrollPos,
                    GUILayout.Height(Math.Min(200, requiredIngredients.Count * 20)));

                try
                {
                    foreach (var item in requiredIngredients)
                    {
                        EditorGUILayout.LabelField($"{item.ItemName}: {item.ItemAmount}");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        void _drawProducts(List<Item> products)
        {
            if (products.Count == 1)
            {
                EditorGUILayout.LabelField($"{products[0].ItemName}: {products[0].ItemAmount}");
            }
            else
            {
                _recipeScrollPos = EditorGUILayout.BeginScrollView(_recipeScrollPos,
                    GUILayout.Height(Math.Min(200, products.Count * 20)));

                try
                {
                    foreach (var item in products)
                    {
                        EditorGUILayout.LabelField($"{item.ItemName}: {item.ItemAmount}");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        void _drawVocations(List<VocationRequirement> requiredVocations)
        {
            if (requiredVocations.Count == 1)
            {
                EditorGUILayout.LabelField(
                    $"{requiredVocations[0].VocationName} "                     +
                    $"- Min: {requiredVocations[0].ExpectedVocationExperience}" +
                    $"- Expected: {requiredVocations[0].ExpectedVocationExperience}"
                );
            }
            else
            {
                _recipeScrollPos = EditorGUILayout.BeginScrollView(_recipeScrollPos,
                    GUILayout.Height(Math.Min(200, requiredVocations.Count * 20)));

                try
                {
                    foreach (var vocation in requiredVocations)
                    {
                        EditorGUILayout.LabelField(
                            $"{vocation.VocationName} "                     +
                            $"- Min: {vocation.ExpectedVocationExperience}" +
                            $"- Expected: {vocation.ExpectedVocationExperience}"
                        );
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
        }
    }
}