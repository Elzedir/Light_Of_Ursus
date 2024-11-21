using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
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
    }

    public enum ActionGroup
    {
        Normal,
        Combat,
        Recreation,
        Work,
    }
    
    public abstract class Manager_ActorAction : MonoBehaviour
    {
        static readonly Dictionary<ActionGroup, List<ActorActionName>> _allActionGroups = new()
        {
            {
                ActionGroup.Normal, new List<ActorActionName>()
                {
                    ActorActionName.Idle,
                    ActorActionName.Scavenge,
                }
            },
            {
                ActionGroup.Combat, new List<ActorActionName>()
                {
                    ActorActionName.Attack,
                    ActorActionName.Defend,
                }
            },
            {
                ActionGroup.Work, new List<ActorActionName>()
                {
                    ActorActionName.Deliver,
                    ActorActionName.Fetch,
                }
            },
            {
                ActionGroup.Recreation, new List<ActorActionName>()
                {
                    ActorActionName.Wander,
                }
            },
        };
        
        public static List<ActorActionName> GetAllActionsInActionGroup(ActionGroup actionGroup) => _allActionGroups[actionGroup];

        static readonly Dictionary<ActorActionName, ActionGroup> _allActions = new()
        {
            {ActorActionName.Idle, ActionGroup.Normal},
            {ActorActionName.Scavenge, ActionGroup.Normal},
            {ActorActionName.Attack, ActionGroup.Combat},
            {ActorActionName.Defend, ActionGroup.Combat},
            {ActorActionName.Deliver, ActionGroup.Work},
            {ActorActionName.Fetch, ActionGroup.Work},
            {ActorActionName.Wander, ActionGroup.Recreation},
        };
        
        public static ActionGroup GetActorActionGroup(ActorActionName actorActionName) => _allActions[actorActionName];
    }
}
