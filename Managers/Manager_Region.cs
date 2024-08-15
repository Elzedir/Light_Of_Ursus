using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager_Region : MonoBehaviour
{
    public static AllRegions_SO AllRegions;
    public static Dictionary<int, RegionComponent> AllRegionComponents = new();

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");
        AllRegions.PrepareToInitialise();

        Manager_Initialisation.OnInitialiseManagerRegion += _initialise;
    }

    void _initialise()
    {
        foreach (var region in _findAllRegionComponents())
        {
            AllRegionComponents.Add(region.RegionData.RegionID, region);

            // Replace with this soon, excpet modify for region

            //if (jobsite.JobsiteData == null) { Debug.Log($"Jobsite: {jobsite.name} does not have JobsiteData."); continue; }

            //if (!AllJobsiteComponents.ContainsKey(jobsite.JobsiteData.JobsiteID)) AllJobsiteComponents.Add(jobsite.JobsiteData.JobsiteID, jobsite);
            //else
            //{
            //    if (AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].gameObject == jobsite.gameObject) continue;
            //    else
            //    {
            //        Debug.LogError($"JobsiteID {jobsite.JobsiteData.JobsiteID} and name {jobsite.name} already exists for jobsite {AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].name}");
            //        jobsite.JobsiteData.JobsiteID = GetRandomJobsiteID();
            //    }
            //}
        }
    }

    static List<RegionComponent> _findAllRegionComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<RegionComponent>()
            .ToList();
    }

    public static void AddToOrUpdateAllRegionList(RegionData regionData)
    {
        AllRegions.AddToOrUpdateAllRegionDataList(regionData);
    }

    public static RegionData GetRegionData(int regionID)
    {
        return AllRegions.GetRegionData(regionID);
    }

    public static RegionComponent GetRegion(int regionID)
    {
        return AllRegionComponents[regionID];
    }

    public static void GetNearestRegion(Vector3 position, out RegionComponent nearestRegion)
    {
        nearestRegion = null;
        float nearestDistance = float.MaxValue;

        foreach (var region in AllRegionComponents)
        {
            float distance = Vector3.Distance(position, region.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestRegion = region.Value;
                nearestDistance = distance;
            }
        }
    }

    public static int GetRandomRegionID()
    {
        return AllRegions.GetRandomRegionID();
    }
}