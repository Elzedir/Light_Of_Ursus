using NUnit.Framework.Constraints;
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
    public InventoryData InventoryData;

    public List<ActorData> CurrentOperators;

    public StationProgressData StationProgressData;
    public ProductionData ProductionData;

    public List<OperatingAreaData> AllOperatingAreaData;

    public void InitialiseStationData(int jobsiteID)
    {
        JobsiteID = jobsiteID;

        var station = Manager_Station.GetStation(StationID);

        station.Initialise();

        foreach (var operatingArea in station.AllOperatingAreasInStation)
        {
            if (!AllOperatingAreaData.Any(oa => oa.OperatingAreaID == operatingArea.OperatingAreaData.OperatingAreaID))
            {
                Debug.Log($"OperatingArea: {operatingArea.name} with ID: {operatingArea.OperatingAreaData.OperatingAreaID} was not in AllOperatingAreaData");
                AllOperatingAreaData.Add(operatingArea.OperatingAreaData);
            }

            operatingArea.SetOperatingAreaData(Manager_OperatingArea.GetOperatingAreaData(operatingAreaID: operatingArea.OperatingAreaData.OperatingAreaID, stationID: StationID));
        }

        for (int i = 0; i < AllOperatingAreaData.Count; i++)
        {
            AllOperatingAreaData[i].InitialiseOperatingAreaData(StationID);
        }

        Manager_Initialisation.OnInitialiseStationDatas += _initialiseStationData;
    }

    void _initialiseStationData()
    {
        // Start working
    }

    public bool AddOperatorToStation(ActorData operatorData)
    {
        if (CurrentOperators.Any(o => o.ActorID == operatorData.ActorID)) 
        { 
            Debug.Log($"CurrentOperators already contain operator: {operatorData.ActorID}: {operatorData.ActorName.GetName()}"); 
            return false; 
        }

        CurrentOperators.Add(operatorData);
        Manager_Station.GetStation(StationID).AddOperatorToArea(operatorData);

        return true;
    }

    public bool RemoveOperatorFromStation(ActorData operatorData)
    {
        var operatorToRemove = CurrentOperators.FirstOrDefault(o => o.ActorID == operatorData.ActorID);

        if (operatorToRemove != null)
        {
            CurrentOperators.Remove(operatorToRemove);

            return true;
        }

        Debug.Log($"CurrentOperators does not contain operator: {operatorData.ActorID}: {operatorData.ActorName.GetName()}");
        return false;
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