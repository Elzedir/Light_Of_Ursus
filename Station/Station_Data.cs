using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Items;
using JobSite;
using Priority;
using Recipes;
using TickRates;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using WorkPosts;
using Object = UnityEngine.Object;

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        public uint        StationID;
        public StationName StationName;
        public string      StationDescription;
        public bool        StationIsActive;
        public uint        JobsiteID;

        [SerializeField] List<WorkPost_Data> _allWorkPostData;
        public Dictionary<uint, WorkPost_Data> AllWorkPost_Data;
        public Dictionary<uint, uint> WorkPost_Workers;
        
        Station_Component _station_Component;
        public Station_Component Station_Component => _station_Component ??= Station_Manager.GetStation_Component(StationID);
        
        JobSite_Component        _jobSite_Component;
        public JobSite_Component JobSite_Component => _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(JobsiteID);
        
        InventoryData _inventoryData;
        public InventoryData InventoryData => _inventoryData ??= new InventoryData_Station(StationID);
        
        StationProgressData        _stationProgressData;
        public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
        
        ProductionData             _productionData;
        public ProductionData      ProductionData => _productionData ??= new ProductionData(new List<Item>(), StationID);

        
        Dictionary<uint, WorkPost_Component> _allWorkPostComponents;
        public Dictionary<uint, WorkPost_Component> AllWorkPost_Components => _allWorkPostComponents ??= _populateWorkPlace_Components();

        public Station_Data(uint                            stationID,        StationName            stationName,        string stationDescription, uint jobsiteID,
                            Dictionary<uint, WorkPost_Data> allWorkPost_Data, Dictionary<uint, uint> workPostWorkers, bool   stationIsActive = true)
        {
            StationID               = stationID;
            StationName             = stationName;
            StationDescription      = stationDescription;
            JobsiteID               = jobsiteID;
            AllWorkPost_Data        = allWorkPost_Data;
            WorkPost_Workers = workPostWorkers;
            StationIsActive         = stationIsActive;
        }

        public void InitialiseStationData()
        {
            var station = Station_Manager.GetStation_Component(StationID);

            if (station is null)
            {
                Debug.Log($"Station not found for StationID: {StationID}");
                return;
            }
            
            PopulateWorkPost_Workers();
        }
        
        void PopulateWorkPost_Workers()
        {
            WorkPost_Workers = new Dictionary<uint, uint>();
            
            foreach (var workPostData in AllWorkPost_Data.Values)
            {
                WorkPost_Workers.Add(workPostData.WorkPostID, workPostData.CurrentWorker.ActorID);
            }
        }
        
        public Transform CollectionPoint;
        
        Dictionary<uint, WorkPost_Component> _populateWorkPlace_Components()
        {
            foreach(Transform child in Station_Component.transform)
            {
                if (child.GetComponent<WorkPost_Component>() is null) continue;

                Object.Destroy(child);
            }

            var workPlace_Components = AllWorkPost_Data.Values.Select(_createWorkPost).ToList();

            // Check this, maybe just assign one of the workPosts as the collection point in the WorkPlace_List.
            CollectionPoint = new GameObject("CollectionPoint").transform;
            CollectionPoint.SetParent(Station_Component.transform);
            CollectionPoint.localPosition = new Vector3(0, 0, -2);

            return workPlace_Components.ToDictionary(workPost => workPost.WorkPostID);
        }

        WorkPost_Component _createWorkPost(WorkPost_Data workPost_Data)
        {
            var workPost_Component = new GameObject($"WorkPost_{workPost_Data.WorkPostID}").AddComponent<WorkPost_Component>();
            workPost_Component.transform.SetParent(Station_Component.transform);
            workPost_Component.SetWorkPostData(workPost_Data);

            var workPost_TransformValues = WorkPost_List.GetWorkPlace_TransformValues(StationName);

            if (workPost_TransformValues is null)
            {
                Debug.Log($"WorkPost_TransformValue not found for StationName: {StationName}");
                return null;
            }

            if (workPost_TransformValues.TryGetValue(workPost_Component.WorkPostID, out var transformValue))
            {
                workPost_Component.transform.localPosition = transformValue.Position;
                workPost_Component.transform.localRotation = transformValue.Rotation;
                workPost_Component.transform.localScale = transformValue.Scale;
            }
            else
            {
                Debug.Log($"WorkPost_TransformValue not found for WorkPostID: {workPost_Component.WorkPostID}");
            }

            var workPost_Collider = workPost_Component.gameObject.AddComponent<BoxCollider>();
            workPost_Collider.isTrigger = true;
            workPost_Component.Initialise();

            return workPost_Component;
        }

        public bool AddOperatorToStation(uint workerID)
        {
            var openWorkPost_Data = AllWorkPost_Data.Values.FirstOrDefault(workPostData => workPostData.CurrentWorker.ActorID == 0);
            
            if (openWorkPost_Data is null)
            {
                Debug.Log($"No open WorkPosts found for Worker: {workerID}");
                return false;
            }
            
            openWorkPost_Data.AddWorkerToWorkPost(workerID);
            WorkPost_Workers[openWorkPost_Data.WorkPostID] = workerID;

            return true;
        }

        public bool RemoveOperatorFromStation(uint operatorID)
        {
            var workPostID = WorkPost_Workers.FirstOrDefault(x => x.Value == operatorID).Key;
            
            if (workPostID == 0)
            {
                Debug.Log($"Operator {operatorID} not found in operating areas.");
                return false;
            }
            
            AllWorkPost_Data[workPostID].RemoveCurrentWorkerFromWorkPost();
            WorkPost_Workers[workPostID] = 0;
            
            return true;
        }
        
        bool                _initialised;
        public TickRateName CurrentTickRateName;
        
        public void OnTick()
        {
            if (!_initialised) return;

            _operateStation();
        }

        void _operateStation()
        {
            if (!_passesStationChecks()) return;

            foreach (var workPost in AllWorkPost_Components.Values)
            {
                var progressMade = workPost.Operate(_baseProgressRatePerHour,
                    StationProgressData.CurrentProduct);
                
                var itemCrafted = StationProgressData.Progress(progressMade);
                
                Debug.Log(3);

                if (!itemCrafted) continue;

                // For now is the final person who adds the last progress, but change to a cumulative system later.
                Station_Component.CraftItem(
                    StationProgressData.CurrentProduct.RecipeName,
                    workPost.WorkPostData.CurrentWorker
                );
            }
        }

        bool _passesStationChecks()
        {
            if (!StationIsActive) 
                return false;
        
            if (!_isStationBeingOperated) 
                return false;
            
            Debug.Log(1);

            var success = false;

            while (!success)
            {
                try
                {
                    success = InventoryData.InventoryContainsAllItems(
                        StationProgressData.CurrentProduct.RequiredIngredients);
                    break;
                }
                catch (Exception e)
                {
                    StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Master(_defaultProduct);
                    
                    Debug.Log(2);

                    if (StationProgressData.CurrentProduct.RecipeName != RecipeName.None ||
                        _defaultProduct                                            == RecipeName.None)
                    {
                        break;
                    }
                
                    Console.WriteLine(e);
                }    
            }
        
            return success;
        }

        public List<Item> GetEstimatedProductionRatePerHour()
        {
            float totalProductionRate = 0;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var currentOperatorID in WorkPost_Workers)
            {
                var individualProductionRate = _baseProgressRatePerHour;

                foreach (var vocation in StationProgressData.CurrentProduct.RequiredVocations)
                {
                    individualProductionRate *= Actor_Manager.GetActor_Data(currentOperatorID).VocationData
                                                             .GetProgress(vocation);
                }

                totalProductionRate += individualProductionRate;
                // Don't forget to add in estimations for travel time.
            }

            float requiredProgress         = StationProgressData.CurrentProduct.RequiredProgress;
            var   estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

            var estimatedProductionItems = new List<Item>();

            for (var i = 0; i < Mathf.FloorToInt(estimatedProductionCount); i++)
            {
                foreach (var item in StationProgressData.CurrentProduct.RecipeProducts)
                {
                    estimatedProductionItems.Add(new Item(item));
                }
            }

            return estimatedProductionItems;
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
                        $"Current Operator IDs: {string.Join(", ", WorkPost_Workers)}"
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
                    data: WorkPost_Workers.Select(operatorID => $"{operatorID}").ToList()));
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
            return EstimatedProductionRatePerHour = Station.Station_Data.GetEstimatedProductionRatePerHour();
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