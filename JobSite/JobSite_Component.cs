using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using EmployeePosition;
using Initialisation;
using Inventory;
using Jobs;
using Priority;
using TickRates;
using UnityEngine;
using Station_Component = Station.Station_Component;

namespace JobSite
{
    public abstract class JobSite_Component : MonoBehaviour
    {
        public abstract JobSiteName  JobSiteName { get; }
        public          uint         JobSiteID   => JobSiteData.JobSiteID;
        public          JobSite_Data JobSiteData;
        bool                         _initialised;

        public void SetJobSiteData(JobSite_Data jobSiteData)
        {
            JobSiteData = jobSiteData;
        }
        
        public Dictionary<uint,Station_Component> GetAllStationsInJobSite() =>
            GetComponentsInChildren<Station_Component>().ToDictionary(station => station.StationID);

        public PriorityComponent_JobSite PriorityComponent;

        public float IdealRatio;
        public void  SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int   PermittedProductionInequality = 10;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseJobSites += _initialise;
        }

        void _initialise()
        {
            JobSiteData.InitialiseJobSiteData();
            
            PriorityComponent = new PriorityComponent_JobSite(JobSiteID);

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

            JobSiteData.ProductionData.GetEstimatedProductionRatePerHour();
            JobSiteData.ProductionData.GetActualProductionRatePerHour();

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void         _adjustProduction(float               idealRatio);
        protected abstract VocationName _getRelevantVocation(EmployeePositionName positionName);
        
        public bool GetNewCurrentJob(Actor_Component actor, uint stationID = 0)
        {
            var highestPriorityJob = PriorityComponent.GetHighestSpecificPriority(
                actor.ActorData.CareerData.AllJobs.Select(jobName => (uint)jobName).ToList(), stationID);

            if (highestPriorityJob == null) return false;

            var jobName = (JobName)highestPriorityJob.PriorityID;

            var relevantStations       = _getOrderedRelevantStationsForJob(jobName, actor);

            foreach (var station in relevantStations)
            {
                if (!JobSiteData.AddEmployeeToStation(actor.ActorID, station))
                {
                    Debug.LogWarning($"Station: {station.StationName} is full.");
                    continue;
                }

                var job = new Job(jobName, station.StationID, JobSiteData.GetWorkPostIDFromWorkerID(actor.ActorID));

                actor.ActorData.CareerData.SetCurrentJob(job);

                return true;    
            }

            Debug.LogWarning($"No relevant stations found for job: {jobName}.");
            return false;
        }

        List<Station_Component> _getOrderedRelevantStationsForJob(JobName jobName, Actor_Component actor)
        {
            var relevantStations = JobSiteData.AllStationComponents.Values
                                                       .Where(station => station.AllowedJobs.Contains(jobName))
                                                       .ToList();

            if (relevantStations.Count != 0)
                return relevantStations
                       .OrderBy(station =>
                           Vector3.Distance(actor.transform.position, station.transform.position))
                       .ToList();

            Debug.LogError($"No relevant stations found for job: {jobName}.");
            return null;
        }

        protected void _assignAllEmployeesToStations(Dictionary<uint, Actor_Component> allEmployees)
        {
            JobSiteData.RemoveAllWorkersFromAllStations();

            var tempEmployees = allEmployees.Select(employee => employee.Value.ActorData).ToList();

            foreach (var station in JobSiteData.AllStationComponents.Values)
            {
                var employeesForStation = tempEmployees
                                          .OrderByDescending(actor_Data =>
                                              actor_Data.VocationData.GetVocationExperience(
                                                  _getRelevantVocation(actor_Data.CareerData.EmployeePositionName)))
                                          .ToList();

                foreach (var employee in employeesForStation)
                {
                    JobSiteData.AddEmployeeToStation(employee.ActorID, station);
                    tempEmployees.Remove(employee);
                }

                if (tempEmployees.Count > 0)
                {
                    Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
                }
            }
        }

        protected List<Dictionary<uint, Actor_Component>> _getAllCombinations(Dictionary<uint, Actor_Component> employees)
        {
            var result           = new List<Dictionary<uint, Actor_Component>>();
            var employeeKeys     = new List<uint>(employees.Keys);
            var combinationCount = (int)Mathf.Pow(2, employeeKeys.Count);

            for (var i = 1; i < combinationCount; i++)
            {
                var combination = employeeKeys.Where((_, j) => (i & (1 << j)) != 0)
                                              .ToDictionary(key => key, key => employees[key]);

                result.Add(combination);
            }

            return result;
        }

        public List<Station_Component> GetRelevantStations(JobTaskName jobTaskName, InventoryData inventoryData)
        {
            return jobTaskName switch
            {
                JobTaskName.Fetch_Items   => _relevantStations_Fetch(),
                JobTaskName.Deliver_Items => _relevantStations_Deliver(inventoryData),
                _                         => new List<Station_Component>()
            };
        }

        List<Station_Component> _relevantStations_Fetch()
        {
            return JobSiteData.AllStationComponents.Values.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();
        }

        List<Station_Component> _relevantStations_Deliver(InventoryData inventoryData)
        {
            return JobSiteData.AllStationComponents.Values.Where(station => station.GetInventoryItemsToDeliver(inventoryData).Count > 0)
                              .ToList();
        }
    }
}