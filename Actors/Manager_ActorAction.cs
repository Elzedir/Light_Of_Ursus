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
        public static bool IsHigherPriorityThan(ActorActionName actorActionName, ActorActionName otherActorActionName)
        {
            return _allPriorityPerAction[actorActionName] < _allPriorityPerAction[otherActorActionName];
        }

        public static ActorAction GetActorAction(ActorActionName actorActionName) =>
            new(actorActionName, GetDefaultActionParameters(actorActionName));

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

        public static Dictionary<PriorityParameterName, object> GetDefaultActionParameters(
            ActorActionName actorActionName)
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

        static readonly Dictionary<ActorActionName, PriorityImportance> _allPriorityPerAction = new()
        {
            { ActorActionName.Wander, PriorityImportance.Low },
            { ActorActionName.Deliver, PriorityImportance.Medium },
            { ActorActionName.Fetch, PriorityImportance.Medium },
            { ActorActionName.Scavenge, PriorityImportance.Medium },
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

        public ActorAction_Master ActionMaster =>
            _actionMaster ??= Manager_ActorAction.GetActorAction_Master(ActionName);

        public Dictionary<PriorityParameterName, object> ActionParameters;
        
        public List<IEnumerator> ActionList => ActionMaster.ActionList;

        public ActorAction(ActorActionName actionName, Dictionary<PriorityParameterName, object> actionParameters)
        {
            ActionName       = actionName;
            ActionParameters = actionParameters;
        }
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
        Idle,
        All,

        Attack,
        Defend,

        Deliver,
        Fetch,
        Scavenge,

        Wander,

        Drink_Health_Potion,
        Flee,
        Explore_Area,
        Loot_Chest,
        Interact_With_Object,
        Heal_Ally,
        Equip_Armor,
        Inspect_Tool,
        Cast_Spell,
        Parry_Attack,
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