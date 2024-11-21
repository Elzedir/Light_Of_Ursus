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
        readonly Dictionary<ActorActionName, Dictionary<PriorityParameter, object>> _actionPriorityParameters = new()
        {
            { ActorActionName.Fetch, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
            }},

            { ActorActionName.Deliver, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
                { PriorityParameter.CurrentStationType, null },
                { PriorityParameter.AllStationTypes, null },
            }},
            
            { ActorActionName.Scavenge, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
                { PriorityParameter.TotalItems, null },
                { PriorityParameter.TotalDistance, null },
                { PriorityParameter.InventoryHauler, null },
                { PriorityParameter.InventoryTarget, null },
            }},
            
            { ActorActionName.Wander, new Dictionary<PriorityParameter, object>
            {
                { PriorityParameter.MaxPriority, null },
            }},
        };

        public readonly ObservableDictionary<ActorActionName, PriorityQueue> AllPriorityActions = new()
        {
            { ActorActionName.Fetch, new PriorityQueue(1) },
            { ActorActionName.Deliver, new PriorityQueue(1) },
            { ActorActionName.Scavenge, new PriorityQueue(1) },
            { ActorActionName.Wander, new PriorityQueue(1) },
        };

        Dictionary<PriorityImportance, List<PriorityValue>>              _cachedPriorityQueue;
        protected abstract Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; }

        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        ActorActionName _currentActorAction;

        public ActorActionName GetCurrentAction()                       => _currentActorAction;
        protected void              _setCurrentAction(ActorActionName actorActionName)
        {
            Debug.Log($"Setting current action to: {actorActionName}.");
            _currentActorAction = actorActionName;
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
                var parameters = UpdateExistingPriorityParameters(action.ActorActionName, changedParameters);

                var priorities = PriorityGenerator.GeneratePriorities(action.ActorActionName, parameters);

                switch (action.PriorityImportance)
                {
                    case PriorityImportance.Critical:
                        if (!AllPriorityActions[action.ActorActionName].Update((uint)action.ActorActionName, priorities))
                        {
                            Debug.LogError($"Action: {action} unable to be updated in PriorityQueue.");
                        }
                        
                        break;
                    case PriorityImportance.High:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActorActionName, priorities), PriorityImportance.High);
                        break;
                    case PriorityImportance.Medium:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActorActionName, priorities), PriorityImportance.Medium);
                        break;
                    case PriorityImportance.Low:
                        _addToCachedPriorityQueue(new PriorityValue((uint)action.ActorActionName, priorities), PriorityImportance.Low);
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

        public void UpdateAction(ActorActionName actorActionName, List<float> priorities)
        {
            if (!AllPriorityActions[actorActionName].Update((uint)actorActionName, priorities))
            {
                Debug.LogError($"ActionName: {actorActionName} unable to be updated in PriorityQueue.");
            }
        }

        public void RemoveAction(ActorActionName actorActionName)
        {
            if (!AllPriorityActions[actorActionName].Remove((uint)actorActionName))
            {
                Debug.LogError($"ActionName: {actorActionName} unable to be removed from PriorityQueue.");
            }
        }

        public PriorityValue PeekHighestSpecificPriority(ActorActionName actorActionName)
        {
            if (actorActionName is not (ActorActionName.All or ActorActionName.Idle)) 
                return AllPriorityActions[actorActionName].Peek();
            
            Debug.LogError($"ActionName: {actorActionName} not allowed in PeekHighestSpecificPriority.");
            return null;
        }
        
        public PriorityValue PeekHighestPriority(PriorityStatus priorityStatus)
        {
            var overallHighestPriority = new PriorityValue((uint)ActorActionName.Idle, new List<float>());

            if (AllPriorityActions.Count is 0)
            {
                Debug.LogWarning("No priority queues found.");
                return new PriorityValue((uint)ActorActionName.Idle, new List<float>());
            };

            var relevantPriorityQueues = _getRelevantPriorityQueues(priorityStatus);

            overallHighestPriority = relevantPriorityQueues.Aggregate(overallHighestPriority, 
                (current, priorityQueue) =>
            {
                var highestPriority = priorityQueue.Value.Peek();

                if (highestPriority is null) return current;

                return highestPriority.PriorityID != (uint)ActorActionName.Idle &&
                       highestPriority.PriorityID > current.PriorityID
                    ? highestPriority
                    : current;
            });

            return overallHighestPriority.PriorityID != (uint)ActorActionName.Idle
                ? overallHighestPriority
                : null;
        }
        
        ObservableDictionary<ActorActionName, PriorityQueue> _getRelevantPriorityQueues(PriorityStatus priorityStatus)
        {
            if (priorityStatus is PriorityStatus.None) return AllPriorityActions;

            var relevantPriorityQueues = new ObservableDictionary<ActorActionName, PriorityQueue>();
            
            switch (priorityStatus)
            {
                case PriorityStatus.InCombat:
                    foreach (var actorActionName in Manager_ActorAction.GetAllActionsInActionGroup(ActionGroup.Combat))
                    {
                        relevantPriorityQueues.Add(actorActionName, AllPriorityActions[actorActionName]);    
                    }
                    break;
                case PriorityStatus.HasWork:
                    foreach (var actorActionName in Manager_ActorAction.GetAllActionsInActionGroup(ActionGroup.Work))
                    {
                        relevantPriorityQueues.Add(actorActionName, AllPriorityActions[actorActionName]);    
                    }
                    break;
                case PriorityStatus.None:
                default:
                    break;
            }

            return relevantPriorityQueues;
        }

        public PriorityValue GetHighestSpecificPriority(ActorActionName actorActionName)
        {
            var highestPriority = PeekHighestSpecificPriority(actorActionName);

            return highestPriority != null ? AllPriorityActions[actorActionName].Dequeue(highestPriority.PriorityID) : null;            
        }
        
        public PriorityValue GetHighestPriority(PriorityStatus priorityStatus)
        {
            var highestPriority = PeekHighestPriority(priorityStatus);

            return highestPriority != null ? AllPriorityActions[(ActorActionName)highestPriority.PriorityID]
                .Dequeue(highestPriority.PriorityID) : null;
        }

        public Dictionary<PriorityParameter, object> UpdateExistingPriorityParameters(ActorActionName actorActionName, Dictionary<PriorityParameter, object> parameters)
        {
            if (!_actionPriorityParameters.TryGetValue(actorActionName, out var existingPriorityParameters))
            {
                Debug.LogError($"ActionName: {actorActionName} not found in _existingParameters.");
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
                    new(ActorActionName.Deliver, PriorityImportance.High)
                }
            },

            {
                DataChanged.DroppedItems, new List<ActionToChange>
                {
                    new(ActorActionName.Fetch, PriorityImportance.High),
                    new(ActorActionName.Scavenge, PriorityImportance.Medium),
                }
            },

            {
                DataChanged.PriorityCompleted, new List<ActionToChange>
                {
                    new(ActorActionName.Wander, PriorityImportance.High),
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
                new ActionToChange(ActorActionName.Deliver, PriorityImportance.High),
                new ActionToChange(ActorActionName.Fetch,   PriorityImportance.High),
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
            
            var allRelevantStations = _jobsite.GetRelevantStations(ActorActionName.Fetch, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.LogError("No stations to fetch from.");
                return (null, null);
            }

            var totalItemsToFetch = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
            var totalDistance     = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActorActionName.Fetch, new Dictionary<PriorityParameter, object>
                {
                    { PriorityParameter.TotalItems, totalItemsToFetch },
                    { PriorityParameter.TotalDistance, totalDistance },
                    { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameter.InventoryTarget, station.StationData.InventoryData },
                });

                var newPriorities = PriorityGenerator.GeneratePriorities(ActorActionName.Fetch, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                AllPriorityActions[ActorActionName.Fetch].Update(station.StationID, newPriorities);
            }

            var peekedStation = Manager_Station.GetStation(AllPriorityActions[ActorActionName.Fetch].Peek().PriorityID);
        
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
                
                var itemMaster = Manager_Item.GetItem_Master(item.ItemID);
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
            
            AllPriorityActions[ActorActionName.Fetch].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToFetch);
        }

        public (StationComponent Station, List<Item> Items) GetStationToDeliverTo(ActorComponent hauler)
        {
            // Reduce the redundancy of the constant GetItemsToDeliver in relevant stations and here.
            var allRelevantStations = _jobsite.GetRelevantStations(ActorActionName.Deliver, hauler.ActorData.InventoryData);

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
                var priorityParameters = station.PriorityComponent.UpdateExistingPriorityParameters(ActorActionName.Deliver, new Dictionary<PriorityParameter, object>
                {
                    { PriorityParameter.TotalItems, totalItemsToDeliver },
                    { PriorityParameter.TotalDistance, totalDistance },
                    { PriorityParameter.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameter.InventoryTarget, station.StationData.InventoryData }, 
                    { PriorityParameter.CurrentStationType, station.StationName },
                    { PriorityParameter.AllStationTypes, allStationTypes },
                });

                var newPriorities = PriorityGenerator.GeneratePriorities(ActorActionName.Deliver, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                AllPriorityActions[ActorActionName.Deliver].Update(station.StationID, newPriorities);
            }

            var peek = AllPriorityActions[ActorActionName.Deliver].Peek();
            
            if (peek is null) return (null, null);

            var peekedStation = Manager_Station.GetStation(peek.PriorityID);
        
            if (peekedStation is null) return (null, null);

            var itemsToDeliver = peekedStation.GetInventoryItemsToDeliver(hauler.ActorData.InventoryData);

            if (itemsToDeliver.Count is 0) return (null, null);

            AllPriorityActions[ActorActionName.Deliver].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToDeliver);
        }

        protected override Dictionary<DataChanged, List<ActionToChange>> _actionsToChange { get; } = new();
    }
}