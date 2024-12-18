using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using City;
using EmployeePosition;
using Items;
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

        List<uint>              _allEmployeeIDs => ;
            
            // Change to all current wokrers. AllStationsInJobSite.Values.SelectMany(station => station.Station_Data.AllWorkPost_Data.Values.Select(workPost => workPost.CurrentWorkerID)).ToList();;
        Dictionary<uint, Actor_Component>        _allEmployees;
        public Dictionary<uint, Actor_Component> AllEmployees => _allEmployees ??= _populateAllEmployees();
        
        int                                 _currentAllStationsLength;
        Dictionary<uint, Station_Component> _allStationsInJobSite;
        public Dictionary<uint, Station_Component> AllStationsInJobSite
        {
            get
            {
                if (_allStationsInJobSite is not null && _allStationsInJobSite.Count != 0 && _allStationsInJobSite.Count == _currentAllStationsLength) return _allStationsInJobSite;
                
                _currentAllStationsLength = _allStationsInJobSite?.Count ?? 0;
                return JobSite_Component.GetAllStationsInJobSite();
            }
        }
        
        // Call when a new city is formed.
        public void RefreshAllStations() => _currentAllStationsLength = 0;

        Dictionary<uint, uint> _allEmployeePositions;
        public Dictionary<uint, uint> AllEmployeePositions => _allEmployeePositions ??= _populateAllEmployeePositions();
        
        Dictionary<uint, uint> _populateAllEmployeePositions()
        {
            var allEmployeePositions = new Dictionary<uint, uint>();

            foreach (var station in AllStationsInJobSite)
            {
                foreach (var operatingArea in station.Value.AllOperatingAreasInStation)
                {
                    allEmployeePositions.Add(uint.Parse(station.Key.ToString() + operatingArea.WorkPostID.ToString()), 0);
                    Debug.Log(uint.Parse(station.Key.ToString() + operatingArea.WorkPostID.ToString()));
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
            foreach (var station in AllStationsInJobSite
                         .Where(station_Component => !AllStationIDs.Contains(station_Component.Key)))
            {
                Debug.Log($"Station_Component: {station.Value?.Station_Data?.StationID}: {station.Value?.Station_Data?.StationName} doesn't exist in DataList");
            }
            
            foreach (var stationID in AllStationIDs
                         .Where(stationID => !AllStationsInJobSite.ContainsKey(stationID)))
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
            foreach (var station in AllStationsInJobSite.Values)
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

            if (station.Station_Data.Worker.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} is already an operator at StationID: {station.StationID}");
                return false;
            }

            if (!station.Station_Data.AddOperatorToStation(employeeID))
            {
                Debug.Log($"Couldn't add employee to station: {station.StationID}");
                return false;
            }

            var actorCurrentJob = Actor_Manager.GetActor_Data(employeeID)?.CareerData?.CurrentJob;

            if (actorCurrentJob is null)
            {
                Debug.LogWarning($"Actor Current Job for employeeID: {employeeID} does is null.");
                return false;
            }
                
            actorCurrentJob.SetStationID(station.StationID);

            return true;
        }

        public bool RemoveEmployeeFromCurrentStation(uint employeeID)
        {
            try
            {
                var station =
                    Station_Manager.GetStation_Data(Actor_Manager.GetActor_Data(employeeID).CareerData.CurrentJob
                                                                 .StationID);

                if (!station.RemoveOperatorFromStation(employeeID))
                {
                    Debug.Log($"Couldn't remove employee from station: {station.StationID}");
                    return false;
                }

                Actor_Manager.GetActor_Data(employeeID)?.CareerData.CurrentJob.SetStationID(0);

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
        
        public List<Item> GetEstimatedProductionRatePerHour()
        {
            foreach (var station in AllStationsInJobSite.Values)
            {
                station.Station_Data.GetEstimatedProductionRatePerHour();
            }
        }
        
        public Station_Component GetNearestRelevantStationInJobsite(Vector3 position, List<StationName> stationNames)
            => AllStationsInJobSite
               .Where(station => stationNames.Contains(station.Value.StationName))
               .OrderBy(station => Vector3.Distance(position, station.Value.transform.position))
               .FirstOrDefault().Value;

        public void FillEmptyJobsitePositions()
        {
            var prosperityRatio      = ProsperityData.GetProsperityPercentage();
            var maxOperatorCount     = AllStationsInJobSite.Values.SelectMany(station => station.Station_Data.AllWorkPost_Components).ToList().Count;
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

                foreach (var station in AllStationsInJobSite.Values)
                {
                    if (iteration >= desiredOperatorCount - _allEmployees.Count)
                    {
                        break;
                    }

                    if (station.Station_Data.WorkPost_Workers.Count >= station.Station_Data.AllWorkPost_Components.Count)
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

            return new Data_Display(
                title: "JobSite Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
        }
    }
}