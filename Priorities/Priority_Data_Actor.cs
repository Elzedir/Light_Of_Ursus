using System;
using System.Collections.Generic;
using Actor;
using ActorActions;
using Actors;
using Priority;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Priorities
{
    [Serializable]
    public class Priority_Data_Actor : Priority_Data
    {
        public Priority_Element CurrentAction;
        
        public override void RegenerateAllPriorities(DataChangedName dataChangedName, bool forceRegenerateAll = false)
        {
            if (!forceRegenerateAll)
            {
                if (dataChangedName == DataChangedName.None
                    || !ActorActionsToRegenerate.TryGetValue(dataChangedName, out var actionsToRegenerate))
                {
                    if (dataChangedName != DataChangedName.None)
                        Debug.LogError(
                            $"DataChangedName: {dataChangedName} not found in _priorityIDsToUpdateOnDataChange.");
                    
                    foreach (var actorAction in AllowedActions)
                    {
                        _regeneratePriority((ulong)actorAction);
                    }

                    return;
                }
                
                foreach (var actorAction in actionsToRegenerate)
                {
                    _regeneratePriority((ulong)actorAction);
                }

                return;
            }
            
            foreach (ActorActionName actorAction in Enum.GetValues(typeof(ActorActionName)))
            {
                _regeneratePriority((ulong)actorAction);
            }
        }

        protected override void _regeneratePriority(ulong priorityID)
        {
            switch (priorityID)
            {
                case (ulong)ActorActionName.All:
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in _regeneratePriority.");
                    return;
                case (ulong)ActorActionName.Idle:
                    PriorityQueue.Update(priorityID, 1, new Priority_Parameters { DefaultPriorityValue = 1});
                    return;
            }

            var priorityParameters = _getPriorityParameters((ActorActionName)priorityID);

            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            PriorityQueue.Update(priorityID, priorityValue, priorityParameters);
        }

        protected override void _setActorID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.ActorID_Source = ActorID;
        }

        protected override void _setJobSiteID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.JobSiteID_Source = _actor.ActorData.Career.JobSiteID;
        }

        protected override void _setStationID_Source(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            priority_Parameters.StationID_Source = _actor.ActorData.Career.JobSite?.JobSite_Data.GetActorJob(ActorID)?.StationID ?? 0;
        }

        protected override void _setActorID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            priority_Parameters.ActorID_Target = 0;
        }

        protected override void _setJobSiteID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            priority_Parameters.JobSiteID_Target = 0;
        }

        readonly ComponentReference_Actor _actorReferences;

        public    ulong           ActorID => _actorReferences.ActorID;
        protected Actor_Component _actor  => _actorReferences.Actor_Component;

        public Priority_Data_Actor(ulong actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);

            //AllPriorities.DictionaryChanged += SetCurrentAction;
        }

        protected override List<ulong> _getPermittedPriorities(List<ulong> priorityIDs)
        {
            var allowedPriorities = new List<ulong>();

            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (ulong)ActorActionName.All or (ulong)ActorActionName.Idle)
                {
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
                    continue;
                }

                allowedPriorities.Add(priorityID);
            }

            return allowedPriorities;
        }

        protected override List<ulong> _getRelevantPriorityIDs(List<ulong> priorityIDs, ulong limiterID)
        {
            return priorityIDs;
        }

        public void MakeDecision()
        {
            // Change tick rate according to number of zones to player.
            // Local region is same zone.
            // Regional region is within 1 zone distance.
            // Distant region is 2+ zones.
            
            //* Change the regeneration of priorities here to be more efficient, but for now, always regenerate.
            RegenerateAllPriorities(DataChangedName.None);
            
            var nextHighestPriorityValue = PeekHighestPriority();

            if (nextHighestPriorityValue is null)
            {
                Debug.LogError("No next highest priority value.");
                return;
            }

            if (CurrentAction != null && nextHighestPriorityValue.PriorityID == CurrentAction.PriorityID)
                return;

            CurrentAction = DequeueHighestPriority();
        }
        
        protected override HashSet<ActorActionName> _getAllowedActions() => 
            _actorReferences.Actor_Component.ActorData.GetAllowedActions();

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor ID", $"{ActorID}" },
                { "Current Actor Action", CurrentAction?.PriorityID != null 
                    ? $"{ (ActorActionName)CurrentAction.PriorityID}({CurrentAction.PriorityID})" 
                    : "No current actor action" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Priority Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Priority Queue",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: _convertUlongIDToStringID(PriorityQueue?.GetDataToDisplay(toggleMissingDataDebugs)));

            return DataToDisplay;
        }
        
        protected override string _getPriorityID(string iteration, ulong priorityID) => 
            $"PriorityID({iteration}) - {(ActorActionName)priorityID}";
    }
}