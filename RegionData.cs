using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class RegionData
{
    public int RegionID;
    public string RegionName;

    public bool OverwriteDataInRegion = false;

    public string RegionDescription;

    public FactionName Faction;

    public List<CityData> AllCityData;

    public RegionData()
    {
        Manager_Region.AddToAllRegionList(this);
    }
}

[CustomPropertyDrawer(typeof(RegionData))]
public class RegionData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var cityNameProp = property.FindPropertyRelative("RegionName");
        label.text = !string.IsNullOrEmpty(cityNameProp.stringValue) ? cityNameProp.stringValue : "No Name";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}