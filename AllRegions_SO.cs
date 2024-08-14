using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllRegions_SO", menuName = "SOList/AllRegions_SO")]
public class AllRegions_SO : ScriptableObject
{
    public List<RegionData> AllRegionData; // Eventually add in a wanderer list, and if an actor in the AllActors_SO is not in a place for sufficient time and
    //is not important, they disappear and get removed from the game.

    public List<int> AllRegionIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedRegionID = 0;

    public List<int> AllCityIDs;
    public int LastUnusedCityID = 0;

    public List<int> AllJobsiteIDs;
    public int LastUnusedJobsiteID = 0;

    public List<int> AllStationIDs;
    public int LastUnusedStationID = 0;

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllRegionSO += _initialise;
    }

    void _initialise()
    {
        // Clear the List, and load all the region Data from JSON.

        _initialiseAllRegionData();

        _initialiseEditorIDLists();
    }

    void _initialiseAllRegionData()
    {
        foreach (var region in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<RegionComponent>().ToList())
        {
            if (!AllRegionData.Any(r => r.RegionID == region.RegionData.RegionID))
            {
                Debug.Log($"Region: {region.RegionData.RegionName} with ID: {region.RegionData.RegionID} was not in AllRegionData");
                AllRegionData.Add(region.RegionData);
            }

            region.SetRegionData(GetRegionData(region.RegionData.RegionID));
        }

        for (int i = 0; i < AllRegionData.Count; i++)
        {
            AllRegionData[i].InitialiseRegionData();
        }
    }

    void _initialiseEditorIDLists()
    {
        AllRegionIDs.Clear();
        AllCityIDs.Clear();
        AllJobsiteIDs.Clear();
        AllStationIDs.Clear();

        _initialiseRuntimeIDLists();
    }

    void _initialiseRuntimeIDLists()
    {
        foreach (var regionData in AllRegionData)
        {
            if (!AllRegionIDs.Contains(regionData.RegionID)) AllRegionIDs.Add(regionData.RegionID);

            foreach (var cityData in regionData.AllCityData)
            {
                if (!AllRegionIDs.Contains(cityData.CityID)) AllCityIDs.Add(cityData.CityID);

                foreach (var jobsiteData in cityData.AllJobsiteData)
                {
                    if (!AllRegionIDs.Contains(jobsiteData.JobsiteID)) AllJobsiteIDs.Add(jobsiteData.JobsiteID);

                    foreach (var stationData in jobsiteData.AllStationData)
                    {
                        if (!AllRegionIDs.Contains(stationData.StationID)) AllStationIDs.Add(stationData.StationID);
                    }
                }
            }
        }
    }

    public void AddToOrUpdateAllRegionDataList(RegionData regionData)
    {
        var existingRegion = AllRegionData.FirstOrDefault(r => r.RegionID == regionData.RegionID);

        if (existingRegion == null) AllRegionData.Add(regionData);
        else AllRegionData[AllRegionData.IndexOf(existingRegion)] = regionData;
    }

    public RegionData GetRegionData(int regionID)
    {
        if (!AllRegionData.Any(r => r.RegionID == regionID)) { Debug.Log($"AllRegionData does not contain RegionID: {regionID}"); return null; }

        return AllRegionData.FirstOrDefault(r => r.RegionID == regionID);
    }

    public int GetRandomRegionID()
    {
        while (AllRegionIDs.Contains(LastUnusedRegionID))
        {
            LastUnusedRegionID++;
        }

        AllRegionIDs.Add(LastUnusedRegionID);

        return LastUnusedRegionID;
    }

    public void AddToOrUpdateAllCityDataList(int regionID, CityData cityData)
    {
        var allCityData = Manager_Region.GetRegion(regionID).RegionData.AllCityData;

        var existingCity = allCityData.FirstOrDefault(c => c.CityID == cityData.CityID);

        if (existingCity == null) allCityData.Add(cityData);
        else allCityData[allCityData.IndexOf(existingCity)] = cityData;
    }

    public CityData GetCityData(int regionID, int cityID)
    {
        var allCityData = Manager_Region.GetRegion(regionID).RegionData.AllCityData;

        if (!allCityData.Any(c => c.CityID == cityID)) { Debug.Log($"AllCityData does not contain CityID: {cityID}"); return null; }

        return allCityData.FirstOrDefault(c => c.CityID == cityID);
    }

    public int GetRandomCityID()
    {
        while (AllCityIDs.Contains(LastUnusedCityID))
        {
            LastUnusedCityID++;
        }

        AllCityIDs.Add(LastUnusedCityID);

        return LastUnusedCityID;
    }

    public void AddToOrUpdateAllJobsiteDataList(int cityID, JobsiteData jobsiteData)
    {
        var allJobsiteData = Manager_City.GetCity(cityID).CityData.AllJobsiteData;

        var existingJobsite = allJobsiteData.FirstOrDefault(j => j.JobsiteID == jobsiteData.JobsiteID);

        if (existingJobsite == null) allJobsiteData.Add(jobsiteData);
        else allJobsiteData[allJobsiteData.IndexOf(existingJobsite)] = jobsiteData;
    }

    public JobsiteData GetJobsiteData(int cityID, int jobsiteID)
    {
        var allJobsiteData = Manager_City.GetCity(cityID).CityData.AllJobsiteData;

        if (!allJobsiteData.Any(c => c.JobsiteID == jobsiteID)) { Debug.Log($"AllJobsiteData does not contain JobsiteID: {jobsiteID}"); return null; }

        return allJobsiteData.FirstOrDefault(c => c.JobsiteID == jobsiteID);
    }

    public int GetRandomJobsiteID()
    {
        while (AllJobsiteIDs.Contains(LastUnusedJobsiteID))
        {
            LastUnusedJobsiteID++;
        }

        AllJobsiteIDs.Add(LastUnusedJobsiteID);

        return LastUnusedJobsiteID;
    }
    public void AddToOrUpdateAllStationDataList(int jobsiteID, StationData stationData)
    {
        var allStationData = Manager_Jobsite.GetJobsite(jobsiteID).JobsiteData.AllStationData;

        var existingStation = allStationData.FirstOrDefault(s => s.StationID == stationData.StationID);

        if (existingStation == null) allStationData.Add(stationData);
        else allStationData[allStationData.IndexOf(existingStation)] = stationData;
    }

    public StationData GetStationData(int jobsiteID, int stationID)
    {
        var allStationData = Manager_Jobsite.GetJobsite(jobsiteID).JobsiteData.AllStationData;

        if (!allStationData.Any(s => s.StationID == stationID)) { Debug.Log($"AllStationData does not contain StationID: {stationID}"); return null; }

        return allStationData.FirstOrDefault(s => s.StationID == stationID);
    }

    public int GetRandomStationID()
    {
        while (AllStationIDs.Contains(LastUnusedStationID))
        {
            LastUnusedStationID++;
        }

        AllStationIDs.Add(LastUnusedStationID);

        return LastUnusedStationID;
    }

    public void ClearRegionData()
    {
        AllRegionData.Clear();
    }
}

//[CustomEditor(typeof(AllRegions_SO))]
//public class AllRegionsSOEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        AllRegions_SO myScript = (AllRegions_SO)target;

//        SerializedProperty allRegionDataProp = serializedObject.FindProperty("AllRegionData");
//        EditorGUILayout.PropertyField(allRegionDataProp, true);

//        if (GUILayout.Button("Clear Region Data"))
//        {
//            myScript.ClearRegionData();
//            EditorUtility.SetDirty(myScript);
//        }

//        DrawPropertiesExcluding(serializedObject, "AllRegionData");

//        serializedObject.ApplyModifiedProperties();
//    }
//}

[CustomEditor(typeof(AllRegions_SO))]
public class AllRegionsSOEditor : Editor
{
    private int selectedRegionIndex = -1;
    private int selectedCityIndex = -1;
    private int selectedJobsiteIndex = -1;
    private int selectedStationIndex = -1;

    private Vector2 regionScrollPos;
    private Vector2 cityScrollPos;
    private Vector2 jobsiteScrollPos;
    private Vector2 stationScrollPos;

    public override void OnInspectorGUI()
    {
        AllRegions_SO allRegionsSO = (AllRegions_SO)target;

        if (GUILayout.Button("Clear Region Data"))
        {
            allRegionsSO.ClearRegionData();
            EditorUtility.SetDirty(allRegionsSO);
        }

        EditorGUILayout.LabelField("Regions", EditorStyles.boldLabel);
        regionScrollPos = EditorGUILayout.BeginScrollView(regionScrollPos, GUILayout.Height(GetListHeight(allRegionsSO.AllRegionData.Count)));
        selectedRegionIndex = GUILayout.SelectionGrid(selectedRegionIndex, GetRegionNames(allRegionsSO), 1);
        EditorGUILayout.EndScrollView();

        if (selectedRegionIndex >= 0 && selectedRegionIndex < allRegionsSO.AllRegionData.Count)
        {
            RegionData selectedRegion = allRegionsSO.AllRegionData[selectedRegionIndex];
            EditorGUILayout.LabelField($"Cities in {selectedRegion.RegionName}", EditorStyles.boldLabel);
            cityScrollPos = EditorGUILayout.BeginScrollView(cityScrollPos, GUILayout.Height(GetListHeight(selectedRegion.AllCityData.Count)));
            selectedCityIndex = GUILayout.SelectionGrid(selectedCityIndex, GetCityNames(selectedRegion), 1);
            EditorGUILayout.EndScrollView();

            if (selectedCityIndex >= 0 && selectedCityIndex < selectedRegion.AllCityData.Count)
            {
                CityData selectedCity = selectedRegion.AllCityData[selectedCityIndex];
                EditorGUILayout.LabelField($"Jobsites in {selectedCity.CityName}", EditorStyles.boldLabel);
                jobsiteScrollPos = EditorGUILayout.BeginScrollView(jobsiteScrollPos, GUILayout.Height(GetListHeight(selectedCity.AllJobsiteData.Count)));
                selectedJobsiteIndex = GUILayout.SelectionGrid(selectedJobsiteIndex, GetJobsiteNames(selectedCity), 1);
                EditorGUILayout.EndScrollView();

                if (selectedJobsiteIndex >= 0 && selectedJobsiteIndex < selectedCity.AllJobsiteData.Count)
                {
                    JobsiteData selectedJobsite = selectedCity.AllJobsiteData[selectedJobsiteIndex];
                    EditorGUILayout.LabelField($"Stations in {selectedJobsite.JobsiteName}", EditorStyles.boldLabel);
                    stationScrollPos = EditorGUILayout.BeginScrollView(stationScrollPos, GUILayout.Height(GetListHeight(selectedJobsite.AllStationData.Count)));
                    selectedStationIndex = GUILayout.SelectionGrid(selectedStationIndex, GetStationNames(selectedJobsite), 1);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        DrawAdditionalFields(allRegionsSO);
    }

    private string[] GetRegionNames(AllRegions_SO allRegionsSO)
    {
        return allRegionsSO.AllRegionData.Select(r => r.RegionName).ToArray();
    }

    private string[] GetCityNames(RegionData regionData)
    {
        return regionData.AllCityData.Select(c => c.CityName).ToArray();
    }

    private string[] GetJobsiteNames(CityData cityData)
    {
        return cityData.AllJobsiteData.Select(j => j.JobsiteName.ToString()).ToArray();
    }

    private string[] GetStationNames(JobsiteData jobsiteData)
    {
        return jobsiteData.AllStationData.Select(s => s.StationName.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawAdditionalFields(AllRegions_SO allRegionsSO)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Additional Data", EditorStyles.boldLabel);

        if (selectedStationIndex >= 0)
        {
            DrawStationAdditionalData(allRegionsSO.AllRegionData[selectedRegionIndex]
                .AllCityData[selectedCityIndex]
                .AllJobsiteData[selectedJobsiteIndex]
                .AllStationData[selectedStationIndex]);
        }
        else if (selectedJobsiteIndex >= 0)
        {
            DrawJobsiteAdditionalData(allRegionsSO.AllRegionData[selectedRegionIndex]
                .AllCityData[selectedCityIndex]
                .AllJobsiteData[selectedJobsiteIndex]);
        }
        else if (selectedCityIndex >= 0)
        {
            DrawCityAdditionalData(allRegionsSO.AllRegionData[selectedRegionIndex]
                .AllCityData[selectedCityIndex]);
        }
        else if (selectedRegionIndex >= 0)
        {
            DrawRegionAdditionalData(allRegionsSO.AllRegionData[selectedRegionIndex]);
        }
        else
        {
            DrawGlobalAdditionalData(allRegionsSO);
        }
    }

    private void DrawGlobalAdditionalData(AllRegions_SO allRegionsSO)
    {
        EditorGUILayout.LabelField("Global Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Region IDs", string.Join(", ", allRegionsSO.AllRegionIDs));
        EditorGUILayout.LabelField("City IDs", string.Join(", ", allRegionsSO.AllCityIDs));
        EditorGUILayout.LabelField("Jobsite IDs", string.Join(", ", allRegionsSO.AllJobsiteIDs));
        EditorGUILayout.LabelField("Station IDs", string.Join(", ", allRegionsSO.AllStationIDs));

        allRegionsSO.LastUnusedRegionID = EditorGUILayout.IntField("Last Unused Region ID", allRegionsSO.LastUnusedRegionID);
        allRegionsSO.LastUnusedCityID = EditorGUILayout.IntField("Last Unused City ID", allRegionsSO.LastUnusedCityID);
        allRegionsSO.LastUnusedJobsiteID = EditorGUILayout.IntField("Last Unused Jobsite ID", allRegionsSO.LastUnusedJobsiteID);
        allRegionsSO.LastUnusedStationID = EditorGUILayout.IntField("Last Unused Station ID", allRegionsSO.LastUnusedStationID);
    }

    private void DrawRegionAdditionalData(RegionData regionData)
    {
        EditorGUILayout.LabelField("Region Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Region Name", regionData.RegionName);
        EditorGUILayout.LabelField("Region ID", regionData.RegionID.ToString());
    }

    private void DrawCityAdditionalData(CityData cityData)
    {
        EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("City Name", cityData.CityName);
        EditorGUILayout.LabelField("City ID", cityData.CityID.ToString());
    }

    private void DrawJobsiteAdditionalData(JobsiteData jobsiteData)
    {
        EditorGUILayout.LabelField("Jobsite Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Jobsite Name", jobsiteData.JobsiteName.ToString());
        EditorGUILayout.LabelField("Jobsite ID", jobsiteData.JobsiteID.ToString());
    }

    private void DrawStationAdditionalData(StationData stationData)
    {
        EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Station Name", stationData.StationName.ToString());
        EditorGUILayout.LabelField("Station ID", stationData.StationID.ToString());
    }
}