using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Jobs;
using Priority;
using StateAndCondition;
using Station;
using UnityEngine;

namespace ActorActions
{
    public abstract class ActorAction_List
    {
        static Dictionary<ActorActionName, ActorAction_Data> _allActorAction_Data;

        public static Dictionary<ActorActionName, ActorAction_Data> AllActorAction_Data =>
            _allActorAction_Data ??= _initialiseAllActorAction_Data();

        static Dictionary<ActorActionName, ActorAction_Data> _initialiseAllActorAction_Data()
        {
            return new Dictionary<ActorActionName, ActorAction_Data>
            {
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
                        requiredParameters: new List<PriorityParameterName>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>())
                },
                {
                    ActorActionName.Fetch_Items, new ActorAction_Data(
                        actionName: ActorActionName.Fetch_Items,
                        actionDescription: "Fetch Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _fetchItems
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Deliver_Items, new ActorAction_Data(
                        actionName: ActorActionName.Deliver_Items,
                        actionDescription: "Deliver Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _deliverItems
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Beat_Metal, new ActorAction_Data(
                        actionName: ActorActionName.Beat_Metal,
                        actionDescription: "Beat Iron into Shape",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Smith,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _beatIron
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Chop_Wood, new ActorAction_Data(
                        actionName: ActorActionName.Chop_Wood,
                        actionDescription: "Chop Wood",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Logger,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>(),
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Process_Logs, new ActorAction_Data(
                        actionName: ActorActionName.Process_Logs,
                        actionDescription: "Process Logs",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Sawyer,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _processLogs
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Stand_At_Counter, new ActorAction_Data(
                        actionName: ActorActionName.Stand_At_Counter,
                        actionDescription: "Stand at Counter",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Vendor,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _standAtCounter
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Restock_Shelves, new ActorAction_Data(
                        actionName: ActorActionName.Restock_Shelves,
                        actionDescription: "Restock Shelves",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Vendor,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _restockShelves
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Defend_Ally, new ActorAction_Data(
                        actionName: ActorActionName.Defend_Ally,
                        actionDescription: "Defend Ally",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Guard,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _defendAlly
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    ActorActionName.Defend_Neutral, new ActorAction_Data(
                        actionName: ActorActionName.Defend_Neutral,
                        actionDescription: "Defend Neutral",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Guard,
                        actionList: new List<Func<ActorAction_Parameters, IEnumerator>>
                        {
                            _defendNeutral
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
            };
        }

        static IEnumerator _beatIron(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }

        static IEnumerator _chopWood(ActorAction_Parameters actorAction_Parameters)
        {
            var actor_Component = actorAction_Parameters.Actor_Component_Source;
            var station_Source = Station_Manager.GetStation_Component(actorAction_Parameters.StationID_Source);
            var itemsToFetch = actorAction_Parameters.Items;
            
            yield return _moveToWorkPost(actorAction_Parameters);
            
            
        }

        static IEnumerator _processLogs(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }

        static IEnumerator _fetchItems(ActorAction_Parameters actorAction_Parameters)
        {
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

        static IEnumerator _deliverItems(ActorAction_Parameters actorAction_Parameters)
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

        static IEnumerator _standAtCounter(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }

        static IEnumerator _restockShelves(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }

        static IEnumerator _defendAlly(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(ActorAction_Parameters actorAction_Parameters)
        {
            yield return null;
        }
        
        static IEnumerator _moveToWorkPost(ActorAction_Parameters actorAction_Parameters)
        {
            var actor_Component = actorAction_Parameters.Actor_Component_Source;
            var workPost_Component = actorAction_Parameters.WorkPost_Component_Destination;
            
            yield return actor_Component.StartCoroutine(_moveToPosition(actor_Component, workPost_Component.transform.position));
        }
        
        static IEnumerator _moveToPosition(Actor_Component actor_Component, Vector3 position)
        {
            yield return actor_Component.StartCoroutine(actor_Component.BasicMove(position));
            
            if (actor_Component.transform.position != position) actor_Component.transform.position = position;
        }

        static Dictionary<StateName, Dictionary<ActorActionName, bool>> _actorActionStateDictionary;

        public static Dictionary<StateName, Dictionary<ActorActionName, bool>> ActorActionStateDictionary =>
            _actorActionStateDictionary ??= _initialiseActorActionStateDictionary();

        public static Dictionary<StateName, Dictionary<ActorActionName, bool>> GetActorActionStateDictionary()
        {
            return ActorActionStateDictionary;
        }

        static Dictionary<StateName, Dictionary<ActorActionName, bool>> _initialiseActorActionStateDictionary()
        {
            var actorActionStateDictionary = new Dictionary<StateName, Dictionary<ActorActionName, bool>>();

            foreach (var actorActionData in AllActorAction_Data)
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