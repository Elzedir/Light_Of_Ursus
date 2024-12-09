using System.Collections.Generic;
using System.Linq;
using Actor;
using EmployeePosition;
using Initialisation;
using Inventory;
using Jobs;
using Managers;
using Priority;
using Station;
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
        public void                  SetJobSiteData(JobSite_Data jobSiteData) => JobSiteData = jobSiteData;
        public void                  SetCityID(uint              cityID)      => JobSiteData.CityID = cityID;

        Dictionary<uint, Station_Component> _allStationInJobSite;

        public Dictionary<uint, Station_Component> AllStationsInJobSite =>
            _allStationInJobSite ??= _getAllStationsInJobsite();

        public List<EmployeePositionName> AllCoreEmployeePositions;

        public PriorityComponent_JobSite PriorityComponent;

        public float IdealRatio;
        public void  SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int   PermittedProductionInequality = 10;
        
        void Awake()
        {
            Manager_Initialisation.OnInitialiseStations += _initialise;
        }

        void _initialise()
        {
            JobSiteData.InitialiseJobSiteData();
            
            PriorityComponent = new PriorityComponent_JobSite(JobSiteID);

            AllStationsInJobSite.Values.ToList().ForEach(station => station.StationData.SetJobsiteID(JobSiteID));

            _setTickRate(TickRate.TenSeconds, false);

            _initialised = true;
        }

        TickRate _currentTickRate;

        void _setTickRate(TickRate tickRate, bool unregister = true)
        {
            if (_currentTickRate == tickRate) return;

            if (unregister) Manager_TickRate.UnregisterTicker(TickerType.Jobsite, _currentTickRate, JobSiteID);
            Manager_TickRate.RegisterTicker(TickerType.Jobsite, tickRate, JobSiteID, _onTick);
            _currentTickRate = tickRate;
        }

        void _onTick()
        {
            if (!_initialised) return;
            
            foreach (var product in AllStationsInJobSite.Values.Select(s => s.StationData.ProductionData))
            {
                product.GetEstimatedProductionRatePerHour();
                product.GetActualProductionRatePerHour();
            }

            _compareProductionOutput();
        }

        protected abstract bool _compareProductionOutput();

        protected abstract void         _adjustProduction(float               idealRatio);
        protected abstract VocationName _getRelevantVocation(EmployeePositionName positionName);

        Dictionary<uint, Station_Component> _getAllStationsInJobsite() =>
            GetComponentsInChildren<Station_Component>().ToDictionary(station => station.StationData.StationID);

        public Station_Component GetNearestRelevantStationInJobsite(Vector3 position, List<StationName> stationNames)
            => AllStationsInJobSite
               .Where(station => stationNames.Contains(station.Value.StationName))
               .OrderBy(station => Vector3.Distance(position, station.Value.transform.position))
               .FirstOrDefault().Value;


        public bool GetNewCurrentJob(Actor_Component actor, uint stationID = 0)
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

        List<Station_Component> _getOrderedRelevantStationsForJob(JobName jobName, Actor_Component actor)
        {
            var relevantStations = AllStationsInJobSite.Values
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

            foreach (var station in AllStationsInJobSite.Values)
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
            foreach (var station in AllStationsInJobSite.Values)
            {
                station.RemoveAllOperatorsFromStation();
            }

            var tempEmployees = employeeIDs.Select(Actor_Manager.GetActor_Data).ToList();

            foreach (var station in AllStationsInJobSite.Values)
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
                    JobSiteData.AddEmployeeToStation(employee.ActorID, station.StationData.StationID);
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
            return AllStationsInJobSite.Values.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();
        }

        List<Station_Component> _relevantStations_Deliver(InventoryData inventoryData)
        {
            return AllStationsInJobSite.Values.Where(station => station.GetInventoryItemsToDeliver(inventoryData).Count > 0)
                                       .ToList();
        }
    }
}