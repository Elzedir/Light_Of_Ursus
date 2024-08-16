using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Manager_Job : MonoBehaviour
{
    public static Dictionary<JobName, Job> AllJobs = new();

    public void OnSceneLoaded()
    {
        _initialiseJobs();
    }

    public static Job GetJob(JobName jobName, int jobsiteID)
    {
        if (!AllJobs.ContainsKey(jobName)) throw new ArgumentException($"Job: {jobName} is not in AllJobs list");

        return new Job(AllJobs[jobName], jobsiteID);
    }

    void _initialiseJobs()
    {
        AllJobs.Add(JobName.Lumberjack, _lumberjack());
        AllJobs.Add(JobName.Smith, _smith());
    }

    Job _lumberjack()
    {
        IEnumerator chopTrees(Actor_Base actor, int jobsiteID)
        {
            // If jobsite is null, then don't do anything unless there's an unowned jobsite
            StationComponent nearestStation = Manager_Jobsite.GetJobsite(jobsiteID).GetNearestStationInJobsite(actor.transform.position, StationName.Tree);
            if (nearestStation == null) { Debug.Log("Nearest Sawmill is null."); yield break; }
            yield return actor.BasicMove(nearestStation.AllOperatingAreasInStation.FirstOrDefault(kv => !kv.Value).Key.bounds.center);
            nearestStation.SetOperator(actor);
        }

        // Find a way to manage the jobs so that it doesn't use a fixed list but rather inventory requirements. Or can use a fixed list like Kenshi, but have
        // limits for time.

        IEnumerator processTrees(Actor_Base actor, int jobsiteID)
        {
            StationComponent nearestStation = Manager_Jobsite.GetJobsite(jobsiteID).GetNearestStationInJobsite(actor.transform.position, StationName.Sawmill);
            if (nearestStation == null) { Debug.Log("Nearest Tree is null."); yield break; }
            yield return actor.BasicMove(nearestStation.AllOperatingAreasInStation.FirstOrDefault(kv => !kv.Value).Key.bounds.center);
            nearestStation.SetOperator(actor);
        }

        IEnumerator dropOffWood(Actor_Base actor, int jobsiteID)
        {
            var nearestStation = Manager_Jobsite.GetJobsite(jobsiteID).GetNearestStationInJobsite(actor.transform.position, StationName.Log_Pile);
            if (nearestStation == null) { Debug.Log("Nearest LogPile is null."); yield break; }
            yield return actor.BasicMove(nearestStation.AllOperatingAreasInStation.FirstOrDefault(kv => !kv.Value).Key.bounds.center);
            yield return actor.ActorData.InventoryAndEquipment.InventoryData.TransferItemFromInventory(nearestStation.StationData.InventoryData, nearestStation.GetItemsToDropOff(actor));
            nearestStation.SetOperator(actor);
        }

        IEnumerator sellWood(Actor_Base actor, int jobsiteID)
        {
            yield return null;
        }

        return new Job(
            jobName: JobName.Lumberjack,
            jobDescription: "Lumberjack",
            new List<Task>
            {
                new Task(
                    taskName: TaskName.Chop_Trees,
                    taskDescription: "Chop trees",
                    jobName: JobName.Lumberjack,
                    taskAnimationClips: null,
                    taskAction: chopTrees
                    ),
                new Task(
                    taskName: TaskName.Process_Trees,
                    taskDescription: "Process logs into wood",
                    jobName: JobName.Lumberjack,
                    taskAnimationClips: null,
                    taskAction: processTrees
                    ),
                new Task(
                    taskName: TaskName.Drop_Off_Wood,
                    taskDescription: "Drop wood in woodpile",
                    jobName: JobName.Lumberjack,
                    taskAnimationClips: null,
                    taskAction: dropOffWood
                    ),
                new Task(
                    taskName: TaskName.Sell_Wood,
                    taskDescription: "Sell wood",
                    jobName: JobName.Lumberjack,
                    taskAnimationClips: null,
                    taskAction: sellWood
                    )
            });
    }

    Job _smith()
    {
        IEnumerator smith(Actor_Base actor, int jobsite)
        {
            if (actor == null) throw new ArgumentException("Actor is null;");

            yield return null;
        }

        return new Job(
            jobName: JobName.Smith,
            jobDescription: "Smith something",
            new List<Task>
            {
                new Task(
                    taskName: TaskName.Beat_Iron,
                    taskDescription: "Beat iron",
                    jobName: JobName.Smith,
                    taskAnimationClips: null,
                    taskAction: smith
                    )
            });
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

[Serializable]
public class Job
{
    public JobName JobName;
    public string JobDescription;
    public int JobsiteID;
    public List<Task> JobTasks = new();

    public Job(JobName jobName, string jobDescription, List<Task> jobTasks)
    {
        if (Manager_Job.AllJobs.ContainsKey(jobName)) throw new ArgumentException("JobName already exists.");

        JobName = jobName;
        JobDescription = jobDescription;
        JobsiteID = -1;
        JobTasks = jobTasks;
    }

    public Job(Job job, int jobsiteID)
    {
        JobName = job.JobName;
        JobDescription = job.JobDescription;
        JobsiteID = jobsiteID;
        JobTasks = job.JobTasks;
    }

    public IEnumerator PerformJob(Actor_Base actor)
    {
        foreach(Task task in JobTasks)
        {
            yield return task.GetTaskAction(actor, JobsiteID);
        }
    }
}

[CustomPropertyDrawer(typeof(Job))]
public class Job_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stationNameProp = property.FindPropertyRelative("JobName");
        string stationName = ((StationName)stationNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}

public enum TaskName 
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
public class Task
{
    public TaskName TaskName;
    public string TaskDescription;

    public JobName JobName;

    public List<AnimationClip> TaskAnimationClips;

    public Func<Actor_Base, int, IEnumerator> TaskAction;

    public Task(TaskName taskName, string taskDescription, JobName jobName, List<AnimationClip> taskAnimationClips, Func<Actor_Base, int, IEnumerator> taskAction)
    {
        TaskName = taskName;
        TaskDescription = taskDescription;

        JobName = jobName;

        TaskAnimationClips = taskAnimationClips;

        TaskAction = taskAction;
    }

    public IEnumerator GetTaskAction(Actor_Base actor, int jobsiteID)
    {
        return TaskAction(actor, jobsiteID);
    }
}

[CustomPropertyDrawer(typeof(Task))]
public class Task_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stationNameProp = property.FindPropertyRelative("TaskName");
        string stationName = ((StationName)stationNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}