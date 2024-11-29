using System.Collections.Generic;
using System.Linq;
using Actors;
using EmployeePositions;
using Inventory;
using Jobs;
using Managers;
using Priority;
using ScriptableObjects;
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

        Dictionary<uint, StationComponent> _allStationInJobsite;

        public Dictionary<uint, StationComponent> AllStationsInJobsite =>
            _allStationInJobsite ??= _getAllStationsInJobsite();

        public List<EmployeePositionName> AllCoreEmployeePositions;

        public PriorityComponent_Jobsite PriorityComponent;

        public float IdealRatio;
        public void  SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int   PermittedProductionInequality = 10;

        public void Initialise()
        {
            PriorityComponent = new PriorityComponent_Jobsite(JobsiteID);

            AllStationsInJobsite.Values.ToList().ForEach(station => station.StationData.SetJobsiteID(JobsiteID));

            _setTickRate(TickRate.TenSeconds, false);
        }

        TickRate _currentTickRate;

        void _setTickRate(TickRate tickRate, bool unregister = true)
        {
            if (_currentTickRate == tickRate) return;

            if (unregister) Manager_TickRate.UnregisterTicker(TickerType.Jobsite, _currentTickRate, JobsiteID);
            Manager_TickRate.RegisterTicker(TickerType.Jobsite, tickRate, JobsiteID, _onTick);
            _currentTickRate = tickRate;
        }

        void _onTick()
        {
            foreach (var product in AllStationsInJobsite.Values.Select(s => s.StationData.ProductionData))
            {
                product.GetEstimatedProductionRatePerHour();
                product.GetActualProductionRatePerHour();
            }

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void         _adjustProduction(float               idealRatio);
        protected abstract VocationName _getRelevantVocation(EmployeePositionName positionName);

        Dictionary<uint, StationComponent> _getAllStationsInJobsite() =>
            GetComponentsInChildren<StationComponent>().ToDictionary(station => station.StationData.StationID);

        public StationComponent GetNearestRelevantStationInJobsite(Vector3 position, List<StationName> stationNames)
            => AllStationsInJobsite
               .Where(station => stationNames.Contains(station.Value.StationName))
               .OrderBy(station => Vector3.Distance(position, station.Value.transform.position))
               .FirstOrDefault().Value;


        public bool GetNewCurrentJob(ActorComponent actor, uint stationID = 0)
        {
            var highestPriorityJob = PriorityComponent.GetHighestSpecificPriority(
                actor.ActorData.CareerData.AllJobs.Select(j => (uint)j).ToList(), stationID);

            if (highestPriorityJob == null) return false;

            var jobName = (JobName)highestPriorityJob.PriorityID;

            var relevantStations       = _getOrderedRelevantStationsForJob(jobName, actor);
            
            var relevantStation = relevantStations.FirstOrDefault();
            
            if (relevantStation is null)
            {
                Debug.LogError($"No relevant stations found for job: {jobName}.");
                return false;
            }
            
            var relevantOperatingArea = relevantStation.GetRelevantOperatingArea(actor);

            var job = new Job(jobName, relevantStation.StationID, relevantOperatingArea.OperatingAreaID);

            actor.ActorData.CareerData.SetCurrentJob(job);

            return true;
        }

        List<StationComponent> _getOrderedRelevantStationsForJob(JobName jobName, ActorComponent actor)
        {
            var relevantStations = AllStationsInJobsite.Values
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

        public List<EmployeePositionName> GetMinimumEmployeePositions()
        {
            HashSet<EmployeePositionName> employeePositions = new();

            foreach (var station in AllStationsInJobsite.Values)
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
            foreach (var station in AllStationsInJobsite.Values)
            {
                station.RemoveAllOperatorsFromStation();
            }

            var tempEmployees = employeeIDs.Select(employeeID => Manager_Actor.GetActorData(employeeID)).ToList();

            foreach (var station in AllStationsInJobsite.Values)
            {
                var allowedPositions = station.AllowedEmployeePositions;
                var employeesForStation = tempEmployees
                                          .Where(e => allowedPositions.Contains(e.CareerData.EmployeePositionName))
                                          .OrderByDescending(e => e.CareerData.EmployeePositionName)
                                          .ThenByDescending(e =>
                                              e.VocationData.GetVocationExperience(
                                                  _getRelevantVocation(e.CareerData.EmployeePositionName)))
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
            return AllStationsInJobsite.Values.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();
        }

        List<StationComponent> _relevantStations_Deliver(InventoryData inventoryData)
        {
            return AllStationsInJobsite.Values.Where(station => station.GetInventoryItemsToDeliver(inventoryData).Count > 0)
                                       .ToList();
        }
    }
}