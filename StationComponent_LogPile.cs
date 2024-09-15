using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StationComponent_LogPile : StationComponent
{
    public override StationName StationName => StationName.Log_Pile;
    public override StationType StationType => StationType.Storage;
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Hauler;

    public override RecipeName DefaultProduct => RecipeName.None; // Fix hauling so that it doesn't need a recipe.
    public override List<RecipeName> AllowedRecipes => new List<RecipeName>();
    public override List<int> AllowedStoredItemIDs => new List<int> { 1100, 2300 };

    public override int OperatingAreaCount => 4;
    protected override OperatingAreaComponent _createOperatingArea(int operatingAreaID)
    {
        var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
        operatingAreaComponent.transform.SetParent(transform);
        
        switch(operatingAreaID)
        {
            case 1:
                operatingAreaComponent.transform.localPosition = new Vector3(0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 2:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, 0.75f);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 3:
                operatingAreaComponent.transform.localPosition = new Vector3(-0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 4:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, -0.75f);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
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
    public bool SalesEnabled = false;

    public override void InitialiseStationNameAndType()
    {
        StationData.SetStationName(StationName.Log_Pile);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.None };
    }

    public override List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.GetInventoryData().AllInventoryItems.Where(i => i.ItemID == 2300 || i.ItemID == 1100)
        .Select(i => new Item(i.ItemID, i.ItemAmount)).ToList();
    }

    protected override void _operateStation()
    {
        Debug.Log($"Station: {name} CurrentProduct: {StationData.StationProgressData.CurrentProduct.RecipeName}");

        foreach (var operatingArea in AllOperatingAreasInStation)
        {
            if (!operatingArea.CanHaul()) continue;

            var stationsToHaulFrom = Manager_Jobsite.GetJobsite(StationData.JobsiteID).AllStationsInJobsite
                .Where(s => s.StationData.StationType == StationType.Crafter)
                .Where(s => s.AllowedRecipes.Contains(RecipeName.Plank))
                .Where(s => s.StationData.InventoryData.InventoryContainsAnyItems(new List<int> { 2300 }).Count > 0)
                .ToList();

            if (stationsToHaulFrom.Count == 0) continue;

            // Temporary for now, later, find the nearest station and haul from there.
            var stationToHaulFrom = stationsToHaulFrom[Random.Range(0, stationsToHaulFrom.Count)];

            var itemsToHaul = stationToHaulFrom.StationData.InventoryData.InventoryContainsAnyItems(new List<int> { 2300 });

            if (itemsToHaul.Count == 0) continue;

            // Find a new way to add new orderIDs.
            var haulOrderFetch = new Order_Haul_Fetch(0, 0, stationToHaulFrom.StationData.StationID, StationData.StationID, OrderStatus.Pending, itemsToHaul);

            // Check if there are any items in any of the stations that need to be hauled, usually from crafter to storage since raw materials would be directly transferred to the gatherer. Or from a raw material storage to the crafter.
            // Check for any dropped items too.
        }
    }
}
