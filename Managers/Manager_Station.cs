using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager_Station : MonoBehaviour
{
    public static AllRegions_SO AllRegions;
    public static Dictionary<int, StationComponent> AllStationComponents = new();
    public static Dictionary<StationComponent, EmployeePosition> EmployeeCanUseList = new();

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");

        Manager_Initialisation.OnInitialiseManagerStation += _initialise;
    }

    void _initialise()
    {
        foreach (var station in _findAllStationComponents())
        {
            if (station.StationData == null) { Debug.Log($"Station: {station.name} does not have StationData."); continue; }

            if (!AllStationComponents.ContainsKey(station.StationData.StationID)) AllStationComponents.Add(station.StationData.StationID, station);
            else
            {
                if (AllStationComponents[station.StationData.StationID].gameObject == station.gameObject) continue;
                else
                {
                    Debug.LogError($"StationID {station.StationData.StationID} and name {station.name} already exists for station {AllStationComponents[station.StationData.StationID].name}");
                    station.StationData.StationID = GetRandomStationID();
                }   
            }
        }
    }

    static List<StationComponent> _findAllStationComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<StationComponent>()
            .ToList();
    }

    public static void AddToOrUpdateAllStationList(int jobsiteID, StationData stationData)
    {
        AllRegions.AddToOrUpdateAllStationDataList(jobsiteID, stationData);
    }

    public static StationData GetStationData(int jobsiteID, int stationID)
    {
        return AllRegions.GetStationData(jobsiteID, stationID);
    }

    public static StationComponent GetStation(int stationID)
    {
        return AllStationComponents[stationID];
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

    public static int GetRandomStationID()
    {
        return AllRegions.GetRandomStationID();
    }
}
