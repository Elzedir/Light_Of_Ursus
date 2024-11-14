using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Managers;
using UnityEngine;

public class StationComponent_LogPile : StationComponent
{
    public override StationName StationName => StationName.Log_Pile;
    public override StationType StationType => StationType.Storage;
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Hauler;

    public override RecipeName DefaultProduct => RecipeName.None; // Fix hauling so that it doesn't need a recipe.
    public override HashSet<RecipeName> AllowedRecipes => new();
    public override HashSet<uint> AllowedStoredItemIDs => new() { 1100, 2300 };
    public override HashSet<uint> DesiredStoredItemIDs => new() { 1100, 2300 };

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

    public override void InitialiseStartingInventory() { }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.None };
    }

    protected override void _operateStation()
    {
        foreach (var operatingArea in AllOperatingAreasInStation)
        {
            var actor = operatingArea.OperatingAreaData.CurrentOperator;
            
            if (actor is null) continue;

            // if (actorHasHaulOrder())
            // {
            //     //actor.ActorData.OrderData.ExecuteNextOrder(new List<OrderType> { OrderType.Haul_Deliver, OrderType.Haul_Fetch });
            //     Debug.Log($"Actor: {operatingArea.OperatingAreaData.CurrentOperatorID} has a haul order.");
            //     return;
            // }

            if (_actorCanHaul(actor))
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
                //Debug.Log($"Actor: {operatingArea.OperatingAreaData.CurrentOperatorID} can't haul.");
            }

            // bool actorHasHaulOrder()
            // {
            //     //if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Deliver)) return true;
            //     //if (actor.ActorData.OrderData.HasCurrentOrder(OrderType.Haul_Fetch)) return true;
            //     if (actor.ActorData.CurrentOrder != null) return true;
            //     return false;
            // }
        }
    }
    
    bool _actorCanHaul(ActorComponent actor)
    {
        // Check if the actor has sufficient space, weight, combat situation, etc.

        if (actor.ActorHaulCoroutine != null) return false;

        return true;
    }

    bool _canDeliverItems(ActorComponent actor)
    {
        if (Jobsite is null)
        {
            Debug.Log($"Jobsite: {StationData.JobsiteID} is null.");
            return false;
        }

        var stationAndItems = Jobsite.GetStationToDeliverTo(actor);

        if (stationAndItems.Station is null)
        {
            //Debug.Log($"No stations to haul to.");
            return false;
        }

        if (stationAndItems.Items.Count is 0)
        {
            Debug.Log($"No items to haul to {stationAndItems.Station.StationName}.");
            return false;
        }

        StartCoroutine(_deliverItems(actor, stationAndItems.Station, stationAndItems.Items));

        return true;
    }

    
    // Also add in a delay to mimic them moving, so that we can allocate properly so that they don't all go to the same station.

    IEnumerator _deliverItems(ActorComponent actor, StationComponent station, List<Item> itemsToDeliver)
    {
        bool orderSuccess = false;

        var actorID = actor.ActorData.ActorID;
        var stationID_Destination = station.StationData.StationID;

        if (actorID is 0 || stationID_Destination is 0)
        {
            Debug.Log($"HaulerID: {actorID}, StationID: {stationID_Destination} is invalid.");
            throw new Exception("Invalid Order.");
        }

        if (actor is null)
        {
            Debug.Log("Actor is null.");
            throw new Exception("Invalid Actor.");
        }

        // Eventually put in a check to see if the station still has the resources. If not, then return.

        if (Vector3.Distance(actor.transform.position, station.transform.position) > (station.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, station.CollectionPoint.position));
        }
        
        if (_deliver(actor, station, itemsToDeliver)) orderSuccess = true;
        
        actor.ActorHaulCoroutine = null;

        if (orderSuccess)
        {
            actor.ActorData.CurrentOrder = null;
        }
    }

    bool _deliver(ActorComponent actor, StationComponent station, List<Item> orderItems)
    {
        actor.ActorData.InventoryData.RemoveFromInventory(orderItems);
        station.StationData.InventoryData.AddToInventory(orderItems);

        //Debug.Log($"Actor: {actor.ActorData.ActorID} successfully delivered items to Station: {station.StationData.StationID}.");

        return true;
    }

    bool _fetchItemsCheck(ActorComponent actor)
    {
        if (Jobsite is null)
        {
            Debug.Log($"Jobsite: {StationData.JobsiteID} is null.");
            return false;
        }

        var stationAndItems = Jobsite.GetStationToFetchFrom(actor);

        if (stationAndItems.Station is null)
        {
            //Debug.Log($"No stations to haul from.");
            return false;
        }

        if (stationAndItems.Items.Count == 0)
        {
            //Debug.Log($"No items to haul from {stationAndItems.Station.StationName}.");
            return false;
        }

        foreach (var item in stationAndItems.Items.Where(item => item.ItemAmount == 0))
        {
            Debug.LogError($"Item: {item.ItemName} - {item.ItemID} has qty: {item.ItemAmount}.");
            return false;
        }

        StationData.InventoryData.AddToInventoryItemsOnHold(stationAndItems.Items);

        StartCoroutine(_fetchItems(actor, stationAndItems.Station, stationAndItems.Items));

        return true;
    }

    IEnumerator _fetchItems( ActorComponent actor, StationComponent stationDestination, List<Item> itemsToFetch)
    {
        var orderSuccess = false;

        var actorID = actor.ActorData.ActorID;
        var stationID_Destination = stationDestination.StationData.StationID;

        if (actorID is 0 || stationID_Destination is 0)
        {
            Debug.LogError($"HaulerID: {actorID}, StationID: {stationID_Destination} is invalid.");
            StationData.InventoryData.RemoveFromFetchItemsOnHold(itemsToFetch);
            throw new Exception("Invalid Order.");
        }

        if (actor is null)
        {
            Debug.LogError("Actor is null.");
            StationData.InventoryData.RemoveFromFetchItemsOnHold(itemsToFetch);
            throw new Exception("Invalid Actor.");
        }

        if (!stationDestination.StationData.InventoryData.InventoryContainsAnyItems(itemsToFetch))
        {
            Debug.LogError($"Station: {stationDestination.StationData.StationID} does not have the items to fetch.");
            yield break;
        }

        if (Vector3.Distance(actor.transform.position, stationDestination.transform.position) > (stationDestination.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, stationDestination.CollectionPoint.position));
        }

        if (_fetch(stationDestination, actor, itemsToFetch)) orderSuccess = true;
        
        StationData.InventoryData.RemoveFromFetchItemsOnHold(itemsToFetch);
        
        actor.ActorHaulCoroutine     = null;
        actor.ActorData.CurrentOrder = null;

        if (!orderSuccess)
        {
            Debug.LogWarning($"Failed to fetch items from Station: {stationDestination.StationData.StationID}.");
        }
    }

    bool _fetch(StationComponent station, ActorComponent actor, List<Item> orderItems)
    {
        station.StationData.InventoryData.RemoveFromInventory(orderItems);

        actor.ActorData.InventoryData.AddToInventory(orderItems);
        
        return true;
    }

    IEnumerator _moveOperatorToOperatingArea(ActorComponent actor, Vector3 position)
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
