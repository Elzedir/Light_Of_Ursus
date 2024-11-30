using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using EmployeePosition;
using Items;
using Jobsite;
using Managers;
using NUnit.Framework;
using ScriptableObjects;
using UnityEngine;

public class Manager_Order : MonoBehaviour, IDataPersistence
{
    static AllOrders_SO _displayAllOrders;
    public static AllOrders_SO DisplayAllOrders { get { return _displayAllOrders ??= Resources.Load<AllOrders_SO>("ScriptableObjects/AllOrders_SO"); } }
    public static Dictionary<int, OrderData> AllOrderData;
    static int _lastUnusedOrderID = 1;

    public void SaveData(SaveData data)
    {
        data.SavedOrderData = new SavedOrderData(AllOrderData.Values.ToList());
    }
    public void LoadData(SaveData data)
    {
        AllOrderData = data.SavedOrderData?.AllOrderData.ToDictionary(x => x.ActorID);
    }

    public void OnSceneLoaded()
    {
        Manager_Initialisation.OnInitialiseManagerOrder += _initialise;
    }

    void _initialise()
    {
        _initialiseAllOrderData();
    }

    void _initialiseAllOrderData()
    {
        if (AllOrderData == null) AllOrderData = new();

        DisplayAllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static void AddOrderData(OrderData orderData)
    {
        if (AllOrderData.ContainsKey(orderData.ActorID))
        {
            Debug.Log($"OrderData with ActorID: {orderData.ActorID} already exists.");
            return;
        }

        AllOrderData.Add(orderData.ActorID, orderData);
        DisplayAllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static void RemoveOrderData(OrderData orderData)
    {
        if (!AllOrderData.ContainsKey(orderData.ActorID))
        {
            Debug.Log($"OrderData with ActorID: {orderData.ActorID} does not exist.");
            return;
        }

        AllOrderData.Remove(orderData.ActorID);
        DisplayAllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static void RemoveAllOrderData()
    {
        AllOrderData.Clear();
        DisplayAllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static void GetOrderData(int orderID)
    {
        if (!AllOrderData.ContainsKey(orderID))
        {
            Debug.Log($"OrderData with ID: {orderID} does not exist.");
            return;
        }

        DisplayAllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static int GetOrderBaseID()
    {
        while(AllOrderData.ContainsKey(_lastUnusedOrderID))
        {
            _lastUnusedOrderID++;
        }

        return _lastUnusedOrderID;
    }
}

[Serializable]
public class OrderData
{
    public int ActorID;
    public OrderData(int actorID) => ActorID = actorID;

    int _lastUnusedOrderID = 1;
    public List<Order_Base> AllCurrentOrders = new();
    public List<Order_Base> AllCompletedOrders = new();

    Coroutine _orderCoroutine;


    // Create a priority queue system so replacement orders can be created and executed in the correct order of priority. And that deliver orders can follow fetch orders.

    public void ExecuteNextOrder(List<OrderType> orderTypes = null)
    {
        if (AllCurrentOrders.Count <= 0) return;

        if (AllCurrentOrders.Any(o => o.OrderStatus == OrderStatus.Active))
        {
            Debug.Log($"Actor: {ActorID} already has an active order.");
            return;
        }

        Order_Base nextOrder = AllCurrentOrders.FirstOrDefault();

        if (nextOrder == null)
        {
            Debug.Log("No highest priority order found.");
            return;
        }

        nextOrder.ExecuteOrder();

        if (orderTypes == null)
        {
            if (AllCurrentOrders.Count > 0)
            {
                AllCurrentOrders[0].ExecuteOrder();
            }
            else
            {
                Debug.Log($"No orders for Actor: {ActorID}");
            }
        }
        else
        {
            if (AllCurrentOrders.Any(o => orderTypes.Contains(o.OrderType)))
            {
                AllCurrentOrders.FirstOrDefault(o => orderTypes.Contains(o.OrderType)).ExecuteOrder();
            }
            else
            {
                Debug.Log($"No orders of type: {string.Join(", ", orderTypes)} for Actor: {ActorID}");
            }
        }
    }

    public void ExecuteOrder(int orderID)
    {
        if (!AllCurrentOrders.Any(o => o.OrderID == orderID))
        {
            Debug.Log($"Order: {orderID} does not exist for Actor: {ActorID}");
            return;
        }

        AllCurrentOrders.FirstOrDefault(o => o.OrderID == orderID).ExecuteOrder();
    }

    public bool HasCurrentOrder(OrderType orderType = OrderType.None)
    {
        if (orderType == OrderType.None) return AllCurrentOrders.Count > 0;

        return AllCurrentOrders.Any(o => o.OrderType == orderType);
    }

    public void AddCurrentOrder(Order_Base order)
    {
        if (AllCurrentOrders.Any(o => o.OrderID == order.OrderID))
        {
            Debug.Log($"Order: {order.OrderID} already exists for Actor: {ActorID}");
            return;
        }

        AllCurrentOrders.Add(order);
    }

    public void ReplaceCurrentOrder(Order_Base order)
    {
        if (!AllCurrentOrders.Any(o => o.OrderID == order.OrderID))
        {
            Debug.Log($"Order: {order.OrderID} does not exist for Actor: {ActorID}");
            return;
        }

        // Stick in the priority queue right behind the current order.

        var index = AllCurrentOrders.IndexOf(AllCurrentOrders.FirstOrDefault(o => o.OrderID == order.OrderID));

        AllCurrentOrders.RemoveAll(o => o.OrderID == order.OrderID);
        AllCurrentOrders.Insert(index, order);
    }

    public void CompleteOrder(int orderID)
    {
        if (!AllCurrentOrders.Any(o => o.OrderID == orderID))
        {
            Debug.Log($"Order: {orderID} does not exist for Actor: {ActorID}");
            return;
        }

        AllCompletedOrders.Add(AllCurrentOrders.FirstOrDefault(o => o.OrderID == orderID));
        AllCurrentOrders.RemoveAll(o => o.OrderID == orderID);
    }

    public void RemoveCurrentOrder(int orderID)
    {
        if (!AllCurrentOrders.Any(o => o.OrderID == orderID))
        {
            Debug.Log($"Order: {orderID} does not exist for Actor: {ActorID}");
            return;
        }

        AllCurrentOrders.RemoveAll(o => o.OrderID == orderID);
    }

    public void RemoveCompletedOrders()
    {
        AllCompletedOrders.Clear();
    }

    public void RemoveCurrentOrders()
    {
        AllCurrentOrders.Clear();
    }

    public Order_Base GetOrderById(int orderID)
    {
        return AllCurrentOrders.FirstOrDefault(o => o.OrderID == orderID);
    }

    public int GetOrderID()
    {
        // Later put in a way to clear them if the orders number over 1000 or 10 000, to prevent overflow.
        while(AllCurrentOrders.Any(o => o.OrderID == _lastUnusedOrderID))
        {
            _lastUnusedOrderID++;
        }

        return _lastUnusedOrderID;
    }
}

public enum OrderType
{
    None,
    Haul_Fetch,
    Haul_Deliver,
    Craft,
    Hire,
}

public enum OrderStatus
{
    Pending,
    Active,
    Complete
}

[Serializable]
public class Order_Base
{
    public uint OrderID;
    public OrderType OrderType;
    public uint ActorID;
    Actor_Component _actor;
    public Actor_Component Actor { get { return _actor ??= Actor_Manager.GetActor(ActorID); } }
    public uint StationID_Source;
    StationComponent _station_Source;
    public StationComponent Station_Source { get { return _station_Source ??= Manager_Station.GetStation(StationID_Source); } }
    public uint StationID_Destination;
    StationComponent _station_Destination;
    public StationComponent Station_Destination { get { return _station_Destination ??= Manager_Station.GetStation(StationID_Destination); } }
    public uint JobsiteID;
    JobsiteComponent _jobsite;
    public JobsiteComponent Jobsite { get { return _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID); } }
    public OrderStatus OrderStatus;
    public List<Item> OrderItems;

    public Order_Base ReturnOrder;

    public void AddReturnOrder(Order_Base returnOrder) => ReturnOrder = returnOrder;

    protected Coroutine _orderCoroutine;
    protected Coroutine _actorMoveCoroutine;

    public Order_Base(OrderType orderType, uint actorID, uint stationID_Source, uint stationID_Destination, OrderStatus orderStatus, List<Item> orderItems)
    {
        OrderType = orderType;
        ActorID = actorID;
        //OrderID = Actor.ActorData.OrderData.GetOrderID();
        StationID_Source = stationID_Source;
        JobsiteID = Station_Source.StationData.JobsiteID;
        StationID_Destination = stationID_Destination;
        OrderStatus = orderStatus;
        OrderItems = orderItems;

        //Actor.ActorData.OrderData.AddCurrentOrder(this);
    }

    public void ChangeActor(uint actorID)
    {
        //Actor.ActorData.OrderData.RemoveCurrentOrder(OrderID);
        ActorID = actorID;
        _actor = null;
        //Actor.ActorData.OrderData.AddCurrentOrder(this);
    }
    
    public void ChangeStationSource(uint stationID)
    {
        StationID_Source = stationID;
        _station_Source = null;
    }

    public void ChangeStationDestination(uint stationID)
    {
        StationID_Destination = stationID;
        _station_Destination = null;
    }

    public void ExecuteOrder()
    {
        // if (OrderStatus == OrderStatus.Pending)
        // {
        //     HaltCurrentOrder();
        //
        //     _orderCoroutine = Actor.StartCoroutine(_executeOrder());
        // }
    }
    // public IEnumerator _executeOrder()
    // {
    //     OrderStatus = OrderStatus.Active;
    //     bool orderSuccess = false;
    //
    //     if (ActorID == 0 || StationID_Destination == 0 || OrderItems.Count <= 0)
    //     {
    //         Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Destination}, or OrderItemIDs {OrderItems.Count} is invalid.");
    //         HaltCurrentOrder();
    //         throw new Exception("Invalid Order.");
    //     }
    //
    //     if (Actor.transform.position == null)
    //     {
    //         Debug.Log("Actor position is null.");
    //         HaltCurrentOrder();
    //         throw new Exception("Invalid Actor Position.");
    //     }
    //
    //     // Eventually put in a check to see if the station still has the resources. If not, then return.
    //
    //     if (Vector3.Distance(Actor.transform.position, Station_Destination.transform.position) > (Station_Destination.BoxCollider.bounds.extents.magnitude + Actor.Collider.bounds.extents.magnitude * 1.1f))
    //     {
    //         HaltCurrentMoveOrder();
    //
    //         yield return _actorMoveCoroutine = Actor.StartCoroutine(_moveOperatorToOperatingArea(Station_Destination.CollectionPoint.position));
    //
    //         if (_transferItems()) orderSuccess = true;
    //     }
    //
    //     OrderStatus = OrderStatus.Complete;
    //
    //     if (OrderType == OrderType.Haul_Fetch && orderSuccess)
    //     {
    //         //Actor.ActorData.OrderData.CompleteOrder(OrderID);
    //         Actor.ActorData.CurrentOrder = null;
    //         AddReturnOrder(_createReturnDeliverOrder());
    //         ReturnOrder.ExecuteOrder();
    //     }
    //     else
    //     {
    //         //Actor.ActorData.OrderData.CompleteOrder(OrderID);
    //         Actor.ActorData.CurrentOrder = null;
    //     }
    // }
    //
    // protected bool _transferItems()
    // {
    //     if (OrderType == OrderType.Haul_Fetch)
    //     {
    //         if (!Station_Destination.StationData.InventoryData.RemoveFromInventory(OrderItems))
    //         {
    //             Debug.Log($"Failed to remove items from Station: {Station_Destination.StationData.StationID} inventory.");
    //             return false;
    //         }
    //
    //         if (!Actor.ActorData.InventoryData.AddToInventory(OrderItems))
    //         {
    //             Debug.Log("Failed to add items to Actor inventory.");
    //
    //             if (Station_Destination.StationData.InventoryData.AddToInventory(OrderItems))
    //             {
    //                 Debug.Log("Failed to add items back to Station inventory.");
    //                 return false;
    //             }
    //
    //             return false;
    //         }
    //
    //         Debug.Log($"Actor: {ActorID} successfully fetched items from Station: {StationID_Destination}.");
    //
    //         return true;
    //     }
    //
    //     if (OrderType == OrderType.Haul_Deliver)
    //     {
    //         if (!Actor.ActorData.InventoryData.RemoveFromInventory(OrderItems))
    //         {
    //             Debug.Log("Failed to remove items from Actor inventory.");
    //             return false;
    //         }
    //
    //         if (!Station_Destination.StationData.InventoryData.AddToInventory(OrderItems))
    //         {
    //             Debug.Log("Failed to add items to Station inventory.");
    //
    //             if (Actor.ActorData.InventoryData.AddToInventory(OrderItems))
    //             {
    //                 Debug.Log("Failed to add items back to Actor inventory.");
    //                 return false;
    //             }
    //
    //             return false;
    //         }
    //
    //         return true;
    //     }
    //
    //     Debug.Log("Invalid OrderType.");
    //     return false;
    // }
    protected Order_Base _createReturnDeliverOrder()
    {
        var stationsToHaulTo = _getAlLStationsToHaulTo();

        if (stationsToHaulTo.Count == 0)
        {
            Debug.Log($"No stations to haul to in Jobsite: {JobsiteID}");
            Debug.Log($"Creating return deliver order for Actor: {ActorID} to Station: {StationID_Source}.");

            return new Order_Base(OrderType.Haul_Deliver, ActorID, StationID_Destination, StationID_Source, OrderStatus.Pending, OrderItems);
        }

        foreach (var stationToHaulTo in stationsToHaulTo)
        {
            List<Item> itemsToHaul = new();//Actor.ActorData.InventoryData.InventoryContainsReturnedItems(stationToHaulTo.AllowedStoredItemIDs);

            if (itemsToHaul.Count == 0)
            {
                Debug.Log($"No items to haul to {stationToHaulTo.StationName}.");
                continue;
            }

            Debug.Log($"Creating deliver order for Actor: {ActorID} to Station: {stationToHaulTo.StationData.StationID}.");

            return new Order_Base(OrderType.Haul_Deliver, ActorID, StationID_Destination, stationToHaulTo.StationData.StationID, OrderStatus.Pending, itemsToHaul);
        }

        Debug.Log($"Cannot haul to existing stations in Jobsite: {JobsiteID}");   

        return null;
    }

    protected List<StationComponent> _getAlLStationsToHaulTo()
    {
        var stationsToHaulTo = new List<StationComponent>();

        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        foreach (var station in jobsite.AllStationsInJobsite.Values)
        {
            if (station.AllowedStoredItemIDs.Contains(1100) || station.AllowedStoredItemIDs.Contains(2300))
            {
                stationsToHaulTo.Add(station);
            }
        }

        return stationsToHaulTo;
    }

    protected IEnumerator _moveOperatorToOperatingArea(Vector3 position)
    {
        yield return Actor.StartCoroutine(Actor.BasicMove(position));

        if (Actor.transform.position != position) Actor.transform.position = position;
    }

    public void HaltCurrentOrder()
    {
        if (_orderCoroutine != null)
        {
            Actor.StopCoroutine(_orderCoroutine);
        }

        OrderStatus = OrderStatus.Pending;
        _orderCoroutine = null;
    }

    public void HaltCurrentMoveOrder()
    {
        if (_actorMoveCoroutine != null)
        {
            Actor.StopCoroutine(_actorMoveCoroutine);
        }

        OrderStatus = OrderStatus.Pending;
        _actorMoveCoroutine = null;
    }
}

public enum RequestStatus
{
    Pending,
    Accepted,
    Cancelled
}

public abstract class Order_Request
{
    public abstract OrderType OrderType { get; }
    public int StationID;
    public RequestStatus RequestStatus;

    public Order_Request(int stationID)
    {
        if (stationID == 0)
        {
            Debug.Log("Invalid StationID.");
            return;
        }

        StationID = stationID;
        RequestStatus = RequestStatus.Pending;
    }

    public void AcceptRequest()
    {
        RequestStatus = RequestStatus.Accepted;
    }
}

public class Order_Request_Haul_Fetch : Order_Request
{
    public override OrderType OrderType => OrderType.Haul_Fetch;
    public List<Item> Items = new();

    public Order_Request_Haul_Fetch(int stationID, List<Item> items) : base(stationID)
    {
        if (items.Count <= 0)
        {
            Debug.Log("No items in desiredItems.");
            return;
        }

        Items = items;
    }

    public void AddOrderItem(Item orderItem)
    {
        Items.Add(orderItem);
    }

    public void RemoveOrderItem(Item orderItem)
    {
        Items.Remove(orderItem);
    }

    public void ClearOrderItems()
    {
        Items.Clear();
    }

    public void ReplaceOrderItems(List<Item> orderItems)
    {
        Items = orderItems;
    }
}

public class Order_Request_Haul_Deliver : Order_Request
{
    public override OrderType OrderType => OrderType.Haul_Deliver;
    public List<Item> Items = new();

    public Order_Request_Haul_Deliver(int stationID, List<Item> items) : base(stationID)
    {
        if (items.Count <= 0)
        {
            Debug.Log("No items in desiredItems.");
            return;
        }

        Items = items;
    }

    public void AddOrderItem(Item orderItem)
    {
        Items.Add(orderItem);
    }

    public void RemoveOrderItem(Item orderItem)
    {
        Items.Remove(orderItem);
    }

    public void ClearOrderItems()
    {
        Items.Clear();
    }

    public void ReplaceOrderItems(List<Item> orderItems)
    {
        Items = orderItems;
    }
}

public class Order_Request_Hire : Order_Request
{
    public override OrderType OrderType => OrderType.Hire;
    public List<EmployeePositionName> DesiredEmployeePositions;

    public Order_Request_Hire(int stationID, List<EmployeePositionName> desiredEmployeePositions) : base(stationID)
    {
        if (desiredEmployeePositions.Count <= 0)
        {
            Debug.Log("No desired employee positions.");
            return;
        }

        DesiredEmployeePositions = desiredEmployeePositions;
    }

    public void AddDesiredEmployeePosition(EmployeePositionName desiredEmployeePositionName)
    {
        DesiredEmployeePositions.Add(desiredEmployeePositionName);
    }

    public void RemoveDesiredEmployeePosition(EmployeePositionName desiredEmployeePositionName)
    {
        DesiredEmployeePositions.Remove(desiredEmployeePositionName);
    }

    public void ClearDesiredEmployeePositions()
    {
        DesiredEmployeePositions.Clear();
    }

    public void ReplaceDesiredEmployeePositions(List<EmployeePositionName> desiredEmployeePositions)
    {
        DesiredEmployeePositions = desiredEmployeePositions;
    }
}