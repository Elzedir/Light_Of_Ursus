using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using ActorPresets;
using Actors;
using Cities;
using City;
using Items;
using Jobs;
using Managers;
using Priorities;
using Recipes;
using Station;
using Tools;
using UnityEngine;
using WorkPosts;

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
        
        JobSite_Component _jobSite;
        City_Component _city;
        
        public SerializableDictionary<(ulong, ulong), Job> AllJobs;
        Dictionary<ulong, (ulong, ulong)> _actorToJobMap;
        
        public ProductionData ProductionData;
        public ProsperityData ProsperityData;
        public Priority_Data_JobSite PriorityData;

        public HashSet<ActorActionName> AllowedActions => AllJobs.Values.SelectMany(job => job.JobActions).ToHashSet();
        
        public JobSite_Component JobSite =>
            _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID) 
                         ?? throw new NullReferenceException($"JobSite with ID {JobSiteID} not found in JobSite_SO.");
        public City_Component City =>
            _city ??= City_Manager.GetCity_Component(CityID) 
                      ?? throw new NullReferenceException($"City with ID {CityID} not found in City_SO.");

        public JobSite_Data(ulong jobSiteID, ulong jobSiteFactionID, ulong cityID, ulong ownerID, 
            JobSiteName jobSiteName,
            ProductionData productionData, ProsperityData prosperityData, Priority_Data_JobSite priorityData)
        {
            JobSiteID = jobSiteID;
            JobSiteName = jobSiteName;
            JobSiteFactionID = jobSiteFactionID;
            CityID = cityID;
            OwnerID = ownerID;
            
            if (!Application.isPlaying) return;
            
            ProductionData = new ProductionData(productionData);
            ProsperityData = new ProsperityData(prosperityData);
            PriorityData = new Priority_Data_JobSite(JobSiteID);
        }
        
        void _populateAllJobs()
        {
            AllJobs = new SerializableDictionary<(ulong, ulong), Job>();
            _actorToJobMap = new Dictionary<ulong, (ulong, ulong)>();
            
            foreach (var station in JobSite.GetComponentsInChildren<Station_Component>())
            {
                foreach(var workPost in station.Station_Data.AllWorkPosts.Values)
                {
                    AllJobs.Add((station.StationID, workPost.WorkPostID), workPost.Job);
                    
                    if (workPost.Job.ActorID != 0)
                        _actorToJobMap.Add(workPost.Job.ActorID, (station.StationID, workPost.WorkPostID));
                }
            }
        }

        public void InitialiseJobSiteData()
        {
            FillEmptyJobSitePositions();
        }
        
        public Job GetActorJob(ulong actorID)
        {
            return _actorToJobMap.TryGetValue(actorID, out var jobKey) 
                ? AllJobs[jobKey] 
                : null;
        }

        protected Actor_Component _findEmployeeFromCity(JobName positionName)
        {
            var allCitizenIDs = City.CityData.Population.AllCitizenIDs;

            if (allCitizenIDs == null || !allCitizenIDs.Any()) return null;

            var citizenID = allCitizenIDs
                .FirstOrDefault(c =>
                    Actor_Manager.GetActor_Data(c)?.Career.CurrentJob == null &&
                    _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(positionName))
                );

            return citizenID != 0 
                ? Actor_Manager.GetActor_Component(actorID: citizenID) 
                : null;
        }

        protected Actor_Component _generateNewEmployee(JobName jobName)
        {
            // For now, set to journeyman of whatever the job is. Later, find a way to hire based on prosperity and needs.
            var actorPresetName = ActorPreset_List.S_ActorDataPresetNameByJobName[jobName];

            return Actor_Manager.SpawnNewActor(City.CitySpawnZone.transform.position, actorPresetName);
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
        
        public bool AssignActorToNewCurrentJob(Actor_Component actor)
        {
            if (actor is null)
            {
                Debug.LogError("Actor is null.");
                return false;
            }
            
            PriorityData.RegenerateAllPriorities(DataChangedName.None);

            var highestPriorityAction = PriorityData.GetHighestPriorityFromGroup(
                AllowedActions.Select(actorActionName => (ulong)actorActionName).ToList())?.PriorityID;

            if (highestPriorityAction is null) return false;

            var relevantStations = _getOrderedRelevantStationsForJob((ActorActionName)highestPriorityAction, actor);

            foreach (var relevantStation in relevantStations)
            {
                if (AddEmployeeToStation(actor, relevantStation))
                    return true;
            }

            return false;
        }
        
        List<Station_Component> _getOrderedRelevantStationsForJob(ActorActionName actorActionName, Actor_Component actor)
        {
            var relevantJobs = AllJobs.Values
                .Where(job => job.ActorID == 0)
                .Where(job => job.JobActions.Contains(actorActionName))
                .ToList();

            if (relevantJobs.Count != 0)
                return relevantJobs
                    .OrderBy(job => Vector3.Distance(actor.transform.position, job.Station.transform.position))
                    .Select(job => job.Station)
                    .ToList();

            Debug.LogError($"No relevant stations found for actorAction: {actorActionName}.");
            return null;
        }

        public void HireEmployee(Actor_Component actor)
        {
            AssignActorToNewCurrentJob(actor);

            // And then apply relevant relation buff
        }

        public void RemoveEmployeeFromJobSite(Actor_Component actor)
        {
            if (GetActorJob(actor.ActorID) is not { } job)
            {
                Debug.LogError($"Actor with ID {actor.ActorID} not found in JobSite with ID {JobSiteID}.");
                return;
            }
            
            AllJobs.Remove((job.StationID, job.WorkPostID));
            _actorToJobMap.Remove(actor.ActorID);
        }

        public void FireEmployee(Actor_Component actor)
        {
            RemoveEmployeeFromJobSite(actor);

            // And then apply relation debuff.
        }

        public bool AddEmployeeToStation(Actor_Component actor, Station_Component station)
        {
            if (station.Station_Data.GetOpenWorkPost() is not { } openWorkPost) return false;
            
            RemoveWorkerFromCurrentStation(actor);
            
            actor.ActorData.Career.CurrentJob = openWorkPost.Job;
            actor.ActorData.Career.CareerName = JobSite.DefaultCareer;
            
            openWorkPost.Job.ActorID = actor.ActorID;
            _actorToJobMap[actor.ActorID] = (station.StationID, openWorkPost.WorkPostID);
            
            return true;
        }

        public void RemoveWorkerFromCurrentStation(Actor_Component actor)
        {
            if (GetActorJob(actor.ActorID) is { } job) job.ActorID = 0;
        }

        public void RemoveAllWorkersFromStation(Station_Component station)
        {
            foreach (var workPost in station.Station_Data.AllWorkPosts.Values)
            {
                workPost.Job.ActorID = 0;
            }
        }

        public void RemoveAllWorkersFromAllJobs()
        {
            foreach (var job in AllJobs.Values)
            {
                RemoveAllWorkersFromStation(job.Station);
            }
        }

        public HashSet<Item> GetEstimatedProductionRatePerHour()
        {
            var estimatedProductionItems = new Dictionary<ulong, Item>();

            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var job in AllJobs.Values)
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

        public Station_Component GetNearestStation(Vector3 position, StationName stationName)
        {
            Station_Component nearestStation = null;

            var nearestDistance = float.MaxValue;

            foreach (var job in AllJobs.Values.Where(job => job.Station.StationName == stationName))
            {
                var distance = Vector3.Distance(position, job.Station.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestStation = job.Station;
                nearestDistance = distance;
            }

            return nearestStation;
        }

        public Station_Component GetNearestRelevantStationInJobSite(Vector3 position, List<StationName> stationNames)
            => AllJobs
                .Where(job => stationNames.Contains(job.Value.Station.StationName))
                .OrderBy(station => Vector3.Distance(position, station.Value.Station.transform.position))
                .Select(station => station.Value.Station)
                .FirstOrDefault();
        
        public void FillEmptyJobSitePositions()
        {
            if (AllJobs?.Count is null or 0) _populateAllJobs();
            
            if (AllJobs?.Count is null or 0)
            {
                Debug.LogError($"AllJobs {AllJobs?.Count} is null or 0.");
                return;
            }

            var relevantStations = AllJobs.Values
                .Where(job => job.Station.StationType != StationType.Recreation)
                .ToList().Count;
            var desiredEmployeeCount = Mathf.RoundToInt(relevantStations * ProsperityData.GetProsperityPercentage());
            var jobQueue = new Queue<Job>(AllJobs.Values);
            var iteration = 0;
            
            while (_actorToJobMap.Count < desiredEmployeeCount)
            {
                if (jobQueue.Count == 0) break;

                var job = jobQueue.Dequeue();

                var openWorkPost = job.Station.Station_Data.GetOpenWorkPost();

                if (openWorkPost is null || openWorkPost.CurrentWorker is not null) continue;

                var newEmployee =
                    _findEmployeeFromCity(openWorkPost.WorkPost_DefaultValues.JobName)
                    ?? _generateNewEmployee(openWorkPost.WorkPost_DefaultValues.JobName);
                
                if (!AssignActorToNewCurrentJob(newEmployee))
                {
                    Debug.LogError($"Could not assign actor {newEmployee.ActorID} to a job.");
                    continue;
                }
                
                if (_actorToJobMap.Count >= desiredEmployeeCount) break;
                
                jobQueue.Enqueue(job);

                iteration++;

                if (iteration <= 99) continue;

                Debug.LogError($"Iteration limit reached. Desired: {desiredEmployeeCount}, Current: {AllJobs.Count}");
                return;
            }
        }

        public class HaulingData
        {
            
        }
        
        public Dictionary<ulong, HaulingData> AllHaulers; 

        public void Haul(List<Job> haulers)
        {
            var itemsToFetch = GetItemsToFetchFromStations();
            var haulerQueue = new Queue<Job>(haulers);
            
            while (itemsToFetch.Count > 0)
            {
                if (haulerQueue.Count == 0) break;
                
                var hauler = haulerQueue.Dequeue();
                
                var bestTask = AssignBestTask(hauler, itemsToFetch);
        
                if (bestTask != null)
                {
                    ExecuteTask(hauler, bestTask);
                }
                else
                {
                    haulerQueue.Enqueue(hauler);
                }
            }
        }
        
        Task AssignBestTask(Job hauler, Dictionary<ulong, Dictionary<ulong, ulong>> itemsToFetch)
        {
            Task bestTask = null;
            float bestScore = float.MaxValue;

            foreach (var (stationID, items) in itemsToFetch)
            {
                foreach (var (itemID, amount) in items)
                {
                    float distance = GetDistance(hauler.Position, GetStationPosition(stationID));
                    float congestion = GetCongestionFactor(stationID);
                    float priority = distance + congestion;

                    if (priority < bestScore)
                    {
                        bestScore = priority;
                        bestTask = new Task(stationID, itemID, amount);
                    }
                }
            }

            return bestTask;
        }

        public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToFetchFromStations()
        {
            var itemsPerStation = new Dictionary<ulong, Dictionary<ulong, ulong>>();

            foreach (var job in AllJobs.Values)
            {
                var itemsToFetchFromThisStation = job.Station.Station_Data.InventoryData
                    .GetItemsToFetchFromThisInventory();
                
                if (itemsToFetchFromThisStation.Count == 0) continue;
                
                itemsPerStation[job.StationID] = itemsToFetchFromThisStation;
            }

            return itemsPerStation;
        }

        public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToEachStation(Dictionary<ulong, ulong> allItemsToFetch)
        {
            
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
                title: "All Employees",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllJobs
                    .Where(job => job.Value.ActorID != 0)
                    .ToDictionary(
                        job => $"{job.Value.ActorID}",
                        job => $"{job.Value.Actor.ActorName}({job.Value.ActorID})"));
            
            _updateDataDisplay(DataToDisplay,
                title: "All WorkPosts",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllJobs.ToDictionary(job => $"{job.Key}", 
                    station => $"{station.Value.Station.StationName}({station.Value.StationID}) - {station.Value.WorkPostID}"));

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