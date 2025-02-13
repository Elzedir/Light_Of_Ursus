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

namespace JobSites
{
    public abstract class JobSite_Component : MonoBehaviour
    {
        public JobSite_Data JobSite_Data;
        
        public abstract JobSiteName JobSiteName { get; }
        public abstract CareerName DefaultCareer { get; }
        public ulong JobSiteID => JobSite_Data.JobSiteID;

        public Job GetActorJob(ulong actorID)
        {
            return JobSite_Data.GetActorJob(actorID);
        }

        public float IdealRatio;
        public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int PermittedProductionInequality = 10;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseJobSites += _initialise;
            Manager_Initialisation.OnInitialiseJobSiteData += _initialiseJobSiteData;
        }

        void OnDestroy()
        {
            Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, TickRateName.OneSecond, JobSiteID);
            Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, TickRateName.TenSeconds, JobSiteID);
            
            Manager_Initialisation.OnInitialiseJobSites -= _initialise;
            Manager_Initialisation.OnInitialiseJobSiteData -= _initialiseJobSiteData;
        }

        void _initialise()
        {
            var jobSiteData = JobSite_Manager.GetJobSite_DataFromName(this);

            if (jobSiteData is null)
            {
                Debug.LogWarning($"JobSite with name {name} not found in JobSite_SO.");
                return;
            }

            JobSite_Data = jobSiteData;

            _setTickers();
        }

        void _initialiseJobSiteData()
        {
            JobSite_Data.InitialiseJobSiteData();
        }

        void _setTickers()
        {
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, TickRateName.OneSecond, JobSiteID, OnTickOneSecond);
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, TickRateName.TenSeconds, JobSiteID, OnTickTenSeconds);
        }

        public void OnTickOneSecond()
        {   
            foreach (var job in JobSite_Data.AllJobs.Values)
            {
                job.OnTick();
            }
        }

        public void OnTickTenSeconds()
        {
            JobSite_Data.ProductionData.GetEstimatedProductionRatePerHour();

            _compareProductionOutput();
            
            JobSite_Data.PriorityData.RegenerateAllPriorities(DataChangedName.None);

            //* Temporary fix to move people from recreation to another job.
            foreach (var worker in JobSite_Data.AllJobs.Values
                         .Where(job => job.Station.StationType == StationType.Recreation && job.ActorID != 0))
            {
                JobSite_Data.AssignActorToNewCurrentJob(worker.Actor);
            }
            
            //* Eventually change to once per day.
            JobSite_Data.FillEmptyJobSitePositions();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void _adjustProduction(float idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            JobSite_Data.RemoveAllWorkersFromAllJobs();

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
                if (!JobSite_Data.AssignActorToNewCurrentJob(employee))
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
        
        public HashSet<StationName> GetStationNames() => JobSite_Data.AllJobs.Values.Select(job => job.Station.StationName).ToHashSet();

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

            foreach (var job in JobSite_Data.AllJobs.Values)
            {
                foreach (var itemToFetch in job.Station.GetItemsToFetchFromThisStation())
                {
                    allFetchStations.TryAdd(job.StationID, job.Station);

                    if (!allFetchItems.TryAdd(itemToFetch.Key, itemToFetch.Value))
                        allFetchItems[itemToFetch.Key] += itemToFetch.Value;
                }
            }

            foreach (var job in JobSite_Data.AllJobs.Values)
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
            priority_Parameters.AllStation_Sources = JobSite_Data.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Tree &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }

        void _relevantStations_Process_Logs(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.AllStation_Sources = JobSite_Data.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Sawmill &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }
    }
}