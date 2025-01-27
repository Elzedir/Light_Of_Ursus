using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
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
        public abstract List<ActorActionName> BaseJobActions { get; }
        public          ulong         JobSiteID   => JobSiteData.JobSiteID;
        public          JobSite_Data JobSiteData;
        bool                         _initialised;

        public void SetJobSiteData(JobSite_Data jobSiteData)
        {
            JobSiteData = jobSiteData;
        }

        public Dictionary<ulong, Station_Component> GetAllStationsInJobSite() =>
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
            var jobSiteData = JobSite_Manager.GetJobSite_DataFromName(this);
            
            if (jobSiteData is null)
            {
                Debug.LogWarning($"JobSite with name {name} not found in JobSite_SO.");
                return;
            }
            
            SetJobSiteData(jobSiteData);
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

        public bool GetNewCurrentJob(Actor_Component actor, ulong stationID = 0)
        {
            JobSiteData.PriorityData.RegenerateAllPriorities(DataChangedName.None);
            
            var highestPriorityElement = JobSiteData.PriorityData.GetHighestPriorityFromGroup(
                actor.ActorData.Career.AllJobActions.Select(actorActionName => (ulong)actorActionName).ToList(), stationID);

            if (highestPriorityElement == null)
            {
                actor.ActorData.Career.SetCurrentJob(new Job(JobName.Idle, 0, 0));
                return true;
            }
            
            var highestPriorityJobTask = (ActorActionName)highestPriorityElement.PriorityID;
            
            if (highestPriorityJobTask == ActorActionName.Idle)
            {
                actor.ActorData.Career.SetCurrentJob(new Job(JobName.Idle, 0, 0));
                return true;
            }

            var relevantStations = _getOrderedRelevantStationsForJob(highestPriorityJobTask, actor);

            return relevantStations.Any(station => JobSiteData.AddEmployeeToStation(actor, station, highestPriorityJobTask));

            //Debug.LogWarning($"No relevant stations found for job: {highestPriorityJobTask}.");
        }

        List<Station_Component> _getOrderedRelevantStationsForJob(ActorActionName actorActionName, Actor_Component actor)
        {
            var relevantStations = JobSiteData.AllStationComponents.Values
                                              .Where(station => station.AllowedJobTasks.Contains(actorActionName))
                                              .ToList();

            if (relevantStations.Count != 0)
                return relevantStations
                       .OrderBy(station =>
                           Vector3.Distance(actor.transform.position, station.transform.position))
                       .ToList();

            Debug.LogError($"No relevant stations found for jobTask: {actorActionName}.");
            return null;
        }

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            JobSiteData.RemoveAllWorkersFromAllStations();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            foreach (var station in JobSiteData.AllStationComponents.Values)
            {
                var employeesForStation = tempEmployees
                                          .OrderByDescending(actor =>
                                              actor.ActorData.Vocation.GetVocationExperience(
                                                  _getRelevantVocation(actor.ActorData.Career
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

        protected List<Dictionary<ulong, Actor_Component>> _getAllCombinations(
            Dictionary<ulong, Actor_Component> employees)
        {
            var result           = new List<Dictionary<ulong, Actor_Component>>();
            var employeeKeys     = new List<ulong>(employees.Keys);
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
            return actorActionName switch
            {
                ActorActionName.Fetch_Items   => _relevantStations_Fetch(),
                ActorActionName.Deliver_Items => _relevantStations_Deliver(),
                ActorActionName.Chop_Wood     => _relevantStations_Chop_Wood(),
                ActorActionName.Process_Logs => _relevantStations_Process_Logs(),
                _                         => throw new ArgumentException($"ActorActionName: {actorActionName} not recognised.")
            };
        }
        
        //* We are calling this twice in priority generator, once to get relevant stations and then once more to get the items to fetch or deliver.

        List<Station_Component> _relevantStations_Fetch()
        {
            return JobSiteData.AllStationComponents.Values
                              .Where(station => station.GetInventoryItems(ActorActionName.Fetch_Items).Count > 0).ToList();
        }

        List<Station_Component> _relevantStations_Deliver()
        {
            return JobSiteData.AllStationComponents.Values
                              .Where(station => station.GetInventoryItems(ActorActionName.Deliver_Items).Count > 0)
                              .ToList();
        }

        List<Station_Component> _relevantStations_Chop_Wood()
        {
            return JobSiteData.AllStationComponents.Values
                       .Where(station => station.StationName == StationName.Tree && station.Station_Data.GetOpenWorkPost() is not null).ToList();   
        }

        List<Station_Component> _relevantStations_Process_Logs()
        {
            return JobSiteData.AllStationComponents.Values
                              .Where(station => station.StationName == StationName.Sawmill && station.Station_Data.GetOpenWorkPost() is not null).ToList();
        }
    }
}