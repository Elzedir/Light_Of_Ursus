using System;
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
    public override List<uint> AllowedStoredItemIDs => new List<uint> { 1100, 2300 };
    public override List<uint> DesiredStoredItemIDs => new List<uint> { 1100, 2300 };

    public override uint OperatingAreaCount => 4;
    protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
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

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.None };
    }

    public override List<Item> GetItemsToDeliver(IInventoryOwner inventoryOwner)
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

            // if (actorHasHaulOrder())
            // {
            //     //actor.ActorData.OrderData.ExecuteNextOrder(new List<OrderType> { OrderType.Haul_Deliver, OrderType.Haul_Fetch });
            //     Debug.Log($"Actor: {operatingArea.OperatingAreaData.CurrentOperatorID} has a haul order.");
            //     return;
            // }

            if (actorCanHaul())
            {
                // Deliver resources first before hauling more

                if (_canDeliverItems(actor)) return;

                if (_fetchItemsCheck(actor)) return;

                //Debug.Log($"No stations to haul from.");
                return;

                // Have the actor drop at anther station, rather than the logpile for now, or create the logic to decide where it will go.

                // Check for any dropped items too.

                // Trying new job system

                // var order = _createNewHaulOrder(operatingArea);

                // if (order == null) return;

                // actor.ActorData.CurrentOrder = order;
                // actor.ActorData.CurrentOrder.ExecuteOrder();
            }
            else
            {
                Debug.Log($"Actor: {operatingArea.OperatingAreaData.CurrentOperatorID} can't haul.");
            }

            // bool actorHasHaulOrder()
            // {
            //     //if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Deliver)) return true;
            //     //if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Fetch)) return true;
            //     if (actor.ActorData.CurrentOrder != null) return true;
            //     return false;
            // }

            bool actorCanHaul()
            {
                // Check if the actor has sufficient space, weight, combat situation, etc.

                if (actor.ActorHaulCoroutine != null) return false;

                return true;
            }
        }
    }

    protected bool _canDeliverItems(ActorComponent actor)
    {
        var stationsToHaulTo = _getAllStationsToHaulTo(actor);

        if (stationsToHaulTo.Count > 0) Debug.Log($"Stations to haul to: {stationsToHaulTo.Count}");

        foreach (var stationToHaulTo in stationsToHaulTo)
        {
            var itemsToDeliver = actor.ActorData.InventoryData.InventoryContainsReturnedItems(stationToHaulTo.AllowedStoredItemIDs);

            Debug.Log($"Items to deliver: {itemsToDeliver.Count}");

            if (itemsToDeliver.Count <= 0) continue;

            StartCoroutine(_deliverItems(actor, stationToHaulTo, itemsToDeliver));
            return true;
        }
 
        return false;
    }

    IEnumerator _deliverItems(ActorComponent actor, StationComponent station, List<Item> itemsToDeliver)
    {
        bool success = false;

        var ActorID = actor.ActorData.ActorID;
        var StationID_Destination = station.StationData.StationID;

        if (ActorID == 0 || StationID_Destination == 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Destination} is invalid.");
            throw new Exception("Invalid Order.");
        }

        if (actor.transform.position == null)
        {
            Debug.Log("Actor position is null.");
            throw new Exception("Invalid Actor Position.");
        }

        // Eventually put in a check to see if the station still has the resources. If not, then return.

        if (Vector3.Distance(actor.transform.position, station.transform.position) > (station.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, station.CollectionPoint.position));

            if (_deliver(actor, station, itemsToDeliver)) success = true;

            actor.ActorHaulCoroutine = null;
        }

        if (success)
        {
            actor.ActorData.CurrentOrder = null;
        }
    }

    bool _deliver(ActorComponent actor, StationComponent station, List<Item> orderItems)
    {
        if (!actor.ActorData.InventoryData.RemoveFromInventory(orderItems))
        {
            Debug.Log("Failed to remove items from Actor inventory.");
            return false;
        }

        if (!station.StationData.InventoryData.AddToInventory(orderItems))
        {
            Debug.Log("Failed to add items to Station inventory.");

            if (actor.ActorData.InventoryData.AddToInventory(orderItems))
            {
                Debug.Log("Failed to add items back to Actor inventory.");
                return false;
            }

            return false;
        }

        return true;
    }

    bool _fetchItemsCheck(ActorComponent actor)
    {
        if (Jobsite == null)
        {
            Debug.Log($"Jobsite: {StationData.JobsiteID} is null.");
            return false;
        }

        var stationAndItems = Jobsite.GetStationToHaulFrom(actor);

        if (stationAndItems.Station == null)
        {
            Debug.Log($"No stations to haul from.");
            return false;
        }

        if (stationAndItems.Items.Count == 0)
        {
            Debug.Log($"No items to haul from {stationAndItems.Station.StationName}.");
            return false;
        }

        StartCoroutine(_fetchItems(actor, stationAndItems.Station, stationAndItems.Items));

        return true;
    }

    IEnumerator _fetchItems( ActorComponent actor, StationComponent stationDestination, List<Item> itemsToFetch)
    {
        bool orderSuccess = false;

        var ActorID = actor.ActorData.ActorID;
        var StationID_Destination = stationDestination.StationData.StationID;

        if (ActorID == 0 || StationID_Destination == 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Destination} is invalid.");
            throw new Exception("Invalid Order.");
        }

        if (actor.transform.position == null)
        {
            Debug.Log("Actor position is null.");
            throw new Exception("Invalid Actor Position.");
        }

        // Eventually put in a check to see if the station still has the resources. If not, then return.

        if (Vector3.Distance(actor.transform.position, stationDestination.transform.position) > (stationDestination.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, stationDestination.CollectionPoint.position));

            if (_fetch(stationDestination, actor, itemsToFetch)) orderSuccess = true;

            actor.ActorHaulCoroutine = null;
        }

        if (orderSuccess)
        {
            actor.ActorData.CurrentOrder = null;
        }
    }

    protected bool _fetch(StationComponent station, ActorComponent actor, List<Item> orderItems)
    {
        if (!station.StationData.InventoryData.RemoveFromInventory(orderItems))
        {
            Debug.Log($"Failed to remove items from Station: {station.StationData.StationID} inventory.");
            return false;
        }

        if (!actor.ActorData.InventoryData.AddToInventory(orderItems))
        {
            Debug.Log("Failed to add items to Actor inventory.");

            if (station.StationData.InventoryData.AddToInventory(orderItems))
            {
                Debug.Log("Failed to add items back to Station inventory.");
                return false;
            }

            return false;
        }

        //Debug.Log($"Actor: {actor.ActorData.ActorID} successfully fetched items from Station: {station.StationData.StationID}.");

        return true;
    }

    protected IEnumerator _moveOperatorToOperatingArea(ActorComponent actor, Vector3 position)
    {
        yield return actor.StartCoroutine(actor.BasicMove(position));

        if (actor.transform.position != position) actor.transform.position = position;
    }

    // Order_Base _createNewHaulOrder(OperatingAreaComponent operatingArea)
    // {
    //     var stationsToHaulFrom = _getAllStationsToHaulFrom();

    //         if (stationsToHaulFrom.Count == 0) 
    //         {
    //             return null;
    //         }

    //         // Temporary for now, later, find the nearest station and haul from there.
    //         var stationToHaulFrom = stationsToHaulFrom[UnityEngine.Random.Range(0, stationsToHaulFrom.Count)];

    //         var itemsToHaul = stationToHaulFrom.StationData.InventoryData.InventoryContainsReturnedItems(AllowedStoredItemIDs);

    //         if (itemsToHaul.Count == 0)
    //         {
    //             Debug.Log($"No items to haul from {stationToHaulFrom.StationName}.");
    //             return null;
    //         }

    //         var haulOrderFetch = new Order_Base(
    //             orderType: OrderType.Haul_Fetch,
    //             actorID: operatingArea.OperatingAreaData.CurrentOperatorID, 
    //             stationID_Source: StationData.StationID, 
    //             stationID_Destination: stationToHaulFrom.StationData.StationID, 
    //             orderStatus: OrderStatus.Pending, 
    //             orderItems: itemsToHaul);
            
    //         Debug.Log($"HaulOrderFetch: {haulOrderFetch.OrderID} for Actor: {haulOrderFetch.ActorID} from {stationToHaulFrom.StationName}");

    //         return haulOrderFetch;

    //         // Have the actor drop at anther station, rather than the logpile for now, or create the logic to decide where it will go.
            
    //         // Check for any dropped items too.
    // }

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
