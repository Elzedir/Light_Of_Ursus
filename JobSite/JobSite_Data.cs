using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using City;
using EmployeePosition;
using Items;
using Jobs;
using Managers;
using Priority;
using Recipes;
using Station;
using TickRates;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JobSite
{
    [Serializable]
    public class JobSite_Data : Data_Class
    {
        public uint        JobSiteID;
        public JobSiteName JobSiteName;

        JobSite_Component _jobSiteComponent;

        public JobSite_Component JobSite_Component =>
            _jobSiteComponent ??= JobSite_Manager.GetJobSite_Component(JobSiteID);

        public uint JobsiteFactionID;
        public uint CityID;

        public bool   JobsiteIsActive = true;
        public void   SetJobsiteIsActive(bool jobsiteIsActive) => JobsiteIsActive = jobsiteIsActive;
        public string JobsiteDescription;
        public uint   OwnerID;

        [SerializeField] List<uint> _allEmployeeIDs;

        Dictionary<uint, Actor_Component>        _allEmployees;
        public Dictionary<uint, Actor_Component> AllEmployees => _allEmployees ??= _populateAllEmployees();

        int _currentAllStationsLength;

        // Call when a new city is formed.
        public void                         RefreshAllStations() => _currentAllStationsLength = 0;
        Dictionary<uint, Station_Component> _allStation_Components;

        public Dictionary<uint, Station_Component> AllStationComponents
        {
            get
            {
                if (_allStation_Components is not null && _allStation_Components.Count != 0 &&
                    _allStation_Components.Count == _currentAllStationsLength) return _allStation_Components;

                _currentAllStationsLength = _allStation_Components?.Count ?? 0;
                return JobSite_Component.GetAllStationsInJobSite();
            }
        }


        ProductionData _productionData;
        public ProductionData ProductionData => _productionData ??= new ProductionData(new List<Item>(), JobSiteID);
        Priority_Data_JobSite _priorityData;
        public Priority_Data_JobSite PriorityData => _priorityData ??= new Priority_Data_JobSite(JobSiteID);

        Dictionary<(uint, uint), uint> _workPost_Workers;

        public Dictionary<(uint StationID, uint WorkPostID), uint> WorkPost_Workers =>
            _workPost_Workers ??= _populateWorkPost_Workers();

        Dictionary<(uint, uint), uint> _populateWorkPost_Workers()
        {
            var allEmployeePositions = new Dictionary<(uint, uint), uint>();

            foreach (var station in AllStationComponents)
            {
                foreach (var workPost in station.Value.Station_Data.AllWorkPost_Data)
                {
                    allEmployeePositions.Add((station.Key, workPost.Key), 0);
                }
            }

            return allEmployeePositions;
        }

        public JobSite_Data(uint           jobSiteID, JobSiteName jobSiteName, uint jobsiteFactionID, uint cityID,
                            string         jobsiteDescription, uint ownerID, List<uint> allStationIDs,
                            List<uint>     allEmployeeIDs,
                            ProsperityData prosperityData)
        {
            JobSiteID          = jobSiteID;
            JobSiteName        = jobSiteName;
            JobsiteFactionID   = jobsiteFactionID;
            CityID             = cityID;
            JobsiteDescription = jobsiteDescription;
            OwnerID            = ownerID;
            AllStationIDs      = allStationIDs;
            _allEmployeeIDs    = allEmployeeIDs;
            ProsperityData     = prosperityData;

            _priorityData = new Priority_Data_JobSite(JobSiteID);
        }

        Dictionary<uint, Actor_Component> _populateAllEmployees()
        {
            var allEmployees = new Dictionary<uint, Actor_Component>();

            foreach (var employeeID in _allEmployeeIDs)
            {
                var employee = Actor_Manager.GetActor_Component(employeeID);

                if (employee is null)
                {
                    Debug.Log($"Employee with ID {employeeID} not found.");
                    continue;
                }

                allEmployees.Add(employeeID, employee);
            }

            return allEmployees;
        }

        public ProsperityData ProsperityData;

        public List<uint>                                           AllStationIDs;
        public Dictionary<(uint ActorID, uint OrderID), Order_Base> AllOrders = new();

        // Work out how to do quotas and set production rate

        public void InitialiseJobSiteData()
        {
            foreach (var station in AllStationComponents
                         .Where(station_Component => !AllStationIDs.Contains(station_Component.Key)))
            {
                Debug.Log(
                    $"Station_Component: {station.Value?.Station_Data?.StationID}: {station.Value?.Station_Data?.StationName} doesn't exist in DataList");
            }

            foreach (var stationID in AllStationIDs
                         .Where(stationID => !AllStationComponents.ContainsKey(stationID)))
            {
                Debug.LogError(
                    $"Station with ID {stationID} doesn't exist physically in JobSite: {JobSiteID}: {JobSiteName}");
            }
            
            PriorityData.RegenerateAllPriorities();

            _jobSiteComponent.StartCoroutine(_populate());
        }

        public void RegisterAllTickers()
        {
            _registerStations();
        }

        void _registerStations()
        {
            foreach (var station in AllStationComponents.Values)
            {
                Manager_TickRate.RegisterTicker(TickerTypeName.Station, TickRateName.OneSecond, station.StationID,
                    station.Station_Data.OnTick);
                station.Station_Data.CurrentTickRateName = TickRateName.OneSecond;
            }
        }

        IEnumerator _populate()
        {
            RegisterAllTickers();

            //Usually this will only happen a few seconds after game start since things won't hire immediately after game start. Instead it will be assigned to
            // TickRate manager to onTick();

            yield return null;

            // Set owner later
            //CheckOwner();

            //AllocateExistingEmployeesToStations();

            // Temporarily for now
            FillEmptyJobsitePositions();
        }

        // public void CheckOwner()
        // {
        //     if (OwnerID == 0) GetNewOwner();
        //
        //     if (OwnerID == 0) return;
        //     
        //     var ownerData = Actor_Manager.GetActor_Data(OwnerID);
        //     var owner     = Actor_Manager.GetActor_Component(OwnerID);
        //     
        //     owner.ActorMaterial.material = Resources.Load<Material>("Materials/Material_Yellow");
        //
        //     // And change all affected things, like perks, job settings, etc.
        // }

        public void SetOwner(uint ownerID)
        {
            OwnerID = ownerID;
        }

        protected Actor_Component _findEmployeeFromCity(EmployeePositionName positionName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            var allCitizenIDs = city?.CityData?.Population?.AllCitizenIDs;

            if (allCitizenIDs == null || !allCitizenIDs.Any())
            {
                //Debug.Log("No citizens found in the city.");
                return null;
            }

            var citizenID = allCitizenIDs
                .FirstOrDefault(c =>
                    Actor_Manager.GetActor_Data(c)?.CareerUpdater.JobSiteID == 0 &&
                    _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(positionName))
                );

            if (citizenID == 0)
            {
                //Debug.LogWarning($"No suitable citizen found for position: {position} in city with ID {CityID}.");
                return null;
            }

            var actor = Actor_Manager.GetActor_Component(actorID: citizenID);

            return actor;
        }

        protected Actor_Component _generateNewEmployee(EmployeePositionName positionName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            var employeeMaster = EmployeePosition_Manager.GetEmployeePosition_Master(positionName);

            if (employeeMaster == null) throw new Exception($"EmployeeMaster for position: {positionName} is null.");

            var actor = Actor_Manager.SpawnNewActor(city.CitySpawnZone.transform.position,
                employeeMaster.EmployeeDataPreset);

            AddEmployeeToJobsite(actor.ActorData.ActorID);

            return actor;
        }

        protected bool _hasMinimumVocationRequired(uint citizenID, List<VocationRequirement> vocationRequirements)
        {
            var actorData = Actor_Manager.GetActor_Data(citizenID);

            foreach (var vocation in vocationRequirements)
            {
                if (vocation.VocationName == VocationName.None) continue;

                if (actorData.VocationUpdater.GetVocationExperience(vocation.VocationName) <
                    vocation.MinimumVocationExperience)
                {
                    return false;
                }
            }

            return true;
        }

        protected List<VocationRequirement> _getVocationAndMinimumExperienceRequired(EmployeePositionName positionName)
        {
            var vocationRequirements = new List<VocationRequirement>();

            switch (positionName)
            {
                case EmployeePositionName.Logger:
                    vocationRequirements = new List<VocationRequirement>
                    {
                        new VocationRequirement(VocationName.Logging, 1000)
                    };
                    break;
                case EmployeePositionName.Sawyer:
                    vocationRequirements = new List<VocationRequirement>
                    {
                        new VocationRequirement(VocationName.Sawying, 1000)
                    };
                    break;
                case EmployeePositionName.Hauler:
                    vocationRequirements = new List<VocationRequirement>();
                    break;
                default:
                    Debug.Log($"EmployeePosition: {positionName} not recognised.");
                    break;
            }

            return vocationRequirements;
        }

        public void AddEmployeeToJobsite(uint employeeID)
        {
            if (_allEmployeeIDs.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} already exists in employee list.");
                return;
            }

            _allEmployeeIDs.Add(employeeID);
            Actor_Manager.GetActor_Data(employeeID).CareerUpdater.SetJobsiteID(JobSiteID);
        }

        public void HireEmployee(uint employeeID)
        {
            AddEmployeeToJobsite(employeeID);

            // And then apply relevant relation buff
        }

        public void RemoveEmployeeFromJobsite(uint employeeID)
        {
            if (!_allEmployeeIDs.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} is not in employee list.");
                return;
            }

            _allEmployeeIDs.Remove(employeeID);
            Actor_Manager.GetActor_Data(employeeID).CareerUpdater.SetJobsiteID(0);

            // Remove employee job from employee job component.
        }

        public void FireEmployee(uint employeeID)
        {
            RemoveEmployeeFromJobsite(employeeID);

            // And then apply relation debuff.
        }

        public bool AddEmployeeToStation(Actor_Component employee, Station_Component station)
        {
            RemoveWorkerFromCurrentStation(employee);

            var actorCurrentJob = employee.ActorData.CareerUpdater.CurrentJob;

            if (actorCurrentJob is null)
            {
                if (!employee.ActorData.CareerUpdater.GetNewCurrentJob())
                {
                    Debug.LogWarning(
                        $"Actor Current Job for employeeID: {employee} is null. Needs to get a job first.");
                    return false;
                }
            }

            if (_addWorkerToStation(employee, station, actorCurrentJob)) return true;

            Debug.Log($"Couldn't add employee to station: {station.StationID}");
            return false;
        }

        bool _addWorkerToStation(Actor_Component worker, Station_Component station, Job actorCurrentJob)
        {
            if (actorCurrentJob.JobName == JobName.Idle)
            {
                Debug.Log("Worker is idling. Adding to station '0'.");
                return true;
            }

            var openWorkPost_Data = station.Station_Data.GetOpenWorkPost()?.WorkPostData;

            if (openWorkPost_Data is null)
            {
                Debug.Log($"No open WorkPosts found for Worker: {worker}");
                return false;
            }

            WorkPost_Workers[(station.StationID, openWorkPost_Data.WorkPostID)] = worker.ActorID;
            openWorkPost_Data.AddWorkerToWorkPost(worker);
            actorCurrentJob.SetStationAndWorkPostID((station.StationID, openWorkPost_Data.WorkPostID));

            return true;
        }

        public bool RemoveWorkerFromCurrentStation(Actor_Component worker)
        {
            try
            {
                var stationWorkPostID = WorkPost_Workers.FirstOrDefault(x => x.Value == worker.ActorID).Key;

                if (stationWorkPostID == (0, 0))
                {
                    //Debug.LogWarning($"Worker {worker} not assigned to any WorkPosts.");
                    return false;
                }

                AllStationComponents[stationWorkPostID.StationID].Station_Data
                                                                 .AllWorkPost_Data[stationWorkPostID.WorkPostID]
                                                                 .RemoveCurrentWorkerFromWorkPost();
                WorkPost_Workers[stationWorkPostID] = 0;

                if (worker.ActorData.CareerUpdater.CurrentJob is null) return true;

                worker.ActorData.CareerUpdater.StopCurrentJob();
                Debug.LogError($"CurrentJob was not stopped for employeeID: {worker.ActorID}. Stopping here.");

                return true;
            }
            catch
            {
                if (worker.ActorData == null)
                {
                    Debug.Log($"ActorData for employeeID: {worker} does not exist.");
                    return false;
                }

                var actorCareer = worker.ActorData.CareerUpdater;

                if (actorCareer == null)
                {
                    Debug.Log($"Employee does not have a career.");
                    return false;
                }

                if (worker.ActorData.CareerUpdater.CurrentJob == null)
                {
                    Debug.Log($"Employee does not have a current job.");
                    return false;
                }

                var stationID = actorCareer.CurrentJob.StationID;

                if (!AllStationIDs.Contains(stationID))
                {
                    Debug.Log($"StationID: {stationID} does not exist in AllStationIDs");
                    return false;
                }

                var station = Station_Manager.GetStation_Data(stationID);

                if (station == null)
                {
                    Debug.Log($"Station does not exist.");
                    return false;
                }

                Debug.Log($"EmployeeID: {worker} is not an operator at StationID: {station.StationID}");
                return false;
            }
        }

        public uint GetWorkPostIDFromWorkerID(uint workerID)
        {
            return WorkPost_Workers.FirstOrDefault(x => x.Value == workerID).Key.WorkPostID;
        }

        public void RemoveAllWorkersFromStation(Station_Component station)
        {
            foreach (var workPost in station.Station_Data.AllWorkPost_Components.Values)
            {
                workPost.WorkPostData.RemoveCurrentWorkerFromWorkPost();
            }
        }

        public void RemoveAllWorkersFromAllStations()
        {
            foreach (var station in AllStationComponents.Values)
            {
                RemoveAllWorkersFromStation(station);
            }
        }

        public List<Item> GetEstimatedProductionRatePerHour()
        {
            var estimatedProductionItems = new List<Item>();

            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var swp in WorkPost_Workers)
            {
                if (swp.Value == 0) continue;
                
                float totalProductionRate = 0;

                var station_Data = AllStationComponents[swp.Key.StationID].Station_Data;

                foreach (var kvp in WorkPost_Workers.Where(keyValuePair =>
                             keyValuePair.Key.StationID == station_Data.StationID && keyValuePair.Value != 0))
                {
                    var individualProductionRate = station_Data.BaseProgressRatePerHour;

                    foreach (var vocation in station_Data.StationProgressData.CurrentProduct.RequiredVocations)
                    {
                        individualProductionRate *= Actor_Manager.GetActor_Data(kvp.Value).VocationUpdater
                                                                 .GetProgress(vocation);
                    }

                    totalProductionRate += individualProductionRate;
                    // Don't forget to add in estimations for travel time.    
                }

                float requiredProgress         = station_Data.StationProgressData.CurrentProduct.RequiredProgress;
                var   estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

                for (var i = 0; i < Mathf.FloorToInt(estimatedProductionCount); i++)
                {
                    foreach (var item in station_Data.StationProgressData.CurrentProduct.RecipeProducts)
                    {
                        estimatedProductionItems.Add(new Item(item));
                    }
                }

            }

            return estimatedProductionItems;
        }

        // public void AllocateEmployeesToStations()
        // {
        //     var allOperators = GetAllOperators();

        //     var allCombinations = GetAllCombinations(allOperators);

        //     var bestCombination = new List<OperatorData>();

        //     var bestRatioDifference = 100f;

        //     foreach (var combination in allCombinations)
        //     {
        //         var ratioDifference = Mathf.Abs(ProsperityData.GetProsperityPercentage() - ProsperityData.GetProsperityPercentage(combination));

        //         if (ratioDifference < bestRatioDifference)
        //         {
        //             Debug.Log($"Combination {combination} the is best ratio");

        //             bestRatioDifference = ratioDifference;
        //             bestCombination = new List<OperatorData>(combination);
        //         }
        //     }

        //     AssignEmployeesToStations(bestCombination);

        //     Debug.Log("Adjusted production to balance the ratio.");
        // }

        public Station_Component GetNearestRelevantStationInJobsite(Vector3 position, List<StationName> stationNames)
            => AllStationComponents
               .Where(station => stationNames.Contains(station.Value.StationName))
               .OrderBy(station => Vector3.Distance(position, station.Value.transform.position))
               .FirstOrDefault().Value;

        public void FillEmptyJobsitePositions()
        {
            var prosperityRatio = ProsperityData.GetProsperityPercentage();
            var maxOperatorCount = AllStationComponents.Values
                                                       .SelectMany(station =>
                                                           station.Station_Data.AllWorkPost_Components).ToList().Count;
            var desiredOperatorCount = Mathf.RoundToInt(maxOperatorCount * prosperityRatio);

            if (AllEmployees.Count >= maxOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than MaxOperatorCount: {maxOperatorCount}.");
                return;
            }

            if (AllEmployees.Count >= desiredOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than DesiredOperatorCount: {desiredOperatorCount}.");
                return;
            }

            var iteration = 0;

            //while (iteration < desiredOperatorCount - AllEmployees.Count && iteration < 100)
            for (var i = 0; i < 10; i++)
            {
                var allPositionsFilled = true;

                if (AllStationComponents.Count == 0)
                {
                    Debug.Log("No stations found in Jobsite.");
                    break;
                }

                foreach (var station in AllStationComponents.Values)
                {
                    var allStationWorkers = WorkPost_Workers.Select(kvp => kvp.Key)
                                                            .Where(kvp => kvp.StationID == station.StationID)
                                                            .Select(workPost => WorkPost_Workers[workPost])
                                                            .Where(workerID => workerID != 0)
                                                            .ToList();

                    if (iteration >= desiredOperatorCount - AllEmployees.Count)
                    {
                        break;
                    }

                    if (allStationWorkers.Count >= station.Station_Data.AllWorkPost_Components.Count)
                    {
                        //Debug.Log($"All operating areas are already filled for StationID: {stationID}");
                        continue;
                    }

                    allPositionsFilled = false;

                    //Debug.Log($"Couldn't find employee from City for position: {station.CoreEmployeePosition}");
                    var newEmployee = _findEmployeeFromCity(station.CoreEmployeePositionName) ??
                                      _generateNewEmployee(station.CoreEmployeePositionName);

                    if (!AddEmployeeToStation(newEmployee, station))
                    {
                        Object.Destroy(newEmployee.gameObject);
                        //Debug.Log($"Couldn't add employee to station: {stationID}");
                        continue;
                    }

                    newEmployee.ActorData.CareerUpdater.SetEmployeePosition(station.CoreEmployeePositionName);

                    iteration++;
                }

                if (allPositionsFilled)
                {
                    //Debug.Log("All necessary positions are filled.");
                    break;
                }
            }

            Debug.Log(iteration);
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base JobSite Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"JobSite ID: {JobSiteID}",
                        $"Jobsite Faction ID: {JobsiteFactionID}",
                        $"City ID: {CityID}",
                        $"Jobsite Description: {JobsiteDescription}",
                        $"Owner ID: {OwnerID}",
                        $"Jobsite is Active: {JobsiteIsActive}"
                    }));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in Base JobSite Data");
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Employee Data",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: _allEmployeeIDs.Select(employeeID => $"{employeeID}").ToList()));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in Employee Data");
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Station Operators",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: WorkPost_Workers.Select(operatorID => $"{operatorID}").ToList()));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Current Operators not found.");
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "All Station IDs",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: AllStationIDs?.Select(stationID => $"{stationID}").ToList()));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in All Station IDs");
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Order Data",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: AllOrders.Select(order => $"{order.Key.ActorID}: {order.Key.OrderID}").ToList()));
            }
            catch
            {
                Debug.LogError("Error in Order Data");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Production Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"All Produced Items: {string.Join(", ", ProductionData.AllProducedItems)}",
                        $"Estimated Production Rate Per Hour: {string.Join(", ", ProductionData.EstimatedProductionRatePerHour)}",
                        $"Station ID: {ProductionData.JobSiteID}"
                    }));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Production Data not found.");
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Priority Data",
                    dataDisplayType: DataDisplayType.SelectableList,
                    subData: PriorityData.DataSO_Object(toggleMissingDataDebugs).SubData));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Priority Data not found.");
                }
            }

            return new Data_Display(
                title: "JobSite Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
        }
    }

    [Serializable]
    public class ProductionData
    {
        public List<Item> AllProducedItems;
        public List<Item> EstimatedProductionRatePerHour;
        public uint       JobSiteID;

        JobSite_Component        _jobSite;
        public JobSite_Component JobSite => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID);

        public ProductionData(List<Item> allProducedItems, uint jobSiteID)
        {
            AllProducedItems = allProducedItems;
            JobSiteID        = jobSiteID;
        }

        public List<Item> GetEstimatedProductionRatePerHour()
        {
            return EstimatedProductionRatePerHour = JobSite.JobSiteData.GetEstimatedProductionRatePerHour();
        }
    }
}