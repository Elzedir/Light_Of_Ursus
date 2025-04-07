using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Inventory;
using Jobs;
using Recipes;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;
using WorkPosts;
using Object = UnityEngine.Object;

namespace Station
{
    [Serializable]
    public class Station_Data : Data_Class
    {
        //* Later, test the possibility of using an ID system similar to WorkPost. Will we ever have a Station without a Building?
        //* Therefore, do Stations need their own ID, or can we just give them an incremental ID like WorkPost.Obviously, a station has
        //* a fixed and default number of WorkPosts, whereas a Building can have varying stations, but maybe each ID can be specific to
        //* each Building, rather than being a global ID. Or maybe it is safer since workPosts will always have the same position relative
        //* to the station, whereas station will always different depending on the map.
        public ulong StationID;
        public ulong BuildingID;

        bool _initialised;
        
        public float BaseProgressRatePerHour = 5;
        
        Station_Component _station;
        Building_Component _building;
        
        public SerializableDictionary<ulong, WorkPost_Component> AllWorkPosts;
        
        public InventoryData_Station InventoryData;
        public StationProgressData StationProgressData;

        public StationName StationName    => Station.StationName;
        public RecipeName  DefaultProduct => Station.DefaultProduct;
        
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);
        public Building_Component Building => _building ??= Building_Manager.GetBuilding_Component(BuildingID);
        
        public Station_Data(ulong stationID, ulong buildingID, StationName stationName, 
            StationProgressData stationProgressData = null, InventoryData_Station inventoryData = null)
        {
            //* stationName; When loading from data, if a station exists, then it doesn't do anything, it just helps display what type of station it is.
            //* But if the station does exist, then we generate a new station with the required stationName type.
            StationID = stationID;
            BuildingID = buildingID;
            StationProgressData = stationProgressData ?? new StationProgressData();
            InventoryData = inventoryData ?? new InventoryData_Station(StationID, null);
        }
        
        public Station_Data(Station_Data stationData)
        {
            StationID = stationData.StationID;
            BuildingID = stationData.BuildingID;
            InventoryData = stationData.InventoryData;
            StationProgressData = stationData.StationProgressData;
        }

        public void InitialiseStationData()
        {
            //* Maybe a reference error, like the ticker and StationData.
            
            //* City and station are breaking if selected in the inspector from beginning, the rest are breaking from starting and then replaying the game
            //* without closing the editor. So find common ground for the two. The City_Component and Station_Component are null.
            //* For now we'll use this GetStation Fix to assign the null value.

            if (Station is null)
            {
                Debug.LogError($"Station with ID {StationID} not found in Station_SO.");
                return;
            }

            if (DefaultProduct != RecipeName.No_Recipe)
                StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Data(DefaultProduct);
            
            _initialised = true;
            
            _populateAllWorkPosts();
        }

        void _populateAllWorkPosts()
        {
            if (!Application.isPlaying) return;
            
            foreach (Transform child in Station.transform)
            {
                if (child.GetComponent<WorkPost_Component>() is null) continue;

                Object.DestroyImmediate(child.gameObject);
            }

            var workPost_DefaultValues = WorkPost_List.GetWorkPost_DefaultValues(StationName);
            AllWorkPosts = new SerializableDictionary<ulong, WorkPost_Component>();

            foreach (var defaultValue in workPost_DefaultValues)
            {
                var job = new Job(
                    jobName: defaultValue.JobName,
                    buildingID: BuildingID,
                    actorID: 0,
                    stationID: StationID,
                    workPostID: defaultValue.WorkPostID);

                var workPost_Component = new GameObject($"WorkPost_{job.WorkPostID}").AddComponent<WorkPost_Component>();
                
                workPost_Component.transform.SetParent(Station.transform);
            
                workPost_Component.transform.localPosition = defaultValue.Position;
                workPost_Component.transform.localRotation = defaultValue.Rotation;
                workPost_Component.transform.localScale    = defaultValue.Scale;
                
                var workPost_Collider = workPost_Component.gameObject.AddComponent<BoxCollider>();
                workPost_Collider.isTrigger = true;
                workPost_Component.Initialise(job);
                
                AllWorkPosts[job.WorkPostID] = workPost_Component;
            }
        }

        public WorkPost_Component GetWorkPost(ulong workPostID)
        {
            if (AllWorkPosts.TryGetValue(workPostID, out var workPost))
                return workPost;
            
            Debug.LogError($"WorkPost with ID {workPostID} not found in Station {StationName}.");
            return null;
        }

        public WorkPost_Component GetOpenWorkPost(JobName jobName = JobName.None)
        {
            foreach(var workPost in AllWorkPosts.Values)
            {
                if ((jobName == JobName.None || workPost.Job.JobName == jobName) && workPost.Job.ActorID == 0)
                    return GetWorkPost(workPost.WorkPostID);
            }
            
            return null;
        }
        
        public void OnTick()
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
                { "Station ID", $"{StationID}" },
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