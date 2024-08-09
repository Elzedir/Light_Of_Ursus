using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AllRegions_SO", menuName = "SOList/AllRegions_SO")]
public class AllRegions_SO : ScriptableObject
{
    public List<int> AllRegionIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedRegionID = 0;

    public List<int> AllCityIDs;
    public int LastUnusedCityID = 0;

    public List<RegionData> AllRegionData; // Eventually add in a wanderer list, and if an actor in the AllActors_SO is not in a place for sufficient time and
    //is not important, they disappear and get removed from the game.

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

        foreach (var regionData in AllRegionData)
        {
            AllRegionIDs.Add(regionData.RegionID);

            foreach (var cityData in regionData.AllCityData)
            {
                AllCityIDs.Add(cityData.CityID);
            }
        }
    }

    void _initialiseRuntimeIDLists()
    {

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
        var region = GetRegionDataFromID(regionID);
        var allCityData = region.AllCityData;

        var existingCity = allCityData.FirstOrDefault(c => c.CityID == cityData.CityID);

        if (existingCity == null) allCityData.Add(cityData);
        else allCityData[allCityData.IndexOf(existingCity)] = cityData;
    }

    public CityData GetCityDataFromID(int regionID, int cityID)
    {
        var allCityData = GetRegionDataFromID(regionID).AllCityData;

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
}
