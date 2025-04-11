using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using ActorPresets;
using Actors;
using Items;
using Jobs;
using Priorities;
using Recipes;
using Settlements;
using Station;
using Tools;
using UnityEngine;

namespace Buildings
{
    [Serializable]
    public class Building_Data : Data_Class
    {
        public ulong ID;
        public ulong FactionID;
        public ulong SettlementID;
        public ulong OwnerID;

        public float Gold;
        
        public string Name;

        public BuildingType BuildingType;
        
        Building_Component _building;
        Settlement_Component _settlement;

        Actor_Data _owner;
        
        public Building_Jobs Jobs;
        public Building_Production Production;
        
        public Building_Prosperity Prosperity;
        public Priority_Data_Building Priorities;

        public HashSet<ActorActionName> AllowedActions => Jobs.AllowedActions;
        
        public Building_Component Building =>
            _building ??= Building_Manager.GetBuilding_Component(ID) 
                         ?? throw new NullReferenceException($"Building with ID {ID} not found in Building_SO.");
        public Settlement_Component Settlement =>
            _settlement ??= Settlement_Manager.GetSettlement_Component(SettlementID) 
                      ?? throw new NullReferenceException($"Settlement with ID {SettlementID} not found in Settlement_SO.");
        
        public Actor_Data Owner =>
            _owner ??= Actor_Manager.GetActor_Data(OwnerID) 
                      ?? throw new NullReferenceException($"Actor with ID {OwnerID} not found in Actor_SO.");

        public Building_Data(ulong id, ulong settlementID, ulong ownerID,
            Building_Jobs jobs, Building_Production production, Building_Prosperity prosperity, 
            Priority_Data_Building priorityData)
        {
            ID = id;
            SettlementID = settlementID;
            OwnerID = ownerID;
            
            if (!Application.isPlaying) return;
            
            Jobs = new Building_Jobs(jobs, this);
            Production = new Building_Production(production);
            Priorities = new Priority_Data_Building(ID);
            Prosperity = new Building_Prosperity(prosperity);
        }
        
        public void InitialiseBuildingData(ulong buildingID)
        {
            if (ID != buildingID) throw new NullReferenceException($"Building ID {buildingID} does not match " +
                $"Building_Data ID {ID}.");

            Jobs.InitialiseJobs();
        }

        public void OnTickOneSecond()
        {
            Jobs.OnTickOneSecond();
        }

        public void OnProgressDay()
        {
            Jobs.OnProgressDay();
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            var income = Production.GenerateIncome();

            var tax = income * liegeTaxRate;
            
            Gold += income - tax;

            return tax;
        }

        public Actor_Component FindEmployeeFromSettlement(JobName positionName)
        {
            var allCitizenIDs = Settlement.Settlement_Data.Population.AllCitizenIDs;

            if (allCitizenIDs == null || !allCitizenIDs.Any()) return null;

            var citizenID = allCitizenIDs
                .FirstOrDefault(citizen =>
                    Actor_Manager.GetActor_Data(citizen)?.Career.Job == null &&
                    _hasMinimumVocationRequired(citizen, _getVocationAndMinimumExperienceRequired(positionName))
                );

            return citizenID != 0 
                ? Actor_Manager.GetActor_Component(actorID: citizenID) 
                : null;
        }

        public Actor_Component GenerateNewEmployee(JobName jobName)
        {
            // For now, set to journeyman of whatever the job is. Later, find a way to hire based on prosperity and needs.
            var actorPresetName = ActorPreset_List.S_ActorDataPresetNameByJobName[jobName];

            return Actor_Manager.SpawnNewActor(Settlement.SettlementSpawnZone.transform.position, actorPresetName);
        }

        protected bool _hasMinimumVocationRequired(ulong citizenID, List<VocationRequirement> vocationRequirements)
        {
            var actorData = Actor_Manager.GetActor_Data(citizenID);

            foreach (var vocation in vocationRequirements)
            {
                if (vocation.VocationName == VocationName.None) continue;

                if (actorData.Vocation.GetVocationExperience(vocation.VocationName) <
                    vocation.MinimumVocationExperience)
                {
                    return false;
                }
            }

            return true;
        }

        protected List<VocationRequirement> _getVocationAndMinimumExperienceRequired(JobName jobName)
        {
            var vocationRequirements = new List<VocationRequirement>();

            switch (jobName)
            {
                case JobName.Logger:
                    vocationRequirements = new List<VocationRequirement>
                    {
                        new(VocationName.Logging, 1000)
                    };
                    break;
                case JobName.Sawyer:
                    vocationRequirements = new List<VocationRequirement>
                    {
                        new(VocationName.Sawying, 1000)
                    };
                    break;
                default:
                    Debug.Log($"JobName: {jobName} not recognised.");
                    break;
            }

            return vocationRequirements;
        }

        public HashSet<Item> GetEstimatedProductionRatePerHour()
        {
            var estimatedProductionItems = new Dictionary<ulong, Item>();

            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var job in Stations.AllStations.Values)
            {
                //* Eventually find a way to group all non-production station types together and skip them all.
                //* Currently only works for LogPile.
                if (job.Station.StationName == StationName.Log_Pile) continue;
                
                foreach (var workPost in job.Station.Station_Data.AllWorkPosts.Values)
                {
                    if (workPost.Job.ActorID == 0) continue;

                    float totalProductionRate = 0;

                    if (job.Station.Station_Data.StationProgressData.CurrentProduct is null
                        || job.Station.Station_Data.StationProgressData.CurrentProduct.RecipeName == RecipeName.None)
                        continue;

                    var individualProductionRate = job.Station.Station_Data.BaseProgressRatePerHour;

                    foreach (var vocation in job.Station.Station_Data.StationProgressData.CurrentProduct.RequiredVocations)
                    {
                        individualProductionRate *= Actor_Manager.GetActor_Data(workPost.Job.ActorID).Vocation
                            .GetProgress(vocation.Value);
                    }

                    totalProductionRate += individualProductionRate;
                    // Don't forget to add in estimations for travel time.

                    float requiredProgress = job.Station.Station_Data.StationProgressData.CurrentProduct.RequiredProgress;
                    var estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

                    foreach (var product in job.Station.Station_Data.StationProgressData.CurrentProduct.RecipeProducts)
                    {
                        if (!estimatedProductionItems.TryGetValue(product.Key, out var item)) 
                            estimatedProductionItems[product.Key] = new Item(product.Key, product.Value);
                        else
                            item.ItemAmount += (ulong)estimatedProductionCount;
                    }
                }
            }

            return estimatedProductionItems.Values.ToHashSet();
        }

        public Station_Component GetNearestRelevantStationInBuilding(Vector3 position, List<StationName> stationNames)
            => Jobs.Stations
                .Where(job => stationNames.Contains(job.Value.Station.Station_Data.StationName))
                .OrderBy(station => Vector3.Distance(position, station.Value.Station.transform.position))
                .Select(station => station.Value.Station)
                .FirstOrDefault();

        public class HaulingData
        {
            
        }
        
        public Dictionary<ulong, HaulingData> AllHaulers;

        bool _tested;

        public void Haul(List<Job_Data> haulers)
        {
            //* Move somewhere
            // if (_tested) return;
            // _tested = true;
            //
            // Building.StartCoroutine(TemporaryTextWriter.RunTest(0.1f));

             var itemsToFetch = GetItemsToFetchFromStations();
             var haulerQueue = new Queue<Job_Data>(haulers);
            
            //  while (itemsToFetch.Count > 0)
            //  {
            //      if (haulerQueue.Count == 0) break;
            //      
            //      var hauler = haulerQueue.Dequeue();
            //      
            //      var bestTask = AssignBestTask(hauler, itemsToFetch);
            //
            //      if (bestTask != null)
            //      {
            //          ExecuteTask(hauler, bestTask);
            //      }
            //      else
            //      {
            //          haulerQueue.Enqueue(hauler);
            //      }
            // }
        }

        // Task AssignBestTask(Job hauler, Dictionary<ulong, Dictionary<ulong, ulong>> itemsToFetch)
        // {
        //     Task bestTask = null;
        //     float bestScore = float.PositiveInfinity;
        //
        //     foreach (var (stationID, items) in itemsToFetch)
        //     {
        //         foreach (var (itemID, amount) in items)
        //         {
        //             float distance = GetDistance(hauler.Position, GetStationPosition(stationID));
        //             float congestion = GetCongestionFactor(stationID);
        //             float priority = distance + congestion;
        //
        //             if (priority < bestScore)
        //             {
        //                 bestScore = priority;
        //                 bestTask = new Task(stationID, itemID, amount);
        //             }
        //         }
        //     }
        //
        //     return bestTask;
        // }
        
        public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToFetchFromStations()
        {
            var itemsPerStation = new Dictionary<ulong, Dictionary<ulong, ulong>>();
        
            foreach (var job in Stations.AllStations.Values)
            {
                var itemsToFetchFromThisStation = job.Station.Station_Data.InventoryData
                    .GetItemsToFetchFromThisInventory();
                
                if (itemsToFetchFromThisStation.Count == 0) continue;
                
                itemsPerStation[job.ID] = itemsToFetchFromThisStation;
            }
        
            return itemsPerStation;
        }
        
        // public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToEachStation(Dictionary<ulong, ulong> allItemsToFetch)
        // {
        //     
        // }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Building ID", $"{ID}" },
                { "Name", $"{Name}" },
                { "Faction ID", $"{FactionID}" },
                { "City ID", $"{SettlementID}" },
                { "Owner ID", $"{OwnerID}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Building Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "All Employees",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: Stations.AllStations
                    .Where(job => job.Value.ActorID != 0)
                    .ToDictionary(
                        job => $"{job.Value.ActorID}",
                        job => $"{job.Value.Actor.ActorName}({job.Value.ActorID})"));
            
            _updateDataDisplay(DataToDisplay,
                title: "All WorkPosts",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: Stations.AllStations.ToDictionary(job => $"{job.Key}", 
                    station => $"{station.Value.Station.StationName}({station.Value.ID}) - {station.Value.WorkPostID}"));

            _updateDataDisplay(DataToDisplay,
                title: "Production Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Production.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Prosperity Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Prosperity.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Priority Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Priorities.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                {
                    "Prosperity Data",
                    Prosperity.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Priority Data",
                    Priorities.GetDataToDisplay(toggleMissingDataDebugs)
                }
            };
        }
    }
}