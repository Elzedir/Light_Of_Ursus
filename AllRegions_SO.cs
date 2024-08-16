using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllRegions_SO", menuName = "SOList/AllRegions_SO")]
public class AllRegions_SO : ScriptableObject
{
    // Use this class as the save class to keep all info, and then distribute to the AllActor's SO. Add a player region and a wanderer region to hold all
    // citizens that aren't in other cities, like the Drifter Faction.

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

        AllRegionIDs.Sort();
        AllRegionIDs.Reverse();

        AllCityIDs.Sort();
        AllCityIDs.Reverse();

        AllJobsiteIDs.Sort();
        AllJobsiteIDs.Reverse();

        AllStationIDs.Sort();
        AllStationIDs.Reverse();

        LastUnusedRegionID = AllRegionIDs.First() + 1;
        LastUnusedCityID = AllCityIDs.First() + 1;
        LastUnusedJobsiteID = AllJobsiteIDs.First() + 1;
        LastUnusedStationID = AllStationIDs.First() + 1;
    }

    void _initialiseRuntimeIDLists()
    {
        foreach (var regionData in AllRegionData)
        {
            if (!AllRegionIDs.Contains(regionData.RegionID)) AllRegionIDs.Add(regionData.RegionID);

            foreach (var cityData in regionData.AllCityData)
            {
                if (!AllCityIDs.Contains(cityData.CityID)) AllCityIDs.Add(cityData.CityID);

                foreach (var jobsiteData in cityData.AllJobsiteData)
                {
                    if (!AllJobsiteIDs.Contains(jobsiteData.JobsiteID)) AllJobsiteIDs.Add(jobsiteData.JobsiteID);

                    foreach (var stationData in jobsiteData.AllStationData)
                    {
                        if (!AllStationIDs.Contains(stationData.StationID)) AllStationIDs.Add(stationData.StationID);
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
        if (regionID != -1)
        {
            var allCityData = Manager_Region.GetRegion(regionID).RegionData.AllCityData;

            if (!allCityData.Any(c => c.CityID == cityID)) { Debug.Log($"AllCityData does not contain CityID: {cityID}"); return null; }

            return allCityData.FirstOrDefault(c => c.CityID == cityID);
        }

        return AllRegionData
            .SelectMany(r => r.AllCityData)
            .FirstOrDefault(c => c.CityID == cityID);
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
        if (cityID != -1)
        {
            var allJobsiteData = Manager_City.GetCity(cityID).CityData.AllJobsiteData;

            if (!allJobsiteData.Any(c => c.JobsiteID == jobsiteID)) { Debug.Log($"AllJobsiteData does not contain JobsiteID: {jobsiteID}"); return null; }

            return allJobsiteData.FirstOrDefault(c => c.JobsiteID == jobsiteID);
        }

        return AllRegionData
            .SelectMany(r => r.AllCityData)
            .SelectMany(c => c.AllJobsiteData)
            .FirstOrDefault(j => j.JobsiteID == jobsiteID);
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

[CustomEditor(typeof(AllRegions_SO))]
public class AllRegionsSOEditor : Editor
{
    int _selectedRegionIndex = -1;
    int selectedRegionIndex { get { return _selectedRegionIndex; } set { if (_selectedRegionIndex == value) return; _selectedRegionIndex = value; _resetIndexes(0); } }
    int _selectedCityIndex = -1;
    int selectedCityIndex { get { return _selectedCityIndex; } set { if (_selectedCityIndex == value) return; _selectedCityIndex = value; _resetIndexes(1); } }
    int selectedJobsiteIndex = -1;
    int selectedStationIndex = -1;

    int selectedPopulationIndex = -1;

    void _resetIndexes(int i)
    {
        selectedPopulationIndex = -1;
        selectedStationIndex = -1;

        switch (i)
        {
            case 0:
                _selectedCityIndex = -1;
                selectedJobsiteIndex = -1;
                break;
            case 1:
                selectedJobsiteIndex = -1;
                break;
            case 3:
                _selectedRegionIndex = -1;
                _selectedCityIndex = -1;
                selectedJobsiteIndex = -1;
                break;
            default:
                Debug.Log($"Int: {i} does nothing.");
                break;
        }
    }

    Vector2 regionScrollPos;
    Vector2 cityScrollPos;
    Vector2 jobsiteScrollPos;
    Vector2 stationScrollPos;
    Vector2 populationScrollPos;

    public override void OnInspectorGUI()
    {
        AllRegions_SO allRegionsSO = (AllRegions_SO)target;

        EditorGUILayout.LabelField("Region IDs", string.Join(", ", allRegionsSO.AllRegionIDs));
        EditorGUILayout.LabelField("City IDs", string.Join(", ", allRegionsSO.AllCityIDs));
        EditorGUILayout.LabelField("Jobsite IDs", string.Join(", ", allRegionsSO.AllJobsiteIDs));
        EditorGUILayout.LabelField("Station IDs", string.Join(", ", allRegionsSO.AllStationIDs));

        allRegionsSO.LastUnusedRegionID = EditorGUILayout.IntField("Last Unused Region ID", allRegionsSO.LastUnusedRegionID);
        allRegionsSO.LastUnusedCityID = EditorGUILayout.IntField("Last Unused City ID", allRegionsSO.LastUnusedCityID);
        allRegionsSO.LastUnusedJobsiteID = EditorGUILayout.IntField("Last Unused Jobsite ID", allRegionsSO.LastUnusedJobsiteID);
        allRegionsSO.LastUnusedStationID = EditorGUILayout.IntField("Last Unused Station ID", allRegionsSO.LastUnusedStationID);

        if (GUILayout.Button("Clear Region Data"))
        {
            _resetIndexes(3);
            allRegionsSO.ClearRegionData();
            EditorUtility.SetDirty(allRegionsSO);
        }

        if (GUILayout.Button("Unselect All")) _resetIndexes(3);

        EditorGUILayout.LabelField("All Regions", EditorStyles.boldLabel);
        regionScrollPos = EditorGUILayout.BeginScrollView(regionScrollPos, GUILayout.Height(GetListHeight(allRegionsSO.AllRegionData.Count)));
        selectedRegionIndex = GUILayout.SelectionGrid(selectedRegionIndex, GetRegionNames(allRegionsSO), 1);
        EditorGUILayout.EndScrollView();

        if (selectedRegionIndex >= 0 && selectedRegionIndex < allRegionsSO.AllRegionData.Count)
        {
            var selectedRegionData = allRegionsSO.AllRegionData[selectedRegionIndex];
            DrawRegionAdditionalData(selectedRegionData);
        }
    }

    private string[] GetRegionNames(AllRegions_SO allRegionsSO)
    {
        // DO something like this to include ID and Name
        //return allActorsSO.AllFactionData.Select(f => $"{f.FactionID}: {f.FactionName}").ToArray();
        return allRegionsSO.AllRegionData.Select(r => r.RegionName).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private float GetPropertyHeight(object data, bool isExpanded)
    {
        var fieldCount = data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Length;
        return isExpanded ? fieldCount * 20 : 20;
    }

    private string[] GetCityNames(RegionData regionData)
    {
        return regionData.AllCityData.Select(c => c.CityName).ToArray();
    }

    private string[] GetStationNames(JobsiteData jobsiteData)
    {
        return jobsiteData.AllStationData.Select(s => s.StationName.ToString()).ToArray();
    }

    private void DrawRegionAdditionalData(RegionData selectedRegionData)
    {
        EditorGUILayout.LabelField("Region Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Region Name", selectedRegionData.RegionName);
        EditorGUILayout.LabelField("Region ID", selectedRegionData.RegionID.ToString());

        EditorGUILayout.LabelField($"All cities in {selectedRegionData.RegionName}", EditorStyles.boldLabel);
        cityScrollPos = EditorGUILayout.BeginScrollView(cityScrollPos, GUILayout.Height(GetListHeight(selectedRegionData.AllCityData.Count)));
        selectedCityIndex = GUILayout.SelectionGrid(selectedCityIndex, GetCityNames(selectedRegionData), 1);
        EditorGUILayout.EndScrollView();

        if (selectedCityIndex >= 0 && selectedCityIndex < selectedRegionData.AllCityData.Count)
        {
            var selectedCityData = selectedRegionData.AllCityData[selectedCityIndex];
            DrawCityAdditionalData(selectedCityData);
        }
    }

    private void DrawCityAdditionalData(CityData selectedCityData)
    {
        EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("City Name", selectedCityData.CityName);
        EditorGUILayout.LabelField("City ID", selectedCityData.CityID.ToString());

        if (selectedCityData.Population != null)
        {
            selectedPopulationIndex = GUILayout.SelectionGrid(selectedPopulationIndex, new string[] { "Population" }, 1);

            if (selectedPopulationIndex >= 0 && selectedPopulationIndex < selectedCityData.Population.AllCitizens.Count)
            {
                DrawPopulationDetails(selectedCityData.Population);
            }
        }

        if (selectedCityData.AllJobsiteData != null)
        {
            selectedJobsiteIndex = GUILayout.SelectionGrid(selectedJobsiteIndex, new string[] { "Jobsites" }, 1);

            if (selectedJobsiteIndex >= 0 && selectedJobsiteIndex < selectedCityData.AllJobsiteData.Count)
            {
                DrawJobsiteAdditionalData(selectedCityData.AllJobsiteData);
            }
        }
    }

    private void DrawJobsiteAdditionalData(List<JobsiteData> allJobsiteData)
    {
        jobsiteScrollPos = EditorGUILayout.BeginScrollView(jobsiteScrollPos, GUILayout.Height(GetListHeight(allJobsiteData.Count)));
        selectedJobsiteIndex = GUILayout.SelectionGrid(selectedJobsiteIndex, allJobsiteData.Select(j => j.JobsiteName.ToString()).ToArray(), 1);
        EditorGUILayout.EndScrollView();

        if (selectedJobsiteIndex >= 0 && selectedJobsiteIndex < allJobsiteData.Count)
        {
            JobsiteData selectedJobsiteData = allJobsiteData[selectedJobsiteIndex];

            EditorGUILayout.LabelField("Jobsite Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Jobsite Name", selectedJobsiteData.JobsiteName.ToString());
            EditorGUILayout.LabelField("Jobsite ID", selectedJobsiteData.JobsiteID.ToString());

            if (selectedJobsiteData.AllStationData != null)
            {
                selectedStationIndex = GUILayout.SelectionGrid(selectedStationIndex, new string[] { "Stations" }, 1);

                if (selectedStationIndex >= 0 && selectedStationIndex < selectedJobsiteData.AllStationData.Count)
                {
                    DrawStationAdditionalData(selectedJobsiteData.AllStationData);
                }
            }
        }
    }

    private void DrawStationAdditionalData(List<StationData> allStationData)
    {
        stationScrollPos = EditorGUILayout.BeginScrollView(stationScrollPos, GUILayout.Height(GetListHeight(allStationData.Count)));
        selectedStationIndex = GUILayout.SelectionGrid(selectedStationIndex, allStationData.Select(s => s.StationName.ToString()).ToArray(), 1);
        EditorGUILayout.EndScrollView();

        if (selectedStationIndex >= 0 && selectedStationIndex < allStationData.Count)
        {
            var selectedStationData = allStationData[selectedStationIndex];

            EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Station Name", selectedStationData.StationName.ToString());
            EditorGUILayout.LabelField("Station ID", selectedStationData.StationID.ToString());
        }
    }

    private void DrawPopulationDetails(DisplayPopulation populationData)
    {
        //Fix the indexing and the fields

        EditorGUILayout.LabelField("Current Population", populationData.CurrentPopulation.ToString());
        EditorGUILayout.LabelField("Expected Population", populationData.ExpectedPopulation.ToString());
        EditorGUILayout.LabelField("Jobsite ID", populationData.MaxPopulation.ToString());

        EditorGUILayout.LabelField("All Citizens", EditorStyles.boldLabel);
        
        foreach (var citizen in populationData.AllCitizens)
        {
            EditorGUILayout.LabelField($"- {citizen.CitizenID}: {citizen.CitizenName}");
        }
    }
}