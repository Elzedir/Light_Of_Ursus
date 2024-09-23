using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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

    a

    // Create a priority queue system so replacement orders can be created and executed in the correct order of priority. And that deliver orders can follow fetch orders.

    public void ExecuteNextOrder(OrderType orderType = OrderType.None)
    {

        if (AllCurrentOrders.Count <= 0) return;

        // Implement priority system later.

        Order_Base highestPriorityOrder = AllCurrentOrders.OrderByDescending(o => o.OrderID).FirstOrDefault();

        if (highestPriorityOrder == null)
        {
            Debug.Log("No highest priority order found.");
            return;
        }

        highestPriorityOrder.ExecuteOrder();

        if (orderType == OrderType.None)
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
            if (AllCurrentOrders.Any(o => o.OrderType == orderType))
            {
                AllCurrentOrders.FirstOrDefault(o => o.OrderType == orderType).ExecuteOrder();
            }
            else
            {
                Debug.Log($"No {orderType} orders for Actor: {ActorID}");
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

        AllCurrentOrders.RemoveAll(o => o.OrderID == order.OrderID);
        AllCurrentOrders.Add(order);
    }

    public void CompleteOrder(int orderID, Order_Base replacementOrder = null)
    {
        if (!AllCurrentOrders.Any(o => o.OrderID == orderID))
        {
            Debug.Log($"Order: {orderID} does not exist for Actor: {ActorID}");
            return;
        }

        AllCompletedOrders.Add(AllCurrentOrders.FirstOrDefault(o => o.OrderID == orderID));
        AllCurrentOrders.RemoveAll(o => o.OrderID == orderID);

        if (replacementOrder != null)
        {
            AddCurrentOrder(replacementOrder);
        }
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
}

public enum OrderStatus
{
    Pending,
    Active,
    Complete
}

[Serializable]
public abstract class Order_Base
{
    public int OrderID;
    public abstract OrderType OrderType { get; }
    public int ActorID;
    ActorComponent _actor;
    public ActorComponent Actor { get { return _actor ??= Manager_Actor.GetActor(ActorID); } }
    public int OperatingAreaID;
    OperatingAreaComponent _operatingArea;
    public OperatingAreaComponent OperatingArea { get { return _operatingArea ??= Manager_Station.GetStation(StationID_Destination).GetOperatingArea(OperatingAreaID); } }
    public int StationID_Source;
    StationComponent _station_Source;
    public StationComponent Station_Source { get { return _station_Source ??= Manager_Station.GetStation(StationID_Source); } }
    public int StationID_Destination;
    StationComponent _station_Destination;
    public StationComponent Station_Destination { get { return _station_Destination ??= Manager_Station.GetStation(StationID_Source); } }
    public int JobsiteID;
    JobsiteComponent _jobsite;
    public JobsiteComponent Jobsite { get { return _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID); } }
    public OrderStatus OrderStatus;
    public List<Item> OrderItems;

    protected Coroutine _orderCoroutine;
    protected Coroutine _actorMoveCoroutine;

    public Order_Base(int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems)
    {
        ActorID = actorID;
        OrderID = Actor.ActorData.OrderData.GetOrderID();
        StationID_Source = stationID_Source;
        JobsiteID = Station_Source.StationData.JobsiteID;
        StationID_Destination = stationID_Destination;
        OrderStatus = orderStatus;
        OrderItems = orderItems;

        Actor.ActorData.OrderData.AddCurrentOrder(this);
    }

    public void ChangeActor(int actorID)
    {
        Actor.ActorData.OrderData.RemoveCurrentOrder(OrderID);
        ActorID = actorID;
        _actor = null;
        Actor.ActorData.OrderData.AddCurrentOrder(this);
    }
    
    public void ChangeStationSource(int stationID)
    {
        StationID_Source = stationID;
        _station_Source = null;
    }

    public void ChangeStationDestination(int stationID)
    {
        StationID_Destination = stationID;
        _station_Destination = null;
    }

    public abstract void ExecuteOrder();
    public abstract IEnumerator _executeOrder();

    protected IEnumerator _moveOperatorToOperatingArea(Vector3 position)
    {
        OrderStatus = OrderStatus.Active;

        yield return Actor.StartCoroutine(Actor.BasicMove(position));

        if (Actor.transform.position != position) Actor.transform.position = position;
    }

    public void HaltCurrentOrder()
    {
        if (_orderCoroutine != null)
        {
            Actor.StopCoroutine(_orderCoroutine);
            OrderStatus = OrderStatus.Pending;
            _orderCoroutine = null;
        }
    }

    public void HaltCurrentMoveOrder()
    {
        if (_actorMoveCoroutine != null)
        {
            Actor.StopCoroutine(_actorMoveCoroutine);
            OrderStatus = OrderStatus.Pending;
            _actorMoveCoroutine = null;
        }
    }
}

[Serializable]
public abstract class Order_Haul : Order_Base
{
    public Order_Haul(int actorID, int stationID_source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(actorID, stationID_source, stationID_Destination, orderStatus, orderItems) { }

    public override void ExecuteOrder()
    {
        if (OrderStatus == OrderStatus.Pending)
        {
            HaltCurrentOrder();

            _orderCoroutine = Actor.StartCoroutine(_executeOrder());
        }
    }

    public override IEnumerator _executeOrder()
    {
        if (ActorID == 0 || StationID_Source == 0 || OrderItems.Count <= 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Source}, or OrderItemIDs {OrderItems.Count} is invalid.");
            throw new Exception("Invalid Order.");
        }

        if (Actor.transform.position == null)
        {
            Debug.Log("Actor position is null.");
            throw new Exception("Invalid Actor Position.");
        }

        if (Vector3.Distance(Actor.transform.position, Station_Destination.transform.position) > (Station_Destination.BoxCollider.bounds.extents.magnitude + Actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            HaltCurrentMoveOrder();

            yield return _actorMoveCoroutine = Actor.StartCoroutine(_moveOperatorToOperatingArea(Station_Destination.transform.position));

            _transferItems();
        }

        OrderStatus = OrderStatus.Complete;

        if (OrderType == OrderType.Haul_Deliver)
        {
            Actor.ActorData.OrderData.CompleteOrder(OrderID, _createReturnDeliverOrder());
        }
        else
        {
            Actor.ActorData.OrderData.CompleteOrder(OrderID);
        }
    }

    protected abstract bool _transferItems();
    protected Order_Haul_Deliver _createReturnDeliverOrder()
    {
        return new Order_Haul_Deliver(ActorID, StationID_Destination, StationID_Source, OrderStatus.Pending, OrderItems);
    }
}

[Serializable]
public class Order_Haul_Fetch : Order_Haul
{
    public override OrderType OrderType => OrderType.Haul_Fetch;
    public Order_Haul_Fetch(int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(actorID, stationID_Source, stationID_Destination, orderStatus, orderItems) { }

    protected override bool _transferItems()
    {
        if (!Station_Destination.StationData.InventoryData.RemoveFromInventory(OrderItems))
        {
            Debug.Log("Failed to remove items from Station inventory.");
            return false;
        }

        if (!Actor.ActorData.InventoryAndEquipment.InventoryData.AddToInventory(OrderItems))
        {
            Debug.Log("Failed to add items to Actor inventory.");

            if (Station_Destination.StationData.InventoryData.AddToInventory(OrderItems))
            {
                Debug.Log("Failed to add items back to Station inventory.");
                return false;
            }

            return false;
        }

        return true;
    }
}

[Serializable]
public class Order_Haul_Deliver : Order_Haul
{
    public override OrderType OrderType => OrderType.Haul_Deliver;
    public Order_Haul_Deliver(int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(actorID, stationID_Source, stationID_Destination, orderStatus, orderItems) { }

    protected override bool _transferItems()
    {
        if (!Actor.ActorData.InventoryAndEquipment.InventoryData.RemoveFromInventory(OrderItems))
        {
            Debug.Log("Failed to remove items from Actor inventory.");
            return false;
        }

        if (!Station_Destination.StationData.InventoryData.AddToInventory(OrderItems))
        {
            Debug.Log("Failed to add items to Station inventory.");

            if (Actor.ActorData.InventoryAndEquipment.InventoryData.AddToInventory(OrderItems))
            {
                Debug.Log("Failed to add items back to Actor inventory.");
                return false;
            }

            return false;
        }

        return true;
    }
}

[Serializable]
public class Order_Craft : Order_Base
{
    public override OrderType OrderType => OrderType.Craft;
    public Recipe Recipe;

    public Order_Craft(int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems, Recipe recipe) : base(actorID, stationID_Source, stationID_Destination, orderStatus, orderItems)
    {
        Recipe = recipe;
    }

    public override void ExecuteOrder()
    {
        if (OrderStatus == OrderStatus.Pending)
        {
            HaltCurrentMoveOrder();
            HaltCurrentOrder();

            _orderCoroutine = Actor.StartCoroutine(_executeOrder());
        }
    }

    public override IEnumerator _executeOrder()
    {
        if (ActorID == 0 || StationID_Source == 0 || OrderItems.Count <= 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Source}, or OrderItemIDs {OrderItems.Count} is invalid.");
            throw new Exception("Invalid Order.");
        }

        if (Actor.transform.position != null && Vector3.Distance(Actor.transform.position, Station_Destination.transform.position) > (Station_Destination.BoxCollider.bounds.extents.magnitude + Actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            HaltCurrentMoveOrder();

            yield return _actorMoveCoroutine = Actor.StartCoroutine(_moveOperatorToOperatingArea(Station_Destination.transform.position));
        }

        OrderStatus = OrderStatus.Complete;
        Actor.ActorData.OrderData.CompleteOrder(OrderID);
    }
}