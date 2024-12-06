using System.Collections;
using System.Collections.Generic;
using City;
using JobSite;
using ScriptableObjects;
using Station;
using UnityEditor;
using UnityEngine;

public class SaveAndLoadSO : EditorWindow
{
    [MenuItem("Tools/Save And Load SO")]
    public static void ShowWindow()
    {
        GetWindow<SaveAndLoadSO>("Save And Load SO");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Save All SOs"))
        {
            SaveAllSOs();
        }

        if (GUILayout.Button("Load All SOs"))
        {
            LoadAllSOs();
        }

        if (GUILayout.Button("Delete Test Save File"))
        {
            DeleteTestSaveFile();
        }
    }

    private void SaveAllSOs()
    {
        DataPersistenceManager.DataPersistence_SO.SaveGame("");
    }

    private void LoadAllSOs()
    {
        var allFactionsSO = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");
        var allActorsSO = Resources.Load<AllActors_SO>("ScriptableObjects/AllActors_SO");
        var allRegionsSO = Resources.Load<AllRegions_SO>("ScriptableObjects/AllRegions_SO");
        var allCitiesSO = Resources.Load<City_SO>("ScriptableObjects/AllCities_SO");
        var allJobsitesSO = Resources.Load<JobSite_SO>("ScriptableObjects/AllJobsites_SO");
        var allStationsSO = Resources.Load<Station_SO>("ScriptableObjects/AllStations_SO");
        var allOperatingAreasSO = Resources.Load<AllOperatingAreas_SO>("ScriptableObjects/AllOperatingAreas_SO");
        var allOrdersSO = Resources.Load<AllOrders_SO>("ScriptableObjects/AllOrders_SO");

        allFactionsSO.ClearFactionData();
        allActorsSO.ClearActorData();
        allRegionsSO.ClearRegionData();
        allCitiesSO.ClearCityData();
        allOperatingAreasSO.ClearOperatingAreaData();
        allOrdersSO.ClearOrderData();

        var saveData = DataPersistenceManager.DataPersistence_SO.GetLatestSaveData();

        allFactionsSO.LoadData(saveData);
        allActorsSO.LoadData(saveData);
        allRegionsSO.LoadData(saveData);
        allCitiesSO.LoadData(saveData);
        allOperatingAreasSO.LoadData(saveData);
        //allOrdersSO.LoadData(saveData);

        Debug.Log("All SOs loaded.");
    }

    private void DeleteTestSaveFile()
    {
        DataPersistenceManager.DataPersistence_SO.DeleteTestSaveFile();
    }
}
