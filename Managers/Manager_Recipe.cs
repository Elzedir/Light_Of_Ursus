using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class Manager_Recipe : MonoBehaviour
    {
        const string  _allRecipesSOPath = "ScriptableObjects/AllRecipes_SO";
        
        static AllRecipes_SO _allRecipes;
        static AllRecipes_SO AllRecipes => _allRecipes ??= _getOrCreateAllRecipesSO();

        public void OnSceneLoaded()
        {
            _initialiseCrafting();
        }

        void _initialiseCrafting()
        {
            var recipeMasterList = new List<Recipe_Master>();
            
            _defaultRecipes(ref recipeMasterList);
            _consumableRecipes(ref recipeMasterList);
            _rawMaterialRecipes(ref recipeMasterList);
            _processedMaterialRecipes(ref recipeMasterList);
            _weaponRecipes(ref recipeMasterList);
            
            var allRecipesArray = recipeMasterList.ToArray();
            
            AllRecipes.InitialiseAllRecipes(allRecipesArray);
        }

        void _defaultRecipes(ref List<Recipe_Master> recipeMasterList)
        {
            recipeMasterList.Add(new Recipe_Master(
                recipeName: RecipeName.None,
                recipeDescription: "Select a recipe",
                requiredProgress: 0,
                requiredIngredients: new List<Item>(),
                requiredStation: StationName.None,
                requiredVocations: new List<VocationRequirement>(),
                recipeProducts: new List<Item>(),
                possibleQualities: new List<CraftingQuality>()
            ));
        }

        void _weaponRecipes(ref List<Recipe_Master> recipeMasterList)
        {

        }

        void _consumableRecipes(ref List<Recipe_Master> recipeMasterList)
        {

        }

        void _rawMaterialRecipes(ref List<Recipe_Master> recipeMasterList)
        {
            recipeMasterList.Add(new Recipe_Master(
                recipeName: RecipeName.Log,
                recipeDescription: "Chop a log",
                requiredProgress: 10,
                requiredIngredients: new List<Item>(),
                requiredStation: StationName.Tree,
                requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Logging, 0) },
                recipeProducts: new List<Item> { new Item(1100, 1) },
                possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
            ));
        }

        void _processedMaterialRecipes(ref List<Recipe_Master> recipeMasterList)
        {
            recipeMasterList.Add(new Recipe_Master(
                recipeName: RecipeName.Plank,
                recipeDescription: "Craft a plank",
                requiredProgress: 10,
                requiredIngredients: new List<Item> { new Item(1100, 2) },
                requiredStation: StationName.Sawmill,
                requiredVocations: new List<VocationRequirement> { new VocationRequirement(VocationName.Sawying, 0) },
                recipeProducts: new List<Item> { new Item(2300, 1) },
                possibleQualities: new List<CraftingQuality> { new CraftingQuality(1, ItemQualityName.Common) }
            ));
        }

        public static Recipe_Master GetRecipe_Master(RecipeName recipeName) => AllRecipes.GetRecipe_Master(recipeName);
        
        static AllRecipes_SO _getOrCreateAllRecipesSO()
        {
            var allRecipesSO = Resources.Load<AllRecipes_SO>(_allRecipesSOPath);
            
            if (allRecipesSO is not null) return allRecipesSO;
            
            allRecipesSO = ScriptableObject.CreateInstance<AllRecipes_SO>();
            AssetDatabase.CreateAsset(allRecipesSO, $"Assets/Resources/{_allRecipesSOPath}");
            AssetDatabase.SaveAssets();
            
            return allRecipesSO;
        }
    }

    public enum RecipeName 
    {
        None,
        Plank,
        Log
    }

    public class Recipe
    {
        public readonly RecipeName                RecipeName;
        public          int                       CurrentProgress;
        
        public         string                    RecipeDescription    => RecipeMaster.RecipeDescription;
        public          int                       RequiredProgress    => RecipeMaster.RequiredProgress;
        public          List<Item>                RequiredIngredients => RecipeMaster.RequiredIngredients;
        public          StationName               RequiredStation     => RecipeMaster.RequiredStation;
        public          List<VocationRequirement> RequiredVocations   => RecipeMaster.RequiredVocations;
        public          List<Item>                RecipeProducts      => RecipeMaster.RecipeProducts;
        
        Recipe_Master _recipeMaster;
        Recipe_Master RecipeMaster => _recipeMaster ??= Manager_Recipe.GetRecipe_Master(RecipeName);
        
        public Recipe (RecipeName recipeName)
        {
            RecipeName = recipeName;
            CurrentProgress = 0;
        }
    }

    [Serializable]
    public class Recipe_Master
    {
        public RecipeName RecipeName;
        public string     RecipeDescription;

        public int                       RequiredProgress;
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

        public Recipe_Master(RecipeName recipeName,       string                recipeDescription,
                      int        requiredProgress, List<Item>            requiredIngredients, StationName requiredStation, List<VocationRequirement> requiredVocations, 
                      List<Item> recipeProducts,   List<CraftingQuality> possibleQualities)
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
    }

    [Serializable]
    public class CraftingQuality
    {
        public int             QualityLevel;
        public ItemQualityName QualityName;

        public CraftingQuality(int qualityLevel, ItemQualityName qualityName)
        {
            QualityLevel = qualityLevel;
            QualityName  = qualityName;
        }

        public CraftingQuality(CraftingQuality other)
        {
            QualityLevel = other.QualityLevel;
            QualityName  = other.QualityName;
        }
    }

    [Serializable]
    public class VocationRequirement
    {
        public VocationName VocationName;
        public int          MinimumVocationExperience;
        public int          ExpectedVocationExperience;

        public VocationRequirement(VocationName vocationName, int expectedVocationExperience, int minimumVocationExperience = 0)
        {
            VocationName               = vocationName;
            MinimumVocationExperience  = minimumVocationExperience;
            ExpectedVocationExperience = expectedVocationExperience;
        }
    }
}