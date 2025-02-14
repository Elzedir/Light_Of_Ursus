using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ActorActions;
using Actors;
using Priorities.Priority_Queues;
using Station;
using Tools;
using UnityEngine;

namespace Priorities
{
    [Serializable]
    public abstract class Priority_Data : Data_Class
    {
        Priority_Queue_MaxHeap<ActorAction_Data>        _priorityQueueMaxHeap;
        public Priority_Queue_MaxHeap<ActorAction_Data> PriorityQueueMaxHeap => _priorityQueueMaxHeap ??= _createNewPriorityQueue();
        
        HashSet<ActorActionName> _allowedActions;
        public HashSet<ActorActionName> AllowedActions => _allowedActions ??= _getAllowedActions();
        protected abstract HashSet<ActorActionName> _getAllowedActions();

        Priority_Queue_MaxHeap<ActorAction_Data> _createNewPriorityQueue()
        {
            var priorityQueue = new Priority_Queue_MaxHeap<ActorAction_Data>(1);
            priorityQueue.OnPriorityRemoved += _regeneratePriority;
            return priorityQueue;
        }

        public void OnDestroy()
        {
            PriorityQueueMaxHeap.OnPriorityRemoved -= _regeneratePriority;
        }
        public abstract    void RegenerateAllPriorities(DataChangedName dataChangedName, bool forceRegenerateAll = false);
        protected abstract void _regeneratePriority(long priorityID);
        
        //* Isn't rearranging the highest priority, Chop_Tree is staying the highest even with 0 priority.
        public Priority_Element<ActorAction_Data> PeekHighestPriority(long priorityID = 1) => PriorityQueueMaxHeap.Peek(priorityID);
        public Priority_Element<ActorAction_Data> PeekHighestPriorityFromGroup(List<long> priorityIDs)
        {
            if (priorityIDs.Count is 0)
            {
                Debug.Log("No permitted priorities found.");
                return null;
            }

            var highestPriority = new Priority_Element<ActorAction_Data>(0, 0);

            foreach (var priority in priorityIDs)
            {
                var priorityElement = PriorityQueueMaxHeap.Peek(priority);

                if (priorityElement is null) continue;

                if (priorityElement.PriorityValue >= highestPriority.PriorityValue)
                {
                    highestPriority = priorityElement;
                }
            }

            return PriorityQueueMaxHeap.Peek(highestPriority.PriorityID);
        }
        public Priority_Element<ActorAction_Data> DequeueHighestPriority(long priorityID = 1) => PriorityQueueMaxHeap.Dequeue(priorityID);
        public Priority_Element<ActorAction_Data> GetHighestPriorityFromGroup(List<long> priorityIDs)
        {
            var highestPriority = PeekHighestPriorityFromGroup(priorityIDs);

            if (highestPriority is not null) return PriorityQueueMaxHeap.Dequeue(highestPriority.PriorityID);

            Debug.Log("No highest priority found.");
            return null;
        }

        protected Priority_Parameters _getPriorityParameters(ActorActionName actorActionName)
        {
            var priorityParameters = new Priority_Parameters();

            _setActorID_Source(priorityParameters);
            _setJobSiteID_Source(priorityParameters);
            _setStationID_Source(priorityParameters);
            
            _setActorID_Target(priorityParameters);
            _setJobSiteID_Target(priorityParameters);
            
            _setAllStationIDs(actorActionName, priorityParameters);

            return priorityParameters;
        }
        
        protected abstract void _setActorID_Source(Priority_Parameters priority_Parameters);
        protected abstract void _setActorID_Target(Priority_Parameters priority_Parameters);
        protected abstract void _setJobSiteID_Source(Priority_Parameters priority_Parameters);
        protected abstract void _setJobSiteID_Target(Priority_Parameters priority_Parameters);
        protected virtual void _setStationID_Source(Priority_Parameters priority_Parameters) { }

        protected void _setAllStationIDs(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            priority_Parameters.JobSite_Component_Source.GetRelevantStations(actorActionName, priority_Parameters);
        }
        
        protected DataToDisplay _convertUlongIDToStringID(DataToDisplay dataToDisplay)
        {
            var regex = new Regex(@"PriorityID\((\d+)\)\s-\s(\d+)", RegexOptions.Compiled);
    
            foreach (var (key, value) in dataToDisplay.AllStringData.ToList())
            {
                if (!key.Contains("Priority Queue")) continue;
                
                //= Using a temporary dictionary to keep the order.

                var updates = new Dictionary<string, string>();
                var keysToRemove = new List<string>();
        
                foreach (var (innerKey, innerValue) in value)
                {
                    var match = regex.Match(innerKey);
                    if (!match.Success) continue;

                    var iteration = match.Groups[1].Value;
                    var priorityIDString = match.Groups[2].Value;

                    if (ulong.TryParse(priorityIDString, out var priorityID))
                    {
                        var newKey = _getPriorityID(iteration, priorityID);
                        updates[newKey] = innerValue;
                        keysToRemove.Add(innerKey);
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse PriorityID from {innerKey}");
                    }
                }
                
                foreach (var (newKey, newValue) in updates)
                {
                    dataToDisplay.AllStringData[key][newKey] = newValue;
                }
                
                foreach (var oldKey in keysToRemove)
                {
                    dataToDisplay.AllStringData[key].Remove(oldKey);
                }
            }
            
            return dataToDisplay;
        }

        protected abstract string _getPriorityID(string iteration, ulong priorityID);

        Dictionary<DataChangedName, List<ActorActionName>> _actorActionsToRegenerate;
        public Dictionary<DataChangedName, List<ActorActionName>> ActorActionsToRegenerate => 
            _actorActionsToRegenerate ??= _initialiseActorActionsToRegeneratePriority();
        
        Dictionary<DataChangedName, List<ActorActionName>> _initialiseActorActionsToRegeneratePriority()
        {
            return new Dictionary<DataChangedName, List<ActorActionName>>
            {
                {
                    DataChangedName.ChangedState, new List<ActorActionName>()
                },
                {
                    DataChangedName.ChangedInventory, new List<ActorActionName>()
                }
            };
        }
    }
}