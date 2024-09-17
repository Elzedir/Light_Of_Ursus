using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Manager_Region : MonoBehaviour, IDataPersistence
{
    const string _allRegionSOPath = "ScriptableObjects/AllRegions_SO";

    public static AllRegions_SO AllRegions;
    public static Dictionary<int, RegionData> AllRegionData = new();
    static int _lastUnusedRegionID = 1;
    public static Dictionary<int, RegionComponent> AllRegionComponents = new();

    public void SaveData(SaveData saveData) => saveData.SavedRegionData = new SavedRegionData(AllRegionData.Values.ToList());
    public void LoadData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.Log("No SaveData found in LoadData.");
            return;
        }
        if (saveData.SavedRegionData == null)
        {
            Debug.Log("No SavedRegionData found in SaveData.");
            return;
        }
        if (saveData.SavedRegionData.AllRegionData == null)
        {
            Debug.Log("No AllRegionData found in SavedRegionData.");
            return;
        }
        if (saveData.SavedRegionData.AllRegionData.Count == 0)
        {
            Debug.Log("AllRegionData count is 0.");
            return;
        }
        
        AllRegionData = saveData.SavedRegionData?.AllRegionData.ToDictionary(x => x.RegionID);
    }

    public void OnSceneLoaded()
    {
        AllRegions = _getOrCreateAllRegionsSO();

        Manager_Initialisation.OnInitialiseManagerRegion += _initialise;
    }

    void _initialise()
    {
        _initialiseAllRegionData();
    }

    void _initialiseAllRegionData()
    {
        foreach (var region in _findAllRegionComponents())
        {
            if (region.RegionData == null) { Debug.Log($"Region: {region.name} does not have RegionData."); continue; }

            if (!AllRegionComponents.ContainsKey(region.RegionData.RegionID)) AllRegionComponents.Add(region.RegionData.RegionID, region);
            else
            {
               if (AllRegionComponents[region.RegionData.RegionID].gameObject == region.gameObject) continue;
               else
               {
                   throw new ArgumentException($"RegionID {region.RegionData.RegionID} and name {region.name} already exists for region {AllRegionComponents[region.RegionData.RegionID].name}");
               }
            }

            if (!AllRegionData.ContainsKey(region.RegionData.RegionID))
            {
                Debug.Log($"Region: {region.RegionData.RegionID}: {region.RegionData.RegionName} was not in AllRegionData");
                AddToAllRegionData(region.RegionData);
            }

            region.SetRegionData(GetRegionData(region.RegionData.RegionID));
        }

        foreach(var region in AllRegionData)
        {
            region.Value.InitialiseRegionData();
        }

        AllRegions.AllRegionData = AllRegionData.Values.ToList();
    }

    AllRegions_SO _getOrCreateAllRegionsSO()
    {
        AllRegions_SO allRegionsSO = Resources.Load<AllRegions_SO>(_allRegionSOPath);
        
        if (allRegionsSO == null)
        {
            allRegionsSO = ScriptableObject.CreateInstance<AllRegions_SO>();
            AssetDatabase.CreateAsset(allRegionsSO, _allRegionSOPath);
            AssetDatabase.SaveAssets();
        }
        // else
        // {
        //     Debug.Log("Loaded existing AllRegions_SO asset.");
        // }

        return allRegionsSO;
    }

    static List<RegionComponent> _findAllRegionComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<RegionComponent>()
            .ToList();
    }

    public static void GetNearestRegion(Vector3 position, out RegionComponent nearestRegion)
    {
        nearestRegion = null;
        float nearestDistance = float.MaxValue;

        foreach (var region in AllRegionComponents)
        {
            float distance = Vector3.Distance(position, region.Value.transform.position);

            if (distance < nearestDistance)
            {
                nearestRegion = region.Value;
                nearestDistance = distance;
            }
        }
    }

    public void AddToAllRegionData(RegionData regionData)
    {
        if (AllRegionData.ContainsKey(regionData.RegionID))
        {
            Debug.Log($"AllRegionData already contains RegionID: {regionData.RegionID}");
            return;
        }

        AllRegionData.Add(regionData.RegionID, regionData);
    }

    public void UpdateAllRegionData(RegionData regionData)
    {
        if (!AllRegionData.ContainsKey(regionData.RegionID))
        {
            Debug.LogError($"RegionData: {regionData.RegionID} does not exist in AllRegionData.");
            return;
        }

        AllRegionData[regionData.RegionID] = regionData;
    }

    public static RegionData GetRegionData(int regionID)
    {
        if (!AllRegionData.ContainsKey(regionID))
        {
            Debug.LogError($"RegionData: {regionID} does not exist in AllRegionData.");
            return null;
        }

        return AllRegionData[regionID];
    }

    public static RegionComponent GetRegion(int regionID)
    {
        if (!AllRegionComponents.ContainsKey(regionID))
        {
            Debug.LogError($"RegionComponent: {regionID} does not exist in AllRegionComponents.");
            return null;
        }

        return AllRegionComponents[regionID];
    }

    public int GetRandomRegionID()
    {
        while (AllRegionData.ContainsKey(_lastUnusedRegionID))
        {
            _lastUnusedRegionID++;
        }

        return _lastUnusedRegionID;
    }
}