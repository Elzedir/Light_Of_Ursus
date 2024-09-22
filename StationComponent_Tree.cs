using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StationComponent_Tree : StationComponent
{
    public override StationName StationName => StationName.Tree;
    public override StationType StationType => StationType.Resource;
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Logger;

    public override RecipeName DefaultProduct => RecipeName.Log;
    public override List<RecipeName> AllowedRecipes => new List<RecipeName> { RecipeName.Log };
    public override List<int> AllowedStoredItemIDs => new List<int>();
    public override int OperatingAreaCount => 4;

    protected override OperatingAreaComponent _createOperatingArea(int operatingAreaID)
    {
        var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
        operatingAreaComponent.transform.SetParent(transform);
        
        switch(operatingAreaID)
        {
            case 1:
                operatingAreaComponent.transform.localPosition = new Vector3(1.5f, -0.8f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(1, 0.333f, 1);
                break;
            case 2:
                operatingAreaComponent.transform.localPosition = new Vector3(0, -0.8f, 1.5f);
                operatingAreaComponent.transform.localScale = new Vector3(1, 0.333f, 1);
                break;
            case 3:
                operatingAreaComponent.transform.localPosition = new Vector3(-1.5f, -0.8f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(1, 0.333f, 1);
                break;
            case 4:
                operatingAreaComponent.transform.localPosition = new Vector3(0, -0.8f, -1.5f);
                operatingAreaComponent.transform.localScale = new Vector3(1, 0.333f, 1);
                break;
            default:
                Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {OperatingAreaCount}.");
                break;
        }

        var operatingArea = operatingAreaComponent.AddComponent<BoxCollider>();
        operatingArea.isTrigger = true;
        operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

        return operatingAreaComponent;
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Logger, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
    }

    public override void InitialiseStartingInventory()
    {
        if (StationData.InventoryData.AllInventoryItems.Count == 0)
        {
            StationData.InventoryData.AddToInventory(new List<Item>
        {
            new Item(1100, 100)
        });
        }
    }

    public override void CraftItem(RecipeName recipeName, ActorComponent actor)
    {
        if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
        if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var cost = _getCost(recipe.RequiredIngredients, actor);
        var yield = _getYield(recipe.RecipeProducts, actor);

        Debug.Log($"Cost: {cost.Count} Yield: {yield.Count}");

        if (!StationData.InventoryData.RemoveFromInventory(cost)) { Debug.Log($"Crafter does not have all required ingredients"); return; }
        // Later allow it to partially remove logs to chop the tree down completely.
        if (!actor.ActorData.InventoryAndEquipment.InventoryData.AddToInventory(yield)) { Debug.Log($"Cannot add products back into inventory"); return; }

        Debug.Log("Crafted item");

        _onCraftItem(yield);
    }

    protected override List<Item> _getCost(List<Item> ingredients, ActorComponent actor)
    {
        return new List<Item> 
        { 
            new Item(1100, 1) 
        };

        // Base resource cost on actor relevant skill
    }

    public override IEnumerator Interact(ActorComponent actor)
    {
        Debug.LogError("No Interact method implemented for Tree.");
        yield return null;
    }

    public override List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        Debug.LogError("No GetItemsToDropOff method implemented for Tree.");
        return new List<Item>();
    }

    protected override List<Item> _getYield(List<Item> products, ActorComponent actor)
    {
        return new List<Item> { new Item(1100, 3) }; // For now

        // Base resource yield on actor relevant skill
    }
}
