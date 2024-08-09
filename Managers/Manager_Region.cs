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

    public static RegionData GetRegionDataFromID(int regionID)
    {
        return AllRegions.GetRegionDataFromID(regionID);
    }

    public static RegionComponent GetRegion(int regionID)
    {
        return AllRegionComponents[regionID];
    }
}