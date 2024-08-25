using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Tree : StationComponent
{
    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Tree);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Logger, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
    }

    public override void InitialiseAllowedRecipes()
    {
        AllowedRecipes.Add(RecipeName.Log);
    }

    public override void InitialiseStartingInventory()
    {
        if (StationData.InventoryData.InventoryItems.Count == 0)
        {
            StationData.InventoryData.AddToInventory(new List<Item>
        {
            (Manager_Item.GetItem(1100, 100))
        });
        }
    }

    public override void CraftItem(RecipeName recipeName, Actor_Base actor)
    {
        if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
        if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var cost = _getCost(recipe.RequiredIngredients, actor);
        var yield = _getYield(recipe.RecipeProducts, actor);

        if (!StationData.InventoryData.RemoveFromInventory(cost)) { Debug.Log($"Crafter does not have all required ingredients"); return; }
        // Later allow it to partially remove logs to chop the tree down completely.
        if (!StationData.InventoryData.AddToInventory(yield)) { Debug.Log($"Cannot add products back into inventory"); return; }

        _onCraftItem(yield);
    }

    protected override List<Item> _getCost(List<Item> ingredients, Actor_Base actor)
    {
        return new List<Item>(); // For now

        // Base resource cost on actor relevant skill
    }

    protected override List<Item> _getYield(List<Item> products, Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(1100, 3) }; // For now

        // Base resource yield on actor relevant skill
    }
}
