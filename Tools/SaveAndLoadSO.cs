using System.Collections;
using System.Collections.Generic;
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
        var allCitiesSO = Resources.Load<AllCities_SO>("ScriptableObjects/AllCities_SO");
        var allJobsitesSO = Resources.Load<AllJobsites_SO>("ScriptableObjects/AllJobsites_SO");
        var allStationsSO = Resources.Load<AllStations_SO>("ScriptableObjects/AllStations_SO");
        var allOperatingAreasSO = Resources.Load<AllOperatingAreas_SO>("ScriptableObjects/AllOperatingAreas_SO");
        var allOrdersSO = Resources.Load<AllOrders_SO>("ScriptableObjects/AllOrders_SO");

        allFactionsSO.ClearFactionData();
        allActorsSO.ClearActorData();
        allRegionsSO.ClearRegionData();
        allCitiesSO.ClearCityData();
        allJobsitesSO.ClearJobsiteData();
        allStationsSO.ClearStationData();
        allOperatingAreasSO.ClearOperatingAreaData();
        allOrdersSO.ClearOrderData();

        var saveData = DataPersistenceManager.DataPersistence_SO.GetLatestSaveData();

        allFactionsSO.LoadData(saveData);
        allActorsSO.LoadData(saveData);
        allRegionsSO.LoadData(saveData);
        allCitiesSO.LoadData(saveData);
        allJobsitesSO.LoadData(saveData);
        allStationsSO.LoadData(saveData);
        allOperatingAreasSO.LoadData(saveData);
        //allOrdersSO.LoadData(saveData);

        Debug.Log("All SOs loaded.");
    }
}
