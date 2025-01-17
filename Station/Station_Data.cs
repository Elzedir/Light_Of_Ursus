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
        public uint        JobSiteID;

        public float BaseProgressRatePerHour = 5;

        public StationName StationName    => Station_Component.StationName;
        public RecipeName  DefaultProduct => Station_Component.DefaultProduct;

        [SerializeField] List<WorkPost_Data> _allWorkPostData;
        public Dictionary<uint, WorkPost_Data> AllWorkPost_Data;
        
        Station_Component _station_Component;
        public Station_Component Station_Component => _station_Component ??= Station_Manager.GetStation_Component(StationID);
        
        JobSite_Component        _jobSite_Component;
        public JobSite_Component JobSite_Component => _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        
        Inventory_Data _inventoryData;
        public Inventory_Data InventoryData => _inventoryData ??= new InventoryData_Station(StationID);
        
        StationProgressData        _stationProgressData;
        public StationProgressData StationProgressData => _stationProgressData ??= new StationProgressData();
        
        Dictionary<uint, WorkPost_Component> _allWorkPostComponents;
        public Dictionary<uint, WorkPost_Component> AllWorkPost_Components => _allWorkPostComponents ??= _populateWorkPlace_Components();
        public bool IsStationBeingOperated =>
            AllWorkPost_Data.Values.Any(workPost => workPost.CurrentWorker != null);

        public Station_Data(uint                            stationID,        StationName            stationName,        string stationDescription, uint jobSiteID,
                            Dictionary<uint, WorkPost_Data> allWorkPost_Data, bool   stationIsActive = true)
        {
            StationID               = stationID;
            //stationName; WHen loading from data, if a station exists, then it doesn't do anything, it just helps display what type of station it is.
            // But if the station does exist, then we generate a new station with the required stationName type.
            StationDescription      = stationDescription;
            JobSiteID               = jobSiteID;
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
            
            AllWorkPost_Data ??= new Dictionary<uint, WorkPost_Data>();
            var workPostDefaultValues = WorkPost_List.GetWorkPlace_DefaultValues(StationName);
            
            if (AllWorkPost_Data.Count > workPostDefaultValues.Count)
            {
                Debug.LogError($"WorkPost_Data count is greater than WorkPost_DefaultValues count for Station: {StationID}: {StationName}. Resetting to 0.");
                AllWorkPost_Data.Clear();
            }
            
            for (uint i = 0; i < workPostDefaultValues.Count; i++)
            {
                if (!AllWorkPost_Data.TryGetValue(i, out var data) || data == null)
                {
                    AllWorkPost_Data[i] = new WorkPost_Data(i, StationID, 0);
                }
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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Station ID:", $"{StationID}" },
                { "Station Name:", $"{StationName}" },
                { "Station Description:", $"{StationDescription}" },
                { "Station IsActive:", $"{StationIsActive}" },
                { "JobSite ID:", $"{JobSiteID}" },
                { "Default Product:", $"{DefaultProduct}" },
                { "Base Progress Rate Per Hour:", $"{BaseProgressRatePerHour}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Base Station Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Station Progress Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StationProgressData.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Station WorkPosts",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllWorkPost_Components?.Values.ToDictionary(
                    workPost => $"{workPost.WorkPostID} -",
                    workPost => $"{workPost.WorkPostData?.CurrentWorker?.ActorID}: " +
                                $"{workPost.WorkPostData?.CurrentWorker}"));

            return _dataToDisplay;
        }
    }

    [Serializable]
    public class StationProgressData : Data_Class
    {
        public float       CurrentProgress;
        public float       CurrentQuality;
        public Recipe_Data CurrentProduct;
        public void        SetCurrentProduct(Recipe_Data currentProduct) => CurrentProduct = currentProduct;

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Progress:", $"{CurrentProgress}" },
                { "Current Quality:", $"{CurrentQuality}" },
                { "Current Product:", $"{CurrentProduct}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Station Progress Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Current Product",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CurrentProduct.GetSubData(toggleMissingDataDebugs));

            return _dataToDisplay;
        }

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