using System.Collections.Generic;
using System.Linq;
using Actors;
using Jobs;
using Managers;
using Priority;
using Station;
using UnityEngine;

namespace Jobsite
{
    public abstract class JobsiteComponent : MonoBehaviour
    {
        public uint        JobsiteID => JobsiteData.JobsiteID;
        public JobsiteData JobsiteData;
        public void        SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
        public void        SetCityID(uint             cityID)      => JobsiteData.CityID = cityID;

        List<StationComponent>        _allStationInJobsite;
        public List<StationComponent> AllStationsInJobsite => _allStationInJobsite ??= _getAllStationsInJobsite();
        public List<EmployeePosition> AllCoreEmployeePositions;

        public PriorityComponent_Jobsite PriorityComponent;

        public float IdealRatio;
        public void  SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int   PermittedProductionInequality = 10;

        public void Initialise()
        {
            PriorityComponent = new PriorityComponent_Jobsite(JobsiteID);

            AllStationsInJobsite.ForEach(station => station.StationData.SetJobsiteID(JobsiteID));
            
            _setTickRate(TickRate.TenSeconds);
        }
        
        TickRate _currentTickRate;

        void _setTickRate(TickRate tickRate)
        {
            if (_currentTickRate == tickRate) return;
            
            Manager_TickRate.UnregisterTicker(TickerType.Jobsite, _currentTickRate, JobsiteID);
            Manager_TickRate.RegisterTicker(TickerType.Jobsite, tickRate, JobsiteID, _onTick);
            _currentTickRate = tickRate;
        }
        
        void _onTick()
        {
            foreach (var product in AllStationsInJobsite.Select(s => s.StationData.ProductionData))
            {
                product.GetEstimatedProductionRatePerHour();
                product.GetActualProductionRatePerHour();
            }

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void         _adjustProduction(float               idealRatio);
        protected abstract VocationName _getRelevantVocation(EmployeePosition position);
        List<StationComponent>          _getAllStationsInJobsite() => GetComponentsInChildren<StationComponent>().ToList();

        public StationComponent GetNearestRelevantStationInJobsite(Vector3 position, StationName stationName)
            => AllStationsInJobsite
               .Where(station => station.StationName == stationName)
               .OrderBy(station => Vector3.Distance(position, station.transform.position))
               .FirstOrDefault();


        public bool GetNewCurrentJob(ActorComponent actor)
        {
            var highestPriorityJob =
                PriorityComponent.GetHighestSpecificPriority(actor.ActorData.CareerData.AllJobs.Select(j => (uint)j)
                                                                   .ToList());

            if (highestPriorityJob == null) return false;

            var jobName = (JobName)highestPriorityJob.PriorityID;

            var relevantStation = _getRelevantStationForJob(jobName, actor);
            var relevantOperatingArea = relevantStation.GetRelevantOperatingArea(actor);
            
            var job = new Job(jobName, relevantStation.StationID, relevantOperatingArea.OperatingAreaID);
            
            actor.ActorData.CareerData.SetCurrentJob(job);
            
            return true;
        }
        
        StationComponent _getRelevantStationForJob(JobName jobName, ActorComponent actor)
        {
            var relevantStations = AllStationsInJobsite
                .Where(station => station.AllowedJobs.Contains(jobName))
                .ToList();

            if (relevantStations.Count != 0)
                return relevantStations
                       .OrderBy(station => Vector3.Distance(actor.transform.position, station.transform.position))
                       .FirstOrDefault();
            
            Debug.LogError($"No relevant stations found for job: {jobName}.");
            return null;

        }

        public List<EmployeePosition> GetMinimumEmployeePositions()
        {
            HashSet<EmployeePosition> employeePositions = new();

            foreach (var station in AllStationsInJobsite)
            {
                foreach (var position in station.AllowedEmployeePositions)
                {
                    employeePositions.Add(position);
                }
            }

            return employeePositions.ToList();
        }

        protected void _assignAllEmployeesToStations(List<uint> employeeIDs)
        {
            foreach (var station in AllStationsInJobsite)
            {
                station.RemoveAllOperators();
            }

            var tempEmployees = employeeIDs.Select(employeeID => Manager_Actor.GetActorData(employeeID)).ToList();

            foreach (var station in AllStationsInJobsite)
            {
                var allowedPositions = station.AllowedEmployeePositions;
                var employeesForStation = tempEmployees
                                          .Where(e => allowedPositions.Contains(e.CareerData.EmployeePosition))
                                          .OrderByDescending(e => e.CareerData.EmployeePosition)
                                          .ThenByDescending(e => e.VocationData.GetVocationExperience(_getRelevantVocation(e.CareerData.EmployeePosition)))
                                          .ToList();

                foreach (var employee in employeesForStation)
                {
                    JobsiteData.AddEmployeeToStation(employee.ActorID, station.StationData.StationID);
                    tempEmployees.Remove(employee);
                }

                if (tempEmployees.Count > 0)
                {
                    Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
                }
            }
        }

        protected List<List<uint>> _getAllCombinations(List<uint> employees)
        {
            var result           = new List<List<uint>>();
            var combinationCount = (int)Mathf.Pow(2, employees.Count);

            for (var i = 1; i < combinationCount; i++)
            {
                var combination = new List<uint>();
                
                for (var j = 0; j < employees.Count; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        combination.Add(employees[j]);
                    }
                }
                result.Add(combination);
            }

            return result;
        }

        public List<StationComponent> GetRelevantStations(JobTaskName jobTaskName, InventoryData inventoryData)
        {
            return jobTaskName switch
            {
                JobTaskName.Fetch_Items   => _relevantStations_Fetch(),
                JobTaskName.Deliver_Items => _relevantStations_Deliver(inventoryData),
                _                         => new List<StationComponent>()
            };
        }

        List<StationComponent> _relevantStations_Fetch()
        {
            return AllStationsInJobsite.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();
        }

        List<StationComponent> _relevantStations_Deliver(InventoryData inventoryData)
        {
            return AllStationsInJobsite.Where(station => station.GetInventoryItemsToDeliver(inventoryData).Count > 0).ToList();
        }
    }
}