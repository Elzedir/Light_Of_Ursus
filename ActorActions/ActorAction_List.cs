using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
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
                    ActorActionName.Haul_Fetch, new ActorAction_Data(
                        actionName: ActorActionName.Haul_Fetch,
                        actionDescription: "Fetch Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<Priority_Parameters, IEnumerator>>
                        {
                            _fetchItems
                        })
                },
                {
                    ActorActionName.Haul_Deliver, new ActorAction_Data(
                        actionName: ActorActionName.Haul_Deliver,
                        actionDescription: "Deliver Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<Priority_Parameters, IEnumerator>>
                        {
                            _deliverItems
                        })
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

        static IEnumerator _fetchItems(Priority_Parameters priority_Parameters)
        {
            _moveToWorkPost(priority_Parameters);
            yield return null;
            // if (Vector3.Distance(actor.transform.position, stationDestination.transform.position) > (stationDestination.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
            // {
            //     yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, stationDestination.CollectionPoint.position));
            // }
            //
            // station.StationData.InventoryData.RemoveFromInventory(orderItems);
            //
            // actor.ActorData.InventoryData.AddToInventory(orderItems);
            //
            // StationData.InventoryData.RemoveFromFetchItemsOnHold(itemsToFetch);
        }

        static IEnumerator _deliverItems(Priority_Parameters priority_Parameters)
        {
            yield return null;

            // if (Vector3.Distance(actor.transform.position, station.transform.position) > (station.BoxCollider.bounds.extents.magnitude + actor.Collider.bounds.extents.magnitude * 1.1f))
            // {
            //     yield return actor.ActorHaulCoroutine = actor.StartCoroutine(_moveOperatorToOperatingArea(actor, station.CollectionPoint.position));
            // }
            //
            // actor.ActorData.InventoryData.RemoveFromInventory(orderItems);
            // station.StationData.InventoryData.AddToInventory(orderItems);
        }
        
        static IEnumerator _defendAlly(Priority_Parameters priority_Parameters)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(Priority_Parameters priority_Parameters)
        {
            yield return null;
        }
        
        static IEnumerator _moveToWorkPost(Priority_Parameters priority_Parameters)
        {
            yield return null;
            // var actor_Component = priority_Parameters.Actor_Component_Source;
            // //var workPost_Component = priority_Parameters.WorkPost_Component_Target;
            //
            // yield return actor_Component.StartCoroutine(_moveToPosition(actor_Component, workPost_Component.transform.position));
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