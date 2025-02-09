using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using ActorPresets;
using Actors;
using City;
using Items;
using Jobs;
using Managers;
using Priorities;
using Recipes;
using Station;
using Tools;
using UnityEngine;

namespace JobSites
{
    [Serializable]
    public class JobSite_Data : Data_Class
    {
        public ulong JobSiteID;
        public ulong JobSiteFactionID;
        public ulong CityID;
        public ulong OwnerID;

        public JobSiteName JobSiteName;
        
        JobSite_Component _jobSite_Component;
        
        public SerializableDictionary<ulong, Actor_Component> AllEmployees;
        public SerializableDictionary<ulong, Station_Component> AllStations;
        
        public ProductionData ProductionData;
        public ProsperityData ProsperityData;
        public Priority_Data_JobSite PriorityData;

        public HashSet<ActorActionName> AllowedActions => AllStations.Values.SelectMany(station => station.AllowedActions).ToHashSet();
        
        public JobSite_Component JobSite_Component =>
            _jobSite_Component ??= JobSite_Manager.GetJobSite_Component(JobSiteID);

        public JobSite_Data(ulong jobSiteID, ulong jobSiteFactionID, ulong cityID, ulong ownerID,
            JobSiteName jobSiteName, List<ulong> allEmployeeIDs, List<ulong> allStationIDs,
            ProductionData productionData, ProsperityData prosperityData, Priority_Data_JobSite priorityData)
        {
            JobSiteID = jobSiteID;
            JobSiteName = jobSiteName;
            JobSiteFactionID = jobSiteFactionID;
            CityID = cityID;
            OwnerID = ownerID;
            
            if (!Application.isPlaying) return;
            
            _populateAllEmployees(allEmployeeIDs);
            _populateAllStations(allStationIDs);
            
            ProductionData = productionData ?? new ProductionData(JobSiteID);
            ProsperityData = prosperityData ?? new ProsperityData(50, 100, 1);
            PriorityData = priorityData ?? new Priority_Data_JobSite(JobSiteID);
        }

        public JobSite_Data(JobSite_Data jobSite_Data)
        {
            JobSiteID = jobSite_Data.JobSiteID;
            JobSiteName = jobSite_Data.JobSiteName;
            JobSiteFactionID = jobSite_Data.JobSiteFactionID;
            CityID = jobSite_Data.CityID;
            OwnerID = jobSite_Data.OwnerID;
            
            if (!Application.isPlaying) return;
            
            AllEmployees = jobSite_Data.AllEmployees;
            AllStations = jobSite_Data.AllStations;
            
            ProductionData = jobSite_Data.ProductionData;
            ProsperityData = jobSite_Data.ProsperityData;
            PriorityData = jobSite_Data.PriorityData;
        }
        
        void _populateAllEmployees(List<ulong> allEmployeeIDs)
        {
            AllEmployees = new SerializableDictionary<ulong, Actor_Component>();

            foreach (var employeeID in allEmployeeIDs)
            {
                var employee = Actor_Manager.GetActor_Component(employeeID);

                if (employee is null)
                {
                    Debug.Log($"Employee with ID {employeeID} not found.");
                    continue;
                }

                AllEmployees.Add(employeeID, employee);
            }
        }

        void _populateAllStations(List<ulong> allStationIDs)
        {
            AllStations = new SerializableDictionary<ulong, Station_Component>();
            
            foreach (var station in JobSite_Component.GetComponentsInChildren<Station_Component>())
            {
                AllStations.Add(station.StationID, station);
            }
            
            foreach (var stationID in allStationIDs)
            {
                if (AllStations.ContainsKey(stationID)) continue;
                
                var station = Station_Manager.GetStation_Component(stationID);

                if (station is null)
                {
                    Debug.Log($"Station with ID {stationID} not found.");
                    continue;
                }

                AllStations.Add(stationID, station);
            }
        }

        public void InitialiseJobSiteData()
        {
            _jobSite_Component = JobSite_Manager.GetJobSite_Component(JobSiteID);

            if (_jobSite_Component is null)
            {
                Debug.LogError($"JobSite with ID {JobSiteID} not found in JobSite_SO.");
                return;
            }

            _populate();
        }

        void _populate()
        {
            //Usually this will only happen a few seconds after game start since things won't hire immediately after game start. Instead it will be assigned to
            // TickRate manager to onTick();

            //* Temporarily for now
            JobSite_Component.StartCoroutine(FillEmptyJobSitePositions());
        }
        
        public Job GetActorJob(ulong actorID)
        {
            foreach (var station in AllStations.Values)
            {
                foreach (var workPost in station.Station_Data.AllWorkPosts.Values)
                {
                    if (workPost.Job.ActorID == actorID)
                        return workPost.Job;
                }
            }

            return null;
        }

        protected Actor_Component _findEmployeeFromCity(JobName positionName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            var allCitizenIDs = city?.CityData?.Population?.AllCitizenIDs;

            if (allCitizenIDs == null || !allCitizenIDs.Any())
            {
                //Debug.Log("No citizens found in the city.");
                return null;
            }

            var citizenID = allCitizenIDs
                .FirstOrDefault(c =>
                    Actor_Manager.GetActor_Data(c)?.Career.JobSiteID == 0 &&
                    _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(positionName))
                );

            if (citizenID == 0)
            {
                //Debug.LogWarning($"No suitable citizen found for position: {position} in city with ID {CityID}.");
                return null;
            }

            var actor = Actor_Manager.GetActor_Component(actorID: citizenID);

            return actor;
        }

        protected Actor_Component _generateNewEmployee(JobName jobName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            // For now, set to journeyman of whatever the job is. Later, find a way to hire based on prosperity and needs.
            var actorPresetName = ActorPreset_List.S_ActorDataPresetNameByJobName[jobName];

            var actor = Actor_Manager.SpawnNewActor(city.CitySpawnZone.transform.position, actorPresetName);

            AddEmployeeToJobSite(actor);

            return actor;
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
        
        public void AssignActorToNewCurrentJob(Actor_Component actor)
        {
            PriorityData.RegenerateAllPriorities(DataChangedName.None);

            var highestPriorityAction = PriorityData.GetHighestPriorityFromGroup(
                AllowedActions.Select(actorActionName => (ulong)actorActionName).ToList())?.PriorityID;

            if (highestPriorityAction is null or (ulong)ActorActionName.Idle) return;

            var relevantStations = _getOrderedRelevantStationsForJob((ActorActionName)highestPriorityAction, actor);
            
            AddEmployeeToStation(actor.ActorID, relevantStations.FirstOrDefault());
        }
        
        List<Station_Component> _getOrderedRelevantStationsForJob(ActorActionName actorActionName,
            Actor_Component actor)
        {
            var relevantStations = AllStations.Values
                .Where(station => station.AllowedActions.Contains(actorActionName))
                .ToList();

            if (relevantStations.Count != 0)
                return relevantStations
                    .OrderBy(station =>
                        Vector3.Distance(actor.transform.position, station.transform.position))
                    .ToList();

            Debug.LogError($"No relevant stations found for actorAction: {actorActionName}.");
            return null;
        }

        public void AddEmployeeToJobSite(Actor_Component actor)
        {
            if (AllEmployees.ContainsKey(actor.ActorID))
            {
                Debug.LogWarning($"EmployeeID: {actor.ActorID} already exists in employee list.");
                return;
            }

            AllEmployees.Add(actor.ActorID, actor);
            actor.ActorData.Career.JobSiteID = JobSiteID;
        }

        public void HireEmployee(Actor_Component actor)
        {
            AddEmployeeToJobSite(actor);

            // And then apply relevant relation buff
        }

        public void RemoveEmployeeFromJobSite(Actor_Component actor)
        {
            if (!AllEmployees.ContainsKey(actor.ActorID))
            {
                Debug.LogWarning($"EmployeeID: {actor.ActorID} is not in employee list.");
                return;
            }

            AllEmployees.Remove(actor.ActorID);
            actor.ActorData.Career.JobSiteID = 0;
        }

        public void FireEmployee(Actor_Component actor)
        {
            RemoveEmployeeFromJobSite(actor);

            // And then apply relation debuff.
        }

        public void AddEmployeeToStation(ulong actorID, Station_Component station)
        {
            if (station.Station_Data.GetOpenWorkPost() is not { } openWorkPost) return;
            
            RemoveWorkerFromCurrentStation(actorID);
                
            openWorkPost.Job.ActorID = actorID;
        }

        public void RemoveWorkerFromCurrentStation(ulong actorID)
        {
            if (GetActorJob(actorID) is { } job)
                job.ActorID = 0;
        }

        public void RemoveAllWorkersFromStation(Station_Component station)
        {
            foreach (var workPost in station.Station_Data.AllWorkPosts.Values)
            {
                workPost.Job.ActorID = 0;
            }
        }

        public void RemoveAllWorkersFromAllStations()
        {
            foreach (var station in AllStations.Values)
            {
                RemoveAllWorkersFromStation(station);
            }
        }

        public HashSet<Item> GetEstimatedProductionRatePerHour()
        {
            var estimatedProductionItems = new Dictionary<ulong, Item>();

            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var station in AllStations.Values)
            {
                //* Eventually find a way to group all non-production station types together and skip them all.
                //* Currently only works for LogPile.
                if (station.StationName == StationName.Log_Pile) continue;
                
                foreach (var workPost in station.Station_Data.AllWorkPosts.Values)
                {
                    if (workPost.Job.ActorID == 0) continue;

                    float totalProductionRate = 0;

                    if (station.Station_Data.StationProgressData.CurrentProduct is null
                        || station.Station_Data.StationProgressData.CurrentProduct.RecipeName == RecipeName.None)
                        continue;

                    var individualProductionRate = station.Station_Data.BaseProgressRatePerHour;

                    foreach (var vocation in station.Station_Data.StationProgressData.CurrentProduct.RequiredVocations)
                    {
                        individualProductionRate *= Actor_Manager.GetActor_Data(workPost.Job.ActorID).Vocation
                            .GetProgress(vocation.Value);
                    }

                    totalProductionRate += individualProductionRate;
                    // Don't forget to add in estimations for travel time.

                    float requiredProgress = station.Station_Data.StationProgressData.CurrentProduct.RequiredProgress;
                    var estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

                    foreach (var product in station.Station_Data.StationProgressData.CurrentProduct.RecipeProducts)
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

        // public void AllocateEmployeesToStations()
        // {
        //     var allOperators = GetAllOperators();

        //     var allCombinations = GetAllCombinations(allOperators);

        //     var bestCombination = new List<OperatorData>();

        //     var bestRatioDifference = 100f;

        //     foreach (var combination in allCombinations)
        //     {
        //         var ratioDifference = Mathf.Abs(ProsperityData.GetProsperityPercentage() - ProsperityData.GetProsperityPercentage(combination));

        //         if (ratioDifference < bestRatioDifference)
        //         {
        //             Debug.Log($"Combination {combination} the is best ratio");

        //             bestRatioDifference = ratioDifference;
        //             bestCombination = new List<OperatorData>(combination);
        //         }
        //     }

        //     AssignEmployeesToStations(bestCombination);

        //     Debug.Log("Adjusted production to balance the ratio.");
        // }

        public Station_Component GetNearestRelevantStationInJobSite(Vector3 position, List<StationName> stationNames)
            => AllStations
                .Where(station => stationNames.Contains(station.Value.StationName))
                .OrderBy(station => Vector3.Distance(position, station.Value.transform.position))
                .FirstOrDefault().Value;
        
        public IEnumerator FillEmptyJobSitePositions()
        {
            yield return new WaitForSeconds(2);
            
            var prosperityRatio = ProsperityData.GetProsperityPercentage();
            var maxEmployeeCount = AllStations.Values.Sum(station => station.Station_Data.AllWorkPosts.Count);
            var desiredEmployeeCount = Mathf.RoundToInt(maxEmployeeCount * prosperityRatio);

            if (AllEmployees.Count >= Mathf.Min(maxEmployeeCount, desiredEmployeeCount))
            {
                Debug.Log($"Employee Count: {AllEmployees.Count}/{desiredEmployeeCount} (Max: {maxEmployeeCount})");
                yield break;
            }

            if (AllStations.Count == 0)
            {
                Debug.LogWarning("No stations found in JobSite.");
                yield break;
            }

            var stationQueue = new Queue<Station_Component>(AllStations.Values);
            var iteration = 0;
    
            while (AllEmployees.Count < desiredEmployeeCount)
            {
                if (stationQueue.Count == 0) break;

                var station = stationQueue.Dequeue();

                foreach (var workPost in station.Station_Data.AllWorkPosts.Values)
                {
                    if (AllEmployees.Count >= desiredEmployeeCount) break;
                    if (workPost.Job.ActorID != 0) continue;

                    var newEmployee = _findEmployeeFromCity(station.DefaultJobName) ??
                                      _generateNewEmployee(station.DefaultJobName);

                    JobSite_Component.AssignActorToNewCurrentJob(newEmployee);
            
                    stationQueue.Enqueue(station);
            
                    iteration++;
                    
                    if (iteration <= 99) continue;
                    
                    Debug.LogError($"Iteration limit reached. Desired: {desiredEmployeeCount}, Current: {AllEmployees.Count}");
                    yield break;
                }
            }
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "JobSite ID", $"{JobSiteID}" },
                { "JobSite Name", $"{JobSiteName}" },
                { "JobSite Faction ID", $"{JobSiteFactionID}" },
                { "City ID", $"{CityID}" },
                { "Owner ID", $"{OwnerID}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base JobSite Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Employee Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllEmployees.ToDictionary(actor => $"{actor.Key}",
                    actor => $"{actor.Value.ActorName}({actor.Value.ActorID})"));

            _updateDataDisplay(DataToDisplay,
                title: "All Stations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllStations.ToDictionary(station => $"{station.Key}", 
                    station => $"{station.Value.StationName}({station.Value.StationID})"));

            _updateDataDisplay(DataToDisplay,
                title: "Production Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ProductionData.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Prosperity Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ProsperityData.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Priority Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: PriorityData.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                {
                    "Prosperity Data",
                    ProsperityData.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Priority Data",
                    PriorityData.GetDataToDisplay(toggleMissingDataDebugs)
                }
            };
        }
    }

    [Serializable]
    public class ProductionData : Data_Class
    {
        public List<Item> AllProducedItems;
        public HashSet<Item> EstimatedProductionRatePerHour;
        public ulong JobSiteID;

        JobSite_Component _jobSite;
        public JobSite_Component JobSite => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        
        public ProductionData(ulong jobSiteID)
        {
            JobSiteID = jobSiteID;
            AllProducedItems = new List<Item>();
        }

        public ProductionData(ProductionData productionData)
        {
            JobSiteID = productionData.JobSiteID;
            AllProducedItems = productionData.AllProducedItems;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var productionData = new Dictionary<string, string>
            {
                { "Station ID", $"{JobSiteID}" }
            };

            var allProducedItems = AllProducedItems?.ToDictionary(item => item.ItemID.ToString(),
                item => item.ItemName.ToString()) ?? new Dictionary<string, string>();
            var estimatedProductionRatePerHour = EstimatedProductionRatePerHour?.ToDictionary(
                item => item.ItemID.ToString(),
                item => item.ItemName.ToString()) ?? new Dictionary<string, string>();

            return productionData.Concat(allProducedItems).Concat(estimatedProductionRatePerHour)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Production Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public HashSet<Item> GetEstimatedProductionRatePerHour()
        {
            return EstimatedProductionRatePerHour = JobSite.JobSite_Data.GetEstimatedProductionRatePerHour();
        }
    }
}