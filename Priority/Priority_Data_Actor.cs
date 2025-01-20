using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Managers;
using StateAndCondition;
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
                PriorityParameterName.Jobsite_Component => _actor.ActorData.Career.JobSite,
                PriorityParameterName.Worker_Component => _actor,
                _ => null
            };
        }

        IEnumerator _performCurrentActionFromStart()
        {
            a
            //* Now ,we need to connect the actor Actions to the JobTasks. Either we need to consolidate JobTasks and ActorActions
            // so that they're the same, or we need to create a way to connect them. Try make them the same. But either way, connect to
            // performJobTask ActorAction.
            foreach (var action in _currentActorAction.ActionList)
            {
                yield return CurrentActionCoroutine = _actor.StartCoroutine(action);
            }
        }

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
        
        public void MakeDecision()
        {
            // Change tick rate according to number of zones to player.
            // Local region is same zone.
            // Regional region is within 1 zone distance.
            // Distant region is 2+ zones.

            var priorityState = _getPriorityState();

            if (!_mustChangeCurrentAction(priorityState, out var nextHighestPriorityValue))
            {
                //Debug.Log("No need to change current action.");
                return;
            }

            SetCurrentAction(nextHighestPriorityValue.PriorityID);
        }

        ActorPriorityState _getPriorityState()
        {
            var actorData = _actorReferences.Actor_Component.ActorData;
            
            if (actorData.StatesAndConditions.States.GetState(StateName.IsInCombat))
            {
                return ActorPriorityState.InCombat;
            }

            if (!actorData.Career.JobsActive || !actorData.Career.HasCurrentJob())
                return actorData.Career.GetNewCurrentJob() ? ActorPriorityState.HasJob : ActorPriorityState.None;
            
            return ActorPriorityState.HasJob;

        }

        bool _mustChangeCurrentAction(ActorPriorityState  actorPriorityState,
                                      out PriorityElement nextHighestPriorityElement)
        {
            RegenerateAllPriorities();
            
            var currentAction = GetCurrentAction();
            nextHighestPriorityElement = _peekHighestPriority(actorPriorityState);

            if (nextHighestPriorityElement is not null)
                return currentAction == null ||
                       (uint)currentAction.ActionName != nextHighestPriorityElement.PriorityID;
            
            Debug.LogWarning("There is no next highest priority.");
            return false;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var highestPriority = _peekHighestPriority(_getPriorityState());
            
            return new Dictionary<string, string>
            {
                { "Actor ID", $"{ActorID}" },
                { "Current Actor Action", $"{_currentActorAction?.ActionName}" },
                { "Is Performing Current Action", $"{IsPerformingAction}" },
                { "Current Action Coroutine", $"{CurrentActionCoroutine}" },
                { "Current Priority State", $"{_getPriorityState()}" },
                { "Must Change Current Action", $"{_mustChangeCurrentAction(_getPriorityState(), out _)}" },
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
    
    
    public enum ActorPriorityState
    {
        None,
            
        InCombat,
        HasJob,
    }
}