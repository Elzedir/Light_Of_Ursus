using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Tools;
using UnityEngine;

namespace Priority
{
    public class Priority_Data_Actor : Priority_Data
    {
        public Coroutine                CurrentActionCoroutine { get; private set; }
        public bool                     IsPerformingAction     => CurrentActionCoroutine != null;
        ActorAction_Master                     _currentActorAction;
        public             ActorAction_Master  GetCurrentAction() => _currentActorAction;
        protected override PriorityType _priorityType      => PriorityType.ActorAction;

        public void SetCurrentAction(uint actorActionName)
        {
            _stopCurrentAction();
            
            Debug.Log($"Setting current action to: {(ActorActionName)actorActionName}.");
            
            var actorAction = ActorAction_Manager.GetActorAction_Master((ActorActionName)actorActionName);

            _currentActorAction = actorAction;

            _actor.StartCoroutine(_performCurrentActionFromStart());
        }
        
        public override void RegenerateAllPriorities(bool includeOptionalPriorities = false)
        {
            if (!includeOptionalPriorities)
            {
                foreach (var actorAction in Priority_Manager.BasePriorityActorActions)
                {
                    _regeneratePriority((uint)actorAction);
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
            
            var priorityParameters = _getPriorityParameters(priorityID);

            var priorityValue = Priority_Generator.GeneratePriority(_priorityType, priorityID, priorityParameters);
            
            Debug.Log($"PriorityID: {priorityID}, PriorityValue: {priorityValue}");

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override Dictionary<PriorityParameterName, object> _getPriorityParameters(uint priorityID)
        {
            var actorAction_Master = ActorAction_Manager.GetActorAction_Master((ActorActionName)priorityID);
            var parameters = actorAction_Master.RequiredParameters.ToDictionary(parameter => parameter, _getParameter);

            return ActorAction_Manager.PopulateActionParameters((ActorActionName)priorityID, parameters);
        }

        object _getParameter(PriorityParameterName parameter)
        {
            return parameter switch
            {
                // PriorityParameterName.Target_Component => find a way to see which target we'd be talking about.
                PriorityParameterName.Jobsite_Component => _actor.ActorData.CareerDataPreset.JobSite,
                PriorityParameterName.Worker_Component => _actor,
                _ => null
            };
        }

        IEnumerator _performCurrentActionFromStart()
        {
            foreach (var action in _currentActorAction.ActionList)
            {
                yield return CurrentActionCoroutine = _actor.StartCoroutine(action);
            }
        }

        void _stopCurrentAction()
        {
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
        
        protected override Dictionary<uint, PriorityElement> _getRelevantPriorities(ActorPriorityState actorPriorityState)
        {
            if (actorPriorityState is ActorPriorityState.None)
            {
                var allPriorities = PriorityQueue.PeekAll();

                allPriorities ??= Array.Empty<PriorityElement>();
                
                return allPriorities.ToDictionary(priority => priority.PriorityID);
            }

            var relevantPriorities = new Dictionary<uint, PriorityElement>();

            switch (actorPriorityState)
            {
                case ActorPriorityState.InCombat:
                    foreach (var actorActionName in ActorBehaviour_Manager.GetActorActionsOfActorBehaviour(ActorBehaviourName.Combat))
                    {
                        relevantPriorities.Add((uint)actorActionName, PriorityQueue.Peek((uint)actorActionName));
                    }

                    break;
                case ActorPriorityState.HasJob:
                    foreach (var actorActionName in ActorBehaviour_Manager.GetActorActionsOfActorBehaviour(ActorBehaviourName.Work))
                    {
                        relevantPriorities.Add((uint)actorActionName, PriorityQueue.Peek((uint)actorActionName));
                    }

                    break;
                case ActorPriorityState.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actorPriorityState), actorPriorityState, null);
            }

            return relevantPriorities;
        }
        
        protected override Dictionary<PriorityUpdateTrigger, List<uint>> _priorityIDsToUpdateOnDataChange { get; } = new()
        {
            {
                PriorityUpdateTrigger.ChangedInventory, new List<uint>
                {
                    (uint)ActorActionName.Perform_JobTask
                }
            },

            {
                PriorityUpdateTrigger.DroppedItems, new List<uint>
                {
                    (uint)ActorActionName.Scavenge
                }
            },

            {
                PriorityUpdateTrigger.PriorityCompleted, new List<uint>
                {
                    (uint)ActorActionName.Wander,
                }
            },
        };

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Priority Data Actor",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    subData: new Dictionary<string, Data_Display>(),
                    firstData: true);

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Base Priority Data", out var basePriorityData))
                {
                    dataSO_Object.SubData["Base Priority Data"] = new Data_Display(
                        title: "Base Priority Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (basePriorityData is not null)
                {
                    basePriorityData.Data = new Dictionary<string, string>
                    {
                        { "Actor ID", $"{ActorID}" },
                        { "Actor Action", $"{_currentActorAction}" },
                        { "Is Performing Action", $"{IsPerformingAction}" },
                        { "Current Action Coroutine", $"{CurrentActionCoroutine}" }
                    };
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in Base Priority Data");
                }
            }

            return dataSO_Object;
        }
    }
}