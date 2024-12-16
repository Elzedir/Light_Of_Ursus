using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Items;
using JobSite;
using Priority;
using Recipe;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using WorkPosts;

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        public           uint        StationID;
        public           StationName StationName;
        public           string      StationDescription;
        public           bool        StationIsActive;
        [SerializeField] uint        _jobsiteID;

        [SerializeField] List<WorkPost_Data> _allWorkPostData;
        public Dictionary<uint, WorkPost_Data> AllWorkPost_Data;
        
        
        Station_Component _station_Component;
        public Station_Component Station_Component => _station_Component ??= Station_Manager.GetStation_Component(StationID);
        
        JobSite_Component        _jobSite_Component;
        public JobSite_Component JobSite_Component => _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(_jobsiteID);
        
        InventoryData _inventoryData;
        public InventoryData InventoryData => _inventoryData ??= new InventoryData_Station(StationID);
        
        StationProgressData        _stationProgressData;
        public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
        
        ProductionData             _productionData;
        public ProductionData      ProductionData => _productionData ??= new ProductionData(new List<Item>(), StationID);

        
        Dictionary<uint, WorkPost_Component> _allWorkPostComponents;
        public Dictionary<uint, WorkPost_Component> AllWorkPostComponents => _allWorkPostComponents ??= _getAllWorkPostComponents();

        Dictionary<uint, WorkPost_Component> _getAllWorkPostComponents()
        {
                
        }

        public Station_Data(uint stationID, StationName stationName, string stationDescription, uint jobsiteID,
                            Dictionary<uint, WorkPost_Data> allWorkPostComponents, bool stationIsActive = true)
        {
            StationID          = stationID;
            StationName        = stationName;
            StationDescription = stationDescription;
            _jobsiteID         = jobsiteID;
            AllWorkPost_Data   = allWorkPostComponents;
            StationIsActive    = stationIsActive;
        }

        public void InitialiseStationData()
        {
            var station = Station_Manager.GetStation_Component(StationID);

            foreach (var operatingArea in station.AllOperatingAreasInStation
                                                 .Where(operatingArea => !AllOperatingAreaIDs.Contains(operatingArea.WorkPostData.WorkPostID)))
            {
                AllOperatingAreaIDs.Add(operatingArea.WorkPostData.WorkPostID);
            }
        }

        public bool AddOperatorToStation(uint operatorID)
        {
            if (CurrentOperatorIDs.Contains(operatorID)) 
            { 
                Debug.Log($"CurrentOperators already contain operator: {operatorID}"); 
                return false; 
            }

            CurrentOperatorIDs.Add(operatorID);
            Station_Manager.GetStation_Component(StationID).AddOperatorToArea(operatorID);

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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Station Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Station ID: {StationID}",
                        $"Station Description: {StationDescription}",
                        $"Station IsActive: {StationIsActive}",
                        $"Inventory Data: {InventoryData}",
                        $"Current Operator IDs: {string.Join(", ", CurrentOperatorIDs)}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Base Station Data not found.");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Station Items",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: InventoryData.AllInventoryItems.Values.Select(item => $"{item.ItemID}: {item.ItemName} Qty - {item.ItemAmount}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Inventory Data not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Station Progress Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Current Progress: {StationProgressData.CurrentProgress}",
                        $"Current Quality: {StationProgressData.CurrentQuality}",
                        $"Current Product: {StationProgressData.CurrentProduct}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Station Progress Data not found.");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Station Operators",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: CurrentOperatorIDs.Select(operatorID => $"{operatorID}").ToList()));
            }
            catch
            {
                Debug.LogError("Error: Current Operators not found.");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Production Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"All Produced Items: {string.Join(", ", ProductionData.AllProducedItems)}",
                        $"Estimated Production Rate Per Hour: {string.Join(", ", ProductionData.EstimatedProductionRatePerHour)}",
                        $"Actual Production Rate Per Hour: {string.Join(", ", ProductionData.ActualProductionRatePerHour)}",
                        $"Station ID: {ProductionData.StationID}"
                    }));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Production Data not found.");
                }
            }

            return new Data_Display(
                title: "Base Station Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
        }
    }

    [Serializable]
    public class ProductionData
    {
        public List<Item>        AllProducedItems;
        public List<Item>        EstimatedProductionRatePerHour;
        public List<Item>        ActualProductionRatePerHour;
        public uint              StationID;

        Station_Component        _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);

        public ProductionData(List<Item> allProducedItems, uint stationID)
        {
            AllProducedItems = allProducedItems;
            StationID        = stationID;
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
        public float       CurrentProgress;
        public float       CurrentQuality;
        public Recipe_Data CurrentProduct;
        public void        SetCurrentProduct(Recipe_Data currentProduct) => CurrentProduct = currentProduct;

        public bool Progress(float progress)
        {
            if (progress == 0 || CurrentProduct.RecipeName == RecipeName.None) return false;

            if (CurrentProduct.RequiredProgress == 0)
            {
                Debug.LogError($"For Recipe {CurrentProduct.RecipeName} CurrentProgress: {CurrentProgress} ProgressRate: {progress} MaxProgress: {CurrentProduct.RequiredProgress}");
                return false;
            }

            CurrentProgress += progress;
            CurrentQuality  += progress;

            if (CurrentProgress < CurrentProduct.RequiredProgress) return false;
        
            CurrentProgress = 0;
            CurrentQuality  = 0;
            return true;

        }
    }
}