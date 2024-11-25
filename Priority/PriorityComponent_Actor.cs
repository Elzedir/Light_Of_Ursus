using System;
using System.Collections.Generic;
using Actors;
using Managers;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_Actor : PriorityComponent
    {
        ActorActionName _currentActorAction;
        
        static readonly Dictionary<ActorActionName, Dictionary<PriorityParameterName, object>> _priorityParameters = new()
        {
            {
                ActorActionName.Fetch, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.MaxPriority, null },
                    { PriorityParameterName.TotalItems, null },
                    { PriorityParameterName.TotalDistance, null },
                    { PriorityParameterName.InventoryHauler, null },
                    { PriorityParameterName.InventoryTarget, null },
                }
            },

            {
                ActorActionName.Deliver, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.MaxPriority, null },
                    { PriorityParameterName.TotalItems, null },
                    { PriorityParameterName.TotalDistance, null },
                    { PriorityParameterName.InventoryHauler, null },
                    { PriorityParameterName.InventoryTarget, null },
                    { PriorityParameterName.CurrentStationType, null },
                    { PriorityParameterName.AllStationTypes, null },
                }
            },

            {
                ActorActionName.Scavenge, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.MaxPriority, null },
                    { PriorityParameterName.TotalItems, null },
                    { PriorityParameterName.TotalDistance, null },
                    { PriorityParameterName.InventoryHauler, null },
                    { PriorityParameterName.InventoryTarget, null },
                }
            },

            {
                ActorActionName.Wander, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.MaxPriority, null },
                }
            },
        };

        public ActorActionName GetCurrentAction()                       => _currentActorAction;

        void              _setCurrentAction(uint actorActionName)
        {
            Debug.Log($"Setting current action to: {(ActorActionName)actorActionName}.");
            _currentActorAction = (ActorActionName)actorActionName;
        }

        readonly ComponentReference_Actor _actorReferences;

        public    uint           ActorID => _actorReferences.ActorID;
        protected ActorComponent _actor  => _actorReferences.Actor;

        public PriorityComponent_Actor(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);

            AllPriorities.DictionaryChanged += _setCurrentAction;
        }

        static readonly Dictionary<DataChanged, List<ActionToChange>> _prioritiesToChange = new()
        {
            {
                DataChanged.ChangedInventory, new List<ActionToChange>
                {
                    new(ActorActionName.Deliver, PriorityImportance.High)
                }
            },

            {
                DataChanged.DroppedItems, new List<ActionToChange>
                {
                    new(ActorActionName.Fetch, PriorityImportance.High),
                    new(ActorActionName.Scavenge, PriorityImportance.Medium),
                }
            },

            {
                DataChanged.PriorityCompleted, new List<ActionToChange>
                {
                    new(ActorActionName.Wander, PriorityImportance.High),
                }
            },
        };

        protected override bool _priorityExists(uint priorityID, out Dictionary<PriorityParameterName, object> existingPriorityParameters)
        {
            if (_priorityParameters.TryGetValue((ActorActionName)priorityID, out var parameters))
            {
                existingPriorityParameters = parameters;
                return true;
            }
            
            Debug.LogError($"ActionName: {priorityID} not found in _existingParameters.");
            existingPriorityParameters = null;
            return false;
        }
        
        protected override bool _canPeek(uint priorityID)
        {
            if (priorityID is (uint)ActorActionName.All or (uint)ActorActionName.Idle) return true;
            
            Debug.LogError($"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
            return false;
        }

        public override void OnDataChanged(DataChanged dataChanged, Dictionary<PriorityParameterName, object> changedParameters)
        {
            if (!_prioritiesToChange.TryGetValue(dataChanged, out var actionsToChange))
            {
                Debug.Log($"DataChanged: {dataChanged} not found in _actionsToChange for {this}.");
                return;
            }

            foreach (var actionToChange in actionsToChange)
            {
                var parameters = _updateExistingPriorityParameters((uint)actionToChange.ActorActionName, changedParameters);

                var priorities = PriorityGenerator.GeneratePriorities(actionToChange.ActorActionName, parameters);

                switch (actionToChange.PriorityImportance)
                {
                    case PriorityImportance.Critical:
                        if (!AllPriorities[(uint)actionToChange.ActorActionName].Update((uint)actionToChange.ActorActionName, priorities))
                        {
                            Debug.LogError($"Action: {actionToChange} unable to be updated in PriorityQueue.");
                        }
                        
                        break;
                    case PriorityImportance.High:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities), PriorityImportance.High);
                        break;
                    case PriorityImportance.Medium:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities), PriorityImportance.Medium);
                        break;
                    case PriorityImportance.Low:
                        _addToCachedPriorityQueue(new PriorityValue((uint)actionToChange.ActorActionName, priorities), PriorityImportance.Low);
                        break;
                    case PriorityImportance.None:
                    default:
                        Debug.LogError($"PriorityImportance: {actionToChange} not found.");
                        break;
                }
            }
        }
        
        protected override ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(PriorityState priorityState)
        {
            if (priorityState is PriorityState.None) return AllPriorities;

            var relevantPriorityQueues = new ObservableDictionary<uint, PriorityQueue>();
            
            switch (priorityState)
            {
                case PriorityState.InCombat:
                    foreach (var actorActionName in Manager_ActorAction.GetAllActionsInActionGroup(ActionGroup.Combat))
                    {
                        relevantPriorityQueues.Add((uint)actorActionName, AllPriorities[(uint)actorActionName]);    
                    }
                    break;
                case PriorityState.HasWork:
                    foreach (var actorActionName in Manager_ActorAction.GetAllActionsInActionGroup(ActionGroup.Work))
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
