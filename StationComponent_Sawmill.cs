using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationComponent_Sawmill : StationComponent
{
    public override StationName StationName => StationName.Sawmill;
    public override StationType StationType => StationType.Crafter;
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Sawyer;
    public float PercentageStorageFilled = 0;
    public float PercentageStorageThreshold = 50; // The percent at which you should transfer products to storage.

    public override RecipeName DefaultProduct => RecipeName.Plank;
    public override List<RecipeName> AllowedRecipes => new List<RecipeName> { RecipeName.Plank };
    public override List<uint> AllowedStoredItemIDs => new List<uint> { 1100, 2300 };
    public override uint OperatingAreaCount => 4;

    protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
    {
        var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
        operatingAreaComponent.transform.SetParent(transform);
        
        switch(operatingAreaID)
        {
            case 1:
                operatingAreaComponent.transform.localPosition = new Vector3(0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.333f, 1f, 1);
                break;
            case 2:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, 1f);
                operatingAreaComponent.transform.localScale = new Vector3(0.333f, 1f, 1);
                break;
            case 3:
                operatingAreaComponent.transform.localPosition = new Vector3(-0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.333f, 1f, 1);
                break;
            case 4:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, -1f);
                operatingAreaComponent.transform.localScale = new Vector3(0.333f, 1f, 1);
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

    public override void InitialiseStartingInventory() { }

    public override List<Item> GetInventoryItemsToHaul()
    {
        return StationData.InventoryData.AllInventoryItems.Where(i => i.ItemID ==2300).ToList();
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Sawyer };
    }

    public override IEnumerator Interact(ActorComponent actor)
    {
        yield break;
        // Open inventory
    }

    public override void CraftItem(RecipeName recipeName, ActorComponent actor)
    {
        if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
        if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var cost = _getCost(recipe.RequiredIngredients, actor);
        var yield = _getYield(recipe.RecipeProducts, actor);

        if (!StationData.InventoryData.RemoveFromInventory(cost)) { Debug.Log($"Crafter does not have all required ingredients"); return; }
        if (!StationData.InventoryData.AddToInventory(yield)) { Debug.Log($"Cannot add products back into inventory"); return; }

        _onCraftItem(yield);
    }

    protected override List<Item> _getCost(List<Item> ingredients, ActorComponent actor)
    {
        return ingredients;

        // Base resource cost on actor relevant skill
    }

    protected override List<Item> _getYield(List<Item> products, ActorComponent actor)
    {
        return products; // For now

        // Base resource yield on actor relevant skill
    }

    public override List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.GetInventoryData().AllInventoryItems
        .Where(item => AllowedStoredItemIDs.Contains(item.ItemID))
        .Select(i => new Item(i.ItemID, i.ItemAmount))
        .ToList();
    }
}
