using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Priority;
using Station;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    public abstract class Manager_JobTask
    {
        public static JobTask_Master GetJobTask(JobTaskName jobTaskName) =>
            _allJobTasks[jobTaskName];

        static readonly Dictionary<JobTaskName, JobTask_Master> _allJobTasks =
            new()
            {
                {
                    JobTaskName.Beat_Metal, new JobTask_Master(
                        JobTaskName.Beat_Metal, "Beat Iron into Shape",
                        (actor, jobsiteID) => actor.StartCoroutine(_beatIron(actor, jobsiteID)))
                },
                {
                    JobTaskName.Chop_Wood, new JobTask_Master(
                        JobTaskName.Chop_Wood, "Chop Wood",
                        (actor, jobsiteID) => actor.StartCoroutine(_chopWood(actor, jobsiteID)))
                },
                {
                    JobTaskName.Process_Logs, new JobTask_Master(
                        JobTaskName.Process_Logs, "Process Logs",
                        (actor, jobsiteID) => actor.StartCoroutine(_processLogs(actor, jobsiteID)))
                },
                {
                    JobTaskName.Drop_Off_Wood, new JobTask_Master(
                        JobTaskName.Drop_Off_Wood, "Drop Off Wood",
                        (actor, jobsiteID) => actor.StartCoroutine(_dropOffWood(actor, jobsiteID)))
                },
                {
                    JobTaskName.Stand_At_Counter, new JobTask_Master(
                        JobTaskName.Stand_At_Counter, "Stand at Counter",
                        (actor, jobsiteID) => actor.StartCoroutine(_standAtCounter(actor, jobsiteID)))
                },
                {
                    JobTaskName.Restock_Shelves, new JobTask_Master(
                        JobTaskName.Restock_Shelves, "Restock Shelves",
                        (actor, jobsiteID) => actor.StartCoroutine(_restockShelves(actor, jobsiteID)))
                },
                {
                    JobTaskName.Defend_Ally, new JobTask_Master(
                        JobTaskName.Defend_Ally, "Defend Ally",
                        (actor, jobsiteID) => actor.StartCoroutine(_defendAlly(actor, jobsiteID)))
                },
                {
                    JobTaskName.Defend_Neutral, new JobTask_Master(
                        JobTaskName.Defend_Neutral, "Defend Neutral",
                        (actor, jobsiteID) => actor.StartCoroutine(_defendNeutral(actor, jobsiteID)))
                },
            };

        static IEnumerator _beatIron(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _chopWood(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _processLogs(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _dropOffWood(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _standAtCounter(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _restockShelves(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _defendAlly(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _defendNeutral(ActorComponent actor, int jobsiteID)
        {
            yield return null;
        }

        static IEnumerator _defendAllies(ActorComponent actor, int jobsiteID)
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
    }

    [Serializable]
    public class JobTask_Master
    {
        public          JobTaskName                               JobTaskName;
        public          JobTaskGroup                              TaskGroup;
        public          string                                    TaskDescription;
        public readonly Dictionary<PriorityParameterName, object> JobTaskParameters;
        public          Func<ActorComponent, int, Coroutine>      TaskAction;

        a
        // Initialise JobTask_Master here

        public Coroutine GetTaskAction(ActorComponent actor, int jobsiteID)
        {
            return TaskAction(actor, jobsiteID);
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
        // General
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
    
    public class JobTaskToChange
    {
        public JobTaskName    JobTaskName;
        public PriorityImportance PriorityImportance;

        public JobTaskToChange(JobTaskName jobTaskName, PriorityImportance priorityImportance)
        {
            JobTaskName    = jobTaskName;
            PriorityImportance = priorityImportance;
        }
    }
}
