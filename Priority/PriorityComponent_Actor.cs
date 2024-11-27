using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_Actor : PriorityComponent
    {
        public Coroutine CurrentActionCoroutine { get; private set; }
        public bool     IsPerformingAction     => CurrentActionCoroutine != null;
        public Dictionary<uint, PriorityQueue> AllActionPriorities => AllPriorities;
        ActorAction                            _currentActorAction;
        public ActorAction                     GetCurrentAction() => _currentActorAction;
        PriorityGenerator_Actor                _priorityGenerator;
        PriorityGenerator_Actor                PriorityGenerator => _priorityGenerator ??= new PriorityGenerator_Actor();

        public void SetCurrentAction(uint actorActionName)
        {
            _stopCurrentAction();
            
            Debug.Log($"Setting current action to: {(ActorActionName)actorActionName}.");
            
            var actorAction = Manager_ActorAction.GetActorAction((ActorActionName)actorActionName);

            _currentActorAction = actorAction;

            _actor.StartCoroutine(_performCurrentActionFromStart());
        }

        protected override void _regeneratePriority(uint priorityQueueID, uint priorityID = 1)
        {
            if (!_priorityExists(priorityQueueID, priorityID, out var existingPriorityParameters)) return;
            
            switch (priorityQueueID)
            {
                case (uint)ActorActionName.All:
                    Debug.LogError($"ActorActionName: {(ActorActionName)priorityQueueID} not allowed in _regeneratePriority.");
                    return;
                case (uint)ActorActionName.Idle:
                    existingPriorityParameters = new Dictionary<PriorityParameterName, object>
                    {
                        { PriorityParameterName.DefaultPriority, 1 }
                    };
                    break;
            }

            var newPriorities = _regenerate();

            AllPriorities[priorityQueueID].Update(priorityID, newPriorities);
        }

        Dictionary<PriorityParameterName, object> _regenerate()
        {
            var newPriorities = new Dictionary<PriorityParameterName, object>();
            
            
            
            return newPriorities;
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
        protected ActorComponent _actor  => _actorReferences.Actor;

        public PriorityComponent_Actor(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);

            //AllPriorities.DictionaryChanged += SetCurrentAction;
        }

        protected override PriorityElement _createPriorityElement(uint priorityID,
                                                                  Dictionary<PriorityParameterName, object>
                                                                      priorityParameters)
        {
            return new PriorityElement(priorityID,
                priorityParameters ?? Manager_ActorAction.GetDefaultActionParameters((ActorActionName)priorityID));
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
        
        protected override Dictionary<DataChanged, List<PriorityToChange>> _prioritiesToChange { get; } = new()
        {
            {
                DataChanged.ChangedInventory, new List<PriorityToChange>
                {
                    new((uint)ActorActionName.Deliver, PriorityImportance.High)
                }
            },

            {
                DataChanged.DroppedItems, new List<PriorityToChange>
                {
                    new((uint)ActorActionName.Fetch, PriorityImportance.High),
                    new((uint)ActorActionName.Scavenge, PriorityImportance.Medium),
                }
            },

            {
                DataChanged.PriorityCompleted, new List<PriorityToChange>
                {
                    new((uint)ActorActionName.Wander, PriorityImportance.High),
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