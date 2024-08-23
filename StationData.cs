using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public enum StationName
{
    None,

    Iron_Node,

    Anvil,

    Tree,
    Sawmill,
    Log_Pile,

    Fishing_Spot,
    Farming_Plot,

    Campfire,

    Tanning_Station,
}

[Serializable]
public class StationData : IStationInventory
{
    public int StationID;
    public StationType StationType;
    public StationName StationName;
    public int JobsiteID;

    public bool StationIsActive = true;

    public string StationDescription;
    public Recipe CurrentRecipe;
    public InventoryData InventoryData;

    public List<int> CurrentOperators;

    public void InitialiseStationData(int jobsiteID)
    {
        Manager_Station.GetStation(StationID).Initialise();
    }

    public List<Item> GetStationYield(Actor_Base actor)
    {
        throw new ArgumentException("Base class cannot be used.");
    }

    public Vector3 GetOperatingPosition()
    {
        throw new ArgumentException("Base class cannot be used.");
    }

    public void SetStationIsActive(bool stationIsActive)
    {
        StationIsActive = stationIsActive;
    }

    public void SetStationName(StationName stationName)
    {
        StationName = stationName;
    }

    public GameObject GetGameObject()
    {
        return Manager_Station.GetStation(StationID).gameObject;
    }

    public StationName GetStationName()
    {
        return StationName;
    }

    public InventoryData GetInventoryData()
    {
        return InventoryData;
    }
}

[CustomPropertyDrawer(typeof(StationData))]
public class StationData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stationNameProp = property.FindPropertyRelative("StationName");
        string stationName = ((StationName)stationNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}