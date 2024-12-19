using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Items;
using JobSite;
using Recipes;
using TickRates;
using Tools;
using UnityEngine;
using WorkPosts;
using Object = UnityEngine.Object;

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        public uint        StationID;
        public string      StationDescription;
        public bool        StationIsActive;
        public uint        JobsiteID;

        public float BaseProgressRatePerHour = 5;

        public StationName StationName    => Station_Component.StationName;
        public RecipeName  DefaultProduct => Station_Component.DefaultProduct;

        [SerializeField] List<WorkPost_Data> _allWorkPostData;
        public Dictionary<uint, WorkPost_Data> AllWorkPost_Data;
        
        Station_Component _station_Component;
        public Station_Component Station_Component => _station_Component ??= Station_Manager.GetStation_Component(StationID);
        
        JobSite_Component        _jobSite_Component;
        public JobSite_Component JobSite_Component => _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(JobsiteID);
        
        InventoryData _inventoryData;
        public InventoryData InventoryData => _inventoryData ??= new InventoryData_Station(StationID);
        
        StationProgressData        _stationProgressData;
        public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
        
        Dictionary<uint, WorkPost_Component> _allWorkPostComponents;
        public Dictionary<uint, WorkPost_Component> AllWorkPost_Components => _allWorkPostComponents ??= _populateWorkPlace_Components();
        public bool IsStationBeingOperated =>
            AllWorkPost_Data.Values.Any(workPost => workPost.CurrentWorker != null);

        public Station_Data(uint                            stationID,        StationName            stationName,        string stationDescription, uint jobsiteID,
                            Dictionary<uint, WorkPost_Data> allWorkPost_Data, bool   stationIsActive = true)
        {
            StationID               = stationID;
            //stationName; WHen loading from data, if a station exists, then it doesn't do anything, it just helps display what type of station it is.
            // But if the station does exist, then we generate a new station with the required stationName type.
            StationDescription      = stationDescription;
            JobsiteID               = jobsiteID;
            AllWorkPost_Data        = allWorkPost_Data;
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

        public WorkPost_Component GetOpenWorkPost()
        {
            return AllWorkPost_Components.Values.FirstOrDefault(workPost => workPost.WorkPostData.CurrentWorker.ActorID == 0);
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
                var progressMade = workPost.Operate(BaseProgressRatePerHour,
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
        
            if (!IsStationBeingOperated) 
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
                    StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Master(DefaultProduct);
                    
                    Debug.Log(2);

                    if (StationProgressData.CurrentProduct.RecipeName != RecipeName.None ||
                        DefaultProduct                                            == RecipeName.None)
                    {
                        break;
                    }
                
                    Console.WriteLine(e);
                }    
            }
        
            return success;
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
                        $"Inventory Data: {InventoryData}"
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

            return new Data_Display(
                title: "Base Station Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
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