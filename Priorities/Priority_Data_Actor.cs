using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using Actors;
using Items;
using Tools;
using UnityEngine;

namespace Priority
{
    [Serializable]
    public class Priority_Data_Actor : Priority_Data
    {
        public Coroutine                CurrentActionCoroutine { get; private set; }
        public bool                     IsPerformingAction     => CurrentActionCoroutine != null;
        [SerializeField] ActorAction_Data _currentAction;

        public void SetCurrentAction(ActorActionName actorActionName)
        {
            _stopCurrentAction();
            
            var actorAction = ActorAction_Manager.GetActorAction_Data(actorActionName);

            _currentAction = actorAction;

            // _actor.StartCoroutine(_performCurrentActionFromStart());
        }
        
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
                    PriorityQueue.Update(priorityID, 1);
                    return;
            }

            var priorityParameters = _getPriorityParameters((ActorActionName)priorityID);

            Debug.Log($"PriorityID: {(ActorActionName)priorityID}");

            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            Debug.Log($"PriorityID: {(ActorActionName)priorityID} - {priorityValue}");

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override Priority_Parameters _getPriorityParameters(ActorActionName actorActionName)
        {
            var priorityParameters = new Priority_Parameters();

            _setActorID_Source(priorityParameters);
            _setJobSiteID_Source(priorityParameters);
            _setStationID_Source(priorityParameters);
            
            _setActorID_Target(actorActionName, priorityParameters);
            _setJobSiteID_Target(actorActionName, priorityParameters);
            _setStationID_Target(actorActionName, priorityParameters);

            return priorityParameters;
        }

        protected override void _setActorID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.ActorID_Source = ActorID;
        }

        protected override void _setJobSiteID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.JobSiteID_Source = _actor.ActorData.Career.JobSiteID;
        }

        protected override void _setStationID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.StationID_Source = _actor.ActorData.Career.JobSite?.JobSiteData.GetStationIDFromWorkerID(ActorID) ?? 0;
        }

        protected override void _setActorID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            throw new NotImplementedException("ActorID_Target not implemented for GetActor_Target.");
        }

        protected override void _setJobSiteID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            throw new NotImplementedException("JobSiteID_Target not implemented for GetJobSite_Target.");
        }

        protected override void _setStationID_Target(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            var allRelevantStations = priority_Parameters.JobSite_Component_Source.GetRelevantStations(actorActionName);

            if (priority_Parameters.ActorID_Source is not 0 
                && !priority_Parameters.Actor_Component_Source.ActorData.Career.HasCurrentJob())
            {
                allRelevantStations = allRelevantStations
                    .Where(station => station.Station_Data.GetOpenWorkPost() is not null).ToList();
            }

            if (allRelevantStations.Count is 0)
            {
                Debug.LogError($"No relevant station for {actorActionName}.");
                priority_Parameters.StationID_Target = 0;
                return;
            }

            priority_Parameters.TotalItems = allRelevantStations.Sum(station =>
                (int)Item.GetItemListTotal_CountAllItems(station.GetInventoryItems(actorActionName)));

            if (priority_Parameters.TotalItems is 0)
            {
                Debug.LogError("No items to fetch from all stations.");
                priority_Parameters.StationID_Target = 0;
                return;
            }

            float highestPriorityValue = 0;
            ulong highestPriorityStationID = 0;

            foreach (var station in allRelevantStations)
            {
                priority_Parameters.StationID_Target = station.StationID;
                var stationPriority =
                    Priority_Generator.GeneratePriority((ulong)actorActionName, priority_Parameters);

                if (stationPriority is 0 || stationPriority < highestPriorityValue) continue;

                highestPriorityValue = stationPriority;
                highestPriorityStationID = station.StationID;
            }

            if (highestPriorityStationID is 0)
            {
                Debug.LogWarning("No station with items to fetch from.");
                priority_Parameters.StationID_Target = 0;
                return;
            }

            priority_Parameters.StationID_Target = highestPriorityStationID;
        }

        // IEnumerator _performCurrentActionFromStart()
        // {
        //     foreach (var action in _currentAction.ActionList)
        //     {
        //         yield return CurrentActionCoroutine = _actor.StartCoroutine(action(_currentAction.ActorAction_Parameters));
        //     }
        //     
        //     _stopCurrentAction();
        //     _currentAction = null;
        // }

        void _stopCurrentAction()
        {
            if (CurrentActionCoroutine == null) return;
            
            _actor.StopCoroutine(CurrentActionCoroutine);
            CurrentActionCoroutine = null;
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

            var nextHighestPriorityValue = _getNextHighestPriorityValue();

            if (nextHighestPriorityValue is null)
            {
                Debug.LogError("No next highest priority value.");
                return;
            }

            if (_currentAction != null && nextHighestPriorityValue.PriorityID == (ulong)_currentAction.ActionName)
                return;
            
            Debug.Log($"Actor: {ActorID} performing action: {(ActorActionName)nextHighestPriorityValue.PriorityID}.");

            SetCurrentAction((ActorActionName)nextHighestPriorityValue.PriorityID);
        }

        PriorityElement _getNextHighestPriorityValue()
        {
            //* Do we need to regenerate all priorities here? Eventually detach priority regeneration from priority getting.
            RegenerateAllPriorities(DataChangedName.None);

            return PeekHighestPriority();
        }
        
        protected override HashSet<ActorActionName> _getAllowedActions() => 
            _actorReferences.Actor_Component.ActorData.GetAllowedActions();

        public override Dictionary<string, string> GetStringData()
        {
            var highestPriority = PeekHighestPriority();
            
            return new Dictionary<string, string>
            {
                { "Actor ID", $"{ActorID}" },
                { "Current Actor Action", $"{_currentAction?.ActionName}" },
                { "Is Performing Current Action", $"{IsPerformingAction}" },
                { "Current Action Coroutine", $"{CurrentActionCoroutine}" },
                { "Next Highest Priority", highestPriority?.PriorityID != null 
                    ? $"{(ActorActionName)highestPriority.PriorityID}({highestPriority.PriorityID}) - {highestPriority.PriorityValue}" 
                    : "No Highest Priority" }
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