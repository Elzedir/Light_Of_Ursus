using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jobsite;
using UnityEngine;

public class Manager_Jobsite : MonoBehaviour, IDataPersistence
{
    static AllJobsites_SO _displayAllJobsites;
    public static AllJobsites_SO DisplayAllJobsites { get => _displayAllJobsites ??= Resources.Load<AllJobsites_SO>("ScriptableObjects/AllJobsites_SO"); }

    public static Dictionary<uint, JobsiteData> AllJobsiteData = new();
    static uint _lastUnusedJobsiteID = 1;
    public static Dictionary<uint, JobsiteComponent> AllJobsiteComponents = new();

    public void SaveData(SaveData data) => data.SavedJobsiteData = new SavedJobsiteData(AllJobsiteData.Values.ToList());
    public void LoadData(SaveData data)
    {
        if (data == null)
        {
            //Debug.Log("No SaveData found in LoadData.");
            return;
        }
        if (data.SavedJobsiteData == null)
        {
            //Debug.Log("No SavedJobsiteData found in SaveData.");
            return;
        }
        if (data.SavedJobsiteData.AllJobsiteData == null)
        {
            //Debug.Log("No AllJobsiteData found in SavedJobsiteData.");
            return;
        }
        if (data.SavedJobsiteData.AllJobsiteData.Count == 0)
        {
            //Debug.Log("AllJobsiteData count is 0.");
            return;
        }
        
        AllJobsiteData = data.SavedJobsiteData.AllJobsiteData.ToDictionary(x => x.JobsiteID);
    }

    public void OnSceneLoaded()
    {
        Manager_Initialisation.OnInitialiseManagerJobsite += _initialise;
    }

    void _initialise()
    {
        foreach (var jobsite in _findAllJobsiteComponents())
        {
            if (jobsite.JobsiteData == null) { Debug.Log($"Jobsite: {jobsite.name} does not have JobsiteData."); continue; }

            if (!AllJobsiteComponents.ContainsKey(jobsite.JobsiteData.JobsiteID)) AllJobsiteComponents.Add(jobsite.JobsiteData.JobsiteID, jobsite);
            else
            {
                if (AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].gameObject == jobsite.gameObject) continue;
                else
                {
                    throw new ArgumentException($"JobsiteID {jobsite.JobsiteData.JobsiteID}: {jobsite.name} already exists for jobsite {AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].name}");
                }
            }

            if (!AllJobsiteData.ContainsKey(jobsite.JobsiteData.JobsiteID))
            {
                //Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteID}: {jobsite.JobsiteData.JobsiteName} was not in AllJobsiteData");
                AddToAllJobsiteData(jobsite.JobsiteData);
            }

            jobsite.SetJobsiteData(GetJobsiteData(jobsite.JobsiteData.JobsiteID));
        }

        foreach (var jobsiteData in AllJobsiteData.Values)
        {
            jobsiteData.InitialiseJobsiteData();
        }

        DisplayAllJobsites.AllJobsiteData = AllJobsiteData.Values.ToList();

        foreach (var jobsite in AllJobsiteData)
        {
            jobsite.Value.ProsperityData.SetProsperity(50);
            jobsite.Value.ProsperityData.MaxProsperity = 100;
        }
    }

    static List<JobsiteComponent> _findAllJobsiteComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<JobsiteComponent>()
            .ToList();
    }

    public static void GetNearestJobsite(Vector3 position, out JobsiteComponent nearestJobsite)
    {
        nearestJobsite = null;
        float nearestDistance = float.MaxValue;

        foreach (var jobsite in AllJobsiteComponents)
        {
            float distance = Vector3.Distance(position, jobsite.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestJobsite = jobsite.Value;
                nearestDistance = distance;
            }
        }
    }

    public void AddToAllJobsiteData(JobsiteData jobsiteData)
    {
        if (AllJobsiteData.ContainsKey(jobsiteData.JobsiteID))
        {
            Debug.Log($"AllJobsiteData already contains JobsiteID: {jobsiteData.JobsiteID}");
            return;
        }

        AllJobsiteData.Add(jobsiteData.JobsiteID, jobsiteData);
    }

    public void UpdateAllJobsiteData(JobsiteData jobsiteData)
    {
        if (!AllJobsiteData.ContainsKey(jobsiteData.JobsiteID))
        {
            Debug.LogError($"JobsiteData: {jobsiteData.JobsiteID} does not exist in AllJobsiteData.");
            return;
        }

        AllJobsiteData[jobsiteData.JobsiteID] = jobsiteData;
    }

    public static JobsiteData GetJobsiteData(uint jobsiteID)
    {
        if (!AllJobsiteData.ContainsKey(jobsiteID))
        {
            Debug.LogError($"JobsiteData: {jobsiteID} does not exist in AllJobsiteData.");
            return null;
        }

        return AllJobsiteData[jobsiteID];
    }

    
    public static JobsiteComponent GetJobsite(uint jobsiteID)
    {
        if (!AllJobsiteComponents.ContainsKey(jobsiteID))
        {
            Debug.LogError($"JobsiteComponent: {jobsiteID} does not exist in AllJobsiteComponents.");
            return null;
        }

        return AllJobsiteComponents[jobsiteID];
    }

    public uint GetRandomJobsiteID()
    {
        while (AllJobsiteData.ContainsKey(_lastUnusedJobsiteID))
        {
            _lastUnusedJobsiteID++;
        }

        return _lastUnusedJobsiteID;
    }
}
