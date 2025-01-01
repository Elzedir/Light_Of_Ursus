using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
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
            
            StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Master(DefaultProduct);

            if (station is not null) return;
            
            Debug.Log($"Station not found for StationID: {StationID}");
        }

        Dictionary<uint, WorkPost_Component> _populateWorkPlace_Components()
        {
            if (!Application.isPlaying)
            {
                return null;
            }
            
            foreach (Transform child in Station_Component.transform)
            {
                if (child.GetComponent<WorkPost_Component>() is null) continue;

                Object.DestroyImmediate(child.gameObject);
            }

            var workPlace_Components = _createWorkPost(AllWorkPost_Data.Values.ToList());

            if (workPlace_Components is not null) return workPlace_Components;

            Debug.Log($"WorkPlace_Components not found for StationName: {StationName}");
            return null;
        }

        Dictionary<uint, WorkPost_Component> _createWorkPost(List<WorkPost_Data> allWorkPost_Data)
        {
            var workPost_DefaultValues = WorkPost_List.GetWorkPlace_DefaultValues(StationName);

            if (workPost_DefaultValues is null)
            {
                Debug.Log($"WorkPost_TransformValue not found for StationName: {StationName}");
                return null;
            }
            
            var workPost_Components = new Dictionary<uint, WorkPost_Component>();

            for (var i = 0; i < allWorkPost_Data.Count; i++)
            {
                var workPost_Data = allWorkPost_Data[i];
                var transformValue = workPost_DefaultValues[i % workPost_DefaultValues.Count];

                if (transformValue is null)
                {
                    Debug.Log($"WorkPost_TransformValue not found for StationName: {StationName}");
                    return null;
                }

                var workPost_Component = new GameObject($"WorkPost_{workPost_Data.WorkPostID}").AddComponent<WorkPost_Component>();
                
                workPost_Component.transform.SetParent(Station_Component.transform);
                workPost_Component.SetWorkPostData(workPost_Data);
            
                workPost_Component.transform.localPosition = transformValue.Position;
                workPost_Component.transform.localRotation = transformValue.Rotation;
                workPost_Component.transform.localScale    = transformValue.Scale;
                
                var workPost_Collider = workPost_Component.gameObject.AddComponent<BoxCollider>();
                workPost_Collider.isTrigger = true;
                workPost_Component.Initialise();
                
                workPost_Components[workPost_Data.WorkPostID] = workPost_Component;
            }
            
            return workPost_Components;
        }

        public WorkPost_Component GetOpenWorkPost()
        {
            return AllWorkPost_Components.Values.FirstOrDefault(workPost => workPost.WorkPostData.CurrentWorker is null);
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

            foreach (var workPost in AllWorkPost_Components.Values.Where(workPost => workPost.WorkPostData.CurrentWorker is not null))
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
            
            Debug.Log(1);

            var success = false;

            for (var i = 0; i < 2; i++)
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            dataSO_Object ??= new Data_Display(
                title: "Station Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: null,
                subData: new Dictionary<string, Data_Display>());
            
            var dataObjects = new Dictionary<string, Data_Display>(dataSO_Object.SubData);

            try
            {
                dataObjects["Base Station Data"] = new Data_Display(
                    title: "Base Station Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Station ID:", $"{StationID}" },
                        { "Station Name:", $"{StationName}" },
                        { "Station Description:", $"{StationDescription}" },
                        { "Station IsActive:", $"{StationIsActive}" },
                        { "Jobsite ID:", $"{JobsiteID}" },
                        { "Default Product:", $"{DefaultProduct}" },
                        { "Base Progress Rate Per Hour:", $"{BaseProgressRatePerHour}" }
                    });
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Base Station Data not found.");
                }
            }
            
            try
            {
                dataObjects["Station Items"] = new Data_Display(
                    title: "Station Items",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: InventoryData.AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:", item =>  $"{item.ItemName} Qty - {item.ItemAmount}"));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Inventory Data not found.");
                }
            }

            try
            {
                dataObjects["Station Progress Data"] = new Data_Display(
                    title: "Station Progress Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Current Progress:", $"{StationProgressData.CurrentProgress}" },
                        { "Current Quality:", $"{StationProgressData.CurrentQuality}" },
                        { "Current Product:", $"{StationProgressData.CurrentProduct}" }
                    });
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Station Progress Data not found.");
                }
            }
            
            try
            {
                dataObjects["StationWorkPosts"] = new Data_Display(
                    title: "Station WorkPosts",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: AllWorkPost_Components.Values.ToDictionary(workPost => $"{workPost.WorkPostID} -",
                        workPost =>
                            $"{workPost.WorkPostData?.CurrentWorker?.ActorID}: {workPost.WorkPostData?.CurrentWorker}"));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: WorkPost Data not found.");
                    Debug.LogError($"WorkPost Data: {AllWorkPost_Data}");
                    Debug.LogError($"WorkPost Data: {AllWorkPost_Data?.Values}");

                    foreach (var workPost in AllWorkPost_Components.Values)
                    {
                        Debug.LogError($"WorkPost Data: {workPost}");
                        Debug.LogError($"WorkPost Data: {workPost?.WorkPostID}");
                        Debug.LogError($"WorkPost Data: {workPost?.WorkPostData}");
                        Debug.LogError($"WorkPost Data: {workPost?.WorkPostData?.CurrentWorker}");
                        Debug.LogError($"WorkPost Data: {workPost?.WorkPostData?.CurrentWorker?.ActorID}");
                    }

                    foreach (var workPostData in AllWorkPost_Data?.Values)
                    {
                        Debug.LogError($"WorkPost Data: {workPostData}");
                        Debug.LogError($"WorkPost Data: {workPostData?.WorkPostID}");
                        Debug.LogError($"WorkPost Data: {workPostData?.CurrentWorker}");
                        Debug.LogError($"WorkPost Data: {workPostData?.CurrentWorker?.ActorID}");
                    }
                }
            }

            dataSO_Object.SubData = dataObjects;
            
            return dataSO_Object;
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