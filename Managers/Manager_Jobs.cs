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

    public static Job GetJob(JobName jobName)
    {
        if (!AllJobs.Any(j => j.JobName == jobName)) throw new ArgumentException($"Job: {jobName} is not in AllJobs list");

        return AllJobs.FirstOrDefault(j => j.JobName == jobName);
    }

    public static Collider GetTaskArea(Actor_Base actor, string taskObjectName)
    {
        float radius = 100; // Change the distance to depend on the area somehow, later.
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;

        Collider[] colliders = Physics.OverlapSphere(actor.transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.name.Contains(taskObjectName))
            {
                float distance = Vector3.Distance(actor.transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = collider;
                }
            }
        }

        return closestCollider;
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
            var nearestResource = Manager_ResourceGathering.GetNearestResource(ResourceName.Tree, actor.transform.position);

            yield return actor.BasicMove(nearestResource.GetComponent<Collider>().bounds.center);
            yield return actor.GatheringComponent.GatherResource(nearestResource);
        }

        IEnumerator processTrees(Actor_Base actor)
        {
            var nearestCraftingStation = Manager_Crafting.GetNearestCraftingStation(craftingStationName: CraftingStationName.Sawmill, actor.transform.position);
            yield return actor.BasicMove(nearestCraftingStation.GetComponent<Collider>().bounds.center);
            yield return actor.CraftingComponent.CraftItemAll(RecipeName.Plank, nearestCraftingStation);
        }

        IEnumerator dropOffWood(Actor_Base actor)
        {
            yield return actor.BasicMove(Manager_Jobs.GetTaskArea(actor, "DropOffZone").bounds.center);

            yield return new WaitForSeconds(3);
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

public class JobComponent : ITickable
{
    public Actor_Base Actor;
    public Career Career;
    public bool JobsActive;
    public List<Job> AllCurrentJobs = new();

    Coroutine _jobCoroutine;

    public JobComponent(Actor_Base actor, CareerName careerName, List<Job> allCurrentJobs, bool jobsActive = false)
    {
        Actor = actor;
        Career = Manager_Career.GetCareer(careerName);
        JobsActive = jobsActive;
        AllCurrentJobs = allCurrentJobs;

        Manager_TickRate.Instance.RegisterTickable(this);
    }

    public void OnTick()
    {
        Manager_Game.Instance.StartCoroutine(PerformJobs());
    }

    public TickRate GetTickRate()
    {
        return TickRate.One;
    }

    public void ToggleDoJobs(bool jobsActive)
    {
        JobsActive = jobsActive;
    }

    public void AddJob(Job job)
    {
        if (job == null) { Debug.Log("Job is null"); return; }

        if (AllCurrentJobs.Contains(job)) { Debug.Log("AllCurrentJobs already contains job."); return; }

        AllCurrentJobs.Add(job);
    }

    public void RemoveJob(Job job = null)
    {
        for (int i = 0; i < AllCurrentJobs.Count; i++)
        {
            if (job == AllCurrentJobs[i] || job == null)
            {
                AllCurrentJobs.Remove(job);
            }
        }
    }

    public void ReorganiseJobs(Job job, int index)
    {
        if (job == null) { Debug.Log("Job is null"); return; }
        if (index < 0 || index > AllCurrentJobs.Count) { Debug.Log($"Index: {index} is less than 0 or greater than AllCurrentJobs length: {AllCurrentJobs.Count}."); return; }

        for (int i = AllCurrentJobs.Count - 1; i > index; i--)
        {
            AllCurrentJobs[i] = AllCurrentJobs[i - 1];
        }

        AllCurrentJobs[index] = job;
    }

    public IEnumerator PerformJobs()
    {
        if (_jobCoroutine != null) yield break;

        foreach (Job job in AllCurrentJobs)
        {
            yield return _jobCoroutine = Manager_Game.Instance.StartCoroutine(job.PerformJob(Actor));
        }

        _jobCoroutine = null;
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
    public Collider JobArea;
    public List<Task> JobTasks = new();

    public Job(JobName jobName, string jobDescription, List<Task> jobTasks)
    {
        if (Manager_Jobs.AllJobs.Any(j => j.JobName == jobName)) throw new ArgumentException("JobName already exists.");

        JobName = jobName;
        JobDescription = jobDescription;
        JobTasks = jobTasks;
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

public class Task
{
    public TaskName TaskName;
    public string TaskDescription;

    public JobName JobName;

    public Collider TaskArea;
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