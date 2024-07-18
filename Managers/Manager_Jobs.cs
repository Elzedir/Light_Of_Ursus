using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager_Jobs : MonoBehaviour
{
    public static List<Job> AllJobsList { get; private set; } = new();
    static Dictionary<int, (Actor_Base Actor, bool Trigger)> _entities = new();

    public void OnSceneLoaded()
    {
        _initialiseJobs();
    }

    public static Job GetAbility(string name)
    {
        foreach (var ability in AllJobsList)
        {
            if (ability.Name == name) return ability;
        }

        return null;
    }

    void _initialiseJobs()
    {
        AllJobsList.Add(_chopTrees());
        AllJobsList.Add(_smith());
    }

    Job _chopTrees()
    {
        return new Job(
            name: "Chop Trees",
            jobType: JobType.ChopTrees,
            description: "Chop all able trees",
            Resources.Load<AnimationClip>(""),
            null
            );
    }

    Job _smith()
    {
        IEnumerator smith(int ID)
        {
            float elapsedTime = 0;

            while (elapsedTime < 3)
            {
                elapsedTime += Time.deltaTime;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("Player")))
                {
                    if (_entities[ID].Trigger)
                    {
                        // Do something
                        break;
                    }
                }

                yield return null;
            }
        }

        return new Job(
            name: "Smith",
            jobType: JobType.Smith,
            description: "Smith something",
            animationClip: null,
            abilityFunctions: new List<(string Name, Action<int> Function)>()
            {
                ("Smith", (int ID) => StartCoroutine(smith(ID)))
            }
            );
    }
}

public class CharacterJobManager : ITickable
{
    public bool JobsActive { get; private set; }
    public List<Job> AllCurrentJobs { get; private set; }

    Coroutine _jobCoroutine;

    public void OnTick()
    {
        PerformJobs();
    }

    public TickRate GetTickRate()
    {
        return TickRate.Ten;
    }

    public void SetJobActivity(bool jobsActive)
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
        Manager_TickRate.Instance.RegisterTickable(this);

        foreach (Job job in AllCurrentJobs)
        {
            yield return _jobCoroutine = Manager_Game.Instance.StartCoroutine(job.PerformJob());
        }
    }
}

public enum JobType
{
    Patrol,

    DefendAllies,
    DefendNeutral,

    HealSelf,
    SplintSelf,
    HealAllies,
    SplintAllies,
    HealNeutral,
    SplintNeutral,
    HealEnemies,
    SplintEnemies,

    Research,

    Harvest,

    ChopTrees,
    ProcessTrees,

    Smith,
    
}

public class Job
{
    public string Name { get; private set; }
    public JobType JobType { get; private set; }
    public string Description { get; private set; }
    public AnimationClip AnimationClip { get; private set; }
    public List<(string Name, Action<int> Function)> JobFunctions { get; private set; }

    public Job(string name, JobType jobType, string description, AnimationClip animationClip, List<(string, Action<int>)> abilityFunctions)
    {
        Name = name;
        JobType = jobType;
        Description = description;
        AnimationClip = animationClip;
        JobFunctions = abilityFunctions;
    }

    public IEnumerator PerformJob()
    {
        yield return null;
    }

    public Action<int> GetAction(string functionName)
    {
        foreach (var function in JobFunctions)
        {
            if (function.Name == functionName)
            {
                return function.Function;
            }
        }

        return null;
    }
}
