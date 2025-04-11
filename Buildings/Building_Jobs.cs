using System.Collections.Generic;
using System.Linq;
using Jobs;
using Station;
using UnityEngine;

namespace Buildings
{
    public class Building_Stations
    {
        readonly Building_Data _building_Data;
        public readonly Dictionary<ulong, Station_Data> AllStations;
        
        public readonly Dictionary<ulong, ulong> ActorToJobMap = new();
        public readonly Dictionary<ulong, ulong> JobToActorMap = new();

        public Building_Stations(Building_Stations data, Building_Data building_Data)
        {
            _building_Data = building_Data;
            AllStations = new Dictionary<ulong, Station_Data>(data.AllStations);
        }
        
        public Building_Stations(Dictionary<ulong, Station_Data> allStations = null)
        {
            AllStations = allStations ?? new Dictionary<ulong, Station_Data>();
        }

        public void InitialiseStations()
        {
            foreach (var station in AllStations.Values)
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
        }

        public void OnTickOneSecond()
        {
            foreach (var station in AllStations.Values)
            {
                station.OnTickOneSecond();
            }
        }

        public void OnProgressDay()
        {
            FillEmptyBuildingPositions();
        }
        
        public void FillEmptyBuildingPositions()
        {
            if (AllStations?.Count is null or 0)
            {
                Debug.LogError($"AllStations {AllStations?.Count} is null or 0.");
                return;
            }

            var relevantStations = AllStations.Values
                .Where(station => station.StationType != StationType.Recreation)
                .ToList().Count;
            var desiredEmployeeCount = Mathf.RoundToInt(relevantStations * _building_Data.Prosperity.GetProsperityPercentage());
            var jobQueue = new Queue<Job_Data>(AllStations.Values);
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

                Debug.LogError($"Iteration limit reached. Desired: {desiredEmployeeCount}, Current: {AllStations.Count}");
                return;
            }
        }
    }
}