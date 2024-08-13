using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Manager_Job : MonoBehaviour
{
    public static Dictionary<JobName, Job> AllJobs = new();

    public void OnSceneLoaded()
    {
        _initialiseJobs();
    }

    public static Job GetJob(JobName jobName, JobsiteComponent jobsite)
    {
        if (!AllJobs.ContainsKey(jobName)) throw new ArgumentException($"Job: {jobName} is not in AllJobs list");

        return new Job(AllJobs[jobName], jobsite);
    }

    void _initialiseJobs()
    {
        AllJobs.Add(JobName.Lumberjack, _lumberjack());
        AllJobs.Add(JobName.Smith, _smith());
    }

    Job _lumberjack()
    {
        IEnumerator chopTrees(Actor_Base actor, JobsiteComponent jobsite)
        {
            // If jobsite is null, then don't do anything unless there's an unowned jobsite
            StationComponent_Resource nearestStation = jobsite.GetNearestResourceStationInJobsite(actor.transform.position, StationName.Tree);
            if (nearestStation == null) { Debug.Log("NearestStation is null."); yield break; }
            yield return actor.BasicMove(nearestStation.GetNearestOperatingAreaInStation(actor.transform.position));
            yield return nearestStation.GatherResource(actor);
        }

        IEnumerator processTrees(Actor_Base actor, JobsiteComponent jobsite)
        {
            StationComponent_Crafter nearestStation = jobsite.GetNearestCraftingStationInJobsite(actor.transform.position, StationName.Sawmill);
            if (nearestStation == null) { Debug.Log("NearestStation is null."); yield break; }
            yield return actor.BasicMove(nearestStation.GetNearestOperatingAreaInStation(actor.transform.position));
            yield return nearestStation.CraftItemAll(actor);
        }

        IEnumerator dropOffWood(Actor_Base actor, JobsiteComponent jobsite)
        {
            var nearestDropOffZone = jobsite.GetNearestDropOffStationInJobsite(actor.transform.position, StationName.None);

            yield return actor.BasicMove(nearestDropOffZone.transform.position);

            // yield return actor.ActorData.InventoryAndEquipment.Inventory.TransferItemFromInventory(nearestDropOffZone.InventoryData, nearestDropOffZone.GetItemsToDropOff(actor));

            yield return new WaitForSeconds(1);
        }

        IEnumerator sellWood(Actor_Base actor, JobsiteComponent jobsite)
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
        IEnumerator smith(Actor_Base actor, JobsiteComponent jobsite)
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
    public JobsiteComponent Jobsite;
    public List<Task> JobTasks = new();

    public Job(JobName jobName, string jobDescription, List<Task> jobTasks)
    {
        if (Manager_Job.AllJobs.ContainsKey(jobName)) throw new ArgumentException("JobName already exists.");

        JobName = jobName;
        JobDescription = jobDescription;
        Jobsite = null;
        JobTasks = jobTasks;
    }

    public Job(Job job, JobsiteComponent jobsite)
    {
        JobName = job.JobName;
        JobDescription = job.JobDescription;
        Jobsite = jobsite;
        JobTasks = job.JobTasks;
    }

    public IEnumerator PerformJob(Actor_Base actor)
    {
        foreach(Task task in JobTasks)
        {
            yield return task.GetTaskAction(actor, Jobsite);
        }
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

    public Func<Actor_Base, JobsiteComponent, IEnumerator> TaskAction;

    public Task(TaskName taskName, string taskDescription, JobName jobName, List<AnimationClip> taskAnimationClips, Func<Actor_Base, JobsiteComponent, IEnumerator> taskAction)
    {
        TaskName = taskName;
        TaskDescription = taskDescription;

        JobName = jobName;

        TaskAnimationClips = taskAnimationClips;

        TaskAction = taskAction;
    }

    public IEnumerator GetTaskAction(Actor_Base actor, JobsiteComponent jobsite)
    {
        return TaskAction(actor, jobsite);
    }
}