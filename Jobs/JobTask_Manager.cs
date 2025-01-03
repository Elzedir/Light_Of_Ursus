using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Items;
using JobSite;
using Priority;
using Station;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    public abstract class JobTask_Manager
    {
        public static JobTask_Master GetJobTask_Master(JobTaskName jobTaskName)
        {
            if (_allJobTask_Masters.TryGetValue(jobTaskName, out var jobTaskMaster))
            {
                return jobTaskMaster;
            }

            Debug.LogError($"No JobTaskMaster found for {jobTaskName}. Returning null.");
            return null;
        }

        static readonly Dictionary<JobTaskName, JobTask_Master> _allJobTask_Masters =
            new()
            {
                {
                    JobTaskName.Fetch_Items, new JobTask_Master(
                        taskName: JobTaskName.Fetch_Items,
                        taskDescription: "Fetch Items",
                        primaryJob: JobName.Any,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _fetchItems
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Deliver_Items, new JobTask_Master(
                        taskName: JobTaskName.Deliver_Items,
                        taskDescription: "Deliver Items",
                        primaryJob: JobName.Any,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _deliverItems
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })  
                },
                {
                    JobTaskName.Beat_Metal, new JobTask_Master(
                        taskName: JobTaskName.Beat_Metal,
                        taskDescription: "Beat Iron into Shape",
                        primaryJob: JobName.Smith,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _beatIron
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Chop_Wood, new JobTask_Master(
                        taskName: JobTaskName.Chop_Wood,
                        taskDescription: "Chop Wood",
                        primaryJob: JobName.Logger,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _chopWood
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Process_Logs, new JobTask_Master(
                        taskName: JobTaskName.Process_Logs,
                        taskDescription: "Process Logs",
                        primaryJob: JobName.Sawyer,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _processLogs
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Stand_At_Counter, new JobTask_Master(
                        taskName: JobTaskName.Stand_At_Counter,
                        taskDescription: "Stand at Counter",
                        primaryJob: JobName.Vendor,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _standAtCounter
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Restock_Shelves, new JobTask_Master(
                        taskName: JobTaskName.Restock_Shelves,
                        taskDescription: "Restock Shelves",
                        primaryJob: JobName.Vendor,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _restockShelves
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Defend_Ally, new JobTask_Master(
                        taskName: JobTaskName.Defend_Ally,
                        taskDescription: "Defend Ally",
                        primaryJob: JobName.Guard,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _defendAlly
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
                {
                    JobTaskName.Defend_Neutral, new JobTask_Master(
                        taskName: JobTaskName.Defend_Neutral,
                        taskDescription: "Defend Neutral",
                        primaryJob: JobName.Guard,
                        taskList: new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _defendNeutral
                        },
                        requiredParameters: new List<PriorityParameterName>
                        {
                            PriorityParameterName.Jobsite_Component
                        })
                },
            };

        static IEnumerator _beatIron(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _chopWood(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _processLogs(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _fetchItems(Actor_Component actor, uint jobsiteID)
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

        static IEnumerator _standAtCounter(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _restockShelves(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _defendAlly(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(Actor_Component actor, uint jobsiteID)
        {
            yield return null;
        }

        public static Dictionary<PriorityParameterName, object> PopulateTaskParameters(
            JobTaskName jobTaskName, Dictionary<PriorityParameterName, object> parameters)
        {
            JobSite_Component jobSite_Component = null; 

            var requiredParameters = _allJobTask_Masters[jobTaskName].RequiredParameters;
                    
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
            
            return jobTaskName switch
            {
                JobTaskName.Fetch_Items => _populateFetch_Items(jobSite_Component),
                JobTaskName.Deliver_Items => _populateDeliver_Items(jobSite_Component),
                JobTaskName.Chop_Wood => _populateChop_Wood(jobSite_Component),
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
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(JobTaskName.Fetch_Items);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to fetch from.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetchFromStation()));
            
            return _setParameters(taskParameters, allRelevantStations, JobTaskName.Fetch_Items);
        }
        
        static Dictionary<PriorityParameterName, object> _populateDeliver_Items(JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(JobTaskName.Deliver_Items);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to deliver to.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToDeliverFromOtherStations()));
            
            return _setParameters(taskParameters, allRelevantStations, JobTaskName.Deliver_Items);
        }
        
        static Dictionary<PriorityParameterName, object> _populateChop_Wood(JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };
            
            var allRelevantStations = jobSite_Component.GetRelevantStations(JobTaskName.Chop_Wood);
            
            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to chop wood.");
                return null;
            }
            
            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetchFromStation()));
            
            return _setParameters(taskParameters, allRelevantStations, JobTaskName.Chop_Wood);
        }

        static Dictionary<PriorityParameterName, object> _setParameters(
            Dictionary<PriorityParameterName, object> taskParameters, List<Station_Component> allRelevantStations,
            JobTaskName                               jobTaskName)
        {
            float highestPriority          = 0;
            var   currentStationParameters = new Dictionary<PriorityParameterName, object>(taskParameters);

            foreach (var station in allRelevantStations)
            {
                currentStationParameters[PriorityParameterName.Target_Component] =
                    station.Station_Data.InventoryDataPreset;

                var stationPriority = Priority_Generator.GeneratePriority(PriorityType.JobTask,
                    (uint)jobTaskName, currentStationParameters);

                if (stationPriority is 0 || stationPriority < highestPriority) continue;

                highestPriority                                        = stationPriority;
                taskParameters[PriorityParameterName.Target_Component] = station.Station_Data.InventoryDataPreset;
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

    [Serializable]
    public class JobTask_Master
    {
        public JobTaskName                                    TaskName;
        public string                                         TaskDescription;
        public List<PriorityParameterName>                    RequiredParameters;
        public JobName                                        PrimaryJob;
        public List<Func<Actor_Component, uint, IEnumerator>> TaskList;

        public JobTask_Master(JobTaskName                                    taskName, string taskDescription, JobName primaryJob,
                              List<Func<Actor_Component, uint, IEnumerator>> taskList,
                              List<PriorityParameterName>                    requiredParameters)
        {
            TaskName           = taskName;
            TaskDescription    = taskDescription;
            PrimaryJob         = primaryJob;
            TaskList           = taskList;
            RequiredParameters = requiredParameters;
        }
    }

    [CustomPropertyDrawer(typeof(JobTask_Master))]
    public class Task_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var stationNameProp = property.FindPropertyRelative("TaskName");
            var stationName     = ((StationName)stationNameProp.enumValueIndex).ToString();

            label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }

    // Task:
    // An action performed as part of a structured job or process,
    // typically repetitive and contributing to resource production, crafting, or other systematic gameplay mechanics.
    // Tasks often require specific tools, conditions, or environments.
    public enum JobTaskName
    {
        // General (Can do in all situations)
        Idle,

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
