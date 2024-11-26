using System;
using System.Collections;
using System.Collections.Generic;
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

        public static ActorAction_Master GetActorAction(ActorActionName actorActionName) =>
            _allActorActions[actorActionName];

        static readonly Dictionary<ActorActionName, ActorAction_Master> _allActorActions =
            new()
            {
                {
                    ActorActionName.Fetch, new ActorAction_Master(
                        ActorActionName.Fetch, ActorActionGroup.Work, new Dictionary<PriorityParameterName, object>
                        {
                            { PriorityParameterName.MaxPriority, null },
                            { PriorityParameterName.TotalItems, null },
                            { PriorityParameterName.TotalDistance, null },
                            { PriorityParameterName.InventoryHauler, null },
                            { PriorityParameterName.InventoryTarget, null },
                        },
                        new List<IEnumerator>
                        {

                        })
                },

                {
                    ActorActionName.Deliver, new ActorAction_Master(
                        ActorActionName.Deliver, ActorActionGroup.Work, new Dictionary<PriorityParameterName, object>
                        {
                            { PriorityParameterName.MaxPriority, null },
                            { PriorityParameterName.TotalItems, null },
                            { PriorityParameterName.TotalDistance, null },
                            { PriorityParameterName.InventoryHauler, null },
                            { PriorityParameterName.InventoryTarget, null },
                            { PriorityParameterName.CurrentStationType, null },
                            { PriorityParameterName.AllStationTypes, null },
                        },
                        new List<IEnumerator>
                        {

                        })
                },

                {
                    ActorActionName.Wander, new ActorAction_Master(
                        ActorActionName.Wander, ActorActionGroup.Recreation,
                        new Dictionary<PriorityParameterName, object>
                        {
                            { PriorityParameterName.MaxPriority, null },
                        },
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

    [Serializable]
    public class ActorAction_Master
    {
        public readonly ActorActionName                           ActionName;
        public readonly ActorActionGroup                          ActorActionGroup;
        public readonly Dictionary<PriorityParameterName, object> ActionParameters;
        public readonly List<IEnumerator>                         Actions;

        public ActorAction_Master(ActorActionName actionName, ActorActionGroup actorActionGroup,
                                  Dictionary<PriorityParameterName, object> actionParameters, List<IEnumerator> actions)
        {
            ActionName       = actionName;
            ActorActionGroup = actorActionGroup;
            ActionParameters = actionParameters;
            Actions          = actions;
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
    
    public class ActorActionToChange
    {
        public ActorActionName    ActorActionName;
        public PriorityImportance PriorityImportance;

        public ActorActionToChange(ActorActionName actorActionName, PriorityImportance priorityImportance)
        {
            ActorActionName    = actorActionName;
            PriorityImportance = priorityImportance;
        }
    }
}
