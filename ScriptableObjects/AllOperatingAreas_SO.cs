using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OperatingArea;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllOperatingAreas_SO", menuName = "SOList/AllOperatingAreas_SO")]
[Serializable]
public class AllOperatingAreas_SO : ScriptableObject
{
    public List<OperatingAreaData> AllOperatingAreaData;

    public void SetAllOperatingAreaData(List<OperatingAreaData> allOperatingAreaData)
    {
        AllOperatingAreaData = allOperatingAreaData;
    }

    public void LoadData(SaveData saveData)
    {
        // Temporary, later put in checks
        AllOperatingAreaData = saveData?.SavedOperatingAreaData?.AllOperatingAreaData;
    }

    public void ClearOperatingAreaData()
    {
        AllOperatingAreaData.Clear();
    }
}

[CustomEditor(typeof(AllOperatingAreas_SO))]
public class AllOperatingAreasSOEditor : Editor
{
    int _selectedOperatingAreaIndex = -1;

    public override void OnInspectorGUI()
    {
        AllOperatingAreas_SO allOperatingAreasSO = (AllOperatingAreas_SO)target;

        if (GUILayout.Button("Clear Operating Area Data"))
        {
            allOperatingAreasSO.ClearOperatingAreaData();
            EditorUtility.SetDirty(allOperatingAreasSO);
        }

        EditorGUILayout.LabelField("All Operating Areas", EditorStyles.boldLabel);

        Vector2 operatingAreaScrollPos = Vector2.zero;

        operatingAreaScrollPos = EditorGUILayout.BeginScrollView(operatingAreaScrollPos, GUILayout.Height(GetListHeight(allOperatingAreasSO.AllOperatingAreaData.Count)));
        
        _selectedOperatingAreaIndex = GUILayout.SelectionGrid(_selectedOperatingAreaIndex, GetOperatingAreaNames(allOperatingAreasSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedOperatingAreaIndex >= 0 && _selectedOperatingAreaIndex < allOperatingAreasSO.AllOperatingAreaData.Count)
        {
            var selectedOperatingAreaData = allOperatingAreasSO.AllOperatingAreaData[_selectedOperatingAreaIndex];
            DrawOperatingAreaAdditionalData(selectedOperatingAreaData);
        }
    }

    private string[] GetOperatingAreaNames(AllOperatingAreas_SO allOperatingAreasSO)
    {
        return allOperatingAreasSO.AllOperatingAreaData.Select(o => o.OperatingAreaID.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawOperatingAreaAdditionalData(OperatingAreaData selectedOperatingAreaData)
    {
        EditorGUILayout.LabelField("Operating Area Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Operating Area ID", selectedOperatingAreaData.OperatingAreaID.ToString());
        EditorGUILayout.LabelField("Station ID", selectedOperatingAreaData.StationID.ToString());
        EditorGUILayout.LabelField("Current Operator", $"{selectedOperatingAreaData.CurrentOperatorID}");
    }
}
