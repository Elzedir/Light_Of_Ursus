using System.Collections.Generic;
using System.Linq;
using Actors;
using Managers;
using Tools;
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
            
            { ActionName.Scavenge, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
            }},
            
            { ActionName.Wander, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
            }},
        };
        
        public readonly ObservableDictionary<BehaviourName, PriorityQueue> AllPriorityBehaviours = new()
        {
            { BehaviourName.None, new PriorityQueue(1) },
            { BehaviourName.Hostile, new PriorityQueue(1) },
            { BehaviourName.Stealth, new PriorityQueue(1) },
            { BehaviourName.Work, new PriorityQueue(1) },
        };

        public readonly ObservableDictionary<ActionName, PriorityQueue> AllPriorityActions = new()
        {
            { ActionName.Fetch, new PriorityQueue(1) },
            { ActionName.Deliver, new PriorityQueue(1) },
            { ActionName.Scavenge, new PriorityQueue(1) },
            { ActionName.Wander, new PriorityQueue(1) },
        };

        Dictionary<PriorityImportance, List<PriorityValue>>              _cachedPriorityQueue;
        protected abstract Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; }

        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        ActionName _currentAction;

        public ActionName GetCurrentAction()                       => _currentAction;
        protected void              _setCurrentAction(ActionName actionName)
        {
            Debug.Log($"Setting current action to: {actionName}.");
            _currentAction = actionName;
        }

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
                        if (!AllPriorityActions[action.ActionName].Update((uint)action.ActionName, priorities))
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
            foreach (var priority in from priorityQueue in AllPriorityActions.Values 
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
            if (!AllPriorityActions[actionName].Update((uint)actionName, priorities))
            {
                Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
            }
        }

        public void RemoveAction(ActionName actionName)
        {
            if (!AllPriorityActions[actionName].Remove((uint)actionName))
            {
                Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
            }
        }

        public PriorityValue PeekHighestSpecificPriority(ActionName actionName)
        {
            if (actionName is not (ActionName.All or ActionName.None)) 
                return AllPriorityActions[actionName].Peek();
            
            Debug.LogError($"ActionName: {actionName} not allowed in PeekHighestSpecificPriority.");
            return null;
        }
        
        public PriorityValue PeekHighestPriority(PriorityStatus priorityStatus)
        {
            var overallHighestPriority = new PriorityValue((uint)ActionName.None, new List<float>());

            if (AllPriorityActions.Count is 0)
            {
                Debug.LogWarning("No priority queues found.");
                return new PriorityValue((uint)ActionName.None, new List<float>());
            };

            var relevantPriorityQueues = _getRelevantPriorityQueues(priorityStatus);

            overallHighestPriority = relevantPriorityQueues.Aggregate(overallHighestPriority, 
                (current, priorityQueue) =>
            {
                var highestPriority = priorityQueue.Value.Peek();

                if (highestPriority is null) return current;

                return highestPriority.PriorityID != (uint)ActionName.None &&
                       highestPriority.PriorityID > current.PriorityID
                    ? highestPriority
                    : current;
            });

            return overallHighestPriority.PriorityID != (uint)ActionName.None
                ? overallHighestPriority
                : null;
        }
        
        ObservableDictionary<ActionName, PriorityQueue> _getRelevantPriorityQueues(PriorityStatus priorityStatus)
        {
            if (priorityStatus is PriorityStatus.None) return AllPriorityActions;

            var relevantPriorityQueues = new ObservableDictionary<ActionName, PriorityQueue>();
            
            switch (priorityStatus)
            {
                case PriorityStatus.InCombat:
                    relevantPriorityQueues.Add(ActionName.Attack, AllPriorityActions[ActionName.Wander]);
                    break;
                case PriorityStatus.HasJob:
                    break;
                default:
                    break;
            }

            return relevantPriorityQueues;
        }

        public PriorityValue GetHighestSpecificPriority(ActionName actionName)
        {
            var highestPriority = PeekHighestSpecificPriority(actionName);

            return highestPriority != null ? AllPriorityActions[actionName].Dequeue(highestPriority.PriorityID) : null;            
        }
        
        public PriorityValue GetHighestPriority(PriorityStatus priorityStatus)
        {
            var highestPriority = PeekHighestPriority(priorityStatus);

            return highestPriority != null ? AllPriorityActions[(ActionName)highestPriority.PriorityID]
                .Dequeue(highestPriority.PriorityID) : null;
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
            
            AllPriorityActions.DictionaryChanged += _setCurrentAction;
        }

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; } = new()
        {
            {
                DataChanged.ChangedInventory, new List<ActionToChange>
                {
                    new(ActionName.Deliver, PriorityImportance.High)
                }
            },

            {
                DataChanged.DroppedItems, new List<ActionToChange>
                {
                    new(ActionName.Fetch, PriorityImportance.High),
                    new(ActionName.Scavenge, PriorityImportance.Medium),
                }
            },

            {
                DataChanged.PriorityCompleted, new List<ActionToChange>
                {
                    new(ActionName.Wander, PriorityImportance.High),
                }
            },
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

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; } = new()
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
            _jobsiteReferences                  =  new ComponentReference_Jobsite(jobsiteID);
        } 

        readonly ComponentReference_Jobsite _jobsiteReferences;

        public uint      JobsiteID => _jobsiteReferences.JobsiteID;
        JobsiteComponent _jobsite  => _jobsiteReferences.Jobsite;

        public (StationComponent Station, List<Item> Items) GetStationToFetchFrom(ActorComponent hauler)
        {
            // Reduce the redundancy of the constant GetItemsToFetch in relevant stations and here.
            
            var allRelevantStations = _jobsite.GetRelevantStations(ActionName.Fetch, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.LogError("No stations to fetch from.");
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

                AllPriorityActions[ActionName.Fetch].Update(station.StationID, newPriorities);
            }

            var peekedStation = Manager_Station.GetStation(AllPriorityActions[ActionName.Fetch].Peek().PriorityID);
        
            if (peekedStation is null) return (null, null);

            var allItemsToFetch = peekedStation.GetInventoryItemsToFetch();

            if (allItemsToFetch.Count is 0) return (null, null);

            var availableSpace = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;

            var itemsToFetch = new List<Item>();

            for (var i = 0; i < allItemsToFetch.Count; i++)
            {
                if (availableSpace <= 0) break;
                    
                var item       = new Item(allItemsToFetch[i]);
                
                // Find out how to remove items with 0 amount rather than skip them.
                if (item.ItemAmount <= 0) continue;
                
                var itemMaster = Manager_Item.GetMasterItem(item.ItemID);
                var itemWeight = itemMaster.CommonStats_Item.ItemWeight;
                
                if (itemWeight > availableSpace) continue;
                
                var totalItemWeight =  itemWeight * item.ItemAmount;
                
                if (totalItemWeight < availableSpace)
                {
                    itemsToFetch.Add(item);
                    allItemsToFetch.Remove(item);
                    availableSpace -= totalItemWeight;
                    
                    continue;
                }

                var unitsAbleToCarry = (uint)(availableSpace / itemWeight);
                
                itemsToFetch.Add(item);
                item.ItemAmount -= unitsAbleToCarry;
                availableSpace -= unitsAbleToCarry * itemWeight;
            }

            if (itemsToFetch.Count is 0) return (null, null);
            
            AllPriorityActions[ActionName.Fetch].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToFetch);
        }

        public (StationComponent Station, List<Item> Items) GetStationToDeliverTo(ActorComponent hauler)
        {
            // Reduce the redundancy of the constant GetItemsToDeliver in relevant stations and here.
            var allRelevantStations = _jobsite.GetRelevantStations(ActionName.Deliver, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.Log("No stations to deliver to.");
                return (null, null);
            }
        
            var                totalItemsToDeliver      = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToDeliver(hauler.ActorData.InventoryData)));
            var                totalDistance   = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));
            var allStationTypes = new HashSet<StationName>(allRelevantStations.Select(station => station.StationName));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActionName.Deliver, new Dictionary<PriorityParameter, object>
                {
                    { PriorityParameter.TotalItems, totalItemsToDeliver },
                    { PriorityParameter.TotalDistance, totalDistance },
                    { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameter.InventoryTarget, station.StationData.InventoryData }, 
                    { PriorityParameter.CurrentStationType, station.StationName },
                    { PriorityParameter.AllStationTypes, allStationTypes },
                });

                var newPriorities = PriorityGenerator.GeneratePriorities(ActionName.Deliver, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                AllPriorityActions[ActionName.Deliver].Update(station.StationID, newPriorities);
            }

            var peek = AllPriorityActions[ActionName.Deliver].Peek();
            
            if (peek is null) return (null, null);

            var peekedStation = Manager_Station.GetStation(peek.PriorityID);
        
            if (peekedStation is null) return (null, null);

            var itemsToDeliver = peekedStation.GetInventoryItemsToDeliver(hauler.ActorData.InventoryData);

            if (itemsToDeliver.Count is 0) return (null, null);

            AllPriorityActions[ActionName.Deliver].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToDeliver);
        }

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; } = new();
    }
}