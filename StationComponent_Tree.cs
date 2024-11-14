using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Managers;
using UnityEngine;

public class StationComponent_Tree : StationComponent
{
    public override StationName StationName => StationName.Tree;
    public override StationType StationType => StationType.Resource;
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Logger;

    public override RecipeName DefaultProduct => RecipeName.Log;
    public override HashSet<RecipeName> AllowedRecipes => new() { RecipeName.Log };
    public override HashSet<uint> AllowedStoredItemIDs => new();
    public override HashSet<uint> DesiredStoredItemIDs => new();
    public override uint OperatingAreaCount => 4;

    protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
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

        var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
        operatingArea.isTrigger = true;
        operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

        return operatingAreaComponent;
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Logger, };
    }

    public override void InitialiseStartingInventory()
    {
        if (StationData.InventoryData.AllInventoryItems.Count == 0)
        {
            StationData.InventoryData.AddToInventory(new List<Item>());
        }
    }

    public override void CraftItem(RecipeName recipeName, ActorComponent actor)
    {
        if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
        if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var cost = _getCost(recipe.RequiredIngredients, actor);
        var yield = _getYield(recipe.RecipeProducts, actor);
        
        if (!StationData.InventoryData.InventoryContainsAllItems(cost)) { Debug.Log($"Inventory does not contain cost items."); return; }
        if (!StationData.InventoryData.HasSpaceForItems(yield)) { Debug.Log($"Inventory does not have space for yield items."); return; }

        StationData.InventoryData.RemoveFromInventory(cost);
        // Have another system where the tree loses durability instead or something.
        // Later allow it to partially remove logs to chop the tree down completely.
        StationData.InventoryData.AddToInventory(yield);

        _onCraftItem(yield);
    }

    protected override List<Item> _getCost(List<Item> ingredients, ActorComponent actor)
    {
        return new List<Item>();

        // Base resource cost on actor relevant skill
    }

    public override IEnumerator Interact(ActorComponent actor)
    {
        Debug.LogError("No Interact method implemented for Tree.");
        yield return null;
    }

    protected override List<Item> _getYield(List<Item> products, ActorComponent actor)
    {
        return new List<Item> { new Item(1100, 1) }; // For now

        // Base resource yield on actor relevant skill
    }
}
