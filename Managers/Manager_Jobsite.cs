using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Jobsite : MonoBehaviour
{
    public static AllRegions_SO AllRegions;

    public static Dictionary<int, JobsiteComponent> AllJobsiteComponents = new();

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");

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
                    Debug.LogError($"JobsiteID {jobsite.JobsiteData.JobsiteID} and name {jobsite.name} already exists for jobsite {AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].name}");
                    jobsite.JobsiteData.JobsiteID = GetRandomJobsiteID();
                }
            }
        }
    }

    static List<JobsiteComponent> _findAllJobsiteComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<JobsiteComponent>()
            .ToList();
    }

    public static void AddToOrUpdateAllJobsiteList(int cityID, JobsiteData jobsiteData)
    {
        AllRegions.AddToOrUpdateAllJobsiteDataList(cityID, jobsiteData);
    }

    public static JobsiteData GetJobsiteData(int jobsiteID, int cityID = -1)
    {
        return AllRegions.GetJobsiteData(cityID, jobsiteID);
    }

    public static JobsiteComponent GetJobsite(int jobsiteID)
    {
        return AllJobsiteComponents[jobsiteID];
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

    public static int GetRandomJobsiteID()
    {
        return AllRegions.GetRandomJobsiteID();
    }
}
