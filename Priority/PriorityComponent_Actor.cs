using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Priority;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_Actor : PriorityComponent
    {
        public Coroutine CurrentActionCoroutine { get; private set; }
        public bool     IsPerformingAction     => CurrentActionCoroutine != null;
        public Dictionary<uint, PriorityQueue> AllActionPriorities => AllPriorities;
        ActorAction_Master                            _currentActorActionMaster;
        public ActorAction_Master                     GetCurrentAction() => _currentActorActionMaster;
        PriorityGenerator_Actor                _priorityGenerator;
        PriorityGenerator_Actor                PriorityGenerator => _priorityGenerator ??= new PriorityGenerator_Actor();

        public void SetCurrentAction(uint actorActionName)
        {
            _stopCurrentAction();
            
            Debug.Log($"Setting current action to: {(ActorActionName)actorActionName}.");
            
            var actorAction = Manager_ActorAction.GetActorAction((ActorActionName)actorActionName);
            var currentActorAction = Manager_ActorAction.GetActorAction(_currentActorActionMaster.ActionName);

            UpdatePriority((uint)_currentActorActionMaster.ActionName,
                PriorityGenerator.GeneratePriority(_currentActorActionMaster.ActionName,
                    currentActorAction.ActionParameters));

            _currentActorActionMaster = actorAction;

            _actor.StartCoroutine(_performCurrentActionFromStart());
        }

        protected override void _regeneratePriority(uint priorityQueueID, uint priorityID)
        {
            if (!_priorityExists(priorityID, out var existingPriorityParameters)) return;
            
            if (priorityQueueID is (uint)ActorActionName.All or (uint)ActorActionName.Idle)
            {
                Debug.LogError($"PriorityQueueID: {priorityQueueID} not allowed in _regeneratePriority.");
                return;
            }
            
            var newPriorities = PriorityGenerator.GeneratePriority((ActorActionName)priorityID, existingPriorityParameters);

            if (newPriorities is null || newPriorities.Count is 0) return;

            AllPriorities[priorityQueueID].Update(priorityID, newPriorities);
            
            var parameters = _updateExistingPriorityParameters(priorityID, actorAction.ActionParameters);

            UpdatePriority(priorityID, PriorityGenerator.GeneratePriority((ActorActionName)priorityID, parameters));
            
            

            

            
        }

        IEnumerator _performCurrentActionFromStart()
        {
            foreach (var action in _currentActorActionMaster.Actions)
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
        protected ActorComponent _actor  => _actorReferences.Actor;

        public PriorityComponent_Actor(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);

            //AllPriorities.DictionaryChanged += SetCurrentAction;
        }

        protected override bool _priorityExists(uint priorityID,
                                                out Dictionary<PriorityParameterName, object>
                                                    existingPriorityParameters)
        {
            var actorAction = Manager_ActorAction.GetActorAction((ActorActionName)priorityID);
                
            if (actorAction != null)
            {
                existingPriorityParameters = actorAction.ActionParameters;
                return true;
            }

            Debug.LogError($"ActionName: {priorityID} not found in _existingParameters.");
            existingPriorityParameters = null;
            return false;
        }

        protected override List<uint> _canPeek(List<uint> priorityIDs)
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

        public override void OnDataChanged(DataChanged                               dataChanged,
                                           Dictionary<PriorityParameterName, object> changedParameters)
        {
            if (!_prioritiesToChange.TryGetValue(dataChanged, out var actionsToChange))
            {
                Debug.Log($"DataChanged: {dataChanged} not found in _actionsToChange for {this}.");
                return;
            }

            foreach (var actionToChange in actionsToChange)
            {
                var parameters =
                    _updateExistingPriorityParameters((uint)actionToChange.ActorActionName, changedParameters);

                var priorities = PriorityGenerator.GeneratePriority(actionToChange.ActorActionName, parameters);

                switch (actionToChange.PriorityImportance)
                {
                    case PriorityImportance.Critical:
                        if (!AllPriorities[(uint)actionToChange.ActorActionName]
                                .Update((uint)actionToChange.ActorActionName, priorities))
                        {
                            Debug.LogError($"Action: {actionToChange} unable to be updated in PriorityQueue.");
                        }

                        break;
                    case PriorityImportance.High:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities),
                            PriorityImportance.High);
                        break;
                    case PriorityImportance.Medium:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities),
                            PriorityImportance.Medium);
                        break;
                    case PriorityImportance.Low:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities),
                            PriorityImportance.Low);
                        break;
                    case PriorityImportance.None:
                    default:
                        Debug.LogError($"PriorityImportance: {actionToChange} not found.");
                        break;
                }
            }
        }
        
        static readonly Dictionary<DataChanged, List<ActorActionToChange>> _prioritiesToChange = new()
        {
            {
                DataChanged.ChangedInventory, new List<ActorActionToChange>
                {
                    new(ActorActionName.Deliver, PriorityImportance.High)
                }
            },

            {
                DataChanged.DroppedItems, new List<ActorActionToChange>
                {
                    new(ActorActionName.Fetch, PriorityImportance.High),
                    new(ActorActionName.Scavenge, PriorityImportance.Medium),
                }
            },

            {
                DataChanged.PriorityCompleted, new List<ActorActionToChange>
                {
                    new(ActorActionName.Wander, PriorityImportance.High),
                }
            },
        };

        protected override ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(
            PriorityState priorityState)
        {
            if (priorityState is PriorityState.None) return AllPriorities;

            var relevantPriorityQueues = new ObservableDictionary<uint, PriorityQueue>();

            switch (priorityState)
            {
                case PriorityState.InCombat:
                    foreach (var actorActionName in Manager_ActorAction.GetActionGroup(ActorActionGroup.Combat))
                    {
                        relevantPriorityQueues.Add((uint)actorActionName, AllPriorities[(uint)actorActionName]);
                    }

                    break;
                case PriorityState.HasJob:
                    foreach (var actorActionName in Manager_ActorAction.GetActionGroup(ActorActionGroup.Work))
                    {
                        relevantPriorityQueues.Add((uint)actorActionName, AllPriorities[(uint)actorActionName]);
                    }

                    break;
                case PriorityState.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(priorityState), priorityState, null);
            }

            return relevantPriorityQueues;
        }
    }
}