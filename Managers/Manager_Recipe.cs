using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Recipe : MonoBehaviour
{
    public static Dictionary<RecipeName, Recipe> AllRecipes = new();

    

    public void OnSceneLoaded()
    {
        _initialiseCrafting();
    }

    void _initialiseCrafting()
    {
        _consumableRecipes();
        _processedMaterialRecipes();
        _weaponRecipes();
    }

    void _weaponRecipes()
    {

    }

    void _consumableRecipes()
    {

    }

    void _processedMaterialRecipes()
    {
        IEnumerator craftPlank(Actor_Base actor, StationComponent jobsite)
        {
            if (actor == null) throw new ArgumentException("Actor is null.");

            //yield return actor.BasicMove(GetTaskArea(actor, "Sawmill").bounds.center);
            yield return new WaitForSeconds(0.5f);
            // Play some animation.
            yield return null;
        }

        AllRecipes.Add(RecipeName.Plank, new Recipe(
            recipeName: RecipeName.Plank,
            recipeDescription: "Craft a plank",
            recipeIngredients: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Log"), 2) },
            stationName: StationName.Sawmill,
            recipeProducts: new List<(Item, int)> { (Manager_Item.GetItem(itemName: "Plank"), 1) },
            recipeActions: new List<(string Name, Func<Actor_Base, StationComponent, IEnumerator>)> { ("Craft plank", craftPlank) }
            ));
    }

    public static Recipe GetRecipe(RecipeName recipeName)
    {
        if (!AllRecipes.ContainsKey(recipeName)) throw new ArgumentException($"RecipeName: {recipeName} is not in recipe list.");

        return AllRecipes[recipeName];
    }
}

public enum RecipeName 
{
    Plank
}

public class Recipe
{
    public RecipeName RecipeName;
    public string RecipeDescription;

    public StationName StationName;
    public List<(Item, int)> RecipeIngredients;
    public List<(Item, int)> RecipeProducts = new();

    public List<(string Name, Func<Actor_Base, StationComponent, IEnumerator> Action)> RecipeActions;

    public Recipe(RecipeName recipeName, string recipeDescription, 
        List<(Item, int)> recipeIngredients, StationName stationName, List<(Item, int)> recipeProducts, 
        List<(string Name, Func<Actor_Base, StationComponent, IEnumerator>)> recipeActions)
    {
        RecipeName = recipeName;
        RecipeDescription = recipeDescription;

        StationName = stationName;
        RecipeIngredients = recipeIngredients;
        RecipeProducts = recipeProducts;

        RecipeActions = recipeActions;
    }

    public IEnumerator GetAction(string actionName, Actor_Base actor, StationComponent stationComponent)
    {
        if (!RecipeActions.Any(a => a.Name == actionName)) throw new ArgumentException($"RecipeActions does not contain ActionName: {actionName}");

        return RecipeActions.FirstOrDefault(a => a.Name == actionName).Action(actor, stationComponent);
    }
}