using System.Collections.Generic;
using System.Linq;
using Actor;
using Managers;
using Tools;
using UnityEngine;

namespace Priority
{
    public abstract class PriorityComponent
    {
        PriorityQueue _priorityQueue;
        public PriorityQueue PriorityQueue => _priorityQueue ??= _createNewPriorityQueue();

        Dictionary<PriorityImportance, Dictionary<uint, PriorityElement>> _cachedPriorityQueue = new();
        protected          ActorPriorityState                             _currentPriorityState;
        protected abstract PriorityType                                   _priorityType { get; }

        public void CriticalDataChanged(PriorityUpdateTrigger                               priorityUpdateTrigger,
                                           Dictionary<PriorityParameterName, object> newParameters)
        {
            if (!_priorityIDsToUpdateOnDataChange.TryGetValue(priorityUpdateTrigger, out var priorityIDsToUpdate))
            {
                Debug.Log($"DataChanged: {priorityUpdateTrigger} not found in _priorityIDsToUpdateOnDataChange for {this}.");
                return;
            }

            foreach (var priorityIDToUpdate in priorityIDsToUpdate)
            {
                var newPriorityValue = PriorityGenerator.GeneratePriority(_priorityType, priorityIDToUpdate, newParameters);
                
                if (!PriorityQueue.Update(priorityIDToUpdate, newPriorityValue))
                {
                    Debug.LogError($"Priority: {priorityIDToUpdate} unable to be updated in PriorityQueue.");
                }
            }
        }

        PriorityQueue _createNewPriorityQueue()
        {
            var priorityQueue = new PriorityQueue(1);
            priorityQueue.OnPriorityRemoved += _regeneratePriority;
            return priorityQueue;
        }
        
        public void OnDestroy()
        {
            PriorityQueue.OnPriorityRemoved -= _regeneratePriority;
        }

        protected abstract void _regeneratePriority(uint priorityID);

        public PriorityElement PeekHighestSpecificPriority(List<uint> priorityIDs)
        {
            var permittedPriorities = _canPeek(priorityIDs);
            
            if (permittedPriorities.Count is 0)
            {
                Debug.Log("No permitted priorities found.");
                return null;
            }

            var highestPriority = new PriorityElement(0, 0); 

            foreach (var priority in permittedPriorities)
            {
                var priorityElement = PriorityQueue.Peek(priority);
                
                if (priorityElement.PriorityValue >= highestPriority.PriorityValue)
                {
                    highestPriority = priorityElement;
                }
            }
            
            return PriorityQueue.Dequeue(highestPriority.PriorityID);
        }

        protected abstract List<uint> _canPeek(List<uint> priorityIDs);

        public PriorityElement PeekHighestPriority(ActorPriorityState actorPriorityState)
        {
            var overallHighestPriority = new PriorityElement(0, 0);

            var relevantPriorityQueues = _getRelevantPriorities(actorPriorityState);
            
            if (relevantPriorityQueues.Count is 0)
            {
                //Debug.Log("No relevant priorities found.");
                return null;
            }

            foreach (var priority in relevantPriorityQueues.Where(priority =>
                         priority.Value.PriorityValue >= overallHighestPriority.PriorityValue))
            {
                overallHighestPriority = priority.Value;
            }

            return overallHighestPriority.PriorityID != (uint)ActorActionName.Idle
                ? overallHighestPriority
                : null;
        }

        protected abstract Dictionary<uint, PriorityElement> _getRelevantPriorities(ActorPriorityState actorPriorityState);

        public PriorityElement GetHighestSpecificPriority(List<uint> priorityIDs, uint priorityObjectParameterID = 0)
        {
            priorityIDs = _getRelevantPriorityIDs(priorityIDs, priorityObjectParameterID); 
                
            var highestPriority = PeekHighestSpecificPriority(priorityIDs);

            return PriorityQueue.Dequeue(highestPriority.PriorityID);
        }

        protected abstract List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID);
        
        public PriorityElement GetHighestPriority(ActorPriorityState actorPriorityState)
        {
            var highestPriority = PeekHighestPriority(actorPriorityState);

            return highestPriority != null
                ? PriorityQueue.Dequeue(highestPriority.PriorityID)
                : null;
        }
        
        public PriorityElement GetSpecificPriority(uint priorityID)
        {
            return PriorityQueue.Dequeue(priorityID);
        }

        protected bool _priorityExists(uint priorityID)
        {
            if (PriorityQueue.Peek(priorityID) is not null)
            {
                return true;
            }

            Debug.LogError($"PriorityID: {priorityID} not found in _existingParameters.");
            return false;
        }
        
        protected abstract Dictionary<PriorityParameterName, object> _getPriorityParameters(uint priorityID, Dictionary<PriorityParameterName, object> requiredParameters);

        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        void _syncCachedPriorityQueueHigh(bool syncing = false)
        {
            foreach (var priority in from priority in _cachedPriorityQueue[PriorityImportance.High]
                                     where !PriorityQueue.Update(priority.Value.PriorityID, priority.Value.PriorityValue)
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
            if (!_cachedPriorityQueue.ContainsKey(priorityImportance))
                _cachedPriorityQueue.Add(priorityImportance, new Dictionary<uint, PriorityElement>());

            var priorityElements = _cachedPriorityQueue[priorityImportance]; 
            
            if (!priorityElements.TryAdd(priorityElement.PriorityID, priorityElement))
            {
                priorityElements[priorityElement.PriorityID].UpdatePriority(priorityElement.PriorityValue);
            }
            
            if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
        }

        protected abstract Dictionary<PriorityUpdateTrigger, List<uint>> _priorityIDsToUpdateOnDataChange { get; }
    }
}