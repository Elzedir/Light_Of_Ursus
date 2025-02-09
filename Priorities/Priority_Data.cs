using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ActorActions;
using Actors;
using Station;
using Tools;
using UnityEngine;

namespace Priorities
{
    [Serializable]
    public abstract class Priority_Data : Data_Class
    {
        Priority_Queue        _priorityQueue;
        public Priority_Queue PriorityQueue => _priorityQueue ??= _createNewPriorityQueue();
        
        HashSet<ActorActionName> _allowedActions;
        public HashSet<ActorActionName> AllowedActions => _allowedActions ??= _getAllowedActions();
        protected abstract HashSet<ActorActionName> _getAllowedActions();

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
        protected abstract void _regeneratePriority(ulong priorityID);
        
        //* Isn't rearranging the highest priority, Chop_Tree is staying the highest even with 0 priority.
        public Priority_Element PeekHighestPriority(ulong priorityID = 1) => PriorityQueue.Peek(priorityID);
        public Priority_Element PeekHighestPriorityFromGroup(List<ulong> priorityIDs)
        {
            var permittedPriorities = _getPermittedPriorities(priorityIDs);

            if (permittedPriorities.Count is 0)
            {
                Debug.Log("No permitted priorities found.");
                return null;
            }

            var highestPriority = new Priority_Element(0, 0, new Priority_Parameters());

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
        public Priority_Element DequeueHighestPriority(ulong priorityID = 1) => PriorityQueue.Dequeue(priorityID);
        public Priority_Element GetHighestPriorityFromGroup(List<ulong> priorityIDs, ulong priorityObjectParameterID = 0)
        {
            priorityIDs = _getRelevantPriorityIDs(priorityIDs, priorityObjectParameterID);

            var highestPriority = PeekHighestPriorityFromGroup(priorityIDs);

            if (highestPriority is not null) return PriorityQueue.Dequeue(highestPriority.PriorityID);

            Debug.Log("No highest priority found.");
            return null;
        }

        protected abstract List<ulong> _getPermittedPriorities(List<ulong> priorityIDs);
        protected abstract List<ulong> _getRelevantPriorityIDs(List<ulong> priorityIDs, ulong limiterID);

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
            var allRelevantStations =
                priority_Parameters.JobSite_Component_Source.GetRelevantStations(actorActionName);
            
            if (allRelevantStations.StationTargets.Count is 0 && allRelevantStations.StationSources.Count is 0)
            {
                priority_Parameters.AllStation_Sources ??= new List<Station_Component>();
                priority_Parameters.AllStation_Targets ??= new List<Station_Component>();
                return;
            }
            
            priority_Parameters.AllStation_Sources ??= allRelevantStations.StationSources;
            priority_Parameters.AllStation_Targets ??= allRelevantStations.StationTargets;
        }
        
        protected DataToDisplay _convertUlongIDToStringID(DataToDisplay dataToDisplay)
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
                    
                    if (ulong.TryParse(priorityIDString, out var priorityID))
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

        protected abstract string _getPriorityID(string iteration, ulong priorityID);

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
                        //ActorActionName.Wander,
                        ActorActionName.Idle
                    }
                },
                {
                    DataChangedName.ChangedInventory, new List<ActorActionName>
                    {
                        
                    }
                }
            };
        }
    }
}