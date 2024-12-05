using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using UnityEngine;

public class Manager_City : MonoBehaviour, IDataPersistence
{
    public static AllCities_SO AllCities;
    public static Dictionary<uint, CityData> AllCityData = new();
    static uint _lastUnusedCityID = 1;
    public static Dictionary<uint, CityComponent> AllCityComponents = new();

    public void SaveData(SaveData data)
    {
        AllCityData.Values.ToList().ForEach(cityData => cityData.SaveData());
        data.SavedCityData = new SavedCityData(AllCityData.Values.ToList());
    }
    public void LoadData(SaveData data)
    {
        if (data == null)
        {
            //Debug.Log("No SaveData found in LoadData.");
            return;
        }
        if (data.SavedCityData == null)
        {
            //Debug.Log("No SavedCityData found in SaveData.");
            return;
        }
        if (data.SavedCityData.AllCityData == null)
        {
            //Debug.Log("No AllCityData found in SavedCityData.");
            return;
        }
        if (data.SavedCityData.AllCityData.Count == 0)
        {
            //Debug.Log("AllCityData count is 0.");
            return;
        }
        
        AllCityData = data.SavedCityData?.AllCityData.ToDictionary(x => x.CityID);
        AllCityData?.Values.ToList().ForEach(cityData => cityData.LoadData());
    }

    public void OnSceneLoaded()
    {
        AllCities = Resources.Load<AllCities_SO>("ScriptableObjects/AllCities_SO");

        Manager_Initialisation.OnInitialiseManagerCity += _initialise;
    }

    void _initialise()
    {
        _initialiseAllCityData();

        // Temporary
        foreach (var city in AllCityData)
        {
            city.Value.ProsperityData.SetProsperity(50);
            city.Value.ProsperityData.MaxProsperity = 100;
        }
    }

    void _initialiseAllCityData()
    {
        foreach (var city in _findAllCityComponents())
        {
            if (city.CityData == null) { Debug.Log($"City: {city.name} does not have CityData."); continue; }

            if (!AllCityComponents.ContainsKey(city.CityData.CityID)) AllCityComponents.Add(city.CityData.CityID, city);
            else
            {
               if (AllCityComponents[city.CityData.CityID].gameObject == city.gameObject) continue;
               else
               {
                   throw new ArgumentException($"CityID {city.CityData.CityID} and name {city.name} already exists for city {AllCityComponents[city.CityData.CityID].name}");
               }
            }

            if (!AllCityData.ContainsKey(city.CityData.CityID))
            {
                //Debug.Log($"City: {city.CityData.CityID}: {city.CityData.CityName} was not in AllCityData");
                AddToAllCityData(city.CityData);
            }

            city.SetCityData(GetCityData(city.CityData.CityID));
        }

        foreach (var cityData in AllCityData.Values)
        {
            cityData.InitialiseCityData();
        }

        AllCities.AllCityData = AllCityData.Values.ToList();
    }

    static List<CityComponent> _findAllCityComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<CityComponent>()
            .ToList();
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

    public void AddToAllCityData(CityData cityData)
    {
        if (AllCityData.ContainsKey(cityData.CityID))
        {
            Debug.Log($"AllCityData already contains CityID: {cityData.CityID}");
            return;
        }

        AllCityData.Add(cityData.CityID, cityData);
    }

    public void UpdateCityData(CityData cityData)
    {
        if (!AllCityData.ContainsKey(cityData.CityID))
        {
            Debug.LogError($"CityData: {cityData.CityID} does not exist in AllCityData.");
            return;
        }

        AllCityData[cityData.CityID] = cityData;
    }

    public static CityData GetCityData(uint cityID)
    {
        if (!AllCityData.ContainsKey(cityID))
        {
            Debug.Log($"CityData: {cityID} does not exist in AllCityData.");
            return null;
        }

        return AllCityData[cityID];
    }

    public static CityComponent GetCity(uint cityID)
    {
        if (!AllCityComponents.ContainsKey(cityID))
        {
            Debug.Log($"CityComponent: {cityID} does not exist in AllCityComponents.");
            return null;
        }

        return AllCityComponents[cityID];
    }

    public uint GetRandomCityID()
    {
        while (AllCityData.ContainsKey(_lastUnusedCityID))
        {
            _lastUnusedCityID++;
        }

        return _lastUnusedCityID;
    }
}
