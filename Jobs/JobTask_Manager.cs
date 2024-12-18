using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
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
        public static JobTask GetJobTask(JobTaskName jobTaskName) =>
            new(jobTaskName, GetEmptyTaskParameters(jobTaskName));

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
                    JobTaskName.Beat_Metal, new JobTask_Master(
                        JobTaskName.Beat_Metal,
                        "Beat Iron into Shape",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _beatIron
                        })
                },
                {
                    JobTaskName.Chop_Wood, new JobTask_Master(
                        JobTaskName.Chop_Wood,
                        "Chop Wood",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _chopWood
                        })
                },
                {
                    JobTaskName.Process_Logs, new JobTask_Master(
                        JobTaskName.Process_Logs,
                        "Process Logs",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _processLogs
                        })
                },
                {
                    JobTaskName.Drop_Off_Wood, new JobTask_Master(
                        JobTaskName.Drop_Off_Wood,
                        "Drop Off Wood",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _dropOffWood
                        })
                },
                {
                    JobTaskName.Stand_At_Counter, new JobTask_Master(
                        JobTaskName.Stand_At_Counter,
                        "Stand at Counter",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _standAtCounter
                        })
                },
                {
                    JobTaskName.Restock_Shelves, new JobTask_Master(
                        JobTaskName.Restock_Shelves,
                        "Restock Shelves",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _restockShelves
                        })
                },
                {
                    JobTaskName.Defend_Ally, new JobTask_Master(
                        JobTaskName.Defend_Ally,
                        "Defend Ally",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _defendAlly
                        })
                },
                {
                    JobTaskName.Defend_Neutral, new JobTask_Master(
                        JobTaskName.Defend_Neutral,
                        "Defend Neutral",
                        new List<Func<Actor_Component, uint, IEnumerator>>
                        {
                            _defendNeutral
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

        static IEnumerator _fetchWood(Actor_Component actor, uint jobsiteID)
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

        IEnumerator _moveOperatorToOperatingArea(Actor_Component actor, Vector3 position)
        {
            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.transform.position != position) actor.transform.position = position;
        }

        static IEnumerator _dropOffWood(Actor_Component actor, uint jobsiteID)
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

        public static List<JobTaskName> GetTaskGroup(JobTaskGroup taskGroup) => _allTaskGroups[taskGroup];

        static readonly Dictionary<JobTaskGroup, List<JobTaskName>> _allTaskGroups = new()
        {
            {
                JobTaskGroup.Normal, new List<JobTaskName>()
                {
                    JobTaskName.Beat_Metal,
                    JobTaskName.Chop_Wood,
                    JobTaskName.Process_Logs,
                    JobTaskName.Drop_Off_Wood,
                    JobTaskName.Stand_At_Counter,
                    JobTaskName.Restock_Shelves,
                }
            },
            {
                JobTaskGroup.Combat, new List<JobTaskName>()
                {
                    JobTaskName.Defend_Ally,
                    JobTaskName.Defend_Neutral,
                }
            },
            {
                JobTaskGroup.Recreation, new List<JobTaskName>()
                {
                    JobTaskName.Stand_At_Counter,
                    JobTaskName.Restock_Shelves,
                }
            },
            {
                JobTaskGroup.Work, new List<JobTaskName>()
                {
                    JobTaskName.Beat_Metal,
                    JobTaskName.Chop_Wood,
                    JobTaskName.Process_Logs,
                    JobTaskName.Drop_Off_Wood,
                }
            },
        };

        public static Dictionary<PriorityParameterName, object> GetTaskParameters(
            JobTaskName jobTaskName, Dictionary<PriorityParameterName, object> existingParameters)
        {
            var emptyTaskParameters = GetEmptyTaskParameters(jobTaskName);

            return jobTaskName switch
            {
                JobTaskName.Fetch_Items => _populateFetchTaskParameters(emptyTaskParameters, existingParameters),
                _                       => null
            };
        }

        public static Dictionary<PriorityParameterName, object> GetEmptyTaskParameters(JobTaskName jobTaskName)
        {
            if (_allDefaultTaskParameters.TryGetValue(jobTaskName, out var defaultParameters))
            {
                return defaultParameters.ToDictionary<PriorityParameterName, PriorityParameterName, object>(
                    parameterName => parameterName, _ => null);
            }

            Debug.LogWarning($"No default parameters found for {jobTaskName}. Returning empty dictionary.");
            return new Dictionary<PriorityParameterName, object>();
        }

        static readonly Dictionary<JobTaskName, List<PriorityParameterName>> _allDefaultTaskParameters = new()
        {
            {
                JobTaskName.Beat_Metal, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Chop_Wood, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Process_Logs, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Fetch_Items, new List<PriorityParameterName>()
                {
                    PriorityParameterName.TotalItems,
                    PriorityParameterName.TotalDistance,
                    PriorityParameterName.InventoryHauler,
                    PriorityParameterName.InventoryTarget,
                }
            },
            {
                JobTaskName.Drop_Off_Wood, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Stand_At_Counter, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Restock_Shelves, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Defend_Ally, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            },
            {
                JobTaskName.Defend_Neutral, new List<PriorityParameterName>()
                {
                    PriorityParameterName.DefaultPriority,
                }
            }
        };

        static Dictionary<PriorityParameterName, object> _populateFetchTaskParameters(
            Dictionary<PriorityParameterName, object> emptyTaskParameters,
            Dictionary<PriorityParameterName, object> existingParameters)
        {
            if (!existingParameters.TryGetValue(PriorityParameterName.Jobsite, out var jobsiteObject) ||
                jobsiteObject is not JobSite_Component jobsite)
            {
                Debug.LogError("No jobsite found in existing parameters.");
                return null;
            }

            if (!existingParameters.TryGetValue(PriorityParameterName.InventoryHauler, out var haulerObject) ||
                haulerObject is not Actor_Component hauler)
            {
                Debug.LogError("No hauler found in existing parameters.");
                return null;
            }

            var taskParameters = new Dictionary<PriorityParameterName, object>(emptyTaskParameters)
            {
                [PriorityParameterName.TotalItems]      = 0,
                [PriorityParameterName.TotalDistance]   = 0,
                [PriorityParameterName.InventoryHauler] = null,
                [PriorityParameterName.InventoryTarget] = null,
            };

            var allRelevantStations =
                jobsite.GetRelevantStations(JobTaskName.Fetch_Items, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.LogError("No stations to fetch from.");
                return null;
            }

            var totalItemsToFetch = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
            var totalDistance = allRelevantStations.Sum(station =>
                Vector3.Distance(hauler.transform.position, station.transform.position));
            var highestFetchPriority = PriorityGenerator.GeneratePriority(PriorityType.JobTask,
                (uint)JobTaskName.Fetch_Items, taskParameters);

            var stationParameters = new Dictionary<PriorityParameterName, object>(taskParameters)
            {
                [PriorityParameterName.TotalItems]      = totalItemsToFetch,
                [PriorityParameterName.TotalDistance]   = totalDistance,
                [PriorityParameterName.InventoryHauler] = hauler.ActorData.InventoryData,
            };

            foreach (var station in allRelevantStations)
            {
                stationParameters[PriorityParameterName.InventoryTarget] = station.Station_Data.InventoryData;

                var stationPriority = PriorityGenerator.GeneratePriority(PriorityType.JobTask,
                    (uint)JobTaskName.Fetch_Items, stationParameters);

                if (stationPriority is 0 || stationPriority < highestFetchPriority) continue;

                taskParameters[PriorityParameterName.InventoryTarget] = station.Station_Data.InventoryData;
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

    public class JobTask
    {
        public readonly JobTaskName TaskName;

        public readonly Dictionary<PriorityParameterName, object> TaskParameters;

        JobTask_Master        _jobTask_Master;
        public JobTask_Master JobTask_Master => _jobTask_Master ??= JobTask_Manager.GetJobTask_Master(TaskName);

        public JobTask(JobTaskName taskName, Dictionary<PriorityParameterName, object> taskParameters)
        {
            TaskName       = taskName;
            TaskParameters = taskParameters;
        }
    }

    [Serializable]
    public class JobTask_Master
    {
        public JobTaskName                                   TaskName;
        public JobTaskGroup                                  TaskGroup;
        public string                                        TaskDescription;
        public List<Func<Actor_Component, uint, IEnumerator>> TaskList;

        public JobTask_Master(JobTaskName                                   taskName, string taskDescription,
                              List<Func<Actor_Component, uint, IEnumerator>> taskList)
        {
            TaskName        = taskName;
            TaskDescription = taskDescription;
            TaskList        = taskList;
        }
    }

    [CustomPropertyDrawer(typeof(JobTask_Master))]
    public class Task_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var    stationNameProp = property.FindPropertyRelative("TaskName");
            string stationName     = ((StationName)stationNameProp.enumValueIndex).ToString();

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
        Drop_Off_Wood,

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

        // Minder
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

    public enum JobTaskGroup
    {
        Normal,
        Combat,
        Recreation,
        Work,
    }
}
