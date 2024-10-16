using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllJobsites_SO", menuName = "SOList/AllJobsites_SO")]
[Serializable]
public class AllJobsites_SO : ScriptableObject
{
    public List<JobsiteData> AllJobsiteData;

    public void SetAllJobsiteData(List<JobsiteData> allJobsiteData)
    {
        AllJobsiteData = allJobsiteData;
    }

    public void LoadData(SaveData saveData)
    {
        AllJobsiteData = saveData.SavedJobsiteData.AllJobsiteData;
    }

    public void ClearJobsiteData()
    {
        AllJobsiteData.Clear();
    }
}

[CustomEditor(typeof(AllJobsites_SO))]
public class AllJobsitesSOEditor : Editor
{
    int _selectedJobsiteIndex = -1;
    bool _showStations = false;
    bool _showProsperity = false;

    Vector2 _jobsiteScrollPos;
    Vector2 _stationScrollPos;

    public override void OnInspectorGUI()
    {
        AllJobsites_SO allJobsitesSO = (AllJobsites_SO)target;

        if (GUILayout.Button("Clear Jobsite Data"))
        {
            allJobsitesSO.ClearJobsiteData();
            EditorUtility.SetDirty(allJobsitesSO);
        }

        EditorGUILayout.LabelField("All Jobsites", EditorStyles.boldLabel);
        _jobsiteScrollPos = EditorGUILayout.BeginScrollView(_jobsiteScrollPos, GUILayout.Height(GetListHeight(allJobsitesSO.AllJobsiteData.Count)));
        _selectedJobsiteIndex = GUILayout.SelectionGrid(_selectedJobsiteIndex, GetJobsiteNames(allJobsitesSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedJobsiteIndex >= 0 && _selectedJobsiteIndex < allJobsitesSO.AllJobsiteData.Count)
        {
            var selectedJobsiteData = allJobsitesSO.AllJobsiteData[_selectedJobsiteIndex];
            DrawJobsiteAdditionalData(selectedJobsiteData);
        }
    }

    private string[] GetJobsiteNames(AllJobsites_SO allJobsitesSO)
    {
        return allJobsitesSO.AllJobsiteData.Select(j => j.JobsiteName.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawJobsiteAdditionalData(JobsiteData selectedJobsiteData)
    {
        EditorGUILayout.LabelField("Jobsite Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Jobsite Name", selectedJobsiteData.JobsiteName.ToString());
        EditorGUILayout.LabelField("Jobsite ID", selectedJobsiteData.JobsiteID.ToString());
        EditorGUILayout.LabelField("City ID", selectedJobsiteData.CityID.ToString());

        if (selectedJobsiteData.AllStationIDs != null)
        {
            _showStations = EditorGUILayout.Toggle("Stations", _showStations);

            if (_showStations)
            {
                DrawStationAdditionalData(selectedJobsiteData.AllStationIDs);
            }
        }

        if (selectedJobsiteData.ProsperityData != null)
        {
            _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

            if (_showProsperity)
            {
                DrawProsperityDetails(selectedJobsiteData.ProsperityData);
            }
        }
    }

    private void DrawStationAdditionalData(List<uint> allStationData)
    {
        _stationScrollPos = EditorGUILayout.BeginScrollView(_stationScrollPos, GUILayout.Height(GetListHeight(allStationData.Count)));

        try
        {
            foreach (var stationID in allStationData)
            {
                EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Station Name", station.StationName.ToString());
                EditorGUILayout.LabelField("Station ID", stationID.ToString());
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
