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
                Manager_TickRate.RegisterTicker(TickerTypeName.Station, TickRateName.OneSecond, station.StationID, station.OnTick);
                station.Station_Data.CurrentTickRateName = TickRateName.OneSecond;
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
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void _adjustProduction(float idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);
        public void AssignActorToNewCurrentJob(Actor_Component actor) => JobSite_Data.AssignActorToNewCurrentJob(actor);

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            JobSite_Data.RemoveAllWorkersFromAllStations();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            a
                //* Check this again
            
            foreach (var station in JobSite_Data.AllStations.Values)
            {
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

        public List<Station_Component> GetRelevantStations(ActorActionName actorActionName)
        {
            return RelevantStations.TryGetValue(actorActionName, out var relevantStations)
                ? relevantStations
                : throw new ArgumentException(
                    $"ActorActionName: {actorActionName} not recognised for Station_Source.");
        }

        Dictionary<ActorActionName, List<Station_Component>> _relevantStations;
        public Dictionary<ActorActionName, List<Station_Component>> RelevantStations =>
            _relevantStations ??= _getRelevantStations();

        Dictionary<ActorActionName, List<Station_Component>> _getRelevantStations()
        {
            return new Dictionary<ActorActionName, List<Station_Component>>
            {
                { ActorActionName.Idle, _relevantStations_Idle() },
                { ActorActionName.Haul, _relevantStations_Haul() },
                { ActorActionName.Chop_Wood, _relevantStations_Chop_Wood() },
                { ActorActionName.Process_Logs, _relevantStations_Process_Logs() },
                { ActorActionName.Wander, new List<Station_Component>() }
            };
        }

        //* Later, replace with recreational stations 
        List<Station_Component> _relevantStations_Idle() => new();

        List<Station_Component> _relevantStations_Haul()
        {
            var allFetchStations = JobSite_Data.AllStations.Values
                .Where(station => station.GetInventoryItemsToFetch().Count > 0)
                .ToList();
            
            var allDeliverStations = JobSite_Data.AllStations.Values
                .Where(station => station.GetInventoryItemsToDeliver().Count > 0)
                .ToList();
            
            foreach (var deliverStation in allDeliverStations)
            {
                foreach (var fetchStation in allFetchStations)
                {
                    //* Or rather than continuing, just scale priority based on how much more the distance is to the fetch than
                    //* than it is to the deliver.
                    if (Vector3.Distance(fetchStation.transform.postiion,
                            priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position)
                        < Vector3.Distance(deliverStation.transform.position,
                            priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position))
                        continue;
                    
                    // Generate priority for the JobSite with mainly distance and partially items.
                }
            }
        }

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