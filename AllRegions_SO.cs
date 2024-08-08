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

    public List<RegionData> AllRegionData = new();

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllRegionSO += _initialise;
    }

    void _initialise()
    {
        // Clear the List, and load all the region Data from JSON.

        _initialiseIDLists();
    }

    void _initialiseIDLists()
    {
        foreach (RegionData regionData in AllRegionData)
        {
            AllRegionIDs.Add(regionData.RegionID);

            foreach (CityData cityData in regionData.AllCityData)
            {
                AllCityIDs.Add(cityData.CityID);
            }
        }
    }

    public void AddToAllRegionsList(RegionData regionData)
    {
        var existingRegion = AllRegionData.FirstOrDefault(r => r.RegionID == regionData.RegionID);

        if (existingRegion == null) { AllRegionData.Add(regionData); return; }

        var existingRegionData = AllRegionData[AllRegionData.IndexOf(existingRegion)];

        if (!existingRegionData.OverwriteDataInRegion) return;

        existingRegionData = regionData;
        existingRegionData.OverwriteDataInRegion = false;
    }

    public RegionData GetRegionDataFromID(int regionID)
    {
        List<RegionData> regionDataList = AllRegionData.Where(r => r.RegionID == regionID).ToList();

        if (regionDataList.Count != 1) throw new ArgumentException($"RegionID: {regionID} has {regionDataList.Count} entries in AllRegionData.");

        return regionDataList.FirstOrDefault();
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

    public void AddToAllCityList(int regionID, CityData cityData)
    {
        var allCityData = GetRegionDataFromID(regionID).AllCityData;

        var existingCity = allCityData.FirstOrDefault(c => c.CityID == cityData.CityID);

        if (existingCity == null) { allCityData.Add(cityData); return; }

        var existingCityData = allCityData[allCityData.IndexOf(existingCity)];

        if (!existingCityData.OverwriteDataInCity) return;

        existingCityData = cityData;
        existingCityData.OverwriteDataInCity = false;
    }

    public CityData GetCityDataFromID(int regionID, int cityID)
    {
        var allCityData = GetRegionDataFromID(regionID).AllCityData;

        List<CityData> cityDataList = allCityData.Where(c => c.CityID == cityID).ToList();

        if (cityDataList.Count != 1) throw new ArgumentException($"CityID: {cityID} of RegionID {regionID} has {cityDataList.Count} entries in AllCityData.");

        return cityDataList.FirstOrDefault();
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
