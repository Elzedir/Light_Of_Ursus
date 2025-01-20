using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Actor;
using TickRates;
using Tools;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Data : Data_Class
    {
        Priority_Queue        _priorityQueue;
        public Priority_Queue PriorityQueue => _priorityQueue ??= _createNewPriorityQueue();

        Dictionary<PriorityImportance, Dictionary<uint, PriorityElement>> _cachedPriorityQueue = new();
        protected          ActorPriorityState                             _currentPriorityState;
        protected abstract PriorityType                                   _priorityType { get; }

        public void CriticalDataChanged(PriorityUpdateTrigger                     priorityUpdateTrigger,
                                        Dictionary<PriorityParameterName, object> newParameters)
        {
            if (!_priorityIDsToUpdateOnDataChange.TryGetValue(priorityUpdateTrigger, out var priorityIDsToUpdate))
            {
                Debug.Log(
                    $"DataChanged: {priorityUpdateTrigger} not found in _priorityIDsToUpdateOnDataChange for {this}.");
                return;
            }

            foreach (var priorityIDToUpdate in priorityIDsToUpdate)
            {
                var newPriorityValue =
                    Priority_Generator.GeneratePriority(_priorityType, priorityIDToUpdate, newParameters);

                if (!PriorityQueue.Update(priorityIDToUpdate, newPriorityValue))
                {
                    Debug.LogError($"Priority: {priorityIDToUpdate} unable to be updated in PriorityQueue.");
                }
            }
        }

        Priority_Queue _createNewPriorityQueue()
        {
            var priorityQueue = new Priority_Queue(1);
            priorityQueue.OnPriorityRemoved += _regeneratePriority;
            return priorityQueue;
        }

        public void OnDestroy()
        {
            PriorityQueue.OnPriorityRemoved -= _regeneratePriority;
        }
        public abstract    void RegenerateAllPriorities(bool includeOptionalPriorities = false);
        protected abstract void _regeneratePriority(uint priorityID);

        public PriorityElement PeekHighestSpecificPriority(List<uint> priorityIDs)
        {
            var permittedPriorities = _getPermittedPriorities(priorityIDs);

            if (permittedPriorities.Count is 0)
            {
                Debug.Log("No permitted priorities found.");
                return null;
            }

            var highestPriority = new PriorityElement(0, 0);

            foreach (var priority in permittedPriorities)
            {
                var priorityElement = PriorityQueue.Peek(priority);

                if (priorityElement is null) continue;

                if (priorityElement.PriorityValue >= highestPriority.PriorityValue)
                {
                    highestPriority = priorityElement;
                }
            }

            return PriorityQueue.Peek(highestPriority.PriorityID);
        }

        protected abstract List<uint> _getPermittedPriorities(List<uint> priorityIDs);

        protected PriorityElement _peekHighestPriority(ActorPriorityState actorPriorityState)
        {
            var overallHighestPriority = new PriorityElement(0, 0);

            var relevantPriorityQueues = _getRelevantPriorities(actorPriorityState);

            if (relevantPriorityQueues.Count is 0)
            {
                //Debug.Log("No relevant priorities found.");
                return null;
            }

            foreach (var priority in relevantPriorityQueues.Where(priority =>
                         priority.Value?.PriorityValue >= overallHighestPriority?.PriorityValue))
            {
                overallHighestPriority = priority.Value;
            }

            return overallHighestPriority;
        }

        protected abstract Dictionary<uint, PriorityElement> _getRelevantPriorities(
            ActorPriorityState actorPriorityState);

        public PriorityElement GetHighestSpecificPriority(List<uint> priorityIDs, uint priorityObjectParameterID = 0)
        {
            priorityIDs = _getRelevantPriorityIDs(priorityIDs, priorityObjectParameterID);

            var highestPriority = PeekHighestSpecificPriority(priorityIDs);

            if (highestPriority is not null) return PriorityQueue.Dequeue(highestPriority.PriorityID);

            Debug.Log("No highest priority found.");
            return null;
        }

        protected abstract List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID);

        public PriorityElement GetHighestPriority(ActorPriorityState actorPriorityState)
        {
            var highestPriority = _peekHighestPriority(actorPriorityState);

            return highestPriority != null
                ? PriorityQueue.Dequeue(highestPriority.PriorityID)
                : null;
        }

        public PriorityElement GetSpecificPriority(uint priorityID)
        {
            return PriorityQueue.Dequeue(priorityID);
        }

        protected abstract Dictionary<PriorityParameterName, object> _getPriorityParameters(uint priorityID);

        bool        _syncingCachedQueue;
        const float _timeDeferment = 1f;

        void _syncCachedPriorityQueueHigh(bool syncing = false)
        {
            foreach (var priority in from priority in _cachedPriorityQueue[PriorityImportance.High]
                                     where !PriorityQueue.Update(priority.Value.PriorityID,
                                         priority.Value.PriorityValue)
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
        
        protected DataToDisplay _convertUintIDToStringID(DataToDisplay dataToDisplay)
        {
            var regex = new Regex(@"PriorityID\((\d+)\)\s-\s(\d+)", RegexOptions.Compiled);
            
            foreach(var (key, value) in dataToDisplay.AllStringData.ToList())
            {
                if (!key.Contains("Priority Queue")) continue;
                
                foreach(var (innerKey, innerValue) in value.ToList())
                {
                    var match = regex.Match(innerKey);
                    if (!match.Success) continue;

                    var iteration = match.Groups[1].Value;
                    var priorityIDString = match.Groups[2].Value;
                    
                    if (uint.TryParse(priorityIDString, out var priorityID))
                    {
                        dataToDisplay.AllStringData[key][_getPriorityID(iteration, priorityID)] = innerValue;
                        dataToDisplay.AllStringData[key].Remove($"PriorityID({iteration}) - {priorityID}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse PriorityID from {innerKey}");
                    }
                }
            }
            
            return dataToDisplay;
        }

        protected abstract string _getPriorityID(string iteration, uint priorityID);
    }
}