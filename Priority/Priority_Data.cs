using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Actor;
using ActorActions;
using Tools;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Data : Data_Class
    {
        Priority_Queue        _priorityQueue;
        public Priority_Queue PriorityQueue => _priorityQueue ??= _createNewPriorityQueue();
        
        List<ActorActionName> _allowedActions;
        public List<ActorActionName> AllowedActions => _allowedActions ??= _getAllowedActions();
        protected abstract List<ActorActionName> _getAllowedActions();

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
        public abstract    void RegenerateAllPriorities(DataChangedName dataChangedName, bool forceRegenerateAll = false);
        protected abstract void _regeneratePriority(uint priorityID);
        
        public PriorityElement PeekHighestPriority(uint priorityID = 1) => PriorityQueue.Peek(priorityID);
        public PriorityElement PeekHighestPriorityFromGroup(List<uint> priorityIDs)
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

        public PriorityElement GetHighestPriority(uint priorityID = 1) => PriorityQueue.Dequeue(priorityID);
        public PriorityElement GetHighestPriorityFromGroup(List<uint> priorityIDs, uint priorityObjectParameterID = 0)
        {
            priorityIDs = _getRelevantPriorityIDs(priorityIDs, priorityObjectParameterID);

            var highestPriority = PeekHighestPriorityFromGroup(priorityIDs);

            if (highestPriority is not null) return PriorityQueue.Dequeue(highestPriority.PriorityID);

            Debug.Log("No highest priority found.");
            return null;
        }

        protected abstract List<uint> _getPermittedPriorities(List<uint> priorityIDs);
        protected abstract List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID);
        //protected abstract void _populatePriorityParameters(ref Priority_Parameters priorityParameters);
        
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

        Dictionary<DataChangedName, List<ActorActionName>> _actorActionsToRegenerate;
        public Dictionary<DataChangedName, List<ActorActionName>> ActorActionsToRegenerate => 
            _actorActionsToRegenerate ??= _initialiseActorActionsToRegeneratePriority();
        
        Dictionary<DataChangedName, List<ActorActionName>> _initialiseActorActionsToRegeneratePriority()
        {
            return new Dictionary<DataChangedName, List<ActorActionName>>
            {
                {
                    DataChangedName.ChangedState, new List<ActorActionName>
                    {
                        ActorActionName.Wander,
                        ActorActionName.Idle
                    }
                }
            };
        }
    }
}