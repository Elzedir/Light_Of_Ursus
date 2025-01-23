using System;
using System.Collections.Generic;
using Actor;
using ActorActions;
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
                //* Currently just refreshing, which would mean we would have to clear and repopulate the dictionary. Eventually, make a hybrid
                //* system where we update the actions based on StateChanges, as well as regenerations. Or make regenerations less common.
                //* But first, get regenerations working completely.
                
                if (dataChangedName == DataChangedName.None
                    || !_priorityIDsToUpdateOnDataChange.TryGetValue(dataChangedName, out var priorityIDs))
                {
                    if (dataChangedName != DataChangedName.None)
                        Debug.LogError(
                            $"DataChangedName: {dataChangedName} not found in _priorityIDsToUpdateOnDataChange.");
                    
                    foreach (var actorAction in AllowedActions)
                    {
                        _regeneratePriority((uint)actorAction);
                    }

                    return;
                }
                
                foreach (var priorityID in priorityIDs)
                {
                    _regeneratePriority(priorityID);
                }

                return;
            }
            
            foreach (ActorActionName actorAction in Enum.GetValues(typeof(ActorActionName)))
            {
                _regeneratePriority((uint)actorAction);
            }
        }

        protected override void _regeneratePriority(uint priorityID)
        {
            switch (priorityID)
            {
                case (uint)ActorActionName.All:
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in _regeneratePriority.");
                    return;
                case (uint)ActorActionName.Idle:
                    PriorityQueue.Update(priorityID, 1);
                    return;
            }
            
            //* Need to get priority parameters somehow.
            
            priorityParameters.ActorID_Source= _actor.ActorID; //* Maybe change later to something earlier?
            
            //_populatePriorityParameters(ref priorityParameters);

            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            PriorityQueue.Update(priorityID, priorityValue);
        }

        // protected override void _populatePriorityParameters(ref Priority_Parameters priorityParameters)
        // {
        //     //* PriorityParameterName.Target_Component => find a way to see which target we'd be talking about.
        //     priorityParameters.JobSiteID_Source = _actor.ActorData.Career.JobSite.JobSiteID;
        //     priorityParameters.ActorID_Source = _actor.ActorID;
        // }

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

        public    uint           ActorID => _actorReferences.ActorID;
        protected Actor_Component _actor  => _actorReferences.Actor_Component;

        public Priority_Data_Actor(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);

            //AllPriorities.DictionaryChanged += SetCurrentAction;
        }

        protected override List<uint> _getPermittedPriorities(List<uint> priorityIDs)
        {
            var allowedPriorities = new List<uint>();

            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (uint)ActorActionName.All or (uint)ActorActionName.Idle)
                {
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
                    continue;
                }

                allowedPriorities.Add(priorityID);
            }

            return allowedPriorities;
        }

        protected override List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID)
        {
            return priorityIDs;
        }
        
        //* Check if this is valid in the current system.
        protected override Dictionary<DataChangedName, List<uint>> _priorityIDsToUpdateOnDataChange { get; } = new()
        {
            {
                DataChangedName.ChangedInventory, new List<uint>()
            },

            {
                DataChangedName.DroppedItems, new List<uint>
                {
                    (uint)ActorActionName.Scavenge
                }
            },

            {
                DataChangedName.PriorityCompleted, new List<uint>
                {
                    (uint)ActorActionName.Wander,
                }
            },
        };

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

            if (nextHighestPriorityValue.PriorityID == (uint)_currentAction.ActionName)
                return;

            SetCurrentAction((ActorActionName)nextHighestPriorityValue.PriorityID);
        }

        PriorityElement _getNextHighestPriorityValue()
        {
            //* Do we need to regenerate all priorities here? Eventually detach priority regeneration from priority getting.
            RegenerateAllPriorities();

            return PeekHighestPriority();
        }
        
        protected override List<ActorActionName> _getAllowedActions() => 
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
                allSubData: _convertUintIDToStringID(PriorityQueue?.GetDataToDisplay(toggleMissingDataDebugs)));

            return DataToDisplay;
        }
        
        protected override string _getPriorityID(string iteration, uint priorityID) => 
            $"PriorityID({iteration}) - {(ActorActionName)priorityID}";
    }
}