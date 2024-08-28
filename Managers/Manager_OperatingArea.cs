using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_OperatingArea : MonoBehaviour
{
    public static AllRegions_SO AllRegions;

    public static Dictionary<int, OperatingAreaComponent> AllOperatingAreaComponents = new();
    public static OperatingAreaComponent GetOperatingArea(int operatingAreaID) => AllOperatingAreaComponents[operatingAreaID];

    public void OnSceneLoaded()
    {
        AllRegions = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");

        Manager_Initialisation.OnInitialiseManagerOperatingArea += _initialise;
    }

    void _initialise()
    {
        foreach (var operatingArea in _findAllOperatingAreaComponents())
        {
            if (operatingArea.OperatingAreaData == null) { Debug.Log($"OperatingArea: {operatingArea.name} does not have OperatingAreaData."); continue; }

            if (!AllOperatingAreaComponents.ContainsKey(operatingArea.OperatingAreaData.OperatingAreaID)) AllOperatingAreaComponents.Add(operatingArea.OperatingAreaData.OperatingAreaID, operatingArea);
            else
            {
                if (AllOperatingAreaComponents[operatingArea.OperatingAreaData.OperatingAreaID].gameObject == operatingArea.gameObject) continue;
                else
                {
                    throw new ArgumentException($"OperatingAreaID {operatingArea.OperatingAreaData.OperatingAreaID}: {operatingArea.name} already exists for OperatingArea {AllOperatingAreaComponents[operatingArea.OperatingAreaData.OperatingAreaID].name}");
                }
            }
        }
    }

    static List<OperatingAreaComponent> _findAllOperatingAreaComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<OperatingAreaComponent>()
            .ToList();
    }

    public static void AddToOrUpdateAllOperatingAreaList(int cityID, OperatingAreaData OperatingAreaData)
    {
        AllRegions.AddToOrUpdateAllOperatingAreaDataList(cityID, OperatingAreaData);
    }

    public static OperatingAreaData GetOperatingAreaData(int operatingAreaID, int stationID = -1)
    {
        return AllRegions.GetOperatingAreaData(stationID, operatingAreaID);
    }

    public static void GetNearestOperatingArea(Vector3 position, out OperatingAreaComponent nearestOperatingArea)
    {
        nearestOperatingArea = null;
        float nearestDistance = float.MaxValue;

        foreach (var OperatingArea in AllOperatingAreaComponents)
        {
            float distance = Vector3.Distance(position, OperatingArea.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestOperatingArea = OperatingArea.Value;
                nearestDistance = distance;
            }
        }
    }

    public static int GetRandomOperatingAreaID()
    {
        return AllRegions.GetRandomOperatingAreaID();
    }
}
