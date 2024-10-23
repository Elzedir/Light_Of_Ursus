using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Priority : MonoBehaviour
{
    
}

public abstract class PriorityGenerator
{
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
    => Math.Clamp((current / total - targetPercentage / 100) * 100, 0, maxPriority);
    protected static float _addPriorityIfBelowPercent(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp((targetPercentage / 100 - current / total) * 100, 0, maxPriority);
    protected static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * 100, 0, maxPriority);

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

    protected static float _moreItemsDesired(List<Item> items, float target, float maxPriority)
    {
        return _addPriorityIfAboveTarget(Item.GetItemListCount_AllItems(items), target, maxPriority);
    }

    protected static float _lessItemsDesired(List<Item> items, float target, float maxPriority)
    {
        return _addPriorityIfBelowTarget(Item.GetItemListCount_AllItems(items), target, maxPriority);
    }

    protected static float _moreDistanceDesired(Vector3 currentPosition, Vector3 targetPosition, float targetDistance, float maxPriority)
    {
        return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), targetDistance, maxPriority);
    }

    protected static float _lessDistanceDesired(Vector3 currentPosition, Vector3 targetPosition, float targetDistance, float maxPriority)
    {
        return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), targetDistance, maxPriority);
    }

    public static List<float> GeneratePriorities(ActionName actionName, List<PriorityParameter> parameters)
    {
        // But now the problem is that I must not override the priorities that are already set, so I need to find a way to add to the existing priorities when adding from another source. Instead when we choose an actor priority, we simply add the priority for the station to the
        // priority of the action in his own priority queue.

        switch(actionName)
        {
            case ActionName.Haul:
                return _haul(parameters);
            default:
                Debug.LogError($"ActionName: {actionName} not found.");
                return null;
        }
    }

    static List<float> _haul(List<PriorityParameter> parameters)
    {
        float maxPriority = DefaultMaxPriority;
        List<float> existingPriority = null;
        InventoryData inventory_hauler = null;
        InventoryData inventory_target = null;

        foreach (var parameter in parameters)
        {
            switch (parameter.ParameterName)
            {
                case PriorityParameterName.MaxPriority:
                    maxPriority = (float)parameter.ParameterValue;
                    break;
                case PriorityParameterName.ExistingPriority:
                    existingPriority = (List<float>)parameter.ParameterValue;
                    break;
                case PriorityParameterName.InventoryHauler:
                    inventory_hauler = (InventoryData)parameter.ParameterValue;
                    break;
                case PriorityParameterName.InventoryTarget:
                    inventory_target = (InventoryData)parameter.ParameterValue;
                    break;
                default:
                    Debug.LogError($"Parameter: {parameter.ParameterName} not found.");
                    break;
            }
        }
        
        var allItemsToFetch = station.GetInventoryItemsToHaul();
        var priority_ItemQuantity = allItemsToFetch.Count != 0 ? _moreItemsDesired(allItemsToFetch, 0, maxPriority) : 0;

        var haulerPosition = actor.transform.position;
        var targetPosition = inventory_target..position;
        var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
        ? _lessDistanceDesired(haulerPosition, targetPosition, 5, maxPriority)
        : 0;

        var newPriorities = new List<float>
        {
            priority_ItemQuantity + priority_Distance
        };

        if (existingPriority.Count > 0) newPriorities.AddRange(existingPriority);

        return newPriorities;
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


    ExistingPriority,
    MaxPriority,
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
    readonly ActorReferences _actorReferences;

    public uint ActorID { get { return _actorReferences.ActorID; } }
    protected ActorComponent Actor { get { return _actorReferences.Actor; } }

    public PriorityComponent_Actor(uint actorID)
    {
        _actorReferences = new ActorReferences(actorID);
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
        _jobsiteReferences = new JobsiteReferences(jobsiteID);
        PriorityQueue = new PriorityQueue(100);
    } 

    readonly JobsiteReferences _jobsiteReferences;

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
        var allStations = PriorityQueue.PeekAll();

        if (allStations == null || allStations.Length <= 0) return (null, null);

        foreach (var station in allStations)
        {
            var newPriority = PriorityGenerator.GeneratePriorities(ActionName.Haul, new List<PriorityParameter>
            {
                // new PriorityParameter( PriorityParameterName.ExistingPriority, station.AllPriorities),
                // new PriorityParameter( PriorityParameterName.Actor, hauler),
                // new PriorityParameter( PriorityParameterName.Station, Manager_Station.GetStation(station.PriorityID)),
            });

            PriorityQueue.Update(station.PriorityID, newPriority);
        }

        var highestPriorityStation = Manager_Station.GetStation(PriorityQueue.Dequeue().PriorityID);
        var availableCarryWeight = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;
        var allItemsInStation = highestPriorityStation.GetInventoryItemsToHaul();

        var itemsToHaul = new List<Item>();

        while (allItemsInStation.Count > 0 && availableCarryWeight > 0)
        {
            var item = allItemsInStation[0];
            var itemMaster = Manager_Item.GetMasterItem(item.ItemID);
            var itemWeight = itemMaster.CommonStats_Item.ItemWeight * item.ItemAmount;

            if (itemWeight > availableCarryWeight) break;

            itemsToHaul.Add(item);
            allItemsInStation.Remove(item);
            availableCarryWeight -= itemWeight;
        }

        return (highestPriorityStation, itemsToHaul);
    }
}

public abstract class Priority_Data
{
    public uint PriorityID;
    public Priority_Data(uint priorityID) => PriorityID = priorityID; 

    protected abstract PriorityComponent PriorityComponent { get; }
    
    public Action<DataChanged, List<PriorityParameter>> OnDataChange;
    
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
        if (PriorityParameterList.Count == 0)
        {
            Debug.LogError("ActionsAndParameters is empty.");
            return null;
        }

        if (!PriorityParameterList.TryGetValue(dataChanged, out var priorityParameters))
        {
            Debug.LogError($"DataChanged: {dataChanged} is not in ActionsAndParameters list");
            return null;
        }

        return priorityParameters;
    }

    protected abstract Dictionary<DataChanged, List<PriorityParameter>> PriorityParameterList { get; }
}

public abstract class Priority_Data_Actor : Priority_Data
{
    public Priority_Data_Actor(uint actorID) : base(actorID) { }

    readonly ActorReferences ActorReferences;
    public uint ActorID { get { return ActorReferences.ActorID; } }
    protected ActorComponent Actor { get { return ActorReferences.Actor; }}

    protected override PriorityComponent PriorityComponent { get { return Actor.PriorityComponent; } }
}

public abstract class Priority_Data_Station : Priority_Data
{
    public Priority_Data_Station(uint stationID) : base(stationID) { }

    readonly StationReferences StationReferences;
    public uint StationID { get { return StationReferences.StationID; } }
    protected StationComponent Station { get { return StationReferences.Station; }}

    protected override PriorityComponent PriorityComponent { get { return Station.PriorityComponent; } }
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
    Priority[] _allPriorities;
    Dictionary<uint, int> _priorityQueue;

    public PriorityQueue(int maxPriorities)
    {
        _currentPosition = 0;
        _allPriorities = new Priority[maxPriorities];
        _priorityQueue = new Dictionary<uint, int>();
    }

    public Priority Peek(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            return _allPriorities[1];
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;

            return index == 0 ? null : _allPriorities[index];
        }
    }

    public Priority[] PeekAll()
    {
        if (_currentPosition == 0) return null;

        return _allPriorities;
    }

    public Priority Dequeue(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            Priority priority = _allPriorities[1];
            _allPriorities[1] = _allPriorities[_currentPosition];
            _priorityQueue[_allPriorities[1].PriorityID] = 1;
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

            Priority priority = _allPriorities[index];
            _priorityQueue[priorityID] = 0;
            _allPriorities[index] = _allPriorities[_currentPosition];
            _priorityQueue[_allPriorities[_currentPosition].PriorityID] = index;
            _currentPosition--;
            _moveDown(index);

            return priority;
        }
    }

    public bool Enqueue(uint priorityID, List<float> priorities)
    {
        if (_priorityQueue.TryGetValue(priorityID, out _))
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
        if (_currentPosition == _allPriorities.Length) Array.Resize<Priority>(ref _allPriorities, _allPriorities.Length * 2);
        _allPriorities[_currentPosition] = priority;
        _moveUp(_currentPosition);

        return true;
    }

    public bool Update(uint priorityID, List<float> priorities)
    {
        if (priorities.Count == 0)
        {
            if (!Remove(priorityID))
            {
                Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
                return false;
            }
            
            return true;
        }

        int index;

        if (!_priorityQueue.TryGetValue(priorityID, out index))
        {
            if (!Enqueue(priorityID, priorities))
            {
                Debug.LogError($"PriorityID: {priorityID} unable to be enqueued.");
                return false;
            }

            return true;
        }

        if (index == 0)
        {
            Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
            return false;
        }
        
        Priority priority_New = new Priority(priorityID, priorities);
        Priority priority_Old = _allPriorities[index];

        _allPriorities[index] = priority_New;

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
        int index;

        if (!_priorityQueue.TryGetValue(priorityID, out index))
        {
            return false;
        }

        if (index == 0)
        {
            Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
            return false;
        }

        _priorityQueue[priorityID] = 0;
        _allPriorities[index] = _allPriorities[_currentPosition];
        _priorityQueue[_allPriorities[index].PriorityID] = index;
        _currentPosition--;
        _moveDown(index);

        return true;
    }

    void _moveDown(int index)
    {
        int childL = index * 2;

        if (childL > _currentPosition) return;

        int childR = index * 2 + 1;
        int smallerChild;

        if (childR > _currentPosition)
        {
            smallerChild = childL;
        }
        else if (_allPriorities[childL].CompareTo(_allPriorities[childR]) < 0)
        {
            smallerChild = childL;
        }
        else
        {
            smallerChild = childR;
        }

        if (_allPriorities[index].CompareTo(_allPriorities[smallerChild]) > 0)
        {
            _swap(index, smallerChild);
            _moveDown(smallerChild);
        }
    }

    void _moveUp(int index)
    {
        if (index == 1) return;
        int parent = index / 2;

        if (_allPriorities[parent].CompareTo(_allPriorities[index]) > 0)
        {
            _swap(parent, index);
            _moveUp(parent);
        }
    }

    void _swap(int indexA, int indexB)
    {
        Priority tempPriorityA = _allPriorities[indexA];
        _allPriorities[indexA] = _allPriorities[indexB];
        _priorityQueue[_allPriorities[indexB].PriorityID] = indexA;
        _allPriorities[indexB] = tempPriorityA;
        _priorityQueue[tempPriorityA.PriorityID] = indexB;
    }
}