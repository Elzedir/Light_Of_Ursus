using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager_Station : MonoBehaviour, IDataPersistence
{
    public static AllStations_SO AllStations;
    public static Dictionary<int, StationData> AllStationData;
    public static Dictionary<int, StationComponent> AllStationComponents = new();
    public static Dictionary<StationComponent, EmployeePosition> EmployeeCanUseList = new();

    public void SaveData(SaveData saveData) => saveData.SavedStationData = new SavedStationData(AllStationData.Values.ToList());
    public void LoadData(SaveData saveData)
    {
        AllStationData = saveData.SavedStationData?.AllStationData.ToDictionary(x => x.StationID);
    }

    public void OnSceneLoaded()
    {
        AllStations = Resources.Load<AllStations_SO>("ScriptableObjects/AllStations_SO");

        Manager_Initialisation.OnInitialiseManagerStation += _initialise;
    }

    void _initialise()
    {
        if (AllStationData == null) AllStationData = new();

        foreach (var station in _findAllStationComponents())
        {
            if (station.StationData == null) { Debug.Log($"Station: {station.name} does not have StationData."); continue; }

            if (!AllStationComponents.ContainsKey(station.StationData.StationID)) AllStationComponents.Add(station.StationData.StationID, station);
            else
            {
                if (AllStationComponents[station.StationData.StationID].gameObject == station.gameObject) continue;
                else
                {
                    throw new ArgumentException($"StationID {station.StationData.StationID} and name {station.name} already exists for station {AllStationComponents[station.StationData.StationID].name}");
                }
            }

            if (!AllStationData.ContainsKey(station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationID}: {station.StationData.StationName} was not in AllStationData");
                AddToAllStationData(station.StationData);
            }

            station.SetStationData(GetStationData(station.StationData.StationID));
        }

        foreach (var stationData in AllStationData.Values)
        {
            stationData.InitialiseStationData();
        }

        AllStations.AllStationData = AllStationData.Values.ToList();
    }

    static List<StationComponent> _findAllStationComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<StationComponent>()
            .ToList();
    }

    public void AddToAllStationData(StationData stationData)
    {
        if (AllStationData.ContainsKey(stationData.StationID))
        {
            Debug.Log($"AllStationData already contains StationID: {stationData.StationID}");
            return;
        }

        AllStationData.Add(stationData.StationID, stationData);
    }

    public void UpdateAllStationData(StationData stationData)
    {
        if (!AllStationData.ContainsKey(stationData.StationID))
        {
            Debug.Log($"AllStationData does not contain StationID: {stationData.StationID}");
            return;
        }

        AllStationData[stationData.StationID] = stationData;
    }

    
    public static StationData GetStationData(int stationID)
    {
        if (!AllStationData.ContainsKey(stationID))
        {
            Debug.Log($"Station: {stationID} is not in AllStationData list");
            return null;
        }

        return AllStationData[stationID];
    }
    public static StationComponent GetStation(int stationID)
    {
        if (!AllStationComponents.ContainsKey(stationID))
        {
            Debug.Log($"Station: {stationID} is not in AllStationComponents list");
            return null;
        }

        return AllStationComponents[stationID];
    }


    public int GetRandomStationID()
    {
        int stationID = 1;
        while (AllStationData.ContainsKey(stationID))
        {
            stationID++;
        }
        return stationID;
    }

    public static void GetNearestStationToPosition(Vector3 position, StationName stationName, out StationComponent nearestStation)
    {
        nearestStation = null;
        float nearestDistance = float.MaxValue;

        foreach (var station in AllStationComponents)
        {
            float distance = Vector3.Distance(position, station.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestStation = station.Value;
                nearestDistance = distance;
            }
        }
    }
}
