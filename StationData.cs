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

    Fishing_Spot,
    Farming_Plot,

    Campfire,

    Tanning_Station,
}

[Serializable]
public class StationData : IStationInventory
{
    public int StationID;
    public StationName _stationName;
    public StationName StationName { get { return _stationName; } private set { _stationName = value; } }
    public int JobsiteID;

    public bool StationIsActive = true;

    public bool OverwriteDataInStationFromEditor = false;

    public string StationDescription;
    public GameObject GameObject {  get; private set; }

    public InventoryData InventoryData { get; private set; }

    public void InitialiseInventoryComponent()
    {
        GameObject = Manager_Station.GetStation(StationID).gameObject;
        InventoryData = new InventoryData(this, new List<Item>());
    }

    public void InitialiseStationData(int jobsiteID)
    {
        Manager_Station.GetStation(StationID).Initialise();
    }

    public List<Item> GetStationYield(Actor_Base actor)
    {
        throw new ArgumentException("Base class cannot be used.");

        //return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 7) }; For tree
    }

    public Vector3 GetOperatingPosition()
    {
        throw new ArgumentException("Base class cannot be used.");

        // return Manager_Station.GetStation(StationID).StationArea.bounds.center; Use a list of vector3 from possible operating positions
    }

    public void SetStationIsActive(bool stationIsActive)
    {
        StationIsActive = stationIsActive;
    }

    public virtual List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.InventoryData.Inventory.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }

    public void SetStationName(StationName stationName)
    {
        StationName = stationName;
    }
}

[CustomPropertyDrawer(typeof(StationData))]
public class StationData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stationNameProp = property.FindPropertyRelative("_stationName");
        string stationName = ((StationName)stationNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}