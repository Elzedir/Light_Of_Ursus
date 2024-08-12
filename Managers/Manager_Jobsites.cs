using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Jobsites : MonoBehaviour
{
    public static AllRegions_SO AllRegions;

    public static Dictionary<int, JobsiteComponent> AllJobsiteComponents;

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");

        Manager_Initialisation.OnInitialiseManagerJobsite += _initialise;
    }

    void _initialise()
    {
        foreach (var jobsite in _findAllJobsiteComponents())
        {
            AllJobsiteComponents.Add(jobsite.JobsiteData.JobsiteID, jobsite);
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

    public static JobsiteData GetJobsiteDataFromID(int cityID, int jobsiteID)
    {
        return AllRegions.GetJobsiteDataFromID(cityID, jobsiteID);
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
}
