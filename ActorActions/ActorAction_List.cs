using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Actors;
using Jobs;
using Priorities;
using Priority;
using StateAndCondition;
using UnityEngine;

namespace ActorActions
{
    public abstract class ActorAction_List
    {
        static Dictionary<ActorActionName, ActorAction_Data> s_allActorAction_Data;
        
        public static Dictionary<ActorActionName, ActorAction_Data> S_AllActorAction_Data =>
            s_allActorAction_Data ??= _initialiseAllActorAction_Data();

        static Dictionary<ActorActionName, ActorAction_Data> _initialiseAllActorAction_Data()
        {
            return new Dictionary<ActorActionName, ActorAction_Data>
            {
                {
                    ActorActionName.Idle, new ActorAction_Data(
                        actionName: ActorActionName.Idle,
                        actionDescription: "To idle",
                        requiredStates: new Dictionary<StateName, bool>
                        {
                            {
                                StateName.CanIdle, true
                            }
                        },
                        primaryJob: JobName.Any)
                },
                {
                    ActorActionName.Wander, new ActorAction_Data(
                        actionName: ActorActionName.Wander,
                        actionDescription: "To wander",
                        requiredStates: new Dictionary<StateName, bool>
                        {
                            {
                                StateName.CanIdle, true
                            }
                        },
                        primaryJob: JobName.Any,
                        actionList: new List<Func<Priority_Parameters, IEnumerator>>())
                },
                {
                    ActorActionName.Haul, new ActorAction_Data(
                        actionName: ActorActionName.Haul,
                        actionDescription: "Fetch Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any)
                },
                {
                    ActorActionName.Beat_Metal, new ActorAction_Data(
                        actionName: ActorActionName.Beat_Metal,
                        actionDescription: "Beat Iron into Shape",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Smith)
                },
                {
                    ActorActionName.Chop_Wood, new ActorAction_Data(
                        actionName: ActorActionName.Chop_Wood,
                        actionDescription: "Chop Wood",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Logger)
                },
                {
                    ActorActionName.Process_Logs, new ActorAction_Data(
                        actionName: ActorActionName.Process_Logs,
                        actionDescription: "Process Logs",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Sawyer)
                },
                {
                    ActorActionName.Stand_At_Counter, new ActorAction_Data(
                        actionName: ActorActionName.Stand_At_Counter,
                        actionDescription: "Stand at Counter",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Vendor)
                },
                {
                    ActorActionName.Restock_Shelves, new ActorAction_Data(
                        actionName: ActorActionName.Restock_Shelves,
                        actionDescription: "Restock Shelves",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Vendor)
                },
                {
                    ActorActionName.Defend_Ally, new ActorAction_Data(
                        actionName: ActorActionName.Defend_Ally,
                        actionDescription: "Defend Ally",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Guard,
                        actionList: new List<Func<Priority_Parameters, IEnumerator>>
                        {
                            _defendAlly
                        })
                },
                {
                    ActorActionName.Defend_Neutral, new ActorAction_Data(
                        actionName: ActorActionName.Defend_Neutral,
                        actionDescription: "Defend Neutral",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Guard,
                        actionList: new List<Func<Priority_Parameters, IEnumerator>>
                        {
                            _defendNeutral
                        })
                },
            };
        }
        
        static IEnumerator _defendAlly(Priority_Parameters priority_Parameters)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(Priority_Parameters priority_Parameters)
        {
            yield return null;
        }
        
        static IEnumerator _moveToPosition(Actor_Component actor_Component, Vector3 position)
        {
            yield return actor_Component.StartCoroutine(actor_Component.BasicMove(position));
            
            if (actor_Component.transform.position != position) actor_Component.transform.position = position;
        }

        static Dictionary<StateName, Dictionary<ActorActionName, bool>> s_actorActionStateDictionary;

        public static Dictionary<StateName, Dictionary<ActorActionName, bool>> S_ActorActionStateDictionary =>
            s_actorActionStateDictionary ??= _initialiseActorActionStateDictionary();

        static Dictionary<StateName, Dictionary<ActorActionName, bool>> _initialiseActorActionStateDictionary()
        {
            var actorActionStateDictionary = new Dictionary<StateName, Dictionary<ActorActionName, bool>>();

            foreach (var actorActionData in S_AllActorAction_Data)
            {
                foreach (var state in actorActionData.Value.RequiredStates)
                {
                    if (!actorActionStateDictionary.ContainsKey(state.Key))
                    {
                        actorActionStateDictionary.Add(state.Key, new Dictionary<ActorActionName, bool>());
                    }

                    actorActionStateDictionary[state.Key].Add(actorActionData.Key, state.Value);
                }
            }

            return actorActionStateDictionary;
        }
    }
}