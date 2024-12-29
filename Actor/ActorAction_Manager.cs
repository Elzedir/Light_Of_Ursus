using System;
using System.Collections;
using System.Collections.Generic;
using Priority;
using UnityEngine;

namespace Actor
{
    public abstract class ActorAction_Manager : MonoBehaviour
    {
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
                    ActorActionName.Perform_JobTask, new ActorAction_Master(
                        ActorActionName.Perform_JobTask, ActorBehaviourName.Work,
                        actionList: new List<IEnumerator>())
                },

                {
                    ActorActionName.Wander, new ActorAction_Master(
                        ActorActionName.Wander, ActorBehaviourName.Recreation,
                        actionList: new List<IEnumerator>())
                },
            };

        public static ActorBehaviourName GetActorBehaviourOfActorAction(ActorActionName actorActionName)
        {
            if (_behavioursByAction.TryGetValue(actorActionName, out var actionGroup))
            {
                return actionGroup;
            }

            Debug.LogError($"No action group found for {actorActionName}.");
            return ActorBehaviourName.Normal;
        }

        static readonly Dictionary<ActorActionName, ActorBehaviourName> _behavioursByAction = new()
        {
            { ActorActionName.Idle, ActorBehaviourName.Normal },
            { ActorActionName.Scavenge, ActorBehaviourName.Normal },

            { ActorActionName.Attack, ActorBehaviourName.Combat },
            { ActorActionName.Defend, ActorBehaviourName.Combat },

            { ActorActionName.Perform_JobTask, ActorBehaviourName.Work },

            { ActorActionName.Wander, ActorBehaviourName.Recreation },
        };

        public static Dictionary<PriorityParameterName, object> PopulateActionParameters(
            ActorActionName actorActionName, Dictionary<PriorityParameterName, object> requiredParameters)
        {
            return actorActionName switch
            {
                ActorActionName.Idle            => null, // Replace
                ActorActionName.Perform_JobTask => _populatePerformJobTaskParameters(requiredParameters),
                _                               => null
            };
        }

        static Dictionary<PriorityParameterName, object> _populatePerformJobTaskParameters(
            Dictionary<PriorityParameterName, object> requiredParameters)
        {
            return new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.JobTaskName, requiredParameters[PriorityParameterName.JobTaskName] },
                { PriorityParameterName.Jobsite_Component, requiredParameters[PriorityParameterName.Jobsite_Component] }
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

            { ActorActionName.Perform_JobTask, ActorPriorityState.HasJob },

            { ActorActionName.Wander, ActorPriorityState.None },
            { ActorActionName.Idle, ActorPriorityState.None },
        };
    }

    // ActorAction:
    // A spontaneous or situational action unrelated to structured jobs,
    // often driven by immediate needs, combat, exploration, or player commands.
    // These actions typically occur in reaction to dynamic game states.

    [Serializable]
    public class ActorAction_Master
    {
        public readonly ActorActionName    ActionName;
        public readonly ActorBehaviourName BehaviourName;
        public readonly List<IEnumerator>  ActionList;

        public ActorAction_Master(ActorActionName   actionName, ActorBehaviourName behaviourName,
                                  List<IEnumerator> actionList)
        {
            ActionName    = actionName;
            BehaviourName = behaviourName;
            ActionList    = actionList;
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

        Perform_JobTask,

        //Deliver,
        //Fetch,
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
}