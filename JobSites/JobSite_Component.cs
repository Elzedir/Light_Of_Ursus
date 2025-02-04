using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using Actors;
using Initialisation;
using Jobs;
using Station;
using TickRates;
using UnityEngine;
using Station_Component = Station.Station_Component;

namespace JobSite
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

            _setTickRate(TickRateName.TenSeconds, false);
            _initialised = true;
        }

        TickRateName _currentTickRateName;

        void _setTickRate(TickRateName tickRateName, bool unregister = true)
        {
            if (_currentTickRateName == tickRateName) return;

            if (unregister) Manager_TickRate.UnregisterTicker(TickerTypeName.Jobsite, _currentTickRateName, JobSiteID);
            Manager_TickRate.RegisterTicker(TickerTypeName.Jobsite, tickRateName, JobSiteID, _onTick);
            // Register prosperity tick here. Do it for everything that has a ticker. A top down ticker.
            _currentTickRateName = tickRateName;
        }

        void _onTick()
        {
            if (!_initialised) return;

            JobSite_Data.ProductionData.GetEstimatedProductionRatePerHour();

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void _adjustProduction(float idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);
        public void AssignActorToNewCurrentJob(Actor_Component actor) => JobSite_Data.AssignActorToNewCurrentJob(actor);

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            JobSite_Data.RemoveAllWorkersFromAllStations();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            foreach (var station in JobSite_Data.AllStations.Values)
            {
                var employeesForStation = tempEmployees
                    .OrderByDescending(actor =>
                        actor.ActorData.Vocation.GetVocationExperience(
                            _getRelevantVocation(actor.ActorData.Career
                                .CurrentJob.JobName)))
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

        public List<Station_Component> GetRelevantStations(ActorActionName actorActionName, bool isStation_Source)
        {
            if (isStation_Source)
            {
                return Source_Stations.TryGetValue(actorActionName, out var stations_Source)
                    ? stations_Source()
                    : throw new ArgumentException(
                        $"ActorActionName: {actorActionName} not recognised for Station_Source.");
            }

            return Target_Stations.TryGetValue(actorActionName, out var stations_Target)
                ? stations_Target()
                : throw new ArgumentException($"ActorActionName: {actorActionName} not recognised for Station_Target.");
        }

        Dictionary<ActorActionName, Func<List<Station_Component>>> _source_Stations;
        public Dictionary<ActorActionName, Func<List<Station_Component>>> Source_Stations =>
            _source_Stations ??= _getSourceStations();

        Dictionary<ActorActionName, Func<List<Station_Component>>> _target_Stations;
        public Dictionary<ActorActionName, Func<List<Station_Component>>> Target_Stations =>
            _target_Stations ??= _getTargetStations();

        Dictionary<ActorActionName, Func<List<Station_Component>>> _getSourceStations()
        {
            return new Dictionary<ActorActionName, Func<List<Station_Component>>>
            {
                { ActorActionName.Haul_Fetch, _relevantStations_Fetch },
                { ActorActionName.Haul_Deliver, () => new List<Station_Component>() },
                { ActorActionName.Chop_Wood, _relevantStations_Chop_Wood },
                { ActorActionName.Process_Logs, _relevantStations_Process_Logs },
                { ActorActionName.Wander, () => new List<Station_Component>() }
            };
        }

        Dictionary<ActorActionName, Func<List<Station_Component>>> _getTargetStations()
        {
            return new Dictionary<ActorActionName, Func<List<Station_Component>>>
            {
                { ActorActionName.Haul_Fetch, () => new List<Station_Component>() },
                { ActorActionName.Haul_Deliver, _relevantStations_Deliver },
                { ActorActionName.Chop_Wood, () => new List<Station_Component>() },
                { ActorActionName.Process_Logs, () => new List<Station_Component>() },
                { ActorActionName.Wander, () => new List<Station_Component>() }
            };
        }

        List<Station_Component> _relevantStations_Fetch() =>
            JobSite_Data.AllStations.Values
                .Where(station => station.GetInventoryItems(ActorActionName.Haul_Fetch).Count > 0)
                .ToList();

        List<Station_Component> _relevantStations_Deliver() =>
            JobSite_Data.AllStations.Values
                .Where(station => station.GetInventoryItems(ActorActionName.Haul_Deliver).Count > 0)
                .ToList();

        List<Station_Component> _relevantStations_Chop_Wood() =>
            JobSite_Data.AllStations.Values
                .Where(station => station.StationName == StationName.Tree &&
                                  station.Station_Data.GetOpenWorkPost() is not null)
                .ToList();

        List<Station_Component> _relevantStations_Process_Logs() =>
            JobSite_Data.AllStations.Values
                .Where(station => station.StationName == StationName.Sawmill &&
                                  station.Station_Data.GetOpenWorkPost() is not null)
                .ToList();
    }
}