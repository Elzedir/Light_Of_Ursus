using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Order : MonoBehaviour, IDataPersistence
{
    //public static AllOrders_SO AllOrders;
    public static Dictionary<int, Order_Base> AllOrders;

    public void SaveData(SaveData data)
    {
        //AllOrderData.Values.ToList().ForEach(orderData => orderData.SaveData());
        //data.SavedCityData = new SavedCityData(AllOrderData.Values.ToList());
    }
    public void LoadData(SaveData data)
    {
        //AllOrderData = data.SavedOrderData?.AllOrderData.ToDictionary(x => x.OrderID);
        //AllOrderData?.Values.ToList().ForEach(orderData => orderData.LoadData());
    }

    public void OnSceneLoaded()
    {
        //AllOrders = Resources.Load<AllOrders_SO>("ScriptableObjects/AllOrders_SO");

        //Manager_Initialisation.OnInitialiseManagerOrder += _initialise;
    }

    void _initialise()
    {
        _initialiseAllOrderData();
    }

    void _initialiseAllOrderData()
    {
        if (AllOrders == null) AllOrders = new();

        // foreach (var orderData in AllOrderData.Values)
        // {
        //     orderData.InitialiseOrderData();
        // }

        // AllOrders.AllOrderData = AllOrderData.Values.ToList();
    }

    public static void AddOrder(Order_Base order)
    {
        if (AllOrders.ContainsKey(order.OrderID))
        {
            Debug.Log($"AllOrders already contains OrderID: {order.OrderID}");
            return;
        }

        AllOrders.Add(order.OrderID, order);
    }

    public static void UpdateOrder(Order_Base order)
    {
        if (!AllOrders.ContainsKey(order.OrderID))
        {
            Debug.LogError($"CityData: {order.OrderID} does not exist in AllCityData.");
            return;
        }

        AllOrders[order.OrderID] = order;
    }

    public static Order_Base GetOrder(int orderID)
    {
        if (!AllOrders.ContainsKey(orderID))
        {
            Debug.Log($"OrderID: {orderID} does not exist in AllOrders.");
            return null;
        }

        return AllOrders[orderID];
    }

    public static void RemoveOrder(Order_Base order)
    {
        if (!AllOrders.ContainsKey(order.OrderID))
        {
            Debug.LogError($"OrderID: {order.OrderID} does not exist in AllOrders.");
            return;
        }

        AllOrders.Remove(order.OrderID);
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
    Actor_Base _actor;
    public Actor_Base Actor { get { return _actor ??= Manager_Actor.GetActor(ActorID); } }
    public int OperatingAreaID;
    OperatingAreaComponent _operatingArea;
    public OperatingAreaComponent OperatingArea { get { return _operatingArea ??= Manager_Station.GetStationData(StationID_Destination).GetOperatingArea(OperatingAreaID); } }
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

    protected Coroutine _actorMoveCoroutine;

    public Order_Base(int orderID, int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems)
    {
        OrderID = orderID;
        ActorID = actorID;
        StationID_Source = stationID_Source;
        JobsiteID = Station_Source.StationData.JobsiteID;
        StationID_Destination = stationID_Destination;
        OrderStatus = orderStatus;
        OrderItems = orderItems;

        Manager_Order.AddOrder(this);
        Actor.ActorData.OrderData.AddOrder(OrderID);
    }

    public void ChangeActor(int actorID)
    {
        Actor.ActorData.OrderData.RemoveOrder(OrderID);
        ActorID = actorID;
        _actor = null;
        Actor.ActorData.OrderData.AddOrder(OrderID);
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

    public abstract IEnumerator ExecuteOrder();

    protected IEnumerator _moveOperatorToOperatingArea(Vector3 position)
    {
        OrderStatus = OrderStatus.Active;

        Manager_Order.UpdateOrder(this);

        yield return Actor.StartCoroutine(Actor.BasicMove(position));

        if (Actor.transform.position != position) Actor.transform.position = position;
    }

    public void HaltOrder()
    {
        if (_actorMoveCoroutine != null)
        {
            Actor.StopCoroutine(_actorMoveCoroutine);
            OrderStatus = OrderStatus.Pending;
            Manager_Order.UpdateOrder(this);
        }
    }
}

[Serializable]
public abstract class Order_Haul : Order_Base
{
    public Order_Haul(int orderID, int actorID, int stationID_source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(orderID, actorID, stationID_source, stationID_Destination, orderStatus, orderItems) { }

    public override IEnumerator ExecuteOrder()
    {
        if (ActorID == 0 || StationID_Source == 0 || OrderItems.Count <= 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Source}, or OrderItemIDs {OrderItems.Count} is invalid.");
            yield break;
        }

        if (Actor.transform.position != null && Vector3.Distance(Actor.transform.position, Station_Destination.transform.position) > (Station_Destination.BoxCollider.bounds.extents.magnitude + Actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            HaltOrder();

            yield return _actorMoveCoroutine = Actor.StartCoroutine(_moveOperatorToOperatingArea(Station_Destination.transform.position));

            _transferItems();

            if (OrderType == OrderType.Haul_Fetch) _createDeliverOrder();
        }

        OrderStatus = OrderStatus.Complete;

        Manager_Order.RemoveOrder(this);
    }

    protected abstract bool _transferItems();
    protected void _createDeliverOrder()
    {
        Order_Haul_Deliver order_Deliver = new Order_Haul_Deliver(Manager_Order.AllOrders.Count + 1, ActorID, StationID_Destination, StationID_Source, OrderStatus.Pending, OrderItems);
    }
}

[Serializable]
public class Order_Haul_Fetch : Order_Haul
{
    public override OrderType OrderType => OrderType.Haul_Fetch;
    public Order_Haul_Fetch(int orderID, int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(orderID, actorID, stationID_Source, stationID_Destination, orderStatus, orderItems) { }

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
    public Order_Haul_Deliver(int orderID, int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems) : base(orderID, actorID, stationID_Source, stationID_Destination, orderStatus, orderItems) { }

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

    public Order_Craft(int orderID, int actorID, int stationID_Source, int stationID_Destination, OrderStatus orderStatus, List<Item> orderItems, Recipe recipe) : base(orderID, actorID, stationID_Source, stationID_Destination, orderStatus, orderItems)
    {
        Recipe = recipe;
    }

    public override IEnumerator ExecuteOrder()
    {
        if (ActorID == 0 || StationID_Source == 0 || OrderItems.Count <= 0)
        {
            Debug.Log($"HaulerID: {ActorID}, StationID: {StationID_Source}, or OrderItemIDs {OrderItems.Count} is invalid.");
            yield break;
        }

        if (Actor.transform.position != null && Vector3.Distance(Actor.transform.position, Station_Destination.transform.position) > (Station_Destination.BoxCollider.bounds.extents.magnitude + Actor.Collider.bounds.extents.magnitude * 1.1f))
        {
            HaltOrder();

            yield return _actorMoveCoroutine = Actor.StartCoroutine(_moveOperatorToOperatingArea(Station_Destination.transform.position));
        }

        OrderStatus = OrderStatus.Complete;

        Manager_Order.RemoveOrder(this);
    }
}