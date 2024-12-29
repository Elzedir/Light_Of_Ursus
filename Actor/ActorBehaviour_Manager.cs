using System.Collections.Generic;
using Priority;
using UnityEngine;

namespace Actor
{
    public abstract class ActorBehaviour_Manager
    {
        public static List<PriorityParameterName> GetRequiredParametersOfActorBehaviour(ActorBehaviourName actorBehaviourName)
        {
            if (_requiredParametersByBehaviour.TryGetValue(actorBehaviourName, out var requiredParameters))
            {
                return requiredParameters;
            }
            
            Debug.LogError($"No required parameters found for {actorBehaviourName}.");
            return null;
        }
        
        static readonly Dictionary<ActorBehaviourName, List<PriorityParameterName>> _requiredParametersByBehaviour = new()
        {
            {
                ActorBehaviourName.Combat, new List<PriorityParameterName>
                {
                    PriorityParameterName.Target_Component,
                }  
            },
            {
                ActorBehaviourName.Work, new List<PriorityParameterName>
                {
                    PriorityParameterName.Jobsite_Component,
                    PriorityParameterName.Hauler_Component
                }
            },
        };
        
        public static List<ActorActionName> GetActorActionsOfActorBehaviour(ActorBehaviourName actorBehaviourName)
        {
            if (_actionsByBehaviour.TryGetValue(actorBehaviourName, out var actionList))
            {
                return actionList;
            }
            
            Debug.LogError($"No actions found for {actorBehaviourName}.");
            return null;
        }

        static readonly Dictionary<ActorBehaviourName, List<ActorActionName>> _actionsByBehaviour = new()
        {
            {
                ActorBehaviourName.Normal, new List<ActorActionName>()
                {
                    ActorActionName.Idle,
                    ActorActionName.Scavenge,
                }
            },
            {
                ActorBehaviourName.Combat, new List<ActorActionName>()
                {
                    ActorActionName.Attack,
                    ActorActionName.Defend,
                }
            },
            {
                ActorBehaviourName.Work, new List<ActorActionName>()
                {
                    ActorActionName.Perform_JobTask
                }
            },
            {
                ActorBehaviourName.Recreation, new List<ActorActionName>()
                {
                    ActorActionName.Wander,
                }
            },
        };
    }
    
    public enum ActorBehaviourName
    {
        Normal,
        Combat,
        Recreation,
        Work,
    }
}