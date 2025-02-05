using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ActorActions;
using Actors;
using Items;
using Priority;
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
        
        public PriorityElement PeekHighestPriority(ulong priorityID = 1) => PriorityQueue.Peek(priorityID);
        public PriorityElement PeekHighestPriorityFromGroup(List<ulong> priorityIDs)
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
        public PriorityElement GetHighestPriority(ulong priorityID = 1) => PriorityQueue.Dequeue(priorityID);
        public PriorityElement GetHighestPriorityFromGroup(List<ulong> priorityIDs, ulong priorityObjectParameterID = 0)
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
            _setStationID_Source(actorActionName, priorityParameters);
            
            _setActorID_Target(actorActionName, priorityParameters);
            _setJobSiteID_Target(actorActionName, priorityParameters);
            _setStationID_Target(actorActionName, priorityParameters);

            return priorityParameters;
        }
        
        protected abstract void _setActorID_Source(Priority_Parameters priority_Parameters);
        protected abstract void _setActorID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters);
        protected abstract void _setJobSiteID_Source(Priority_Parameters priority_Parameters);
        protected abstract void _setJobSiteID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters);
        protected abstract void _setStationID_Source(ActorActionName actorActionName, Priority_Parameters priority_Parameters);

        void _setStationID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            _setHighestPriorityStation(actorActionName, priority_Parameters, false);
        }

        protected void _setHighestPriorityStation(ActorActionName actorActionName, Priority_Parameters priority_Parameters, bool isStation_Source)
        {
            //* Recheck this allocation
            
            var allRelevantStations =
                priority_Parameters.JobSite_Component_Source.GetRelevantStations(actorActionName, isStation_Source);
            
            if (allRelevantStations.Count is 0)
            {
                // Debug.LogError(isStation_Source 
                //     ? $"No relevant station_Source for {actorActionName}."
                //     : $"No relevant station_Target for {actorActionName}.");
                
                _setStationID(priority_Parameters, isStation_Source, 0);
                return;
            }

            priority_Parameters.TotalItems = allRelevantStations.Sum(station =>
                (int)Item.GetItemListTotal_CountAllItems(station.GetInventoryItems(actorActionName)));

            float highestPriorityValue = 0;
            ulong highestPriorityStationID = 0;

            foreach (var station in allRelevantStations)
            {
                _setStationID(priority_Parameters, isStation_Source, station.StationID);
                
                var stationPriority =
                    Priority_Generator.GeneratePriority((ulong)actorActionName, priority_Parameters);

                if (stationPriority is 0 || stationPriority < highestPriorityValue) continue;

                highestPriorityValue = stationPriority;
                highestPriorityStationID = station.StationID;
            }

            if (highestPriorityStationID is 0)
            {
                // Debug.LogError(isStation_Source 
                //     ? $"All station_Sources ({allRelevantStations.Count}) have 0 priority for action: {actorActionName}."
                //     : $"All station_Targets ({allRelevantStations.Count}) have 0 priority for action: {actorActionName}.");
                
                _setStationID(priority_Parameters, isStation_Source, 0);
                return;
            }
            
            _setStationID(priority_Parameters, isStation_Source, highestPriorityStationID);
        }
        
        void _setStationID(Priority_Parameters priority_Parameters, bool isStation_Source, ulong stationID)
        {
            if (isStation_Source)
                priority_Parameters.StationID_Source = stationID;
            else
                priority_Parameters.StationID_Target = stationID;
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