using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using Jobs;
using JobSite;
using Priority;
using Station;
using UnityEngine;

namespace Actor
{
    public abstract class ActorAction_Manager : MonoBehaviour
    {
        static Dictionary<ActorActionName, ActorAction_Data> _allActorAction_Data;
        public static Dictionary<ActorActionName, ActorAction_Data> AllActorAction_Data =>
            _allActorAction_Data ??= _initialiseAllActorAction_Data();
        public static ActorAction_Data GetActorAction_Data(ActorActionName actorActionName)
        {
            if (AllActorAction_Data.TryGetValue(actorActionName, out var actorAction_Data))
            {
                return actorAction_Data;
            }

            Debug.LogError($"ActorAction_Data not found for: {actorActionName}.");
            return null;
        }

        static Dictionary<ActorActionName, ActorAction_Data> _initialiseAllActorAction_Data()
        {
            return new Dictionary<ActorActionName, ActorAction_Data>
            {
                {
                    ActorActionName.Wander, new ActorAction_Data(
                        actionName: ActorActionName.Wander, 
                        actionDescription: "To wander",
                        behaviourName: ActorBehaviourName.Recreation,
                        requiredParameters: new List<PriorityParameterName>(),
                        primaryJob: JobName.Any,
                        actionList: new List<Func<Actor_Component, uint, IEnumerator>>())
                },
                {
                    ActorActionName.Fetch_Items, new ActorAction_Data(
                        actionName: ActorActionName.Fetch_Items,
                        actionDescription: "Fetch Items",
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
                        behaviourName: ActorBehaviourName.Work,
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
            ActorActionName actorActionName, Dictionary<PriorityParameterName, object> parameters)
        {
            JobSite_Component jobSite_Component = null; 
            Actor_Component   actor_Component  = null;

            var requiredParameters = GetActorAction_Data(actorActionName).RequiredParameters;
                    
            foreach (var requiredParameter in requiredParameters)
            {
                if (parameters.TryGetValue(requiredParameter, out var parameter))
                {
                    switch (requiredParameter)
                    {
                        case PriorityParameterName.Jobsite_Component:
                            jobSite_Component = parameter as JobSite_Component;
                            break;
                        case PriorityParameterName.Worker_Component:
                            actor_Component = parameter as Actor_Component;
                            break;
                    }

                    continue;
                }
                    
                Debug.LogError($"Required Parameter: {requiredParameter} is null.");
                return null;
            }
            
            return actorActionName switch
            {
                ActorActionName.Perform_JobTask => GetActorAction_Data(actorActionName).RequiredParameters
                    .ToDictionary(parameter => parameter, parameter => parameters[parameter]),
                _ => null
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
        
        a
            //* Integrate PopulateTaskParameters since we made JobTasks become ActorActions.
        
        public static Dictionary<PriorityParameterName, object> PopulateTaskParameters(
            ActorActionName actorActionName, Dictionary<PriorityParameterName, object> parameters)
        {
            JobSite_Component jobSite_Component = null; 

            var requiredParameters = GetActorAction_Data(actorActionName).RequiredParameters;
                    
            foreach (var requiredParameter in requiredParameters)
            {
                if (parameters.TryGetValue(requiredParameter, out var parameter))
                {
                    if (parameter is null)
                    {
                        Debug.LogError($"Parameter: {requiredParameter} is null.");
                        return null;
                    }
                    
                    switch (requiredParameter)
                    {
                        case PriorityParameterName.Jobsite_Component:
                            jobSite_Component = parameter as JobSite_Component;
                            break;
                    }

                    continue;
                }
                    
                Debug.LogError($"Required Parameter: {requiredParameter} doesn't exist in parameters.");
                return null;
            }
            
            return actorActionName switch
            {
                ActorActionName.Fetch_Items => _populateFetch_Items(jobSite_Component),
                ActorActionName.Deliver_Items => _populateDeliver_Items(jobSite_Component),
                ActorActionName.Chop_Wood => _populateChop_Wood(jobSite_Component),
                _ => null
            };
        }

        static Dictionary<PriorityParameterName, object> _populateFetch_Items(JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(ActorActionName.Fetch_Items);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to fetch from.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetchFromStation()));
            
            return _setParameters(taskParameters, allRelevantStations, ActorActionName.Fetch_Items);
        }
        
        static Dictionary<PriorityParameterName, object> _populateDeliver_Items(JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(ActorActionName.Deliver_Items);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to deliver to.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToDeliverFromOtherStations()));
            
            return _setParameters(taskParameters, allRelevantStations, ActorActionName.Deliver_Items);
        }
        
        static Dictionary<PriorityParameterName, object> _populateChop_Wood(JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(ActorActionName.Chop_Wood);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to chop wood.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetchFromStation()));
            
            return _setParameters(taskParameters, allRelevantStations, ActorActionName.Chop_Wood);
        }

        static Dictionary<PriorityParameterName, object> _setParameters(
            Dictionary<PriorityParameterName, object> taskParameters, List<Station_Component> allRelevantStations,
            ActorActionName                               actorActionName)
        {
            float highestPriority          = 0;
            var   currentStationParameters = new Dictionary<PriorityParameterName, object>(taskParameters);

            foreach (var station in allRelevantStations)
            {
                currentStationParameters[PriorityParameterName.Target_Component] =
                    station.Station_Data.InventoryData;

                var stationPriority = Priority_Generator.GeneratePriority(PriorityType.JobTask,
                    (uint)actorActionName, currentStationParameters);

                if (stationPriority is 0 || stationPriority < highestPriority) continue;

                highestPriority                                        = stationPriority;
                taskParameters[PriorityParameterName.Target_Component] = station.Station_Data.InventoryData;
            }

            foreach (var parameter in taskParameters)
            {
                if (parameter.Value is not null && parameter.Value is not 0) continue;

                Debug.LogError($"Parameter: {parameter.Key} is null or 0.");
                return null;
            }

            return taskParameters;
        }
    }

    // ActorAction:
    // A spontaneous or situational action unrelated to structured jobs,
    // often driven by immediate needs, combat, exploration, or player commands.
    // These actions typically occur in reaction to dynamic game states.
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
        
        // JobTasks:
        // An action performed as part of a structured job or process,
        // typically repetitive and contributing to resource production, crafting, or other systematic gameplay mechanics.
        // Tasks often require specific tools, conditions, or environments.

        // Smith
        Beat_Metal,
        Forge_Armor,
        Forge_Weapon,
        Sharpen_Sword,
        Repair_Armor,
        Repair_Tool,

        // Lumberjack
        Chop_Wood,
        Process_Logs,

        // Vendor
        Stand_At_Counter,
        Restock_Shelves,
        Sort_Items,

        // Guard
        Defend_Ally,
        Defend_Neutral,

        // Medic
        HealSelf,
        SplintSelf,
        HealAllies,
        SplintAllies,
        HealNeutral,
        SplintNeutral,
        HealEnemies,
        SplintEnemies,

        // Forager
        Gather_Food,

        // Miner
        Mine_Ore,
        Refine_Ore,

        // Farmer
        Sow_Crops,
        Water_Crops,
        Harvest_Crops,

        // Hauler
        Deliver_Items,
        Fetch_Items,

        // Cook
        Cook_Food,

        // Other
        Tinker_Item,
        Spin_Thread,
        Weave_Cloth,

        // Scouts
        Map_Area,
    }
}