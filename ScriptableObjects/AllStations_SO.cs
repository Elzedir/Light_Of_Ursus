using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllStations_SO", menuName = "SOList/AllStations_SO")]
[Serializable]
public class AllStations_SO : ScriptableObject
{
    public List<StationData> AllStationData;

    public void SetAllStationData(List<StationData> allStationData)
    {
        AllStationData = allStationData;
    }

    public void LoadData(SaveData saveData)
    {
        AllStationData = saveData.SavedStationData.AllStationData;
    }

    public void ClearStationData()
    {
        AllStationData.Clear();
    }
}

[CustomEditor(typeof(AllStations_SO))]
public class AllStationsSOEditor : Editor
{
    int _selectedStationIndex = -1;
    bool _showOperatingAreas = false;

    Vector2 _stationScrollPos;
    Vector2 _operatingAreaScrollPos;

    public override void OnInspectorGUI()
    {
        AllStations_SO allStationsSO = (AllStations_SO)target;

        if (GUILayout.Button("Clear Station Data"))
        {
            allStationsSO.ClearStationData();
            EditorUtility.SetDirty(allStationsSO);
        }

        EditorGUILayout.LabelField("All Stations", EditorStyles.boldLabel);
        _stationScrollPos = EditorGUILayout.BeginScrollView(_stationScrollPos, GUILayout.Height(GetListHeight(allStationsSO.AllStationData.Count)));
        _selectedStationIndex = GUILayout.SelectionGrid(_selectedStationIndex, GetStationNames(allStationsSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedStationIndex >= 0 && _selectedStationIndex < allStationsSO.AllStationData.Count)
        {
            var selectedStationData = allStationsSO.AllStationData[_selectedStationIndex];
            DrawStationAdditionalData(selectedStationData);
        }
    }

    private string[] GetStationNames(AllStations_SO allStationsSO)
    {
        return allStationsSO.AllStationData.Select(s => s.StationID.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawStationAdditionalData(StationData selectedStationData)
    {
        EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
        //EditorGUILayout.LabelField("Station Name", selectedStationData.StationName.ToString());
        EditorGUILayout.LabelField("Station ID", selectedStationData.StationID.ToString());
        EditorGUILayout.LabelField("Jobsite ID", selectedStationData.JobsiteID.ToString());

        if (selectedStationData.AllOperatingAreaIDs != null)
        {
            _showOperatingAreas = EditorGUILayout.Toggle("Operating Areas", _showOperatingAreas);

            if (_showOperatingAreas)
            {
                DrawOperatingAreaAdditionalData(selectedStationData.AllOperatingAreaIDs);
            }
        }
    }

    private void DrawOperatingAreaAdditionalData(List<int> allOperatingAreaData)
    {
        _operatingAreaScrollPos = EditorGUILayout.BeginScrollView(_operatingAreaScrollPos, GUILayout.Height(GetListHeight(allOperatingAreaData.Count)));

        try
        {
            foreach (var operatingAreaID in allOperatingAreaData)
            {
                EditorGUILayout.LabelField("Operating Area Data", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Operating Area Name", operatingArea.OperatingAreaName.ToString());
                EditorGUILayout.LabelField("Operating Area ID", operatingAreaID.ToString());
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
}
