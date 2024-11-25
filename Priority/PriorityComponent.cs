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

        Dictionary<PriorityImportance, List<PriorityValue>>      _cachedPriorityQueue;

        public abstract void OnDataChanged(DataChanged                               dataChanged,
                                           Dictionary<PriorityParameterName, object> changedParameters);

        public void UpdatePriority(uint priorityID, List<float> priorities)
        {
            if (!AllPriorities[priorityID].Update(priorityID, priorities))
            {
                Debug.LogError($"ActionName: {priorityID} unable to be updated in PriorityQueue.");
            }
        }

        public void RemovePriority(uint priorityID)
        {
            if (!AllPriorities[priorityID].Remove(priorityID))
            {
                Debug.LogError($"ActionName: {priorityID} unable to be removed from PriorityQueue.");
            }
        }

        PriorityValue _peekHighestSpecificPriority(uint priorityID)
        {
            return _canPeek(priorityID) ? AllPriorities[priorityID].Peek() : null;
        }

        protected abstract bool _canPeek(uint priorityID);
        
        public PriorityValue PeekHighestPriority(PriorityState priorityState)
        {
            var overallHighestPriority = new PriorityValue((uint)ActorActionName.Idle, new List<float>());

            if (AllPriorities.Count is 0)
            {
                Debug.LogWarning("No priority queues found.");
                return new PriorityValue((uint)ActorActionName.Idle, new List<float>());
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

        public PriorityValue GetHighestSpecificPriority(uint priorityID)
        {
            var highestPriority = _peekHighestSpecificPriority(priorityID);

            return highestPriority != null ? AllPriorities[priorityID].Dequeue(highestPriority.PriorityID) : null;            
        }
        
        public PriorityValue GetHighestPriority(PriorityState priorityState)
        {
            var highestPriority = PeekHighestPriority(priorityState);

            return highestPriority != null ? AllPriorities[highestPriority.PriorityID]
                .Dequeue(highestPriority.PriorityID) : null;
        }

        protected Dictionary<PriorityParameterName, object> _updateExistingPriorityParameters(
            uint priorityID, Dictionary<PriorityParameterName, object> parameters)
        {
            if (!_priorityExists(priorityID, out var existingPriorityParameters))
            {
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
        
        protected abstract bool _priorityExists(uint priorityID, out Dictionary<PriorityParameterName, object> existingPriorityParameters);
        
        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        void _syncCachedPriorityQueueHigh(bool syncing = false)
        {
            foreach (var priority in from priorityQueue in AllPriorities.Values 
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
        
        protected void _addToCachedPriorityQueue(PriorityValue priorityValue, PriorityImportance priorityImportance)
        {
            _cachedPriorityQueue ??= new Dictionary<PriorityImportance, List<PriorityValue>>();
        
            if (!_cachedPriorityQueue.ContainsKey(priorityImportance)) _cachedPriorityQueue.Add(priorityImportance, new List<PriorityValue>());
        
            _cachedPriorityQueue[priorityImportance].Add(priorityValue);
        
            if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
        }
    }
}