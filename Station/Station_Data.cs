using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Buildings;
using IDs;
using Inventory;
using Jobs;
using Recipes;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        public ulong ID;
        public ulong BuildingID;

        bool _initialised;
        
        public float BaseProgressRatePerHour = 5;
        
        Station_Component _station;
        Building_Component _building;

        public List<ActorActionName> JobActions; // Get based on StationType.
        
        public Dictionary<ulong, Job_Data> Jobs = new();
        
        public InventoryData_Station InventoryData;
        public StationProgressData StationProgressData;

        public StationName StationName;
        public StationType StationType;
        public RecipeName  DefaultProduct => Station.DefaultProduct;
        
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(ID);
        public Building_Component Building => _building ??= Building_Manager.GetBuilding_Component(BuildingID);
        
        public Station_Data(ulong id, ulong buildingID, StationName stationName, 
            StationProgressData stationProgressData = null, InventoryData_Station inventoryData = null)
        {
            ID = id;
            BuildingID = buildingID;
            StationProgressData = stationProgressData ?? new StationProgressData();
            InventoryData = inventoryData ?? new InventoryData_Station(ID, null);
        }
        
        public Station_Data(Station_Data stationData)
        {
            ID = stationData.ID;
            BuildingID = stationData.BuildingID;
            InventoryData = stationData.InventoryData;
            StationProgressData = stationData.StationProgressData;
        }

        public void InitialiseStationData(ulong stationID)
        {
            if (ID != stationID)
            {
                Debug.LogError($"Station ID {stationID} does not match Station_Data ID {ID}. Cannot initialise.");
                return;
            }

            if (DefaultProduct != RecipeName.No_Recipe)
                StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Data(DefaultProduct);
            
            _initialised = true;

            InitialiseJobs();
        }
        
        public void InitialiseJobs()
        {
            if (!Application.isPlaying) return;
            
            foreach (Transform child in Station.transform)
            {
                if (child.GetComponent<Job_Component>() is null) continue;

                Object.DestroyImmediate(child.gameObject);
            }

            foreach (var job_Prefab in Job_List.GetStation_JobPrefabs(StationName))
            {
                var jobID = ID_Manager.GetNewID(IDType.Job);
                var job_Data = Job_List.GetJob_Data(job_Prefab.Name);

                _spawnJob(job_Data, job_Prefab);
            }
        }
        
        void _spawnJob(Job_Data job_Data, Job_Prefabs job_Prefab)
        {
            var jobGO = new GameObject($"{job_Data.JobName}_{job_Data.ID}").AddComponent<Job_Component>();
            jobGO.transform.SetParent(Station.transform);
            
            jobGO.transform.localPosition = job_Prefab.Position;
            jobGO.transform.localRotation = job_Prefab.Rotation;
            jobGO.transform.localScale    = job_Prefab.Scale;
                
            var job_Collider = jobGO.gameObject.AddComponent<BoxCollider>();
            job_Collider.isTrigger = true;
            jobGO.Initialise(job_Data);
                
            Jobs[job_Data.ID] = job_Data;
        }

        public Job_Data GetJob(ulong jobID)
        {
            if (Jobs.TryGetValue(jobID, out var job_Data))
                return job_Data;
            
            Debug.LogError($"Job with ID {jobID} not found in Station {StationName}.");
            return null;
        }

        public Job_Data GetOpenJob(JobName jobName = JobName.None)
        {
            foreach(var job in Jobs.Values)
            {
                if ((jobName == JobName.None || job.JobName == jobName) && job.ActorID == 0)
                    return job;
            }
            
            return null;
        }
        
        public void OnTickOneSecond()
        {
            if (!_initialised) return;
            
            _operateStation();
        }

        void _operateStation()
        {
            if (!_passesStationChecks()) return;

            Station.Operate();
        }

        bool _passesStationChecks()
        {
            var success = false;

            for (var i = 0; i < 2; i++)
            {
                try
                {
                    success = DefaultProduct == RecipeName.No_Recipe 
                              || InventoryData.InventoryContainsAllItems(StationProgressData.CurrentProduct.RequiredIngredients);
                    break;
                }
                catch
                {
                    StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Data(DefaultProduct);

                    if (StationProgressData.CurrentProduct.RecipeName != RecipeName.None 
                        || DefaultProduct == RecipeName.None)
                        break;
                }    
            }
        
            return success;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Station ID", $"{ID}" },
                { "Station Name", $"{StationName}" },
                { "Building ID", $"{BuildingID}" },
                { "Default Product", $"{DefaultProduct}" },
                { "Base Progress Rate Per Hour", $"{BaseProgressRatePerHour}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Station Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Station Progress Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StationProgressData.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Station WorkPosts",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: Jobs?.Values.ToDictionary(
                    job_Data => $"{job_Data.ID}",
                    job_Data => $"{job_Data?.Actor_Data?.ActorName} ({job_Data?.Actor_Data?.ActorID})"));

            return DataToDisplay;
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
                { "Current Progress", $"{CurrentProgress}" },
                { "Current Quality", $"{CurrentQuality}" },
                { "Current Product", $"{CurrentProduct}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Station Progress Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Current Product",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CurrentProduct?.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public bool ItemCrafted(float progress)
        {
            if (progress == 0 || CurrentProduct.RecipeName == RecipeName.None) return false;
            if (CurrentProgress >= CurrentProduct.RequiredProgress) return true;
            if (CurrentProduct.RequiredProgress == 0)
            {
                Debug.LogError($"For Recipe {CurrentProduct.RecipeName} CurrentProgress: {CurrentProgress} ProgressRate: {progress} MaxProgress: {CurrentProduct.RequiredProgress}");
                return false;
            }

            CurrentProgress += progress;
            CurrentQuality  += progress;

            return CurrentProgress >= CurrentProduct.RequiredProgress;
        }

        public void ResetProgress()
        {
            CurrentProgress = 0;
            CurrentQuality  = 0;
        }
    }
}