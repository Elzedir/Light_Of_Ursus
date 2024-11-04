using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PriorityComponent
{
    public Dictionary<ActionName, Dictionary<PriorityParameter, object>> _actionPriorityParameters = new()
    {
        { ActionName.Deliver, new Dictionary<PriorityParameter, object>
        {
            { PriorityParameter.MaxPriority, null },
            { PriorityParameter.TotalItems, null },
            { PriorityParameter.TotalDistance, null },
            { PriorityParameter.InventoryHauler, null },
            { PriorityParameter.InventoryTarget, null },
            { PriorityParameter.AllStationTypes, null },
        }},

        { ActionName.Fetch, new Dictionary<PriorityParameter, object>
        {
            { PriorityParameter.MaxPriority, null },
            { PriorityParameter.TotalItems, null },
            { PriorityParameter.TotalDistance, null },
            { PriorityParameter.InventoryHauler, null },
            { PriorityParameter.InventoryTarget, null },
        }},
    };

    public Dictionary<ActionName, PriorityQueue> AllPriorityQueues = new()
    {
        { ActionName.Fetch, new PriorityQueue(1) },
        { ActionName.Deliver, new PriorityQueue(1) },
        { ActionName.Scavenge, new PriorityQueue(1) },
    };

    public Dictionary<PriorityImportance, List<Priority>> CachedPriorityQueue;
    protected abstract Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; }

    protected bool _syncingCachedQueue = false;
    protected float _timeDeferment = 1f;

    public void OnDataChanged(DataChanged dataChanged, Dictionary<PriorityParameter, object> changedParameters)
    {
        if (!_actionsToChange.TryGetValue(dataChanged, out var actionsToChange))
        {
            Debug.Log($"DataChanged: {dataChanged} not found in _actionsToChange for {this}.");
            return;
        }

        foreach (var action in actionsToChange)
        {
            var parameters = UpdateExistingPriorityParameters(action.ActionName, changedParameters);

            var priorities = PriorityGenerator.GeneratePriorities(action.ActionName, parameters);

            switch (action.PriorityImportance)
            {
                case PriorityImportance.Critical:
                    if (!AllPriorityQueues[action.ActionName].Update((uint)action.ActionName, priorities))
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
    }

    public void SyncCachedPriorityQueueHigh(bool syncing = false)
    {
        foreach (PriorityQueue priorityQueue in AllPriorityQueues.Values)
        {
            foreach (var priority in CachedPriorityQueue[PriorityImportance.High])
            {
                if (!priorityQueue.Update(priority.PriorityID, priority.AllPriorities))
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

    public void UpdateAction(ActionName actionName, List<float> priorities)
    {
        if (!AllPriorityQueues[actionName].Update((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
        }
    }

    public void RemoveAction(ActionName actionName)
    {
        if (!AllPriorityQueues[actionName].Remove((uint)actionName))
        {
            Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
        }
    }

    public Priority CheckHighestPriority(ActionName actionName)
    {
        return AllPriorityQueues[actionName].Peek();
    }

    public Priority GetHighestPriority(ActionName actionName)
    {
        return AllPriorityQueues[actionName].Dequeue();
    }

    public Dictionary<PriorityParameter, object> UpdateExistingPriorityParameters(ActionName actionName, Dictionary<PriorityParameter, object> parameters)
    {
        if (!_actionPriorityParameters.TryGetValue(actionName, out var existingPriorityParameters))
        {
            Debug.LogError($"ActionName: {actionName} not found in _existingParameters.");
            return null;
        }

        foreach (var parameter in parameters)
        {
            if (!existingPriorityParameters.ContainsKey(parameter.Key))
            {
                Debug.LogError($"Parameter: {parameter.Key} not found.");
                continue;
            }

            existingPriorityParameters[parameter.Key] = parameter.Value;
        }

        return existingPriorityParameters;
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
    }

    protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; } = new()
    {
        { DataChanged.ChangedInventory, new List<ActionToChange>
        {
            new ActionToChange(ActionName.Deliver, PriorityImportance.High),
        }},

        { DataChanged.DroppedItems, new List<ActionToChange>
        {
            new ActionToChange(ActionName.Fetch, PriorityImportance.High),
            new ActionToChange(ActionName.Scavenge, PriorityImportance.Medium),
        }},
    };
}

public class PriorityComponent_Station : PriorityComponent
{
    public PriorityComponent_Station(uint stationID) 
    {
        _stationReferences = new ComponentReference_Station(stationID);
    } 

    readonly ComponentReference_Station _stationReferences;

    public uint JobsiteID { get { return _stationReferences.StationID; } }
    protected StationComponent Jobsite { get { return _stationReferences.Station; } }

    protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; } = new()
    {
        { DataChanged.ChangedInventory, new List<ActionToChange>
        {
            new ActionToChange(ActionName.Deliver, PriorityImportance.High),
            new ActionToChange(ActionName.Fetch, PriorityImportance.High),
        }},
    };
}

public class PriorityComponent_Jobsite : PriorityComponent
{
    public PriorityComponent_Jobsite(uint jobsiteID) 
    {
        _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
    } 

    readonly ComponentReference_Jobsite _jobsiteReferences;

    public uint JobsiteID { get { return _jobsiteReferences.JobsiteID; } }
    protected JobsiteComponent Jobsite { get { return _jobsiteReferences.Jobsite; } }

    public (StationComponent Station, List<Item> Items) GetStationToFetchFrom(ActorComponent hauler)
    {
        List<StationComponent> allRelevantStations = Jobsite.AllStationsInJobsite.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();

        if (allRelevantStations.Count == 0)
        {
            Debug.Log("No stations to fetch from.");
            return (null, null);
        }

        float totalItemsToFetch = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
        float totalDistance = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));

        foreach (var station in allRelevantStations)
        {
            var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActionName.Fetch, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.TotalItems, totalItemsToFetch },
                { PriorityParameter.TotalDistance, totalDistance },
                { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                { PriorityParameter.InventoryTarget, station.StationData.InventoryData },
            });

            List<float> newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Fetch, priorityParameters);

            if (newPriorities == null || newPriorities.Count == 0) continue;

            AllPriorityQueues[ActionName.Fetch].Update(station.StationID, newPriorities);
        }

        StationComponent peekedStation = Manager_Station.GetStation(AllPriorityQueues[ActionName.Fetch].Peek().PriorityID);
        
        if (peekedStation == null) return (null, null);

        var allItemsInStation = peekedStation.GetInventoryItemsToFetch();

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

        AllPriorityQueues[ActionName.Fetch].Dequeue(peekedStation.StationID);

        return (peekedStation, itemsToHaul);
    }

    public (StationComponent Station, List<Item> Items) GetStationToDeliverTo(ActorComponent hauler)
    {
        List<StationComponent> allRelevantStations = Jobsite.GetRelevantStations(ActionName.Deliver, hauler.ActorData.InventoryData);

        if (allRelevantStations.Count == 0)
        {
            Debug.Log("No stations to fetch from.");
            return (null, null);
        }
        
        float totalItems = Jobsite.AllStationsInJobsite.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToHold()));
        float totalDistance = Jobsite.AllStationsInJobsite.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));
        HashSet<StationName> allStationTypes = new HashSet<StationName>(allRelevantStations.Select(station => station.StationName));

        foreach (var station in allRelevantStations)
        {
            var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActionName.Deliver, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.TotalItems, totalItems },
                { PriorityParameter.TotalDistance, totalDistance },
                { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                { PriorityParameter.InventoryTarget, station.StationData.InventoryData }, 
                { PriorityParameter.AllStationTypes, allStationTypes },
            });

            List<float> newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Deliver, priorityParameters);

            if (newPriorities == null || newPriorities.Count == 0) continue;

            AllPriorityQueues[ActionName.Deliver].Update(station.StationID, newPriorities);
        }

        StationComponent peekedStation = Manager_Station.GetStation(AllPriorityQueues[ActionName.Deliver].Peek().PriorityID);
        
        if (peekedStation == null) return (null, null);

        var allItemsInStation = peekedStation.GetInventoryItemsToFetch();

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

        AllPriorityQueues[ActionName.Deliver].Dequeue(peekedStation.StationID);

        return (peekedStation, itemsToHaul);
    }

    protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; } = new()
    {
        
    };
}
