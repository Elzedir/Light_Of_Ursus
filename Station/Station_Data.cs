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

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        public uint       StationID;
        public StationName StationName;
        Station_Component _station_Component;
        public Station_Component StationComponent => _station_Component ??= Station_Manager.GetStation_Component(StationID);

        ComponentReference_Jobsite _jobsiteReferences;
        public void                SetJobsiteID(uint jobsiteID) => _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
        public uint                JobsiteID                    => _jobsiteReferences.JobsiteID;
        public JobSite_Component    JobSite                      => _jobsiteReferences.JobSite;

        public bool StationIsActive = true;

        public string        StationDescription;
        InventoryData _inventoryData;
        public InventoryData InventoryData { get { return _inventoryData ??= new InventoryData_Station(StationID); } }

        public List<uint>          CurrentOperatorIDs;
        StationProgressData        _stationProgressData;
        public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
        ProductionData             _productionData;
        public ProductionData      ProductionData => _productionData ??= new ProductionData(new List<Item>(), StationID);

        public List<uint> AllOperatingAreaIDs;
        
        public Station_Data(uint stationID, StationName stationName, string stationDescription, uint jobsiteID)
        {
            StationID          = stationID;
            StationName        = stationName;
            StationDescription = stationDescription;
            SetJobsiteID(jobsiteID);

            CurrentOperatorIDs = new List<uint>();
            AllOperatingAreaIDs = new List<uint>();
        }

        public void InitialiseStationData()
        {
            var station = Station_Manager.GetStation_Component(StationID);

            foreach (var operatingArea in station.AllOperatingAreasInStation
                                                 .Where(operatingArea => !AllOperatingAreaIDs.Contains(operatingArea.OperatingAreaData.OperatingAreaID)))
            {
                AllOperatingAreaIDs.Add(operatingArea.OperatingAreaData.OperatingAreaID);
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

        public void SetStationIsActive(bool stationIsActive)
        {
            StationIsActive = stationIsActive;
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
        public  List<Item>       AllProducedItems;
        public  List<Item>       EstimatedProductionRatePerHour;
        public  List<Item>       ActualProductionRatePerHour;
        public  uint             StationID;
        private Station_Component _station;

        public Station_Component Station
        {
            get
            {
                try { return _station ??= Station_Manager.GetStation_Component(StationID); }
                catch (Exception e) { Debug.LogError(e); return null; }
            }
            set => _station = value;
        }

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
        public float         CurrentProgress;
        public float         CurrentQuality;
        public Recipe_Data CurrentProduct;
        public void          SetCurrentProduct(Recipe_Data currentProduct) => CurrentProduct = currentProduct;

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