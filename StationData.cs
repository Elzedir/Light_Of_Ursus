using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
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
public class StationData
{
    public uint StationID;

    ComponentReference_Jobsite _jobsiteReferences;
    public void                SetJobsiteID(uint jobsiteID) => _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
    public uint                JobsiteID                    => _jobsiteReferences.JobsiteID;
    public JobsiteComponent    Jobsite                      => _jobsiteReferences.Jobsite;

    public bool StationIsActive = true;

    public string StationDescription;
    public InventoryData _inventoryData;
    public InventoryData InventoryData { get { return _inventoryData ??= new InventoryData_Station(StationID); } }

    public List<uint>          CurrentOperatorIDs;
    StationProgressData        _stationProgressData;
    public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
    ProductionData             _productionData;
    public ProductionData      ProductionData => _productionData ??= new ProductionData(new List<Item>(), StationID);

    public List<uint> AllOperatingAreaIDs;

    public void InitialiseStationData()
    {
        var station = Manager_Station.GetStation(StationID);

        station.Initialise();

        foreach (var operatingArea in station.AllOperatingAreasInStation
                                             .Where(operatingArea => !AllOperatingAreaIDs.Contains(operatingArea.OperatingAreaData.OperatingAreaID)))
        {
            AllOperatingAreaIDs.Add(operatingArea.OperatingAreaData.OperatingAreaID);
        }

        Manager_Initialisation.OnInitialiseStationDatas += _initialiseStationData;
    }

    void _initialiseStationData()
    {
        
    }

    public bool AddOperatorToStation(uint operatorID)
    {
        if (CurrentOperatorIDs.Contains(operatorID)) 
        { 
            Debug.Log($"CurrentOperators already contain operator: {operatorID}"); 
            return false; 
        }

        CurrentOperatorIDs.Add(operatorID);
        Manager_Station.GetStation(StationID).AddOperatorToArea(operatorID);

        return true;
    }

    public bool RemoveOperatorFromStation(uint operatorID)
    {
        if (CurrentOperatorIDs.Contains(operatorID))
        {
            CurrentOperatorIDs.Remove(operatorID);

            return true;
        }

        Debug.Log($"CurrentOperators does not contain operator: {operatorID}");
        return false;
    }

    public void SetStationIsActive(bool stationIsActive)
    {
        StationIsActive = stationIsActive;
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
public class ProductionData
{
    public List<Item> AllProducedItems;
    public List<Item> EstimatedProductionRatePerHour;
    public List<Item> ActualProductionRatePerHour;
    public uint StationID;
    private StationComponent _station;

    public StationComponent Station
    {
        get
        {
            try { return _station ??= Manager_Station.GetStation(StationID); }
            catch (Exception e) { Debug.LogError(e); return null; }
        }
        set => _station = value;
    }

    public ProductionData(List<Item> allProducedItems, uint stationID)
    {
        AllProducedItems = allProducedItems;
        StationID = stationID;
    }

    public List<Item> GetActualProductionRatePerHour()
    {
        return ActualProductionRatePerHour = Station.GetActualProductionRatePerHour();
    }

    public List<Item> GetEstimatedProductionRatePerHour()
    {
        return EstimatedProductionRatePerHour = Station.GetEstimatedProductionRatePerHour();
    }
}

[Serializable]
public class StationProgressData
{
    public float  CurrentProgress;
    public float  CurrentQuality;
    public Recipe_Master CurrentProduct;
    public void   SetCurrentProduct(Recipe_Master currentProduct) => CurrentProduct = currentProduct;

    public bool Progress(float progress)
    {
        if (progress == 0 || CurrentProduct.RecipeName == RecipeName.None) return false;

        if (CurrentProduct.RequiredProgress == 0)
        {
            Debug.LogError($"For Recipe {CurrentProduct.RecipeName} CurrentProgress: {CurrentProgress} ProgressRate: {progress} MaxProgress: {CurrentProduct.RequiredProgress}");
            return false;
        }

        CurrentProgress += progress;
        CurrentQuality += progress;

        if (CurrentProgress < CurrentProduct.RequiredProgress) return false;
        
        CurrentProgress = 0;
        CurrentQuality  = 0;
        return true;

    }
}