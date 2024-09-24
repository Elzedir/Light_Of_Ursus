using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
        operatingArea.isTrigger = true;
        operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

        return operatingAreaComponent;
    }
    public bool SalesEnabled = false;

    public override void InitialiseStartingInventory() { }

    public override List<Item> GetInventoryItemsToHaul() { return new List<Item>(); }

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
        foreach (var operatingArea in AllOperatingAreasInStation)
        {
            if (operatingArea.OperatingAreaData.CurrentOperatorID == 0) continue;

            var actor = Manager_Actor.GetActor(operatingArea.OperatingAreaData.CurrentOperatorID);

            if (actorHasHaulOrder())
            {
                actor.ActorData.OrderData.ExecuteNextOrder(new List<OrderType> { OrderType.Haul_Deliver, OrderType.Haul_Fetch });
            }
            else if (actorCanHaul())
            {
                _createNewHaulOrder(operatingArea);
            }
            else
            {
                Debug.Log($"Actor: {operatingArea.OperatingAreaData.CurrentOperatorID} can't haul.");
            }

            bool actorHasHaulOrder()
            {
                if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Deliver)) return true;
                if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Fetch)) return true;
                return false;
            }

            bool actorCanHaul()
            {
                // Check if the actor has sufficient space, weight, combat situation, etc.
                return true;
            }
        }
    }

    void _createNewHaulOrder(OperatingAreaComponent operatingArea)
    {
        var stationsToHaulFrom = _getAllStationsToHaulFrom();

            if (stationsToHaulFrom.Count == 0) 
            {
                Debug.Log($"No stations to haul from in Jobsite: {StationData.JobsiteID}");
                return;
            }

            Debug.Log($"Stations to haul from: {string.Join(", ", stationsToHaulFrom.Select(s => $"{s.StationData.StationID}: {s.StationName}"))}");

            // Temporary for now, later, find the nearest station and haul from there.
            var stationToHaulFrom = stationsToHaulFrom[Random.Range(0, stationsToHaulFrom.Count)];

            var itemsToHaul = stationToHaulFrom.StationData.InventoryData.InventoryContainsItems(new List<int> { 1100, 2300 });

            if (itemsToHaul.Count == 0)
            {
                Debug.Log($"No items to haul from {stationToHaulFrom.StationName}.");
                return;
            }

            var haulOrderFetch = new Order_Base(
                orderType: OrderType.Haul_Fetch,
                actorID: operatingArea.OperatingAreaData.CurrentOperatorID, 
                stationID_Source: StationData.StationID, 
                stationID_Destination: stationToHaulFrom.StationData.StationID, 
                orderStatus: OrderStatus.Pending, 
                orderItems: itemsToHaul);
            
            Debug.Log($"HaulOrderFetch: {haulOrderFetch.OrderID} for Actor: {haulOrderFetch.ActorID} from {stationToHaulFrom.StationName}");

            // Have the actor drop at anther station, rather than the logpile for now, or create the logic to decide where it will go.
            
            // Check for any dropped items too.
    }

    List<StationComponent> _getAllStationsToHaulFrom()
    {
        var stationsToHaulFrom = new List<StationComponent>();
        
        var jobsite = Manager_Jobsite.GetJobsite(StationData.JobsiteID);

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (station.GetInventoryItemsToHaul().Count <= 0)
            {
                continue;
            }

            stationsToHaulFrom.Add(station);
        }

        return stationsToHaulFrom;
    }

    public override void CraftItem(RecipeName recipeName, ActorComponent actor)
    {
        Debug.LogError("Log Pile does not craft items.");
    }

    public override IEnumerator Interact(ActorComponent actor)
    {
        Debug.LogError("No Interact method implemented for Log Pile.");
        yield return null;
    }

    protected override List<Item> _getCost(List<Item> ingredients, ActorComponent actor)
    {
        return new List<Item>(); // For now

        // Base resource cost on actor relevant skill
    }

    protected override List<Item> _getYield(List<Item> products, ActorComponent actor)
    {
        return new List<Item>(); // For now

        // Base resource yield on actor relevant skill
    }
}
