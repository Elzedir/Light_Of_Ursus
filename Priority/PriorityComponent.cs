using System.Collections.Generic;
using System.Linq;
using Actors;
using UnityEngine;

namespace Priority
{
    public abstract class PriorityComponent
    {
        readonly Dictionary<ActionName, Dictionary<PriorityParameter, object>> _actionPriorityParameters = new()
        {
            { ActionName.Fetch, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
            }},

            { ActionName.Deliver, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
                { PriorityParameter.CurrentStationType, null },
                { PriorityParameter.AllStationTypes, null },
            }},
        };

        protected readonly Dictionary<ActionName, PriorityQueue> _allPriorityQueues = new()
        {
            { ActionName.Fetch, new PriorityQueue(1) },
            { ActionName.Deliver, new PriorityQueue(1) },
            { ActionName.Scavenge, new PriorityQueue(1) },
        };

        Dictionary<PriorityImportance, List<PriorityValue>> _cachedPriorityQueue;
        protected abstract Dictionary<DataChanged, List<ActionToChange>>       _actionsToChange { get; set; }

        bool           _syncingCachedQueue;
        readonly float _timeDeferment      = 1f;

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
                        if (!_allPriorityQueues[action.ActionName].Update((uint)action.ActionName, priorities))
                        {
                            Debug.LogError($"Action: {action} unable to be updated in PriorityQueue.");
                        }
                        
                        break;
                    case PriorityImportance.High:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActionName, priorities), PriorityImportance.High);
                        break;
                    case PriorityImportance.Medium:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActionName, priorities), PriorityImportance.Medium);
                        break;
                    case PriorityImportance.Low:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActionName, priorities), PriorityImportance.Low);
                        break;
                    case PriorityImportance.None:
                    default:
                        Debug.LogError($"PriorityImportance: {action.PriorityImportance} not found.");
                        break;
                }
            }
        }

        public void FullPriorityUpdate(List<object> allData)
        {
            _syncCachedPriorityQueueHigh();
            //SyncCachedPriorityQueueMedium();
            //SyncCachedPriorityQueueLow();
        }

        void _syncCachedPriorityQueueHigh(bool syncing = false)
        {
            foreach (var priority in from priorityQueue in _allPriorityQueues.Values 
                                     from priority in _cachedPriorityQueue[PriorityImportance.High] 
                                     where !priorityQueue.Update(priority.PriorityID, priority.AllPriorities) 
                                     select priority)
            {
                Debug.LogError($"PriorityID: {priority.PriorityID} unable to be added to PriorityQueue.");
            }

            _cachedPriorityQueue[PriorityImportance.Low].Clear();
            if (syncing) _syncingCachedQueue = false;
        }

        void _syncCachedPriorityQueueHigh_DeferredUpdate()
        {
            _syncingCachedQueue = true;
            Manager_DeferredActions.AddDeferredAction(() => _syncCachedPriorityQueueHigh(true), _timeDeferment);
        }

        void _addToCachedPriorityQueue(PriorityValue priorityValue, PriorityImportance priorityImportance)
        {
            _cachedPriorityQueue ??= new Dictionary<PriorityImportance, List<PriorityValue>>();

            if (!_cachedPriorityQueue.ContainsKey(priorityImportance)) _cachedPriorityQueue.Add(priorityImportance, new List<PriorityValue>());

            _cachedPriorityQueue[priorityImportance].Add(priorityValue);

            if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
        }

        public void UpdateAction(ActionName actionName, List<float> priorities)
        {
            if (!_allPriorityQueues[actionName].Update((uint)actionName, priorities))
            {
                Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
            }
        }

        public void RemoveAction(ActionName actionName)
        {
            if (!_allPriorityQueues[actionName].Remove((uint)actionName))
            {
                Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
            }
        }

        public PriorityValue CheckHighestPriority(ActionName actionName)
        {
            return _allPriorityQueues[actionName].Peek();
        }

        public PriorityValue GetHighestPriority(ActionName actionName)
        {
            return _allPriorityQueues[actionName].Dequeue();
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

        public    uint           ActorID => _actorReferences.ActorID;
        protected ActorComponent _actor  => _actorReferences.Actor;

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
                new ActionToChange(ActionName.Fetch,    PriorityImportance.High),
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

        public    uint             JobsiteID => _stationReferences.StationID;
        protected StationComponent _jobsite  => _stationReferences.Station;

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; } = new()
        {
            { DataChanged.ChangedInventory, new List<ActionToChange>
            {
                new ActionToChange(ActionName.Deliver, PriorityImportance.High),
                new ActionToChange(ActionName.Fetch,   PriorityImportance.High),
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

        public uint      JobsiteID => _jobsiteReferences.JobsiteID;
        JobsiteComponent _jobsite  => _jobsiteReferences.Jobsite;

        public (StationComponent Station, List<Item> Items) GetStationToFetchFrom(ActorComponent hauler)
        {
            var allRelevantStations = _jobsite.AllStationsInJobsite
                                              .Where(station => station.GetInventoryItemsToFetch().Count > 0)
                                              .ToList();

            if (allRelevantStations.Count is 0)
            {
                //Debug.Log("No stations to fetch from.");
                return (null, null);
            }

            var totalItemsToFetch = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
            var totalDistance     = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActionName.Fetch, new Dictionary<PriorityParameter, object>
                {
                    { PriorityParameter.TotalItems, totalItemsToFetch },
                    { PriorityParameter.TotalDistance, totalDistance },
                    { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameter.InventoryTarget, station.StationData.InventoryData },
                });

                var newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Fetch, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                _allPriorityQueues[ActionName.Fetch].Update(station.StationID, newPriorities);
            }

            var peekedStation = Manager_Station.GetStation(_allPriorityQueues[ActionName.Fetch].Peek().PriorityID);
        
            if (peekedStation is null) return (null, null);

            var allItemsInStation = peekedStation.GetInventoryItemsToFetch();

            if (allItemsInStation.Count is 0) return (null, null);

            var availableCarryWeight = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;

            var itemsToHaul = new List<Item>();
            
            Debug.Log($""                                                                  +
                      $"Items: {Item.GetItemListTotal_CountAllItems(allItemsInStation)}, " +
                      $"TotalWeight: {Item.GetItemListTotal_Weight(allItemsInStation)} "   +
                      $"AvailableCarryWeight: {availableCarryWeight}");

            for (var i = 0; i < allItemsInStation.Count; i++)
            {
                Debug.LogWarning(availableCarryWeight);
                
                if (availableCarryWeight <= 0) break;
                    
                var item       = allItemsInStation[i];
                var itemMaster = Manager_Item.GetMasterItem(item.ItemID);
                var itemWeight = itemMaster.CommonStats_Item.ItemWeight;
                
                Debug.LogWarning($"{itemWeight}: {availableCarryWeight}");
                
                if (itemWeight > availableCarryWeight) continue;
                
                var totalItemWeight =  itemWeight * item.ItemAmount;
                
                Debug.LogWarning(totalItemWeight);
                
                if (totalItemWeight < availableCarryWeight)
                {
                    itemsToHaul.Add(item);
                    allItemsInStation.Remove(item);
                    availableCarryWeight -= totalItemWeight;
                    continue;
                }

                var unitsAbleToCarry = (uint)(availableCarryWeight / itemWeight);
                
                Debug.LogWarning(unitsAbleToCarry);
                
                itemsToHaul.Add(new Item(item.ItemID, unitsAbleToCarry));
                item.ItemAmount -= unitsAbleToCarry;
                availableCarryWeight -= unitsAbleToCarry * itemWeight;
            }
            
            Debug.LogWarning(itemsToHaul.Count);

            if (itemsToHaul.Count == 0) return (null, null);

            _allPriorityQueues[ActionName.Fetch].Dequeue(peekedStation.StationID);
            
            Debug.LogWarning($"{peekedStation.StationName} - {itemsToHaul.Count}");

            return (peekedStation, itemsToHaul);
        }

        public (StationComponent Station, List<Item> Items) GetStationToDeliverTo(ActorComponent hauler)
        {
            var allRelevantStations = _jobsite.GetRelevantStations(ActionName.Deliver, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.Log("No stations to fetch from.");
                return (null, null);
            }
        
            var                totalItems      = _jobsite.AllStationsInJobsite.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
            var                totalDistance   = _jobsite.AllStationsInJobsite.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));
            var allStationTypes = new HashSet<StationName>(allRelevantStations.Select(station => station.StationName));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActionName.Deliver, new Dictionary<PriorityParameter, object>
                {
                    { PriorityParameter.TotalItems, totalItems },
                    { PriorityParameter.TotalDistance, totalDistance },
                    { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameter.InventoryTarget, station.StationData.InventoryData }, 
                    { PriorityParameter.CurrentStationType, station.StationName },
                    { PriorityParameter.AllStationTypes, allStationTypes },
                });

                List<float> newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Deliver, priorityParameters);

                if (newPriorities == null || newPriorities.Count == 0) continue;

                _allPriorityQueues[ActionName.Deliver].Update(station.StationID, newPriorities);
            }

            StationComponent peekedStation = Manager_Station.GetStation(_allPriorityQueues[ActionName.Deliver].Peek().PriorityID);
        
            if (peekedStation == null) return (null, null);

            var allItemsInStation = peekedStation.GetInventoryItemsToFetch();

            if (allItemsInStation.Count == 0) return (null, null);

            float availableCarryWeight = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;

            List<Item> itemsToHaul = new List<Item>();

            while (allItemsInStation.Count > 0 && availableCarryWeight > 0)
            {
                Debug.Log($"AllItemsInStation: {allItemsInStation.Count}, AvailableCarryWeight: {availableCarryWeight}");

                Item        item       = allItemsInStation[0];
                Item_Master itemMaster = Manager_Item.GetMasterItem(item.ItemID);
                float       itemWeight = itemMaster.CommonStats_Item.ItemWeight * item.ItemAmount;

                if (itemWeight > availableCarryWeight) break;

                itemsToHaul.Add(item);
                allItemsInStation.Remove(item);
                availableCarryWeight -= itemWeight;
            }

            if (itemsToHaul.Count == 0) return (null, null);

            _allPriorityQueues[ActionName.Deliver].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToHaul);
        }

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; set; } = new();
    }
}