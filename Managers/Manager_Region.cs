using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager_Region
{
    public static List<Region_Data_SO> AllRegions = new();

    public static HashSet<string> RegionIDs = new();
    public static HashSet<string> CityIDs = new();

    public static void Initialise()
    {
        _loadAllRegions();
        _populateRegionAndCityIDs();
    }

    static void _loadAllRegions()
    {
        AllRegions.AddRange(Resources.LoadAll<Region_Data_SO>("ScriptableObjects/Regions"));
    }

    static void _populateRegionAndCityIDs()
    {
        foreach (var region in AllRegions)
        {
            RegionIDs.Add(region.RegionID);

            foreach (var city in region.AllCities)
            {
                CityIDs.Add(city.CityName);
            }
        }
    }

    public static CityComponent GetNearestCity(Vector3 position)
    {
        CityComponent nearestCity = null;
        float nearestDistance = float.MaxValue;

        foreach (var region in AllRegions)
        {
            foreach (var city in region.AllCities)
            {
                float distance = Vector3.Distance(position, city.CityData.CityComponent.transform.position);
                if (distance < nearestDistance)
                {
                    nearestCity = city.CityData.CityComponent;
                    nearestDistance = distance;
                }
            }
        }

        return nearestCity;
    }
}