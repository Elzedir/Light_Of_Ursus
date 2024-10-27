using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Priority : MonoBehaviour
{
    
}

public class PriorityGenerator
{

    // Create a dictionary that contains the action name and a list of priority parameters. This will contiain the "existing priorities" that will be updated accordingly, so that we don't have to add and subract, instead just replace the existing priorities according to the conditions that are met or not met. That way we can simply add the actor distance to the slot and it will automatically update the priority by including it in its calculations.
    static Dictionary<ActionName, Dictionary<PriorityParameterName, object>> _existingParameters = new()
    {
        { ActionName.Haul, new Dictionary<PriorityParameterName, object>
        {
            { PriorityParameterName.PreviousPriority, null },
            { PriorityParameterName.MaxPriority, null },
            { PriorityParameterName.TotalItems, null },
            { PriorityParameterName.TotalDistance, null },
            { PriorityParameterName.InventoryHauler, null },
            { PriorityParameterName.InventoryTarget, null },
        }},
    };

    protected static float DefaultMaxPriority => 10;

    protected static float _addPriorityIfAboveTarget(float current, float target, float maxPriority) 
    => Math.Clamp(current - target, 0, maxPriority);
    protected static float _addPriorityIfBelowTarget(float current, float target, float maxPriority) 
    => Math.Clamp(target - current, 0, maxPriority);
    protected static float _addPriorityIfNotEqualTarget(float current, float target, float maxPriority) 
    => Math.Clamp(Math.Abs(current - target), 0, maxPriority);

    protected static float _addPriorityIfOutsideRange(float current, float min, float max, float maxPriority)
    => (current < min || current > max) ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;
    protected static float _addPriorityIfInsideRange(float current, float min, float max, float maxPriority) 
    => (current > min || current < max) ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;

    protected static float _addPriorityIfAbovePercent(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp((current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);
    protected static float _addPriorityIfBelowPercent(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);
    protected static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

    protected static float _addPriorityIfOutsidePercentRange(float current, float total, float min, float max, float maxPriority)
    {
        current = current / total * 100;
        return (current < min || current > max)
        ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max )), 0, maxPriority)
        : 0;
    }
    protected static float _addPriorityIfInsidePercentRange
    (float current, float total, float min, float max, float maxPriority)
    {
        current = current / total * 100;

        return (current >= min && current <= max) 
        ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
        : 0;
    }

    protected static float _moreItemsDesired_Target(List<Item> items, float target, float maxPriority)
    {
        Debug.Log($"MoreItemsDesired Item Count: {Item.GetItemListTotal_CountAllItems(items)}");
        return _addPriorityIfBelowTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
    }

    protected static float _lessItemsDesired_Target(List<Item> items, float target, float maxPriority)
    {
        return _addPriorityIfAboveTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
    }

    protected static float _exactItemsDesired_Target(List<Item> items, float target, float maxPriority)
    {
        return _addPriorityIfNotEqualTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
    }

    protected static float _moreItemsDesired_Total(List<Item> items, float total, float maxPriority)
    {
        Debug.Log($"MoreItemsDesired Item Count: {Item.GetItemListTotal_CountAllItems(items)}");
        return _addPriorityIfAbovePercent(Item.GetItemListTotal_CountAllItems(items), total, 0, maxPriority);
    }

    protected static float _lessItemsDesired_Total(List<Item> items, float total, float maxPriority)
    {
        return _addPriorityIfBelowPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
    }

    protected static float _exactItemsDesired_Total(List<Item> items, float total, float maxPriority)
    {
        return _addPriorityIfNotEqualPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
    }

    protected static float _moreDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
    {
        return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
    }

    protected static float _lessDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
    {
        return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
    }

    protected static float _exactDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
    {
        return _addPriorityIfNotEqualTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
    }

    protected static float _moreDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
    {
        return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
    }

    protected static float _lessDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
    {
        return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
    }

    protected static float _exactDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
    {
        return _addPriorityIfNotEqualPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
    }

    public static List<float> GeneratePriorities(ActionName actionName, List<PriorityParameter> parameters)
    {
        if (!_existingParameters.TryGetValue(actionName, out var existingPriority))
        {
            Debug.LogError($"ActionName: {actionName} not found in _existingParameters.");
            return null;
        }

        foreach (var parameter in parameters)
        {
            if (!existingPriority.ContainsKey(parameter.ParameterName))
            {
                Debug.LogError($"Parameter: {parameter.ParameterName} not found.");
                continue;
            }

            existingPriority[parameter.ParameterName] = parameter.ParameterValue;
        }

        switch(actionName)
        {
            case ActionName.Haul:
                return _generateHaulPriority(existingPriority);
            default:
                Debug.LogError($"ActionName: {actionName} not found.");
                return null;
        }
    }

    static List<float> _generateHaulPriority(Dictionary<PriorityParameterName, object> existingPriorities)
    {
        List<float> previousPriority = existingPriorities[PriorityParameterName.PreviousPriority] as List<float> ?? new List<float>();
        float maxPriority = existingPriorities[PriorityParameterName.MaxPriority] as float? ?? DefaultMaxPriority;
        float totalDistance = existingPriorities[PriorityParameterName.TotalDistance] as float? ?? 0;
        float totalItems = existingPriorities[PriorityParameterName.TotalItems] as float? ?? 0;
        InventoryData inventory_Hauler = existingPriorities[PriorityParameterName.InventoryHauler] as InventoryData;
        InventoryData inventory_Target = existingPriorities[PriorityParameterName.InventoryTarget] as InventoryData;

        if (maxPriority == 0) 
        {
            Debug.LogError("MaxPriority is 0. Default initialiser failed.");
            return null;
        }

        if (totalItems == 0 && totalDistance == 0)
        {
            Debug.LogError($"MaxItems and MaxDistance are 0.");
            return previousPriority;
        }

        if (inventory_Hauler == null || inventory_Target == null)
        {
            Debug.LogError($"Inventory_Hauler {inventory_Hauler} or Inventory_Target: {inventory_Target} is null.");
            return previousPriority;
        }

        var allItemsToFetch = inventory_Target.GetInventoryItemsToHaul();

        var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
        var targetPosition = inventory_Target.Reference.GameObject.transform.position;

        Debug.Log($"AllItemsCount: {allItemsToFetch.Count}, TotalItems: {totalItems}, maxPriority: {maxPriority}");

        var priority_ItemQuantity = allItemsToFetch.Count != 0 
        ? _moreItemsDesired_Total(allItemsToFetch, totalItems, maxPriority) : 0;

        Debug.Log($"haulerPosition: {haulerPosition}, targetPosition: {targetPosition}, totalDistance: {totalDistance}, maxPriority: {maxPriority}");

        var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
        ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority) : 0;

        Debug.Log($"Station: {inventory_Target.Reference.GameObject.name} with priorities: Item: {priority_ItemQuantity}, Distance: {priority_Distance} wants to haul:");

        foreach (var item in allItemsToFetch)
        {
            Debug.Log($"Item: {item.ItemID} Amount: {item.ItemAmount}");
        }

        return new List<float>
        {
            priority_ItemQuantity + priority_Distance
        };
    }
}


public enum PriorityImportance
{
    Low,
    Medium,
    High,
    Critical,
}

public enum PriorityParameterName
{
    None,

    PreviousPriority,
    MaxPriority,
    TotalItems,
    TotalDistance,
    InventoryHauler,
    InventoryTarget,
    Jobsite,
}

public class ActionToChange
{
    public ActionName ActionName;
    public PriorityImportance PriorityImportance;

    public ActionToChange(ActionName actionName, PriorityImportance priorityImportance)
    {
        ActionName = actionName;
        PriorityImportance = priorityImportance;
    }
}

public abstract class PriorityComponent
{
    public PriorityQueue PriorityQueue;

    public Dictionary<PriorityImportance, List<Priority>> CachedPriorityQueue;
    Dictionary<DataChanged, List<ActionToChange>> _actionsToChange = new()
    {
        { DataChanged.ChangedInventory, new List<ActionToChange> 
        { 
            new ActionToChange(ActionName.Haul, PriorityImportance.High),
        }},

        { DataChanged.DroppedItems, new List<ActionToChange> 
        { 
            new ActionToChange(ActionName.Haul, PriorityImportance.High),
            new ActionToChange(ActionName.Scavenge, PriorityImportance.Medium),
        }},
    };

    protected bool _syncingCachedQueue = false;
    protected float _timeDeferment = 1f;

    public void OnDataChanged(DataChanged dataChanged, List<PriorityParameter> changedParameters)
    {
        if (!_actionsToChange.TryGetValue(dataChanged, out var actionsToChange))
        {
            Debug.LogError($"DataChanged: {dataChanged} not found in _actionsToChange.");
            return;
        }

        foreach (var action in actionsToChange)
        {
            var priorities = PriorityGenerator.GeneratePriorities(action.ActionName, changedParameters);

            switch (action.PriorityImportance)
            {
                case PriorityImportance.Critical:
                    if (!PriorityQueue.Update((uint)action.ActionName, priorities))
                    {
                        Debug.LogError($"Action: {action} unable to be updated in PriorityQueue.");
                    };
                    break;
                case PriorityImportance.High:
                    AddToCachedPriorityQueue(new Priority((uint)action.ActionName, priorities), PriorityImportance.High);
                    break;
                case PriorityImportance.Medium:
                    AddToCachedPriorityQueue(new Priority((uint)action.ActionName, priorities), PriorityImportance.Medium);
                    break;
                case PriorityImportance.Low:
                    AddToCachedPriorityQueue(new Priority((uint)action.ActionName, priorities), PriorityImportance.Low);
                    break;
                default:
                    Debug.LogError($"PriorityImportance: {action.PriorityImportance} not found.");
                    break;
            }
        }
    }

    public void FullPriorityUpdate(List<object> allData)
    {
        SyncCachedPriorityQueueHigh();
        //SyncCachedPriorityQueueMedium();
        //SyncCachedPriorityQueueLow();

        _updateAllPriorities(allData);
    }

    protected abstract void _updateAllPriorities(List<object> allData);

    public void SyncCachedPriorityQueueHigh(bool syncing = false)
    {
        foreach (var priority in CachedPriorityQueue[PriorityImportance.Low])
        {
            if (!PriorityQueue.Update(priority.PriorityID, priority.AllPriorities))
            {
                if (!PriorityQueue.Enqueue(priority.PriorityID, priority.AllPriorities))
                {
                    Debug.LogError($"PriorityID: {priority.PriorityID} unable to be added to PriorityQueue.");
                }
            }
        }

        CachedPriorityQueue[PriorityImportance.Low].Clear();
        if (syncing) _syncingCachedQueue = false;
    }

    void _syncCachedPriorityQueueHigh_DeferredUpdate()
    {
        _syncingCachedQueue = true;
        Manager_DeferredActions.AddDeferredAction(() => SyncCachedPriorityQueueHigh(true), _timeDeferment);
    }

    public void AddToCachedPriorityQueue(Priority priority, PriorityImportance priorityImportance)
    {
        if (CachedPriorityQueue == null) CachedPriorityQueue = new Dictionary<PriorityImportance, List<Priority>>();

        if (!CachedPriorityQueue.ContainsKey(priorityImportance)) CachedPriorityQueue.Add(priorityImportance, new List<Priority>());

        CachedPriorityQueue[priorityImportance].Add(priority);

        if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
    }

    public void AddAction(ActionName actionName, List<float> priorities)
    {
        if (!PriorityQueue.Enqueue((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be added to PriorityQueue.");
        }
    }

    public void UpdateAction(ActionName actionName, List<float> priorities)
    {
        if (!PriorityQueue.Update((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
        }
    }

    public void RemoveAction(ActionName actionName)
    {
        if (!PriorityQueue.Remove((uint)actionName))
        {
            Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
        }
    }

    public Priority CheckNextPriority()
    {
        return PriorityQueue.Peek();
    }

    public Priority CheckSpecificPriority(uint priorityID)
    {
        return PriorityQueue.Peek(priorityID);
    }

    public Priority GetNextPriority()
    {
        return PriorityQueue.Dequeue();
    }

    public Priority GetSpecificPriority(uint priorityID)
    {
        return PriorityQueue.Dequeue(priorityID);
    }    
}

public class PriorityComponent_Actor : PriorityComponent
{
    readonly ComponentReference_Actor _actorReferences;

    public uint ActorID { get { return _actorReferences.ActorID; } }
    protected ActorComponent Actor { get { return _actorReferences.Actor; } }

    public PriorityComponent_Actor(uint actorID)
    {
        _actorReferences = new ComponentReference_Actor(actorID);
        PriorityQueue = new PriorityQueue(100);
    }

    protected override void _updateAllPriorities(List<object> allData)
    {
        throw new NotImplementedException();
    }
}

public class PriorityComponent_Jobsite : PriorityComponent
{
    public PriorityComponent_Jobsite(uint jobsiteID) 
    {
        _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
        PriorityQueue = new PriorityQueue(100);
    } 

    readonly ComponentReference_Jobsite _jobsiteReferences;

    public uint JobsiteID { get { return _jobsiteReferences.JobsiteID; } }
    protected JobsiteComponent Jobsite { get { return _jobsiteReferences.Jobsite; } }

    protected override void _updateAllPriorities(List<object> allData)
    {
        List<StationComponent> allStations = allData.Cast<StationComponent>().ToList();

        foreach (var station in allStations)
        {
            var itemsToHaul = station.GetInventoryItemsToHaul();

            if (itemsToHaul.Count <= 0) continue;

            var priorityValues = PriorityGenerator.GeneratePriorities(ActionName.Haul, new List<PriorityParameter>
            {
                // new PriorityParameter( PriorityParameterName., itemsToHaul),
                // new PriorityParameter( PriorityParameterName.TargetPosition, station.transform.position),
            });

            PriorityQueue.Enqueue(station.StationID, priorityValues);
        }
    }

    public (StationComponent Station, List<Item> Items) GetStationToHaulFrom(ActorComponent hauler)
    {
        float totalItems = Jobsite.AllStationsInJobsite.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToHaul()));
        float totalDistance = Jobsite.AllStationsInJobsite.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));

        Debug.Log($"TotalItems: {totalItems}, TotalDistance: {totalDistance}");

        foreach (var station in Jobsite.AllStationsInJobsite)
        {
            List<float> newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Haul, new List<PriorityParameter>
            {
                new PriorityParameter( PriorityParameterName.TotalItems, totalItems),
                new PriorityParameter (PriorityParameterName.TotalDistance, totalDistance),
                new PriorityParameter( PriorityParameterName.InventoryHauler, hauler.ActorData.InventoryData),
                new PriorityParameter( PriorityParameterName.InventoryTarget, station.StationData.InventoryData),
            });

            if (newPriorities == null || newPriorities.Count == 0) continue;

            PriorityQueue.Update(station.StationID, newPriorities);
        }

        var allStations = PriorityQueue.PeekAll();

        // Correct priorities, however wrong order in PriorityQueue

        foreach(var station in allStations)
        {
            if (station == null) continue;
            Debug.Log($"Station: {station.PriorityID} AllPrioties: {station.AllPriorities[0]}");
        }

        StationComponent peekedStation = Manager_Station.GetStation(PriorityQueue.Peek().PriorityID);
        
        if (peekedStation == null) return (null, null);

        Debug.Log("PriorityQueue has a peek.");

        var allItemsInStation = peekedStation.GetInventoryItemsToHaul();

        Debug.Log($"PeekedStation: {peekedStation.StationID}");

        Debug.Log($"AllItemsInStation: {allItemsInStation.Count}");

        if (allItemsInStation.Count == 0) return (null, null);

        float availableCarryWeight = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;

        List<Item> itemsToHaul = new List<Item>();

        while (allItemsInStation.Count > 0 && availableCarryWeight > 0)
        {
            Debug.Log($"AllItemsInStation: {allItemsInStation.Count}, AvailableCarryWeight: {availableCarryWeight}");

            Item item = allItemsInStation[0];
            Item_Master itemMaster = Manager_Item.GetMasterItem(item.ItemID);
            float itemWeight = itemMaster.CommonStats_Item.ItemWeight * item.ItemAmount;

            if (itemWeight > availableCarryWeight) break;

            itemsToHaul.Add(item);
            allItemsInStation.Remove(item);
            availableCarryWeight -= itemWeight;
        }

        if (itemsToHaul.Count == 0) return (null, null);

        PriorityQueue.Dequeue(peekedStation.StationID);

        return (peekedStation, itemsToHaul);
    }
}

public abstract class PriorityData
{
    public ComponentReference Reference { get; private set; }

    public PriorityData (uint componentID, ComponentType componentType)
    {
        switch(componentType)
        {
            case ComponentType.Actor:
                Reference = new ComponentReference_Actor(componentID);
                break;
            case ComponentType.Station:
                Reference = new ComponentReference_Station(componentID);
                break;
            default:
                Debug.LogError($"ComponentType: {componentType} not found.");
                break;
        }
    }
    
    protected PriorityComponent _priorityComponent;
    public abstract PriorityComponent PriorityComponent { get; }
    
    public Action<DataChanged, List<PriorityParameter>> OnDataChange { get; set; }
    
    protected void _priorityChangeCheck(DataChanged dataChanged, bool forceChange = false)
    {
        if (!_priorityChangeNeeded(dataChanged) && !forceChange) return;

        if (OnDataChange == null) _setOnDataChange();

        if (OnDataChange == null) 
        {
            Debug.LogError("OnDataChange is still null after resetting data change notifications.");
            return;
        }

        OnDataChange(dataChanged, _getActionsToChange(dataChanged));
    }
    
    protected abstract bool _priorityChangeNeeded(object dataChanged);
    protected void _setOnDataChange() => OnDataChange = (DataChanged, changedParameters)
    => PriorityComponent.OnDataChanged(DataChanged, changedParameters);
    protected List<PriorityParameter> _getActionsToChange(DataChanged dataChanged)
    {
        if (_priorityParameterList.Count == 0)
        {
            Debug.LogError("ActionsAndParameters is empty.");
            return null;
        }

        if (!_priorityParameterList.TryGetValue(dataChanged, out var priorityParameters))
        {
            Debug.LogError($"DataChanged: {dataChanged} is not in ActionsAndParameters list");
            return null;
        }

        return priorityParameters;
    }

    protected abstract Dictionary<DataChanged, List<PriorityParameter>> _priorityParameterList { get; set; }
}

public abstract class ComponentReference
{
    public uint ComponentID { get; private set; }
    public ComponentReference(uint componentID) => ComponentID = componentID;

    protected abstract object _component { get; }
    public abstract GameObject GameObject { get; }
}

public class ComponentReference_Actor : ComponentReference
{
    public uint ActorID => ComponentID;
    public ComponentReference_Actor(uint actorID) : base(actorID) { }

    ActorComponent _actor;
    protected override object _component { get => _actor ??= Manager_Actor.GetActor(ComponentID); } 
    public ActorComponent Actor => _component as ActorComponent;

    public override GameObject GameObject => Actor.gameObject;
}

public class ComponentReference_Station : ComponentReference
{
    public uint StationID => ComponentID;
    public ComponentReference_Station(uint stationID) : base(stationID) { }

    StationComponent _station;
    protected override object _component { get => _station ??= Manager_Station.GetStation(StationID); }
    public StationComponent Station => _component as StationComponent;

    public override GameObject GameObject => Station.gameObject;
}

public class ComponentReference_Jobsite : ComponentReference
{
    public uint JobsiteID => ComponentID;
    public ComponentReference_Jobsite(uint jobsiteID) : base(jobsiteID) { }

    JobsiteComponent _jobsite;
    protected override object _component { get => _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID); }
    public JobsiteComponent Jobsite => _component as JobsiteComponent;

    public override GameObject GameObject => Jobsite.gameObject;
}

public class PriorityParameter
{
    public PriorityParameterName ParameterName;
    public object ParameterValue;

    public PriorityParameter(PriorityParameterName parameterName, object parameterValue)
    {
        ParameterName = parameterName;
        ParameterValue = parameterValue;
    }
}



public class Priority
{
    public uint PriorityID;

    public List<float> AllPriorities;

    public Priority(uint priorityID, List<float> priorities)
    {
        PriorityID = priorityID;
        AllPriorities = new List<float>(priorities);
    }

    public int CompareTo(Priority that)
    {
        for (int i = 0; i < Math.Min(AllPriorities.Count, that.AllPriorities.Count); i++)
        {
            if (AllPriorities[i] < that.AllPriorities[i]) return -1;
            else if (AllPriorities[i] > that.AllPriorities[i]) return 1;
        }

        if (AllPriorities.Count > that.AllPriorities.Count) return 1;
        else if (AllPriorities.Count < that.AllPriorities.Count) return -1;

        return 0;
    }
}

public class PriorityQueue
{
    int _currentPosition;
    Priority[] _priorityArray;
    Dictionary<uint, int> _priorityQueue;

    public PriorityQueue(int maxPriorities)
    {
        _currentPosition = 0;
        _priorityArray = new Priority[maxPriorities];
        _priorityQueue = new Dictionary<uint, int>();
    }

    public Priority Peek(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            return _priorityArray[1];
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;

            return index == 0 ? null : _priorityArray[index];
        }
    }

    public Priority[] PeekAll()
    {
        if (_currentPosition == 0) return null;

        return _priorityArray.Skip(1).ToArray();
    }

    public Priority Dequeue(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            Priority priority = _priorityArray[1];
            _priorityArray[1] = _priorityArray[_currentPosition];
            _priorityQueue[_priorityArray[1].PriorityID] = 1;
            _priorityQueue[priority.PriorityID] = 0;
            _currentPosition--;
            _moveDown(1);

            return priority;
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;
            
            if (index == 0) return null;

            Priority priority = _priorityArray[index];
            _priorityQueue[priorityID] = 0;
            _priorityArray[index] = _priorityArray[_currentPosition];
            _priorityQueue[_priorityArray[_currentPosition].PriorityID] = index;
            _currentPosition--;
            _moveDown(index);

            return priority;
        }
    }

    public bool Enqueue(uint priorityID, List<float> priorities)
    {
        if (_priorityQueue.TryGetValue(priorityID, out int index) && index != 0)
        {
            Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

            if (!Update(priorityID, priorities))
            {
                Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                return false;
            }

            return true;
        }

        Priority priority = new Priority(priorityID, priorities);
        _currentPosition++;
        _priorityQueue[priorityID] = _currentPosition;
        if (_currentPosition == _priorityArray.Length) Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
        _priorityArray[_currentPosition] = priority;
        _moveUp(_currentPosition);

        return true;
    }

    public bool Update(uint priorityID, List<float> newPriorities)
    {
        if (newPriorities.Count == 0)
        {
            if (!Remove(priorityID))
            {
                Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
                return false;
            }
            
            return true;
        }

        if (!_priorityQueue.TryGetValue(priorityID, out int index) || index == 0)
        {
            if (!Enqueue(priorityID, newPriorities))
            {
                Debug.LogError($"PriorityID: {priorityID} unable to be enqueued.");
                return false;
            }

            return true;
        }
        
        Priority priority_New = new Priority(priorityID, newPriorities);
        Priority priority_Old = _priorityArray[index];

        _priorityArray[index] = priority_New;

        if (priority_Old.CompareTo(priority_New) < 0)
        {
            _moveDown(index);
        }
        else
        {
            _moveUp(index);
        }

        return true;
    }

    public bool Remove(uint priorityID)
    {
        if (!_priorityQueue.TryGetValue(priorityID, out int index) || index == 0)
        {
            Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
            return false;
        }

        _priorityQueue[priorityID] = 0;
        _priorityArray[index] = _priorityArray[_currentPosition];
        _priorityQueue[_priorityArray[index].PriorityID] = index;
        _currentPosition--;
        _moveDown(index);

        return true;
    }

    void _moveDown(int index)
    {
        int childL = index * 2;

        if (childL > _currentPosition) return;

        int childR = index * 2 + 1;
        int largerChild;

        if (childR > _currentPosition)
        {
            largerChild = childL;
        }
        else if (_priorityArray[childL].CompareTo(_priorityArray[childR]) > 0)
        {
            largerChild = childL;
        }
        else
        {
            largerChild = childR;
        }

        if (_priorityArray[index].CompareTo(_priorityArray[largerChild]) >= 0) return;

        _swap(index, largerChild);
        _moveDown(largerChild);

    }

    void _moveUp(int index)
    {
        if (index == 1) return;
        int parent = index / 2;

        if (_priorityArray[parent].CompareTo(_priorityArray[index]) < 0)
        {
            _swap(parent, index);
            _moveUp(parent);
        }
    }

    void _swap(int indexA, int indexB)
    {
        Priority tempPriorityA = _priorityArray[indexA];
        _priorityArray[indexA] = _priorityArray[indexB];
        _priorityQueue[_priorityArray[indexB].PriorityID] = indexA;
        _priorityArray[indexB] = tempPriorityA;
        _priorityQueue[tempPriorityA.PriorityID] = indexB;
    }
}