using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Jobs;
using JobSites;
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
        public ulong StationID;
        public ulong JobSiteID;

        bool _initialised;
        
        public float BaseProgressRatePerHour = 5;
        
        Station_Component _station_Component;
        JobSite_Component _jobSite_Component;
        public TickRateName CurrentTickRateName;
        
        public SerializableDictionary<ulong, WorkPost_Component> AllWorkPosts;
        
        public InventoryData_Station InventoryData;
        public StationProgressData StationProgressData;

        public StationName StationName    => Station_Component.StationName;
        public RecipeName  DefaultProduct => Station_Component.DefaultProduct;

        public Station_Component Station_Component => _station_Component ??= Station_Manager.GetStation_Component(StationID);
        public JobSite_Component JobSite_Component => _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        
        public Station_Data(ulong stationID, ulong jobSiteID, StationName stationName, StationProgressData stationProgressData = null, InventoryData_Station inventoryData = null)
        {
            //* stationName; When loading from data, if a station exists, then it doesn't do anything, it just helps display what type of station it is.
            //* But if the station does exist, then we generate a new station with the required stationName type.
            StationID = stationID;
            JobSiteID = jobSiteID;
            StationProgressData = stationProgressData ?? new StationProgressData();
            InventoryData = inventoryData ?? new InventoryData_Station(StationID, null);
            _populateAllWorkPosts();
        }
        
        public Station_Data(Station_Data stationData)
        {
            StationID = stationData.StationID;
            JobSiteID = stationData.JobSiteID;
            InventoryData = stationData.InventoryData;
            StationProgressData = stationData.StationProgressData;
            _populateAllWorkPosts();
        }

        public void InitialiseStationData()
        {
            //* Maybe a reference error, like the ticker and StationData.
            
            //* City and station are breaking if selected in the inspector from beginning, the rest are breaking from starting and then replaying the game
            //* without closing the editor. So find common ground for the two. The City_Component and Station_Component are null.
            //* For now we'll use this GetStation Fix to assign the null value.

            if (Station_Component is null)
            {
                Debug.LogError($"Station with ID {StationID} not found in Station_SO.");
                return;
            }

            if (DefaultProduct != RecipeName.No_Recipe)
                StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Data(DefaultProduct);
            
            _initialised = true;
        }

        void _populateAllWorkPosts()
        {
            if (!Application.isPlaying)
                return;
            
            foreach (Transform child in Station_Component.transform)
            {
                if (child.GetComponent<WorkPost_Component>() is null) continue;

                Object.DestroyImmediate(child.gameObject);
            }
            
            var workPost_DefaultValues = WorkPost_List.GetWorkPlace_DefaultValues(StationName);

            if (workPost_DefaultValues is null)
            {
                Debug.Log($"WorkPost_TransformValue not found for StationName: {StationName}");
                return;
            }
            
            AllWorkPosts = new SerializableDictionary<ulong, WorkPost_Component>();

            for (var i = 0; i < workPost_DefaultValues.Count; i++)
            {
                var defaultValues = workPost_DefaultValues[i % workPost_DefaultValues.Count];

                if (defaultValues is null)
                {
                    Debug.Log($"WorkPost_TransformValue not found for StationName: {StationName}");
                    return;
                }
                
                var job = new Job(
                    jobName: defaultValues.JobName,
                    jobSiteID: JobSiteID,
                    actorID: 0,
                    stationID: StationID,
                    workPostID: (ulong)i);

                var workPost_Component = new GameObject($"WorkPost_{job.WorkPostID}").AddComponent<WorkPost_Component>();
                
                workPost_Component.transform.SetParent(Station_Component.transform);
            
                workPost_Component.transform.localPosition = defaultValues.Position;
                workPost_Component.transform.localRotation = defaultValues.Rotation;
                workPost_Component.transform.localScale    = defaultValues.Scale;
                
                var workPost_Collider = workPost_Component.gameObject.AddComponent<BoxCollider>();
                workPost_Collider.isTrigger = true;
                workPost_Component.Initialise(job);
                
                AllWorkPosts[job.WorkPostID] = workPost_Component;
            }
        }

        public WorkPost_Component GetOpenWorkPost(JobName jobName = JobName.None) => 
            jobName == JobName.None
                ? AllWorkPosts.Values.FirstOrDefault(workPost => workPost.Job.Actor is null)
                : AllWorkPosts.Values.Where(workPost => workPost.Job.JobName == jobName)
                    .FirstOrDefault(workPost => workPost.Job.Actor is null);
        
        public void OnTick()
        {
            if (!_initialised) return;
            
            _operateStation();
        }

        void _operateStation()
        {
            if (!_passesStationChecks()) return;
            
            foreach (var workPost in AllWorkPosts.Values)
            {
                if (workPost.Job.Actor is null) continue;
                
                var progressMade = workPost.Operate(BaseProgressRatePerHour,
                    StationProgressData.CurrentProduct);

                if (!StationProgressData.ItemCrafted(progressMade)) continue;
                
                if (Station_Component.CanCraftItem(
                        StationProgressData.CurrentProduct.RecipeName, workPost.Job.Actor))
                    StationProgressData.ResetProgress();
            }
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
                { "Station ID", $"{StationID}" },
                { "Station Name", $"{StationName}" },
                { "JobSite ID", $"{JobSiteID}" },
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
                allStringData: AllWorkPosts?.Values.ToDictionary(
                    workPost => $"{workPost.WorkPostID}",
                    workPost => $"{workPost.Job?.Actor?.ActorData.ActorName} ({workPost.Job?.Actor?.ActorID})"));

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