using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Initialisation;
using Jobs;
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
        public ulong JobSiteID => JobSite_Data.JobSiteID;

        public Job GetActorJob(ulong actorID)
        {
            return JobSite_Data.GetActorJob(actorID);
        }
        
        bool _initialised;

        public float IdealRatio;
        public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int PermittedProductionInequality = 10;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseJobSites += _initialise;
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
            JobSite_Data.InitialiseJobSiteData();
            
            RegisterAllTickers();
            
            _initialised = true;
        }

        public void RegisterAllTickers()
        {
            _setTickRate(TickRateName.TenSeconds, false);
            
            _registerStations();
        }

        void _registerStations()
        {
            foreach (var station in JobSite_Data.AllStations.Values)
            {
                station.Station_Data.CurrentTickRateName = TickRateName.OneSecond;
                Manager_TickRate.RegisterTicker(TickerTypeName.Station, TickRateName.OneSecond, station.StationID, station.OnTick);
            }
        }

        TickRateName _currentTickRateName;

        void _setTickRate(TickRateName tickRateName, bool unregister = true)
        {
            if (_currentTickRateName == tickRateName) return;

            if (unregister) Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, _currentTickRateName, JobSiteID);
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, tickRateName, JobSiteID, OnTick);
            // Register prosperity tick here. Do it for everything that has a ticker. A top down ticker.
            _currentTickRateName = tickRateName;
        }

        public void OnTick()
        {
            if (!_initialised) return;

            JobSite_Data.ProductionData.GetEstimatedProductionRatePerHour();

            _compareProductionOutput();
            
            JobSite_Data.PriorityData.RegenerateAllPriorities(DataChangedName.None);
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void _adjustProduction(float idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);
        public void AssignActorToNewCurrentJob(Actor_Component actor) => JobSite_Data.AssignActorToNewCurrentJob(actor);

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            JobSite_Data.RemoveAllWorkersFromAllStations();

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
                AssignActorToNewCurrentJob(employee);
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
        
        public HashSet<StationName> GetStationNames() => JobSite_Data.AllStations.Values.Select(station => station.StationName).ToHashSet();

        public (List<Station_Component> StationSources, List<Station_Component> StationTargets) GetRelevantStations(ActorActionName actorActionName)
        {
            return RelevantStations.TryGetValue(actorActionName, out var relevantStations)
                ? relevantStations()
                : throw new ArgumentException(
                    $"ActorActionName: {actorActionName} not recognised for Station_Source.");
        }

        Dictionary<ActorActionName, Func<(List<Station_Component> stationSources, List<Station_Component> stationTargets)>> _relevantStations;
        public Dictionary<ActorActionName, Func<(List<Station_Component> stationSources, List<Station_Component> stationTargets)>> RelevantStations => _getRelevantStations();

        Dictionary<ActorActionName, Func<(List<Station_Component> stationSources, List<Station_Component> stationTargets)>> _getRelevantStations()
        {
            return new Dictionary<ActorActionName, Func<(List<Station_Component> stationSources, List<Station_Component> stationTargets)>>
            {
                { ActorActionName.Idle, _relevantStations_Idle },
                { ActorActionName.Haul, _relevantStations_Haul },
                { ActorActionName.Chop_Wood, _relevantStations_Chop_Wood },
                { ActorActionName.Process_Logs, _relevantStations_Process_Logs },
                { ActorActionName.Wander, () => (new List<Station_Component>(), new List<Station_Component>()) }
            };
        }

        //* Later, replace with recreational stations 
        (List<Station_Component> stationSources, List<Station_Component> stationTargets) _relevantStations_Idle() => new();

        (List<Station_Component> stationSources, List<Station_Component> stationTargets) _relevantStations_Haul()
        {
            var allFetchStations = new Dictionary<ulong, Station_Component>();
            var allFetchItems = new Dictionary<ulong, ulong>();
            var allDeliverStations = new Dictionary<ulong, Station_Component>();

            foreach (var station in JobSite_Data.AllStations.Values)
            {
                foreach (var itemToFetch in station.GetItemsToFetchFromThisStation())
                {
                    allFetchStations.TryAdd(station.StationID, station);

                    if (!allFetchItems.TryAdd(itemToFetch.Key, itemToFetch.Value))
                        allFetchItems[itemToFetch.Key] += itemToFetch.Value;
                }
            }

            foreach (var station in JobSite_Data.AllStations.Values)
            {
                foreach (var itemToFetch in allFetchItems)
                {
                    if (!station.DesiredStoredItemIDs.Contains(itemToFetch.Key)) continue;
                    
                    allDeliverStations.TryAdd(station.StationID, station);
                }
            }
            
            return (allFetchStations.Values.ToList(), allDeliverStations.Values.ToList());
        }

        (List<Station_Component> stationSources, List<Station_Component> stationTargets) _relevantStations_Chop_Wood() =>
            (JobSite_Data.AllStations.Values
                    .Where(station => station.StationName == StationName.Tree &&
                                      station.Station_Data.GetOpenWorkPost() is not null).ToList(), 
                new List<Station_Component>());

        (List<Station_Component> stationSources, List<Station_Component> stationTargets) _relevantStations_Process_Logs() =>
            (JobSite_Data.AllStations.Values
                .Where(station => station.StationName == StationName.Sawmill &&
                                  station.Station_Data.GetOpenWorkPost() is not null).ToList(), 
                new List<Station_Component>());
    }
}