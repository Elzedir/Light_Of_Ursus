using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_City : MonoBehaviour
{
    public static AllRegions_SO AllRegions;
    public static Dictionary<int, CityComponent> AllCityComponents = new();

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");

        Manager_Initialisation.OnInitialiseManagerCity += _initialise;
    }

    void _initialise()
    {
        foreach (var city in _findAllCityComponents())
        {
            AllCityComponents.Add(city.CityData.CityID, city);
        }
    }

    static List<CityComponent> _findAllCityComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<CityComponent>()
            .ToList();
    }

    public static void AddToOrUpdateAllCityList(int regionID, CityData cityData)
    {
        AllRegions.AddToOrUpdateAllCityDataList(regionID, cityData);
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
