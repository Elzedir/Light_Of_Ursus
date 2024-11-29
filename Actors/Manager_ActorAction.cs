using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Priority;
using UnityEngine;

namespace Actors
{
    public abstract class Manager_ActorAction : MonoBehaviour
    {
        public static ActorAction GetNewActorAction(ActorActionName actorActionName) =>
            new(actorActionName, GetActionParameters(actorActionName, null),
                GetDefaultActionHighestPriorityState(actorActionName));

        public static ActorAction_Master GetActorAction_Master(ActorActionName actorActionName)
        {
            if (_allActorAction_Masters.TryGetValue(actorActionName, out var actorActionMaster))
            {
                return actorActionMaster;
            }

            Debug.LogError($"ActorActionMaster not found for: {actorActionName}.");
            return null;
        }


        static readonly Dictionary<ActorActionName, ActorAction_Master> _allActorAction_Masters =
            new()
            {
                {
                    ActorActionName.Fetch, new ActorAction_Master(
                        ActorActionName.Fetch, ActorActionGroup.Work,
                        new List<IEnumerator>
                        {

                        })
                },

                {
                    ActorActionName.Deliver, new ActorAction_Master(
                        ActorActionName.Deliver, ActorActionGroup.Work,
                        new List<IEnumerator>
                        {

                        })
                },

                {
                    ActorActionName.Wander, new ActorAction_Master(
                        ActorActionName.Wander, ActorActionGroup.Recreation,
                        new List<IEnumerator>
                        {

                        })
                },
            };

        public static List<ActorActionName> GetActionGroup(ActorActionGroup actorActionGroup) =>
            _allActionGroups[actorActionGroup];

        static readonly Dictionary<ActorActionGroup, List<ActorActionName>> _allActionGroups = new()
        {
            {
                ActorActionGroup.Normal, new List<ActorActionName>()
                {
                    ActorActionName.Idle,
                    ActorActionName.Scavenge,
                }
            },
            {
                ActorActionGroup.Combat, new List<ActorActionName>()
                {
                    ActorActionName.Attack,
                    ActorActionName.Defend,
                }
            },
            {
                ActorActionGroup.Work, new List<ActorActionName>()
                {
                    ActorActionName.Deliver,
                    ActorActionName.Fetch,
                }
            },
            {
                ActorActionGroup.Recreation, new List<ActorActionName>()
                {
                    ActorActionName.Wander,
                }
            },
        };

        public static Dictionary<PriorityParameterName, object> GetActionParameters(
            ActorActionName actorActionName, Dictionary<PriorityParameterName, object> requiredParameters)
        {
            requiredParameters ??= new Dictionary<PriorityParameterName, object>();
            
            var emptyActionParameters = GetEmptyActionParameters(actorActionName);

            return actorActionName switch
            {
                ActorActionName.Fetch => _populateFetchActionParameters(emptyActionParameters, requiredParameters),
                _                     => null
            };
        }

        public static Dictionary<PriorityParameterName, object> GetEmptyActionParameters(ActorActionName actorActionName)
        {
            if (_allDefaultActionParameters.TryGetValue(actorActionName, out var defaultParameters))
            {
                return defaultParameters.ToDictionary<PriorityParameterName, PriorityParameterName, object>(
                    parameterName => parameterName, _ => null);
            }

            Debug.LogWarning($"No default parameters found for {actorActionName}. Returning empty dictionary.");
            return new Dictionary<PriorityParameterName, object>();
        }

        static readonly Dictionary<ActorActionName, List<PriorityParameterName>> _allDefaultActionParameters = new()
        {
            {
                ActorActionName.Fetch, new List<PriorityParameterName>
                {
                    PriorityParameterName.DefaultPriority,
                    PriorityParameterName.TotalItems,
                    PriorityParameterName.TotalDistance,
                    PriorityParameterName.InventoryHauler,
                    PriorityParameterName.InventoryTarget,
                }
            },
            {
                ActorActionName.Deliver, new List<PriorityParameterName>
                {
                    PriorityParameterName.DefaultPriority,
                    PriorityParameterName.TotalItems,
                    PriorityParameterName.TotalDistance,
                    PriorityParameterName.InventoryHauler,
                    PriorityParameterName.InventoryTarget,
                    PriorityParameterName.CurrentStationType,
                    PriorityParameterName.AllStationTypes,
                }
            },
            {
                ActorActionName.Wander, new List<PriorityParameterName>
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
        };

        static Dictionary<PriorityParameterName, object> _populateFetchActionParameters(
            Dictionary<PriorityParameterName, object> actionParameters,
            Dictionary<PriorityParameterName, object> existingParameters)
        {
            return new Dictionary<PriorityParameterName, object>(actionParameters)
            {
                [PriorityParameterName.DefaultPriority] = 1,
                [PriorityParameterName.TotalItems]      = 0,
                [PriorityParameterName.TotalDistance]   = 0,
                [PriorityParameterName.InventoryHauler] = null,
                [PriorityParameterName.InventoryTarget] = null,
            };
        }

        public static ActorPriorityState GetDefaultActionHighestPriorityState(ActorActionName actorActionName)
        {
            if (_allDefaultActionHighestPriorityStates.TryGetValue(actorActionName, out var highestPriorityState))
            {
                return highestPriorityState;
            }

            Debug.LogWarning($"No default highest priority state found for {actorActionName}. Returning None.");
            return ActorPriorityState.None;
        }

        static readonly Dictionary<ActorActionName, ActorPriorityState> _allDefaultActionHighestPriorityStates = new()
        {
            { ActorActionName.Attack, ActorPriorityState.InCombat },
            { ActorActionName.Defend, ActorPriorityState.InCombat },

            { ActorActionName.Fetch, ActorPriorityState.HasJob },
            { ActorActionName.Deliver, ActorPriorityState.HasJob },

            { ActorActionName.Wander, ActorPriorityState.None },
            { ActorActionName.Idle, ActorPriorityState.None },
        };
    }

    // ActorAction:
    // A spontaneous or situational action unrelated to structured jobs,
    // often driven by immediate needs, combat, exploration, or player commands.
    // These actions typically occur in reaction to dynamic game states.

    public class ActorAction
    {
        public readonly ActorActionName ActionName;
        ActorAction_Master              _actionMaster;
        public ActorPriorityState       HighestPriorityState;

        public ActorAction_Master ActionMaster =>
            _actionMaster ??= Manager_ActorAction.GetActorAction_Master(ActionName);

        public Dictionary<PriorityParameterName, object> ActionParameters;
        
        public List<IEnumerator> ActionList => ActionMaster.ActionList;

        public ActorAction(ActorActionName actionName, Dictionary<PriorityParameterName, object> actionParameters, ActorPriorityState highestPriorityState)
        {
            ActionName           = actionName;
            ActionParameters     = actionParameters;
            HighestPriorityState = highestPriorityState;
        }
    }

    public class ActorAction_Target : ActorAction
    {
        public ActorAction_Target(ActorActionName actionName,
                                  Dictionary<PriorityParameterName, object> actionParameters,
                                  ActorPriorityState highestPriorityState, GameObject target) : base(actionName,
            actionParameters, highestPriorityState)
        {
            Target = target;
        }

        public GameObject Target;
    }

    public class ActorAction_Location : ActorAction
    {
        public ActorAction_Location(ActorActionName actionName,
                                    Dictionary<PriorityParameterName, object> actionParameters,
                                    ActorPriorityState highestPriorityState, Vector3 location) : base(actionName,
            actionParameters, highestPriorityState)
        {
            Location = location;
        }
        
        public Vector3 Location;
    }

    [Serializable]
    public class ActorAction_Master
    {
        public readonly ActorActionName   ActionName;
        public readonly ActorActionGroup  ActionGroup;
        public readonly List<IEnumerator> ActionList;

        public ActorAction_Master(ActorActionName   actionName, ActorActionGroup actionGroup,
                                  List<IEnumerator> actionList)
        {
            ActionName  = actionName;
            ActionGroup = actionGroup;
            ActionList  = actionList;
        }
    }

    public enum ActorActionName
    {
        // PriorityState None (Can do in all situations)
        Idle,
        All,
        
        Wander,

        // PriorityState Combat
        
        Attack,
        Defend,
        Cast_Spell,
        Parry_Attack,

        // PriorityState Job
        
        Deliver,
        Fetch,
        Scavenge,

        // PriorityState_All

        Drink_Health_Potion,
        Flee,
        Heal_Ally,
        Explore_Area,
        Loot_Chest,
        Interact_With_Object,
        Equip_Armor,
        Inspect_Tool,
        Open_Door,
        Climb_Wall,
        Eat_Fruit,
        Gather_Herbs,
        Drop_Item,
        Inspect_Inventory,
    }

    public enum ActorActionGroup
    {
        Normal,
        Combat,
        Recreation,
        Work,
    }
}