using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllCities_SO", menuName = "SOList/AllCities_SO")]
[Serializable]
public class AllCities_SO : ScriptableObject
{
    public List<CityData> AllCityData;

    public void SetAllRegionData(List<CityData> allCityData)
    {
        AllCityData = allCityData;
    }

    public void ClearRegionData()
    {
        AllCityData.Clear();
    }

    public void CallSaveData() { Manager_Data.Instance.SaveGame(""); Debug.Log("Saved Game"); }
    public void CallLoadData() { Manager_Data.Instance.LoadGame(""); Debug.Log("Loaded Game"); }
}

[CustomEditor(typeof(AllCities_SO))]
public class AllCitiesSOEditor : Editor
{
    int _selectedCityIndex = -1;
    bool _showJobsites = false;
    bool _showPopulation = false;
    bool _showProsperity = false;

    Vector2 _cityScrollPos;
    Vector2 _jobsiteScrollPos;
    Vector2 _populationScrollPos;

    public override void OnInspectorGUI()
    {
        AllCities_SO allCitiesSO = (AllCities_SO)target;

        if (GUILayout.Button("Clear City Data"))
        {
            allCitiesSO.ClearRegionData();
            EditorUtility.SetDirty(allCitiesSO);
        }

        EditorGUILayout.LabelField("All Cities", EditorStyles.boldLabel);
        _cityScrollPos = EditorGUILayout.BeginScrollView(_cityScrollPos, GUILayout.Height(GetListHeight(allCitiesSO.AllCityData.Count)));
        _selectedCityIndex = GUILayout.SelectionGrid(_selectedCityIndex, GetCityNames(allCitiesSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedCityIndex >= 0 && _selectedCityIndex < allCitiesSO.AllCityData.Count)
        {
            var selectedCityData = allCitiesSO.AllCityData[_selectedCityIndex];
            DrawCityAdditionalData(selectedCityData);
        }
    }

    private string[] GetCityNames(AllCities_SO allCitiesSO)
    {
        return allCitiesSO.AllCityData.Select(c => c.CityName).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawCityAdditionalData(CityData selectedCityData)
    {
        EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("City Name", selectedCityData.CityName);
        EditorGUILayout.LabelField("City ID", selectedCityData.CityID.ToString());
        EditorGUILayout.LabelField("Region ID", selectedCityData.RegionID.ToString());

        if (selectedCityData.Population != null)
        {
            _showPopulation = EditorGUILayout.Toggle("Population", _showPopulation);

            if (_showPopulation)
            {
                DrawPopulationDetails(selectedCityData.Population);
            }
        }

        if (selectedCityData.AllJobsiteIDs != null)
        {
            _showJobsites = EditorGUILayout.Toggle("Jobsites", _showJobsites);

           if (_showJobsites)
            {
                DrawJobsiteAdditionalData(selectedCityData.AllJobsiteIDs);
            }
        }

        if (selectedCityData.ProsperityData != null)
        {
            _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

            if (_showProsperity)
            {
                DrawProsperityDetails(selectedCityData.ProsperityData);
            }
        }
    }

    private void DrawPopulationDetails(PopulationData populationData)
    {
        EditorGUILayout.LabelField("Current Population", populationData.CurrentPopulation.ToString());
        EditorGUILayout.LabelField("Expected Population", populationData.ExpectedPopulation.ToString());
        EditorGUILayout.LabelField("Jobsite ID", populationData.MaxPopulation.ToString());

        EditorGUILayout.LabelField("All Citizens", EditorStyles.boldLabel);

        _populationScrollPos = EditorGUILayout.BeginScrollView(_populationScrollPos, GUILayout.Height(GetListHeight(populationData.AllCitizenIDs.Count)));

        foreach (var citizenID in populationData.AllCitizenIDs)
        {
            EditorGUILayout.LabelField($"- {citizenID}");
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawJobsiteAdditionalData(List<int> allJobsiteIDs)
    {
        _jobsiteScrollPos = EditorGUILayout.BeginScrollView(_jobsiteScrollPos, GUILayout.Height(GetListHeight(allJobsiteIDs.Count)));

        try
        {
            foreach (var jobsiteID in allJobsiteIDs)
            {
                EditorGUILayout.LabelField("Jobsite Data", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Jobsite Name", jobsite.JobsiteName.ToString());
                EditorGUILayout.LabelField("Jobsite ID", jobsiteID.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawProsperityDetails(ProsperityData prosperityData)
    {
        EditorGUILayout.LabelField("Current Prosperity", prosperityData.CurrentProsperity.ToString());
        EditorGUILayout.LabelField("Max Prosperity", prosperityData.MaxProsperity.ToString());
        EditorGUILayout.LabelField("Base Prosperity Growth Per Day", prosperityData.BaseProsperityGrowthPerDay.ToString());
    }
}
