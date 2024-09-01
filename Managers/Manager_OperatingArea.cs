using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_OperatingArea : MonoBehaviour, IDataPersistence
{
    public static AllOperatingAreas_SO AllOperatingAreas;

    public static Dictionary<int, OperatingAreaData> AllOperatingAreaData = new();
    public static Dictionary<int, OperatingAreaComponent> AllOperatingAreaComponents = new();

    public HashSet<int> AllOperatingAreaIDs = new();
    public int LastUnusedOperatingAreaID = 1;

    public void SaveData(SaveData data) => data.SavedOperatingAreaData = new SavedOperatingAreaData(AllOperatingAreaData.Values.ToList());
    public void LoadData(SaveData data) => AllOperatingAreaData = data.SavedOperatingAreaData.AllOperatingAreaData.ToDictionary(x => x.OperatingAreaID);

    public void OnSceneLoaded()
    {
        AllOperatingAreas = Resources.Load<AllOperatingAreas_SO>("ScriptableObjects/AllOperatingAreas_SO");

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

            if (!AllOperatingAreaData.ContainsKey(operatingArea.OperatingAreaData.OperatingAreaID))
            {
                Debug.Log($"OperatingArea: {operatingArea.OperatingAreaData.OperatingAreaID}: {operatingArea.name} was not in AllOperatingAreaData");
                AddToAllOperatingAreaData(operatingArea.OperatingAreaData);
            }

            operatingArea.SetOperatingAreaData(GetOperatingAreaData(operatingArea.OperatingAreaData.OperatingAreaID));
        }

        foreach (var operatingAreaData in AllOperatingAreaData.Values)
        {
            operatingAreaData.InitialiseOperatingAreaData();
        }

        AllOperatingAreas.AllOperatingAreaData = AllOperatingAreaData.Values.ToList();
    }

    static List<OperatingAreaComponent> _findAllOperatingAreaComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<OperatingAreaComponent>()
            .ToList();
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

    public void AddToAllOperatingAreaData(OperatingAreaData OperatingAreaData)
    {
        if (AllOperatingAreaData.ContainsKey(OperatingAreaData.OperatingAreaID))
        {
            Debug.Log($"AllOperatingAreaData already contains OperatingAreaID: {OperatingAreaData.OperatingAreaID}");
            return;
        }

        AllOperatingAreaData.Add(OperatingAreaData.OperatingAreaID, OperatingAreaData);
    }

    public void UpdateAllOperatingAreaData(OperatingAreaData OperatingAreaData)
    {
        if (!AllOperatingAreaData.ContainsKey(OperatingAreaData.OperatingAreaID))
        {
            Debug.LogError($"OperatingAreaData: {OperatingAreaData.OperatingAreaID} does not exist in AllOperatingAreaData.");
            return;
        }

        AllOperatingAreaData[OperatingAreaData.OperatingAreaID] = OperatingAreaData;
    }

    public static OperatingAreaData GetOperatingAreaData(int operatingAreaID)
    {
        if (!AllOperatingAreaData.ContainsKey(operatingAreaID))
        {
            Debug.LogError($"OperatingAreaData: {operatingAreaID} does not exist in AllOperatingAreaData.");
            return null;
        }

        return AllOperatingAreaData[operatingAreaID];
    }

    
    public static OperatingAreaComponent GetOperatingArea(int operatingAreaID)
    {
        if (!AllOperatingAreaComponents.ContainsKey(operatingAreaID))
        {
            Debug.LogError($"OperatingArea: {operatingAreaID} does not exist in AllOperatingAreaComponents.");
            return null;
        }

        return AllOperatingAreaComponents[operatingAreaID];
    }

    public int GetRandomOperatingAreaID()
    {
        int operatingAreaID = 1;
        while (AllOperatingAreaData.ContainsKey(operatingAreaID))
        {
            operatingAreaID++;
        }
        return operatingAreaID;
    }
}
