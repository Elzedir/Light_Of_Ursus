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
    public static Dictionary <int, CityComponent> AllCityComponents = new();

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
            AllRegionComponents.Add(region.RegionID, region);
        }

        foreach (var city in _findAllCityComponents())
        {
            AllCityComponents.Add(city.CityID, city);
        }
    }

    static List<RegionComponent> _findAllRegionComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<RegionComponent>()
            .ToList();
    }

    static List<CityComponent> _findAllCityComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<CityComponent>()
            .ToList();
    }

    public static void AddToAllRegionList(RegionData regionData)
    {
        AllRegions.AddToAllRegionsList(regionData);
    }

    public static RegionData GetRegionDataFromID(int regionID)
    {
        return AllRegions.GetRegionDataFromID(regionID);
    }

    public static RegionComponent GetRegion(int regionID)
    {
        return AllRegionComponents[regionID];
    }

    public static void AddToAllCityList(int regionID, CityData cityData)
    {
        AllRegions.AddToAllCityList(regionID, cityData);
    }

    public static CityData GetCityDataFromID(int regionID, int cityID)
    {
        return AllRegions.GetCityDataFromID(regionID, cityID);
    }

    public static CityComponent GetCity(int cityID)
    {
        return AllCityComponents[cityID];
    }

    public static void GetNearestCity(Vector3 position, out CityComponent nearestCity)
    {
        nearestCity = null;
        float nearestDistance = float.MaxValue;

        foreach (var city in AllCityComponents)
        {
            float distance = Vector3.Distance(position, city.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestCity = city.Value;
                nearestDistance = distance;
            }
        }
    }
}