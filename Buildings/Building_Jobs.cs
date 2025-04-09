using System.Collections.Generic;
using System.Linq;
using Actors;
using Jobs;
using Station;
using Tools;
using UnityEngine;

namespace Buildings
{
    public class Building_Jobs
    {
        public Building_Data Building_Data;
        
        public SerializableDictionary<(ulong, ulong), Job> AllJobs;
        public Dictionary<ulong, (ulong, ulong)> ActorToJobMap;

        public Building_Jobs(Building_Jobs data, Building_Data building_Data)
        {
            Building_Data = building_Data;
            AllJobs = new SerializableDictionary<(ulong, ulong), Job>(data.AllJobs);
            ActorToJobMap = new Dictionary<ulong, (ulong, ulong)>(data.ActorToJobMap);
        }
        
        public Building_Jobs()
        {
            AllJobs = new SerializableDictionary<(ulong, ulong), Job>();
            ActorToJobMap = new Dictionary<ulong, (ulong, ulong)>();
        }

        public void OnTickOneSecond()
        {
            foreach (var job in AllJobs.Values)
            {
                job.OnTickOneSecond();
            }
        }

        public void OnProgressDay()
        {
            FillEmptyBuildingPositions();
        }
        
        public void FillEmptyBuildingPositions()
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
            var desiredEmployeeCount = Mathf.RoundToInt(relevantStations * Building_Data.Prosperity.GetProsperityPercentage());
            var jobQueue = new Queue<Job>(AllJobs.Values);
            var iteration = 0;
            
            while (ActorToJobMap.Count < desiredEmployeeCount)
            {
                if (jobQueue.Count == 0) break;

                var job = jobQueue.Dequeue();

                var openWorkPost = job.Station.Station_Data.GetOpenWorkPost();

                if (openWorkPost is null || openWorkPost.CurrentWorker is not null) continue;

                var newEmployee = Building_Data.FindEmployeeFromSettlement
                                      (openWorkPost.WorkPost_DefaultValues.JobName)
                    ?? Building_Data.GenerateNewEmployee(openWorkPost.WorkPost_DefaultValues.JobName);
                
                if (!Building_Data.AssignActorToNewCurrentJob(newEmployee))
                {
                    Debug.LogError($"Could not assign actor {newEmployee.ActorID} to a job.");
                    continue;
                }
                
                if (ActorToJobMap.Count >= desiredEmployeeCount) break;
                
                jobQueue.Enqueue(job);

                iteration++;

                if (iteration <= 99) continue;

                Debug.LogError($"Iteration limit reached. Desired: {desiredEmployeeCount}, Current: {AllJobs.Count}");
                return;
            }
        }
        
        void _populateAllJobs()
        {
            AllJobs = new SerializableDictionary<(ulong, ulong), Job>();
            ActorToJobMap = new Dictionary<ulong, (ulong, ulong)>();
            
            foreach (var station in Building_Data.Building.GetComponentsInChildren<Station_Component>())
            {
                foreach(var workPost in station.Station_Data.AllWorkPosts.Values)
                {
                    AllJobs.Add((station.StationID, workPost.WorkPostID), workPost.Job);
                    
                    if (workPost.Job.ActorID != 0)
                        ActorToJobMap.Add(workPost.Job.ActorID, (station.StationID, workPost.WorkPostID));
                }
            }
        }
    }
}