using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
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
        public abstract JobSiteName  JobSiteName { get; }
        public          uint         JobSiteID   => JobSiteData.JobSiteID;
        public          JobSite_Data JobSiteData;
        bool                         _initialised;

        public void SetJobSiteData(JobSite_Data jobSiteData)
        {
            JobSiteData = jobSiteData;
        }

        public Dictionary<uint, Station_Component> GetAllStationsInJobSite() =>
            GetComponentsInChildren<Station_Component>().ToDictionary(station => station.StationID);

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

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void         _adjustProduction(float      idealRatio);
        protected abstract VocationName _getRelevantVocation(JobName positionName);

        public bool GetNewCurrentJob(Actor_Component actor, uint stationID = 0)
        {
            var highestPriorityElement = JobSiteData.PriorityData.GetHighestSpecificPriority(
                actor.ActorData.CareerDataPreset.AllJobTasks.Select(jobTaskName => (uint)jobTaskName).ToList(), stationID);

            if (highestPriorityElement == null)
            {
                actor.ActorData.CareerDataPreset.SetCurrentJob(new Job(JobName.Idle, 0, 0));
                return true;
            }
            
            var highestPriorityJobTask = (JobTaskName)highestPriorityElement.PriorityID;
            
            if (highestPriorityJobTask == JobTaskName.Idle)
            {
                actor.ActorData.CareerDataPreset.SetCurrentJob(new Job(JobName.Idle, 0, 0));
                return true;
            }

            var relevantStations = _getOrderedRelevantStationsForJob(highestPriorityJobTask, actor);

            foreach (var station in relevantStations)
            {
                if (JobSiteData.AddEmployeeToStation(actor, station, highestPriorityJobTask)) return true;
                
                //Debug.LogWarning($"Station: {station.StationName} is full.");
            }

            //Debug.LogWarning($"No relevant stations found for job: {highestPriorityJobTask}.");
            return false;
        }

        List<Station_Component> _getOrderedRelevantStationsForJob(JobTaskName jobTaskName, Actor_Component actor)
        {
            var relevantStations = JobSiteData.AllStationComponents.Values
                                              .Where(station => station.AllowedJobTasks.Contains(jobTaskName))
                                              .ToList();

            if (relevantStations.Count != 0)
                return relevantStations
                       .OrderBy(station =>
                           Vector3.Distance(actor.transform.position, station.transform.position))
                       .ToList();

            Debug.LogError($"No relevant stations found for jobTask: {jobTaskName}.");
            return null;
        }

        protected void _assignAllEmployeesToStations(Dictionary<uint, Actor_Component> allEmployees)
        {
            JobSiteData.RemoveAllWorkersFromAllStations();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            foreach (var station in JobSiteData.AllStationComponents.Values)
            {
                var employeesForStation = tempEmployees
                                          .OrderByDescending(actor =>
                                              actor.ActorData.VocationDataPreset.GetVocationExperience(
                                                  _getRelevantVocation(actor.ActorData.CareerDataPreset
                                                                            .CurrentJob.JobName)))
                                          .ToList();

                foreach (var employee in employeesForStation)
                {
                    GetNewCurrentJob(employee, station.StationID);
                    tempEmployees.Remove(employee);
                }

                if (tempEmployees.Count > 0)
                {
                    Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
                }
            }
        }

        protected List<Dictionary<uint, Actor_Component>> _getAllCombinations(
            Dictionary<uint, Actor_Component> employees)
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

        public List<Station_Component> GetRelevantStations(JobTaskName jobTaskName)
        {
            return jobTaskName switch
            {
                JobTaskName.Fetch_Items   => _relevantStations_Fetch(),
                JobTaskName.Deliver_Items => _relevantStations_Deliver(),
                JobTaskName.Chop_Wood     => _relevantStations_Chop_Wood(),
                _                         => throw new ArgumentException($"JobTaskName: {jobTaskName} not recognised.")
            };
        }

        List<Station_Component> _relevantStations_Fetch()
        {
            return JobSiteData.AllStationComponents.Values
                              .Where(station => station.GetInventoryItemsToFetchFromStation().Count > 0).ToList();
        }

        List<Station_Component> _relevantStations_Deliver()
        {
            return JobSiteData.AllStationComponents.Values
                              .Where(station => station.GetInventoryItemsToDeliverFromOtherStations().Count > 0)
                              .ToList();
        }

        List<Station_Component> _relevantStations_Chop_Wood()
        {
            return JobSiteData.AllStationComponents.Values
                       .Where(station => station.StationName == StationName.Tree && station.Station_Data.GetOpenWorkPost() is not null).ToList();   
        }
    }
}