using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllRegions_SO", menuName = "SOList/AllRegions_SO")]
public class AllRegions_SO : ScriptableObject
{
    public List<RegionData> AllRegionData; // Eventually add in a wanderer list, and if an actor in the AllActors_SO is not in a place for sufficient time and
    //is not important, they disappear and get removed from the game.

    public List<int> AllRegionIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedRegionID = 0;

    public List<int> AllCityIDs;
    public int LastUnusedCityID = 0;

    public List<int> AllJobsiteIDs;
    public int LastUnusedJobsiteID = 0;

    public List<int> AllStationIDs;
    public int LastUnusedStationID = 0;

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllRegionSO += _initialise;
    }

    void _initialise()
    {
        // Clear the List, and load all the region Data from JSON.

        _initialiseAllRegionData();

        _initialiseEditorIDLists();
    }

    void _initialiseAllRegionData()
    {
        foreach (var region in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<RegionComponent>().ToList())
        {
            if (!AllRegionData.Any(r => r.RegionID == region.RegionData.RegionID))
            {
                Debug.Log($"Region: {region.RegionData.RegionName} with ID: {region.RegionData.RegionID} was not in AllRegionData");
                AllRegionData.Add(region.RegionData);
            }

            region.SetRegionData(GetRegionDataFromID(region.RegionData.RegionID));
        }

        for (int i = 0; i < AllRegionData.Count; i++)
        {
            AllRegionData[i].InitialiseRegionData();
        }
    }

    void _initialiseEditorIDLists()
    {
        AllRegionIDs.Clear();
        AllCityIDs.Clear();
        AllJobsiteIDs.Clear();
        AllStationIDs.Clear();

        _initialiseRuntimeIDLists();
    }

    void _initialiseRuntimeIDLists()
    {
        foreach (var regionData in AllRegionData)
        {
            if (!AllRegionIDs.Contains(regionData.RegionID)) AllRegionIDs.Add(regionData.RegionID);

            foreach (var cityData in regionData.AllCityData)
            {
                if (!AllRegionIDs.Contains(cityData.CityID)) AllCityIDs.Add(cityData.CityID);

                foreach (var jobsiteData in cityData.AllJobsiteData)
                {
                    if (!AllRegionIDs.Contains(jobsiteData.JobsiteID)) AllJobsiteIDs.Add(jobsiteData.JobsiteID);

                    foreach (var stationData in jobsiteData.AllStationData)
                    {
                        if (!AllRegionIDs.Contains(stationData.StationID)) AllStationIDs.Add(stationData.StationID);
                    }
                }
            }
        }
    }

    public void AddToOrUpdateAllRegionDataList(RegionData regionData)
    {
        var existingRegion = AllRegionData.FirstOrDefault(r => r.RegionID == regionData.RegionID);

        if (existingRegion == null) AllRegionData.Add(regionData);
        else AllRegionData[AllRegionData.IndexOf(existingRegion)] = regionData;
    }

    public RegionData GetRegionDataFromID(int regionID)
    {
        if (!AllRegionData.Any(r => r.RegionID == regionID)) { Debug.Log($"AllRegionData does not contain RegionID: {regionID}"); return null; }

        return AllRegionData.FirstOrDefault(r => r.RegionID == regionID);
    }

    public int GetRandomRegionID()
    {
        while (AllRegionIDs.Contains(LastUnusedRegionID))
        {
            LastUnusedRegionID++;
        }

        AllRegionIDs.Add(LastUnusedRegionID);

        return LastUnusedRegionID;
    }

    public void AddToOrUpdateAllCityDataList(int regionID, CityData cityData)
    {
        var allCityData = Manager_Region.GetRegion(regionID).RegionData.AllCityData;

        var existingCity = allCityData.FirstOrDefault(c => c.CityID == cityData.CityID);

        if (existingCity == null) allCityData.Add(cityData);
        else allCityData[allCityData.IndexOf(existingCity)] = cityData;
    }

    public CityData GetCityDataFromID(int regionID, int cityID)
    {
        var allCityData = Manager_Region.GetRegion(regionID).RegionData.AllCityData;

        if (!allCityData.Any(c => c.CityID == cityID)) { Debug.Log($"AllCityData does not contain CityID: {cityID}"); return null; }

        return allCityData.FirstOrDefault(c => c.CityID == cityID);
    }

    public int GetRandomCityID()
    {
        while (AllCityIDs.Contains(LastUnusedCityID))
        {
            LastUnusedCityID++;
        }

        AllCityIDs.Add(LastUnusedCityID);

        return LastUnusedCityID;
    }

    public void AddToOrUpdateAllJobsiteDataList(int cityID, JobsiteData jobsiteData)
    {
        var allJobsiteData = Manager_City.GetCity(cityID).CityData.AllJobsiteData;

        var existingJobsite = allJobsiteData.FirstOrDefault(j => j.JobsiteID == jobsiteData.JobsiteID);

        if (existingJobsite == null) allJobsiteData.Add(jobsiteData);
        else allJobsiteData[allJobsiteData.IndexOf(existingJobsite)] = jobsiteData;
    }

    public JobsiteData GetJobsiteDataFromID(int cityID, int jobsiteID)
    {
        var allJobsiteData = Manager_City.GetCity(cityID).CityData.AllJobsiteData;

        if (!allJobsiteData.Any(c => c.JobsiteID == jobsiteID)) { Debug.Log($"AllJobsiteData does not contain JobsiteID: {jobsiteID}"); return null; }

        return allJobsiteData.FirstOrDefault(c => c.JobsiteID == jobsiteID);
    }

    public int GetRandomJobsiteID()
    {
        while (AllJobsiteIDs.Contains(LastUnusedJobsiteID))
        {
            LastUnusedJobsiteID++;
        }

        AllJobsiteIDs.Add(LastUnusedJobsiteID);

        return LastUnusedJobsiteID;
    }
    public void AddToOrUpdateAllStationDataList(int jobsiteID, StationData stationData)
    {
        var allStationData = Manager_Jobsites.GetJobsite(jobsiteID).JobsiteData.AllStationData;

        var existingStation = allStationData.FirstOrDefault(s => s.StationID == stationData.StationID);

        if (existingStation == null) allStationData.Add(stationData);
        else allStationData[allStationData.IndexOf(existingStation)] = stationData;
    }

    public StationData GetStationDataFromID(int jobsiteID, int stationID)
    {
        var allStationData = Manager_Jobsites.GetJobsite(jobsiteID).JobsiteData.AllStationData;

        if (!allStationData.Any(s => s.StationID == stationID)) { Debug.Log($"AllStationData does not contain StationID: {stationID}"); return null; }

        return allStationData.FirstOrDefault(s => s.StationID == stationID);
    }

    public int GetRandomStationID()
    {
        while (AllStationIDs.Contains(LastUnusedStationID))
        {
            LastUnusedStationID++;
        }

        AllStationIDs.Add(LastUnusedStationID);

        return LastUnusedStationID;
    }

    public void ClearRegionData()
    {
        AllRegionData.Clear();
    }
}

[CustomEditor(typeof(AllRegions_SO))]
public class AllRegionsSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AllRegions_SO myScript = (AllRegions_SO)target;

        if (GUILayout.Button("Clear Region Data"))
        {
            myScript.ClearRegionData();
            EditorUtility.SetDirty(myScript);
        }
    }
}