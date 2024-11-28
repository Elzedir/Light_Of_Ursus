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
        public ObservableDictionary<uint, PriorityQueue> AllPriorities = new();

        Dictionary<PriorityImportance, Dictionary<uint, PriorityElement>>      _cachedPriorityQueue;

        public void OnDataChanged(DataChanged                               dataChanged,
                                           Dictionary<PriorityParameterName, object> changedParameters,
                                           uint                                      priorityID = 1)
        {
            if (!_prioritiesToChange.TryGetValue(dataChanged, out var prioritiesToChange))
            {
                Debug.Log($"DataChanged: {dataChanged} not found in _actionsToChange for {this}.");
                return;
            }

            foreach (var priorityToChange in prioritiesToChange)
            {
                var priorityElement = _createPriorityElement(priorityToChange.PriorityID, priorityID, changedParameters);
                
                switch (priorityToChange.PriorityImportance)
                {
                    case PriorityImportance.Critical:
                        if (!AllPriorities[priorityToChange.PriorityID]
                                .UpdateAll(changedParameters))
                        {
                            Debug.LogError($"Priority: {priorityToChange} unable to be updated in PriorityQueue.");
                        }

                        break;
                    case PriorityImportance.High:
                    case PriorityImportance.Medium:
                    case PriorityImportance.Low:
                        _addToCachedPriorityQueue(priorityElement, priorityToChange.PriorityImportance);
                        break;
                    case PriorityImportance.None:
                    default:
                        Debug.LogError($"PriorityImportance: {priorityToChange} not found.");
                        break;
                }
            }
        }

        PriorityQueue _createNewPriorityQueue(uint priorityQueueID)
        {
            var priorityQueue = _createPriorityQueue(priorityQueueID);
            AllPriorities.Add(priorityQueueID, priorityQueue);
            priorityQueue.OnPriorityRemoved += priorityID => _regeneratePriority(priorityQueueID, priorityID);
            return priorityQueue;
        }

        protected abstract PriorityQueue _createPriorityQueue(uint priorityQueueID);
        protected abstract void _regeneratePriority(uint  priorityQueueID, uint priorityID);
        
        public void OnDestroy()
        {
            foreach (var priorityQueue in AllPriorities)
            {
                priorityQueue.Value.OnPriorityRemoved -= priorityID => _regeneratePriority(priorityQueue.Key, priorityID);
            }
        }

        protected abstract PriorityElement _createPriorityElement(uint priorityQueueID, uint priorityID,
                                                                  Dictionary<PriorityParameterName, object>
                                                                      priorityParameters);

        public PriorityElement PeekHighestSpecificPriority(List<uint> priorityIDs)
        {
            var permittedPriorities = _canPeek(priorityIDs);
            
            if (permittedPriorities.Count is 0)
            {
                Debug.Log("No permitted priorities found.");
                return null;
            }

            var highestPriority = _createPriorityElement(0, 0, null); 

            foreach (var priority in permittedPriorities)
            {
                if (AllPriorities[priority].Peek(priority).PriorityValue >= highestPriority.PriorityValue)
                {
                    highestPriority = AllPriorities[priority].Peek(priority);
                }
            }
            
            return AllPriorities[highestPriority.PriorityID].Dequeue();
        }

        protected abstract List<uint> _canPeek(List<uint> priorityIDs);
        
        public PriorityElement PeekHighestPriority(PriorityState priorityState)
        {
            var overallHighestPriority = _createPriorityElement(0, 0, null);

            if (AllPriorities.Count is 0)
            {
                Debug.LogWarning("No priority queues found.");
                return overallHighestPriority;
            }

            var relevantPriorityQueues = _getRelevantPriorityQueues(priorityState);

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

        protected abstract ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(PriorityState priorityState);

        public PriorityElement GetHighestSpecificPriority(List<uint> priorityIDs)
        {
            var highestPriority = PeekHighestSpecificPriority(priorityIDs);

            return AllPriorities[highestPriority.PriorityID].Dequeue();
        }
        
        public PriorityElement GetHighestPriority(PriorityState priorityState)
        {
            var highestPriority = PeekHighestPriority(priorityState);

            return highestPriority != null ? AllPriorities[highestPriority.PriorityID]
                .Dequeue(highestPriority.PriorityID) : null;
        }
        
        public PriorityElement GetSpecificPriority(uint priorityID)
        {
            return AllPriorities[priorityID].Dequeue(priorityID);
        }

        protected bool _priorityExists(uint priorityQueueID, uint priorityID,
                                       out Dictionary<PriorityParameterName, object>
                                           existingPriorityParameters)
        {
            if (!AllPriorities.TryGetValue(priorityQueueID, out var priorityQueue))
            {
                Debug.LogError($"PriorityQueueID: {priorityQueueID} not found in AllPriorities.");
                existingPriorityParameters = null;
                return false;
            }

            if (priorityQueue.Peek(priorityID) is not null)
            {
                existingPriorityParameters = priorityQueue.Peek(priorityID).PriorityParameters;
                return true;
            }

            Debug.LogError($"ActionName: {priorityID} not found in _existingParameters.");
            existingPriorityParameters = null;
            return false;
        }

        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        void _syncCachedPriorityQueueHigh(bool syncing = false)
        {
            foreach (var priority in from priority in _cachedPriorityQueue[PriorityImportance.High]
                                     where !AllPriorities[priority.Key].Update(priority.Value.PriorityID, priority.Value.PriorityParameters)
                                     select priority)
            {
                Debug.LogError($"PriorityID: {priority.Value.PriorityID} unable to be added to PriorityQueue.");
            }

            _cachedPriorityQueue[PriorityImportance.Low].Clear();
            if (syncing) _syncingCachedQueue = false;
        }

        void _syncCachedPriorityQueueHigh_DeferredUpdate()
        {
            _syncingCachedQueue = true;
            Manager_DeferredActions.AddDeferredAction(() => _syncCachedPriorityQueueHigh(true), _timeDeferment);
        }
        
        protected void _addToCachedPriorityQueue(PriorityElement priorityElement, PriorityImportance priorityImportance)
        {
            _cachedPriorityQueue ??= new Dictionary<PriorityImportance, Dictionary<uint, PriorityElement>>();

            if (!_cachedPriorityQueue.ContainsKey(priorityImportance))
                _cachedPriorityQueue.Add(priorityImportance, new Dictionary<uint, PriorityElement>());
        
            _cachedPriorityQueue[priorityImportance].Add(priorityElement.PriorityID, priorityElement);
        
            if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
        }

        protected abstract Dictionary<DataChanged, List<PriorityToChange>> _prioritiesToChange { get; }
    }
    
    public class PriorityToChange
    {
        public readonly uint               PriorityID;
        public readonly PriorityImportance PriorityImportance;

        public PriorityToChange(uint priorityID, PriorityImportance priorityImportance)
        {
            PriorityID         = priorityID;
            PriorityImportance = priorityImportance;
        }
    }
}