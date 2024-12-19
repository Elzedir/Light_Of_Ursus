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
using Recipes;
using Station;
using TickRates;
using Tools;
using UnityEngine;

namespace JobSite
{
    [Serializable]
    public class JobSite_Data : Data_Class
    {
        public uint        JobSiteID;
        public JobSiteName JobSiteName;
        
        JobSite_Component _jobSiteComponent;
        public JobSite_Component JobSite_Component => _jobSiteComponent ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        
        public uint   JobsiteFactionID;
        public uint   CityID;

        public bool   JobsiteIsActive = true;
        public void   SetJobsiteIsActive(bool jobsiteIsActive) => JobsiteIsActive = jobsiteIsActive;
        public string JobsiteDescription;
        public uint   OwnerID;

        List<uint> _allEmployeeIDs;
            
            // Change to all current wokrers. AllStationsInJobSite.Values.SelectMany(station => station.Station_Data.AllWorkPost_Data.Values.Select(workPost => workPost.CurrentWorkerID)).ToList();;
        Dictionary<uint, Actor_Component>        _allEmployees;
        public Dictionary<uint, Actor_Component> AllEmployees => _allEmployees ??= _populateAllEmployees();
        
        int                                 _currentAllStationsLength;
        // Call when a new city is formed.
        public void                         RefreshAllStations() => _currentAllStationsLength = 0;
        Dictionary<uint, Station_Component> _allStation_Components;
        public Dictionary<uint, Station_Component> AllStationComponents
        {
            get
            {
                if (_allStation_Components is not null && _allStation_Components.Count != 0 && _allStation_Components.Count == _currentAllStationsLength) return _allStation_Components;
                
                _currentAllStationsLength = _allStation_Components?.Count ?? 0;
                return JobSite_Component.GetAllStationsInJobSite();
            }
        }
        
        
        ProductionData        _productionData;
        public ProductionData ProductionData => _productionData ??= new ProductionData(new List<Item>(), StationID);

        Dictionary<(uint, uint), uint> _workPost_Workers;
        public Dictionary<(uint StationID, uint WorkPostID), uint> WorkPost_Workers => _workPost_Workers ??= _populateWorkPost_Workers();
        
        Dictionary<(uint, uint), uint> _populateWorkPost_Workers()
        {
            var allEmployeePositions = new Dictionary<(uint, uint), uint>();

            foreach (var station in AllStationComponents)
            {
                foreach (var workPost in station.Value.Station_Data.AllWorkPost_Data)
                {
                    allEmployeePositions.Add((station.Key, workPost.Key), 0);
                    Debug.Log((station.Key, workPost.Key));
                }
            }

            return allEmployeePositions;
        }

        public JobSite_Data(uint           jobSiteID, JobSiteName jobSiteName, uint jobsiteFactionID, uint cityID,
                            string         jobsiteDescription, uint ownerID, List<uint> allStationIDs,
                            ProsperityData prosperityData)
        {
            JobSiteID          = jobSiteID;
            JobSiteName        = jobSiteName;
            JobsiteFactionID   = jobsiteFactionID;
            CityID             = cityID;
            JobsiteDescription = jobsiteDescription;
            OwnerID            = ownerID;
            AllStationIDs      = allStationIDs;
            ProsperityData     = prosperityData;
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
                Debug.Log($"Station_Component: {station.Value?.Station_Data?.StationID}: {station.Value?.Station_Data?.StationName} doesn't exist in DataList");
            }
            
            foreach (var stationID in AllStationIDs
                         .Where(stationID => !AllStationComponents.ContainsKey(stationID)))
            {
                Debug.LogError($"Station with ID {stationID} doesn't exist physically in JobSite: {JobSiteID}: {JobSiteName}");
            }

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
                Manager_TickRate.RegisterTicker(TickerTypeName.Station, TickRateName.OneSecond, station.StationID, station.Station_Data.OnTick);
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

        protected uint _findEmployeeFromCity(EmployeePositionName positionName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            var allCitizenIDs = city?.CityData?.Population?.AllCitizenIDs;

            if (allCitizenIDs == null || !allCitizenIDs.Any())
            {
                //Debug.Log("No citizens found in the city.");
                return 0;
            }

            var citizenID = allCitizenIDs
                                      .FirstOrDefault(c =>
                                          Actor_Manager.GetActor_Data(c)?.CareerData.JobSiteID == 0 &&
                                          _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(positionName))
                                      );

            if (citizenID == 0)
            {
                //Debug.LogWarning($"No suitable citizen found for position: {position} in city with ID {CityID}.");
                return 0;
            }

            var actor = Actor_Manager.GetActor_Component(actorID: citizenID);
            
            return actor?.ActorData?.ActorID ?? 0;
        }

        protected uint _generateNewEmployee(EmployeePositionName positionName)
        {
            var city = City_Manager.GetCity_Component(CityID);

            var employeeMaster = EmployeePosition_Manager.GetEmployeePosition_Master(positionName);

            if (employeeMaster == null) throw new Exception($"EmployeeMaster for position: {positionName} is null.");

            var actor = Actor_Manager.SpawnNewActor(city.CitySpawnZone.transform.position, employeeMaster.EmployeeDataPreset);

            AddEmployeeToJobsite(actor.ActorData.ActorID);

            return actor.ActorData.ActorID;
        }

        protected bool _hasMinimumVocationRequired(uint citizenID, List<VocationRequirement> vocationRequirements)
        {
            var actorData = Actor_Manager.GetActor_Data(citizenID);

            foreach(var vocation in vocationRequirements)
            {
                if (vocation.VocationName == VocationName.None) continue;

                if (actorData.VocationData.GetVocationExperience(vocation.VocationName) < vocation.MinimumVocationExperience)
                {
                    return false;
                }
            }

            return true;
        }

        protected List<VocationRequirement> _getVocationAndMinimumExperienceRequired(EmployeePositionName positionName)
        {
            var vocationRequirements = new List<VocationRequirement>();

            switch(positionName)
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
            Actor_Manager.GetActor_Data(employeeID).CareerData.SetJobsiteID(JobSiteID);
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
            Actor_Manager.GetActor_Data(employeeID).CareerData.SetJobsiteID(0);

            // Remove employee job from employee job component.
        }

        public void FireEmployee(uint employeeID)
        {
            RemoveEmployeeFromJobsite(employeeID);

            // And then apply relation debuff.
        }

        public bool AddEmployeeToStation(uint employeeID, Station_Component station)
        {
            RemoveEmployeeFromCurrentStation(employeeID);
            
            var actorCurrentJob = Actor_Manager.GetActor_Data(employeeID)?.CareerData?.CurrentJob;

            if (actorCurrentJob is null)
            {
                Debug.LogWarning($"Actor Current Job for employeeID: {employeeID} is null. Needs to get a job first.");
                // Try implement getting a job here.
                return false;
            }

            if (_addWorkerToStation(employeeID, station, actorCurrentJob)) return true;
            
            Debug.Log($"Couldn't add employee to station: {station.StationID}");
            return false;
        }

        bool _addWorkerToStation(uint workerID, Station_Component station, Job actorCurrentJob)
        {
            var openWorkPost_Data = station.Station_Data.GetOpenWorkPost()?.WorkPostData;
            
            if (openWorkPost_Data is null)
            {
                Debug.Log($"No open WorkPosts found for Worker: {workerID}");
                return false;
            }
            
            WorkPost_Workers[(station.StationID, openWorkPost_Data.WorkPostID)] = workerID;
            openWorkPost_Data.AddWorkerToWorkPost(workerID);

            return true;
        }

        public bool RemoveEmployeeFromCurrentStation(uint employeeID)
        {
            try
            {
                var station =
                    Station_Manager.GetStation_Data(Actor_Manager.GetActor_Data(employeeID).CareerData.CurrentJob
                                                                 .StationID);

                if (!RemoveWorkerFromStation(employeeID))
                {
                    Debug.Log($"Couldn't remove employee from station: {station.StationID}");
                    return false;
                }

                Actor_Manager.GetActor_Data(employeeID)?.CareerData.CurrentJob.SetStationAndWorkPostID((0,0));

                return true;
            }
            catch
            {
                var actorData = Actor_Manager.GetActor_Data(employeeID);

                if (actorData == null)
                {
                    Debug.Log($"ActorData for employeeID: {employeeID} does not exist.");
                    return false;
                }

                var actorCareer = actorData.CareerData;

                if (actorCareer == null)
                {
                    Debug.Log($"Employee does not have a career.");
                    return false;
                }

                if (actorData.CareerData.CurrentJob == null)
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
                
                Debug.Log($"EmployeeID: {employeeID} is not an operator at StationID: {station.StationID}");
                return false;
            }
        }
        
        public uint GetWorkPostIDFromWorkerID(uint workerID)
        {
            return WorkPost_Workers.FirstOrDefault(x => x.Value == workerID).Key.WorkPostID;
        }

        public bool RemoveWorkerFromStation(uint operatorID)
        {
            var stationWorkPostID = WorkPost_Workers.FirstOrDefault(x => x.Value == operatorID).Key;
            
            if (stationWorkPostID == (0, 0))
            {
                Debug.Log($"Operator {operatorID} not found in operating areas.");
                return false;
            }
            
            AllStationComponents[stationWorkPostID.StationID].Station_Data.AllWorkPost_Data[stationWorkPostID.WorkPostID].RemoveCurrentWorkerFromWorkPost();
            WorkPost_Workers[stationWorkPostID] = 0;
            
            return true;
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
            float totalProductionRate = 0;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var kvp in WorkPost_Workers)
            {
                var station_Data = AllStationComponents[kvp.Key.StationID].Station_Data;
                
                var individualProductionRate = station_Data.BaseProgressRatePerHour;

                foreach (var vocation in station_Data.StationProgressData.CurrentProduct.RequiredVocations)
                {
                    individualProductionRate *= Actor_Manager.GetActor_Data(kvp.Value).VocationData
                                                             .GetProgress(vocation);
                }

                totalProductionRate += individualProductionRate;
                // Don't forget to add in estimations for travel time.
            }

            float requiredProgress         = StationProgressData.CurrentProduct.RequiredProgress;
            var   estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

            var estimatedProductionItems = new List<Item>();

            for (var i = 0; i < Mathf.FloorToInt(estimatedProductionCount); i++)
            {
                foreach (var item in StationProgressData.CurrentProduct.RecipeProducts)
                {
                    estimatedProductionItems.Add(new Item(item));
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
            var prosperityRatio      = ProsperityData.GetProsperityPercentage();
            var maxOperatorCount     = AllStationComponents.Values.SelectMany(station => station.Station_Data.AllWorkPost_Components).ToList().Count;
            var desiredOperatorCount = Mathf.RoundToInt(maxOperatorCount * prosperityRatio);

            if (_allEmployees.Count >= maxOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than MaxOperatorCount: {maxOperatorCount}.");
                return;
            }

            if (_allEmployees.Count >= desiredOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than DesiredOperatorCount: {desiredOperatorCount}.");
                return;
            }

            var iteration = 0;

            while (iteration < desiredOperatorCount - _allEmployees.Count)
            {
                var allPositionsFilled = true;

                foreach (var station in AllStationComponents.Values)
                {
                    var allStationWorkPosts = WorkPost_Workers.Select(kvp => kvp.Key)
                                                              .Where(kvp => kvp.StationID == station.StationID)
                                                              .ToList();

                    var allStationWorkers = allStationWorkPosts.Select(workPost => WorkPost_Workers[workPost])
                                                               .Where(workerID => workerID != 0).ToList();
                    
                    if (iteration >= desiredOperatorCount - _allEmployees.Count)
                    {
                        break;
                    }

                    if (allStationWorkers.Count >= station.Station_Data.AllWorkPost_Components.Count)
                    {
                        //Debug.Log($"All operating areas are already filled for StationID: {stationID}");
                        continue;
                    }

                    allPositionsFilled = false;

                    var newEmployeeID = _findEmployeeFromCity(station.CoreEmployeePositionName);

                    if (newEmployeeID == 0)
                    {
                        //Debug.Log($"Couldn't find employee from City for position: {station.CoreEmployeePosition}");
                        newEmployeeID = _generateNewEmployee(station.CoreEmployeePositionName);
                    }

                    var actorData = Actor_Manager.GetActor_Data(newEmployeeID);

                    if (!AddEmployeeToStation(actorData.ActorID, station))
                    {
                        //Debug.Log($"Couldn't add employee to station: {stationID}");
                        continue;
                    }

                    actorData.CareerData.SetEmployeePosition(station.CoreEmployeePositionName);

                    iteration++;
                }

                if (allPositionsFilled)
                {
                    //Debug.Log("All necessary positions are filled.");
                    break;
                }
            }
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
                Debug.LogError("Error in Base JobSite Data");
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
                Debug.LogError("Error in Employee Data");
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
                Debug.LogError("Error: Current Operators not found.");
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
                        $"Actual Production Rate Per Hour: {string.Join(", ", ProductionData.ActualProductionRatePerHour)}",
                        $"Station ID: {ProductionData.StationID}"
                    }));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error: Production Data not found.");
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
        public List<Item> ActualProductionRatePerHour;
        public uint       StationID;

        Station_Component        _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);

        public ProductionData(List<Item> allProducedItems, uint stationID)
        {
            AllProducedItems = allProducedItems;
            StationID        = stationID;
        }

        public List<Item> GetActualProductionRatePerHour()
        {
            return ActualProductionRatePerHour = Station.GetActualProductionRatePerHour();
        }

        public List<Item> GetEstimatedProductionRatePerHour()
        {
            return EstimatedProductionRatePerHour = Station.Station_Data.GetEstimatedProductionRatePerHour();
        }
    }
}