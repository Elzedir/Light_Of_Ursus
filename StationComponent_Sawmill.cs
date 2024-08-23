using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Sawmill : StationComponent
{
    public float PercentageStorageFilled = 0;
    public float PercentageStorageThreshold = 50; // The percent at which you should transfer products to storage.

    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Sawmill);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Sawyer, EmployeePosition.Sawyer, EmployeePosition.Assistant_Sawyer };
    }

    public override void InitialiseAllowedRecipes()
    {
        AllowedRecipes.Add(RecipeName.Plank);
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        yield break;
        // Open inventory
    }

    public override bool EmployeeCanUse(EmployeePosition employeePosition)
    {
        return new List<EmployeePosition> 
        { 
            EmployeePosition.Sawyer, EmployeePosition.Assistant_Sawyer 
        }
        .Contains(employeePosition);
    }

    public override void CraftItem(RecipeName recipeName, Actor_Base actor)
    {
        if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
        if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var cost = _getCost(recipe.RecipeIngredients, actor);
        var yield = _getYield(recipe.RecipeProducts, actor);

        if (!StationData.InventoryData.RemoveFromInventory(cost)) { Debug.Log($"Crafter does not have all required ingredients"); return; }
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
