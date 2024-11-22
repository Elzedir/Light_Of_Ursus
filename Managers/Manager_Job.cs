using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class Manager_Job : MonoBehaviour
    {
        const string  _allJobsSOPath = "ScriptableObjects/AllJobs_SO";
        
        static AllJobs_SO _allJobs;
        static AllJobs_SO AllJobs => _allJobs ??= _getOrCreateAllJobsSO();

        public static Job_Master GetJob_Master(JobName jobName) => AllJobs.GetJob_Master(jobName);
        
        public static void PopulateAllJobs()
        {
            AllJobs.PopulateDefaultJobs();
            // Then populate custom jobs.
        }
        
        static AllJobs_SO _getOrCreateAllJobsSO()
        {
            var allJobsSO = Resources.Load<AllJobs_SO>(_allJobsSOPath);
            
            if (allJobsSO is not null) return allJobsSO;
            
            allJobsSO = ScriptableObject.CreateInstance<AllJobs_SO>();
            AssetDatabase.CreateAsset(allJobsSO, $"Assets/Resources/{_allJobsSOPath}");
            AssetDatabase.SaveAssets();
            
            return allJobsSO;
        }
    }

    public enum JobName
    {
        Patrol,

        Defend_Ally,
        Defend_Neutral,

        Medic_Self,
        Medic_Ally,
        Medic_Neutral,
        Medic_Enemy,

        Research,

        Harvest,

        Lumberjack,

        Smith,
    
    }

    public class Job
    {
        public uint StationID;
        public void SetStationID(uint stationID) => StationID = stationID;

        public uint OperatingAreaID;
        public void SetOperatingAreaID(uint operatingAreaID) => OperatingAreaID = operatingAreaID;
    }

    [Serializable]
    public class Job_Master
    {
    
        public JobName           JobName;
        public string            JobDescription;
        public int               JobsiteID;
        public List<Task_Master> JobTasks = new();

        public Job_Master(JobName jobName, string jobDescription, List<Task_Master> jobTasks)
        {
            if (Manager_Job.AllJobs.ContainsKey(jobName)) throw new ArgumentException("JobName already exists.");

            JobName        = jobName;
            JobDescription = jobDescription;
            JobsiteID      = -1;
            JobTasks       = jobTasks;
        }

        public Job_Master(Job_Master jobMaster, int jobsiteID)
        {
            JobName        = jobMaster.JobName;
            JobDescription = jobMaster.JobDescription;
            JobsiteID      = jobsiteID;
            JobTasks       = jobMaster.JobTasks;
        }

        public IEnumerator PerformJob(ActorComponent actor)
        {
            foreach(Task_Master task in JobTasks)
            {
                yield return task.GetTaskAction(actor, JobsiteID);
            }
        }
    }

    [CustomPropertyDrawer(typeof(Job_Master))]
    public class Job_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var    stationNameProp = property.FindPropertyRelative("JobName");
            string stationName     = ((StationName)stationNameProp.enumValueIndex).ToString();

            label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }

    public enum JobTaskName
    {
        Beat_Iron,

        Chop_Trees, Process_Trees, Drop_Off_Wood, Sell_Wood,

        Stand_At_Counter, Restock_Shelves,

        DefendAllies, DefendNeutral,

        HealSelf, SplintSelf,
        HealAllies, SplintAllies,
        HealNeutral, SplintNeutral,
        HealEnemies, SplintEnemies,
    }

    [Serializable]
    public class Task_Master
    {
        public JobTaskName TaskName;
        public string      TaskDescription;

        public JobName JobName;

        public List<AnimationClip> TaskAnimationClips;

        public Func<ActorComponent, int, IEnumerator> TaskAction;

        public Task_Master(JobTaskName taskName, string taskDescription, JobName jobName, List<AnimationClip> taskAnimationClips, Func<ActorComponent, int, IEnumerator> taskAction)
        {
            TaskName        = taskName;
            TaskDescription = taskDescription;

            JobName = jobName;

            TaskAnimationClips = taskAnimationClips;

            TaskAction = taskAction;
        }

        public IEnumerator GetTaskAction(ActorComponent actor, int jobsiteID)
        {
            return TaskAction(actor, jobsiteID);
        }
    }

    [CustomPropertyDrawer(typeof(Task_Master))]
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
}