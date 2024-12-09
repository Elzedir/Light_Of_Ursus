using System.Collections;
using System.Collections.Generic;
using Actor;
using City;
using Faction;
using JobSite;
using Region;
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
            _deleteTestSaveFile();
        }
    }

    void SaveAllSOs()
    {
        DataPersistenceManager.DataPersistence_SO.SaveGame("");
    }

    void LoadAllSOs()
    {
        var allFactionsSO = Resources.Load<Faction_SO>("ScriptableObjects/AllFactions_SO");
        var allActorsSO = Resources.Load<Actor_SO>("ScriptableObjects/AllActors_SO");
        var allRegionsSO = Resources.Load<Region_SO>("ScriptableObjects/AllRegions_SO");
        var allCitiesSO = Resources.Load<City_SO>("ScriptableObjects/AllCities_SO");
        var allJobsitesSO = Resources.Load<JobSite_SO>("ScriptableObjects/AllJobsites_SO");
        var allStationsSO = Resources.Load<Station_SO>("ScriptableObjects/AllStations_SO");
        var allOperatingAreasSO = Resources.Load<AllOperatingAreas_SO>("ScriptableObjects/AllOperatingAreas_SO");
        var allOrdersSO = Resources.Load<AllOrders_SO>("ScriptableObjects/AllOrders_SO");

        allOperatingAreasSO.ClearOperatingAreaData();

        var saveData = DataPersistenceManager.DataPersistence_SO.GetLatestSaveData();

        allOperatingAreasSO.LoadData(saveData);
        //allOrdersSO.LoadData(saveData);

        Debug.Log("All SOs loaded.");
    }

    void _deleteTestSaveFile()
    {
        DataPersistenceManager.DataPersistence_SO.DeleteTestSaveFile();
    }
}
