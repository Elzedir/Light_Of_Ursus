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

    public List<OperatorData> CurrentOperators;

    public StationProgressData StationProgressData;
    public ProductionData ProductionData;

    public void InitialiseStationData(int jobsiteID)
    {
        Manager_Station.GetStation(StationID).Initialise();
    }

    public void AddOperatorToStation(ActorData operatorData)
    {
        if (CurrentOperators.Any(o => o.ActorData.ActorID == operatorData.ActorID)) 
        { 
            Debug.Log($"CurrentOperators already contain operator: {operatorData.ActorID}: {operatorData.ActorName.GetName()}"); 
            return; 
        }

        var newOperator = new OperatorData(operatorData, null);

        CurrentOperators.Add(newOperator);
        Manager_Station.GetStation(StationID).AddOperatorToArea(newOperator);
    }

    public void RemoveOperatorFromStation(ActorData operatorData)
    {
        var operatorToRemove = CurrentOperators.FirstOrDefault(o => o.ActorData.ActorID == operatorData.ActorID);

        if (operatorToRemove != null)
        {
            CurrentOperators.Remove(operatorToRemove);
        }
        else
        {
            Debug.Log($"CurrentOperators does not contain operator: {operatorData.ActorID}: {operatorData.ActorName.GetName()}");
        }
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

[Serializable]
public class OperatorData
{
    public Transform ActorTransform;
    public ActorData ActorData;
    public OperatingAreaComponent OperatingArea;

    public OperatorData(ActorData actorData, OperatingAreaComponent operatingArea)
    {
        ActorData = actorData;
        OperatingArea = operatingArea;
    }

    public Vector3 GetOperatorPosition()
    {
        if (ActorTransform == null)
        {
            ActorTransform = Manager_Actor.GetActor(ActorData.ActorID, out Actor_Base actor, ActorData.ActorFactionID).transform;

            if (ActorTransform == null)
            {
                throw new ArgumentException($"Operator {ActorData.ActorID}: {ActorData.ActorName.GetName()} does not have a transform.");
            }
        }

        return ActorTransform.position;
    }

    public void SetOperatingArea(OperatingAreaComponent operatingArea)
    {
        OperatingArea = operatingArea;
    }

    public void RemoveOperatingArea()
    {
        OperatingArea = null;
    }
}

[CustomPropertyDrawer(typeof(OperatorData))]
public class OperatorData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var actorDataProp = property.FindPropertyRelative("ActorData");
        var actorIDProp = actorDataProp.FindPropertyRelative("ActorID");
        var actorNameProp = actorDataProp.FindPropertyRelative("ActorName");

        var nameProp = actorNameProp.FindPropertyRelative("Name");
        var name = "Nameless";

        if (nameProp != null)
        {
            var surnameProp = actorNameProp.FindPropertyRelative("Surname");

            if (surnameProp != null)
            {
                name = $"{nameProp.stringValue} {surnameProp.stringValue}";
            }
            else
            {
                name = $"{nameProp.stringValue}";
            }
        }

        label.text = $"{actorIDProp.intValue}: {name}";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}