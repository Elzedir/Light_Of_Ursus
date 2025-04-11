using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Jobs;
using Station;
using UnityEngine;

namespace Buildings
{
    public class Building_Jobs
    {
        readonly Building_Data _building_Data;
        public readonly Dictionary<ulong, Station_Data> Stations;
        public readonly Dictionary<ulong, Job_Data> Jobs;
        public readonly Dictionary<ulong, Actor_Data> Actors;
        
        public readonly Dictionary<ulong, ulong> JobToActorMap = new();
        public readonly Dictionary<ulong, ulong> ActorToJobMap = new();
        
        public HashSet<ActorActionName> AllowedActions => Stations.Values.SelectMany(station => station.JobActions).ToHashSet();

        public Building_Jobs(Building_Jobs data, Building_Data building_Data)
        {
            _building_Data = building_Data;
            Stations = new Dictionary<ulong, Station_Data>(data.Stations);
            Jobs = new Dictionary<ulong, Job_Data>(data.Jobs);
            Actors = new Dictionary<ulong, Actor_Data>(data.Actors);

            foreach(var job in Jobs.Values)
            {
                if (job.Actor_Data is null) continue;
                
                JobToActorMap[job.ID] = job.Actor_Data.ActorID;
            }
            
            foreach (var actor in Actors.Values)
            {
                if (actor.Career.Job is null) continue;
                
                ActorToJobMap[actor.ActorID] = actor.Career.Job.ID;
            }
        }
        
        public Building_Jobs(Dictionary<ulong, Station_Data> allStations = null, 
            Dictionary<ulong, Job_Data> allJobs = null,
            Dictionary<ulong, Actor_Data> allActors = null)
        {
            Stations = allStations ?? new Dictionary<ulong, Station_Data>();
            Jobs = allJobs ?? new Dictionary<ulong, Job_Data>();
            Actors = allActors ?? new Dictionary<ulong, Actor_Data>();
        }

        public void InitialiseJobs()
        {
            foreach (var station in Stations.Values)
            {
                _spawnStation(station);
            }
        }
        
        void _spawnStation(Station_Data station_Data)
        {
            var stationGO = new GameObject($"{station_Data.StationType}_{station_Data.ID}");
            stationGO.transform.SetParent(_building_Data.Building.transform);
            
            var station = stationGO.AddComponent<Station_Component>();
            station.Initialise();

            foreach (var job in station.Station_Data.Jobs)
            {
                Jobs.Add(job.Key, job.Value);
            }
        }

        public void OnTickOneSecond()
        {
            foreach (var station in Stations.Values)
            {
                station.OnTickOneSecond();
            }
        }

        public void OnProgressDay()
        {
            FillEmptyBuildingPositions();
        }
        
        public Job_Data GetActorJob(ulong actorID)
        {
            return ActorToJobMap.TryGetValue(actorID, out var jobID) 
                ? Jobs[jobID] 
                : null;
        }
        
        public void HireEmployee(Actor_Component actor)
        {
            AssignActorToNewCurrentJob(actor);

            // And then apply relevant relation buff
        }

        public void RemoveEmployeeFromBuilding(Actor_Component actor)
        {
            if (GetActorJob(actor.ActorID) is not { } job)
            {
                Debug.LogError($"Actor with ID {actor.ActorID} not found in Building with ID {ID}.");
                return;
            }
            
            Jobs.Remove(job.ID);
        }

        public void FireEmployee(Actor_Component actor)
        {
            RemoveEmployeeFromBuilding(actor);

            // And then apply relation debuff.
        }

        public bool AddEmployeeToStation(Actor_Component actor, Station_Component station)
        {
            if (station.Station_Data.GetOpenJob() is not { } job_Data) return false;
            
            RemoveWorkerFromCurrentStation(actor);
            
            actor.ActorData.Career.Job = job_Data;
            actor.ActorData.Career.CareerName = _building_Data.Building.DefaultCareers[BuildingType];
            
            job_Data.ActorID = actor.ActorID;
            ActorToJobMap[actor.ActorID] = job_Data.ID;
            
            return true;
        }
        
        public void RemoveWorkerFromCurrentStation(Actor_Component actor)
        {
            if (GetActorJob(actor.ActorID) is { } job) job.ActorID = 0;
        }

        public void RemoveAllWorkersFromStation(Station_Component station)
        {
            foreach (var job in station.Station_Data.Jobs.Values)
            {
                job.ActorID = 0;
            }
        }

        public void RemoveAllWorkersFromAllJobs()
        {
            foreach (var job in Stations.Values)
            {
                RemoveAllWorkersFromStation(job.Station);
            }
        }
        
        public void FillEmptyBuildingPositions()
        {
            if (Stations?.Count is null or 0)
            {
                Debug.LogError($"AllStations {Stations?.Count} is null or 0.");
                return;
            }

            var relevantStations = Stations.Values
                .Where(station => station.StationType != StationType.Recreation)
                .ToList().Count;
            var desiredEmployeeCount = Mathf.RoundToInt(relevantStations * _building_Data.Prosperity.GetProsperityPercentage());
            var jobQueue = new Queue<Job_Data>(Stations.Values);
            var iteration = 0;
            
            while (Actors.Count < desiredEmployeeCount)
            {
                if (jobQueue.Count == 0) break;

                var job = jobQueue.Dequeue();

                var openJob = job.Station.Station_Data.GetOpenJob();

                if (openJob is null || openJob.Actor_Data is not null) continue;

                var newEmployee = Building_Data.FindEmployeeFromSettlement
                                      (openJob.WorkPost_DefaultValues.JobName)
                                  ?? Building_Data.GenerateNewEmployee(openJob.WorkPost_DefaultValues.JobName);
                
                if (!AssignActorToNewCurrentJob(newEmployee))
                {
                    Debug.LogError($"Could not assign actor {newEmployee.ActorID} to a job.");
                    continue;
                }
                
                if (ActorToJobMap.Count >= desiredEmployeeCount) break;
                
                jobQueue.Enqueue(job);

                iteration++;

                if (iteration <= 99) continue;

                Debug.LogError($"Iteration limit reached. Desired: {desiredEmployeeCount}, Current: {Stations.Count}");
                return;
            }
        }
        
        public bool AssignActorToNewCurrentJob(Actor_Component actor)
        {
            if (actor is null)
            {
                Debug.LogError("Actor is null.");
                return false;
            }
            
            _building_Data.Priorities.RegenerateAllPriorities(DataChangedName.None);

            var highestPriorityAction = _building_Data.Priorities.GetHighestPriorityFromGroup(
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
            var relevantJobs = Jobs.Values
                .Where(job => job.IsBeingOperated)
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
    }
}