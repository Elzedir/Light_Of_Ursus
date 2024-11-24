using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Recipes
{
    [CreateAssetMenu(fileName = "AllRecipes_SO", menuName = "SOList/AllRecipes_SO")]
    [Serializable]
    public class AllRecipes_SO : Base_SO<Recipe_Master>
    {
        public Recipe_Master[] Recipes => Objects;
        public Recipe_Master   GetRecipe_Master(RecipeName recipeName) => GetObject_Master((uint)recipeName);
        
        public override uint GetObjectID(int id) => (uint)Recipes[id].RecipeName;
        
        public void PopulateDefaultRecipes()
        {
            if (_defaultRecipes.Count == 0)
            {
                Debug.Log("No Default Recipes Found");
            }
        }

        protected override Dictionary<uint, Recipe_Master> _populateDefaultObjects()
        {
            return List_Recipe.GetAllDefaultRecipes();
        }

        Dictionary<uint, Recipe_Master> _defaultRecipes => DefaultObjects;
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
                allRecipeSO.ClearObjectData();
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