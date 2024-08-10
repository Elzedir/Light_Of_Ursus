using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Manager_Jobs : MonoBehaviour
{
    public static List<Job> AllJobs = new();

    public void OnSceneLoaded()
    {
        _initialiseJobs();
    }

    public static Job GetJob(JobName jobName, Jobsite_Base jobsite)
    {
        if (!AllJobs.Any(j => j.JobName == jobName)) throw new ArgumentException($"Job: {jobName} is not in AllJobs list");

        return new Job(AllJobs.FirstOrDefault(j => j.JobName == jobName), jobsite);
    }

    public static Interactable_Lumberjack_DropOffZone GetNearestDropOffZone(string taskObjectName, Actor_Base actor)
    {
        float radius = 100; // Change the distance to depend on the area somehow, later.
        Interactable_Lumberjack_DropOffZone closestDropOffZone = null;
        float closestDistance = float.MaxValue;

        Collider[] colliders = Physics.OverlapSphere(actor.transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Interactable_Lumberjack_DropOffZone dropOffZone = collider.GetComponent<Interactable_Lumberjack_DropOffZone>();

            if (dropOffZone == null) continue;

            if (dropOffZone.name.Contains(taskObjectName))
            {
                float distance = Vector3.Distance(actor.transform.position, dropOffZone.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDropOffZone = dropOffZone;
                }
            }
        }

        return closestDropOffZone;
    }


    void _initialiseJobs()
    {
        AllJobs.Add(_lumberjack());
        AllJobs.Add(_smith());
    }

    Job _lumberjack()
    {
        IEnumerator chopTrees(Actor_Base actor)
        {
            var nearestResource = Manager_ResourceGathering.GetNearestResource(ResourceStationName.Tree, actor.transform.position);
            if (nearestResource == null) { Debug.Log("NearestCraftingStation is null."); yield break; }
            yield return actor.BasicMove(nearestResource.GetGatheringPosition());
            yield return actor.GatheringComponent.GatherResource(nearestResource);
        }

        IEnumerator processTrees(Actor_Base actor)
        {
            var nearestCraftingStation = Manager_Crafting.GetNearestCraftingStation(craftingStationName: CraftingStationName.Sawmill, actor.transform.position);
            if (nearestCraftingStation == null) { Debug.Log("NearestCraftingStation is null."); yield break; }
            yield return actor.BasicMove(nearestCraftingStation.GetCraftingPosition());
            yield return actor.CraftingComponent.CraftItemAll(RecipeName.Plank, nearestCraftingStation);
        }

        IEnumerator dropOffWood(Actor_Base actor)
        {
            var nearestDropOffZone = GetNearestDropOffZone("DropOffZone", actor);

            yield return actor.BasicMove(nearestDropOffZone.transform.position);

            yield return actor.InventoryComponent.TransferItemFromInventory(nearestDropOffZone.InventoryComponent, nearestDropOffZone.GetItemsToDropOff(actor));

            yield return new WaitForSeconds(1);
        }

        IEnumerator sellWood(Actor_Base actor)
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
        IEnumerator smith(Actor_Base actor = null)
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
    public Jobsite_Base Jobsite;
    public List<Task> JobTasks = new();

    public Job(JobName jobName, string jobDescription, List<Task> jobTasks)
    {
        if (Manager_Jobs.AllJobs.Any(j => j.JobName == jobName)) throw new ArgumentException("JobName already exists.");

        JobName = jobName;
        JobDescription = jobDescription;
        JobTasks = jobTasks;
    }

    public Job(Job job, Jobsite_Base jobsite)
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
            yield return Manager_Game.Instance.StartCoroutine(task.GetTaskAction(actor));
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

    public Interactable_Lumberjack_DropOffZone TaskArea;
    public List<AnimationClip> TaskAnimationClips;

    public Func<Actor_Base, IEnumerator> TaskAction;

    public Task(TaskName taskName, string taskDescription, JobName jobName, List<AnimationClip> taskAnimationClips, Func<Actor_Base, IEnumerator> taskAction)
    {
        TaskName = taskName;
        TaskDescription = taskDescription;

        JobName = jobName;

        TaskAnimationClips = taskAnimationClips;

        TaskAction = taskAction;
    }

    public IEnumerator GetTaskAction(Actor_Base actor)
    {
        return TaskAction(actor);
    }
}