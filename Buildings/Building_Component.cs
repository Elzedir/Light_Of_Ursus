using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Careers;
using Initialisation;
using Jobs;
using Priorities;
using Station;
using TickRates;
using UnityEngine;
using Station_Component = Station.Station_Component;

namespace Buildings
{
    public abstract class Building_Component : MonoBehaviour
    {
        public Building_Data Building_Data;
        
        public abstract BuildingName BuildingName { get; }
        
        public abstract CareerName DefaultCareer { get; }
        public ulong ID => Building_Data.ID;

        public Job GetActorJob(ulong actorID)
        {
            return Building_Data.GetActorJob(actorID);
        }

        public float IdealRatio;
        public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int PermittedProductionInequality = 10;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseBuildings += _initialise;
            Manager_Initialisation.OnInitialiseBuildingData += InitialiseBuildingData;
        }

        void OnDestroy()
        {
            Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, TickRateName.OneSecond, ID);
            Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, TickRateName.TenSeconds, ID);
            
            Manager_Initialisation.OnInitialiseBuildings -= _initialise;
            Manager_Initialisation.OnInitialiseBuildingData -= InitialiseBuildingData;
        }

        void _initialise()
        {
            var buildingData = Building_Manager.GetBuilding_DataFromName(this);

            if (buildingData is null)
            {
                Debug.LogWarning($"JobSite with name {name} not found in JobSite_SO.");
                return;
            }

            Building_Data = buildingData;

            _setTickers();
        }

        void InitialiseBuildingData()
        {
            Building_Data.InitialiseBuildingData();
        }

        void _setTickers()
        {
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, TickRateName.OneSecond, ID, OnTickOneSecond);
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, TickRateName.TenSeconds, ID, OnTickTenSeconds);
        }

        public void OnTickOneSecond()
        {   
            foreach (var job in Building_Data.AllJobs.Values)
            {
                job.OnTick();
            }
        }

        public void OnTickTenSeconds()
        {
            Building_Data.ProductionData.GetEstimatedProductionRatePerHour();

            _compareProductionOutput();
            
            Building_Data.PriorityData.RegenerateAllPriorities(DataChangedName.None);

            //* Temporary fix to move people from recreation to another job.
            foreach (var worker in Building_Data.AllJobs.Values
                         .Where(job => job.Station.StationType == StationType.Recreation && job.ActorID != 0))
            {
                Building_Data.AssignActorToNewCurrentJob(worker.Actor);
            }
            
            //* Eventually change to once per day.
            Building_Data.FillEmptyJobSitePositions();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void _adjustProduction(float idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            Building_Data.RemoveAllWorkersFromAllJobs();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            var employeesForStation = tempEmployees
                .OrderByDescending(actor =>
                    actor.ActorData.Career.CurrentJob != null
                        ? actor.ActorData.Vocation.GetVocationExperience(
                            _getRelevantVocation(actor.ActorData.Career.CurrentJob.JobName))
                        : 0)
                .ToList();

            foreach (var employee in employeesForStation)
            {
                if (!Building_Data.AssignActorToNewCurrentJob(employee))
                {
                    Debug.LogError($"Could not assign actor {employee.ActorID} to a job.");
                    continue;
                }
                    
                tempEmployees.Remove(employee);
            }

            if (tempEmployees.Count > 0)
            {
                Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
            }
        }

        protected List<Dictionary<ulong, Actor_Component>> _getAllCombinations(
            Dictionary<ulong, Actor_Component> employees)
        {
            var result = new List<Dictionary<ulong, Actor_Component>>();
            var employeeKeys = new List<ulong>(employees.Keys);
            var combinationCount = (int)Mathf.Pow(2, employeeKeys.Count);

            for (var i = 1; i < combinationCount; i++)
            {
                var combination = employeeKeys.Where((_, j) => (i & (1 << j)) != 0)
                    .ToDictionary(key => key, key => employees[key]);

                result.Add(combination);
            }

            return result;
        }
        
        public HashSet<StationName> GetStationNames() => Building_Data.AllJobs.Values.Select(job => job.Station.StationName).ToHashSet();

        public void GetRelevantStations(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            if (!RelevantStations.TryGetValue(actorActionName, out var relevantStations))
                throw new ArgumentException($"ActorActionName: {actorActionName} not recognised.");
            
            relevantStations(priority_Parameters);
        }

        Dictionary<ActorActionName, Action<Priority_Parameters>> _relevantStations;
        public Dictionary<ActorActionName, Action<Priority_Parameters>> RelevantStations => _getRelevantStations();

        Dictionary<ActorActionName, Action<Priority_Parameters>> _getRelevantStations()
        {
            return new Dictionary<ActorActionName, Action<Priority_Parameters>>
            {
                { ActorActionName.Idle, _relevantStations_Idle },
                { ActorActionName.Haul, _relevantStations_Haul },
                { ActorActionName.Chop_Wood, _relevantStations_Chop_Wood },
                { ActorActionName.Process_Logs, _relevantStations_Process_Logs },
                { ActorActionName.Wander, _relevantStations_Wander }
            };
        }
        
        void _relevantStations_Idle(Priority_Parameters priority_Parameters)
        {
            //* Later, replace with recreational stations    
        }

        void _relevantStations_Wander(Priority_Parameters priority_Parameters)
        {
            //* This wouldn't make sense, I think, you do not wander between stations, that would be idling?
        }


        void _relevantStations_Haul(Priority_Parameters priority_Parameters)
        {
            var allFetchStations = new Dictionary<ulong, Station_Component>();
            var allFetchItems = new Dictionary<ulong, ulong>();
            var allDeliverStations = new Dictionary<ulong, Station_Component>();

            foreach (var job in Building_Data.AllJobs.Values)
            {
                foreach (var itemToFetch in job.Station.GetItemsToFetchFromThisStation())
                {
                    allFetchStations.TryAdd(job.StationID, job.Station);

                    if (!allFetchItems.TryAdd(itemToFetch.Key, itemToFetch.Value))
                        allFetchItems[itemToFetch.Key] += itemToFetch.Value;
                }
            }

            foreach (var job in Building_Data.AllJobs.Values)
            {
                foreach (var itemToFetch in allFetchItems)
                {
                    if (!job.Station.DesiredStoredItemIDs.Contains(itemToFetch.Key)) continue;
                    
                    allDeliverStations.TryAdd(job.StationID, job.Station);
                }
            }
            
            priority_Parameters.AllStation_Sources = allFetchStations.Values.ToList();
            priority_Parameters.AllStation_Targets = allDeliverStations.Values.ToList();
        }
        
        //* Fix these

        void _relevantStations_Chop_Wood(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.AllStation_Sources = Building_Data.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Tree &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }

        void _relevantStations_Process_Logs(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.AllStation_Sources = Building_Data.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Sawmill &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }
    }
}