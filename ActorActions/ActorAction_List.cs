using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Jobs;
using Priority;
using StateAndCondition;
using UnityEngine;

namespace ActorAction
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>())
                },
                {
                    ActorActionName.Fetch_Items, new ActorAction_Data(
                        actionName: ActorActionName.Fetch_Items,
                        actionDescription: "Fetch Items",
                        requiredStates: new Dictionary<StateName, bool>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _chopWood
                        },
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>
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

        static IEnumerator _beatIron(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
        }

        static IEnumerator _chopWood(Actor_Component actor, uint jobSiteID)
        {
            a
                //* Create the actions to chop wood
                yield return null;
        }

        static IEnumerator _processLogs(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
        }

        static IEnumerator _fetchItems(Actor_Component actor, uint jobSiteID)
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

        static IEnumerator _deliverItems(Actor_Component actor, uint jobSiteID)
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

        IEnumerator _moveOperatorToOperatingArea(Actor_Component actor, Vector3 position)
        {
            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.transform.position != position) actor.transform.position = position;
        }

        static IEnumerator _standAtCounter(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
        }

        static IEnumerator _restockShelves(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
        }

        static IEnumerator _defendAlly(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(Actor_Component actor, uint jobSiteID)
        {
            yield return null;
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