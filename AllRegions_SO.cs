using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllRegions_SO", menuName = "SOList/AllRegions_SO")]
[Serializable]
public class AllRegions_SO : ScriptableObject
{
    public List<RegionData> AllRegionData;

    public void SetAllRegionData(List<RegionData> allRegionData)
    {
        AllRegionData = allRegionData;
    }

    public void ClearRegionData()
    {
        AllRegionData.Clear();
    }

    public void CallSaveData() { Manager_Data.Instance.SaveGame(""); Debug.Log("Saved Game"); }
    public void CallLoadData() { Manager_Data.Instance.LoadGame(""); Debug.Log("Loaded Game"); }
}

[CustomEditor(typeof(AllRegions_SO))]
public class AllRegionsSOEditor : Editor
{
    int _selectedRegionIndex = -1;

    bool _showCities = false;

    Vector2 _regionScrollPos;
    Vector2 _cityScrollPos;

    public override void OnInspectorGUI()
    {
        AllRegions_SO allRegionsSO = (AllRegions_SO)target;

        if (GUILayout.Button("Clear Region Data"))
        {
            allRegionsSO.ClearRegionData();
            EditorUtility.SetDirty(allRegionsSO);
        }

        EditorGUILayout.LabelField("All Regions", EditorStyles.boldLabel);
        _regionScrollPos = EditorGUILayout.BeginScrollView(_regionScrollPos, GUILayout.Height(GetListHeight(allRegionsSO.AllRegionData.Count)));
        _selectedRegionIndex = GUILayout.SelectionGrid(_selectedRegionIndex, GetRegionNames(allRegionsSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedRegionIndex >= 0 && _selectedRegionIndex < allRegionsSO.AllRegionData.Count)
        {
            var selectedRegionData = allRegionsSO.AllRegionData[_selectedRegionIndex];
            DrawRegionAdditionalData(selectedRegionData);
        }
    }

    private string[] GetRegionNames(AllRegions_SO allRegionsSO) => allRegionsSO.AllRegionData.Select(r => r.RegionName).ToArray();

    private float GetListHeight(int itemCount) => Mathf.Min(200, itemCount * 20);

    private void DrawRegionAdditionalData(RegionData selectedRegionData)
    {
        EditorGUILayout.LabelField("Region Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Region Name", selectedRegionData.RegionName);
        EditorGUILayout.LabelField("Region ID", selectedRegionData.RegionID.ToString());

        EditorGUILayout.LabelField($"All cities in {selectedRegionData.RegionName}", EditorStyles.boldLabel);

        if (selectedRegionData.AllCityIDs != null)
        {
            _showCities = EditorGUILayout.Toggle("Cities", _showCities);

            if (_showCities)
            {
                DrawCityAdditionalData(selectedRegionData.AllCityIDs);
            }
        }
    }

    private void DrawCityAdditionalData(List<int> allCityIDs)
    {
        _cityScrollPos = EditorGUILayout.BeginScrollView(_cityScrollPos, GUILayout.Height(GetListHeight(allCityIDs.Count)));

        try
        {
            foreach (var cityID in allCityIDs)
            {
                EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("City Name", city.CityName);
                EditorGUILayout.LabelField("City ID", cityID.ToString());
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