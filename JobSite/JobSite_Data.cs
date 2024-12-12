using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using City;
using EmployeePosition;
using Initialisation;
using Priority;
using Recipe;
using Station;
using UnityEditor;
using UnityEngine;

namespace JobSite
{
    [Serializable]
    public class JobSite_Data
    {
        public uint   JobSiteID;
        
        JobSite_Component _jobSiteComponent;
        public JobSite_Component JobSite_Component => _jobSiteComponent ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        public uint   JobsiteFactionID;
        public uint   CityID;

        public bool   JobsiteIsActive = true;
        public void   SetJobsiteIsActive(bool jobsiteIsActive) => JobsiteIsActive = jobsiteIsActive;
        public string JobsiteDescription;
        public uint   OwnerID;

        public List<uint>                        AllEmployeeIDs;
        Dictionary<uint, Actor_Component>        _allEmployees;
        public Dictionary<uint, Actor_Component> AllEmployees => _allEmployees ??= _populateAllEmployees();

        Dictionary<uint, Actor_Component> _populateAllEmployees()
        {
            var allEmployees = new Dictionary<uint, Actor_Component>();

            foreach (var employeeID in AllEmployeeIDs)
            {
                var employee = Actor_Manager.GetActor_Component(employeeID);

                if (employee == null)
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
            var jobsite = JobSite_Manager.GetJobSite_Component(JobSiteID);

            foreach (var station in jobsite.AllStationsInJobSite.Values)
            {
                if (!AllStationIDs.Contains(station.StationData.StationID))
                {
                    //Debug.Log($"Station: {station.StationData.StationID}: {station.StationName}  was not in AllStationIDs");
                    AllStationIDs.Add(station.StationData.StationID);
                }
            }
            
            _initialiseJobsiteData();
        }

        void _initialiseJobsiteData()
        {
            CheckOwner();
            //AllocateEmployeesToStations();

            // Temporarily for now
            FillEmptyJobsitePositions();
        }

        public void CheckOwner()
        {
            if (OwnerID == 0) GetNewOwner();

            if (OwnerID == 0) return;
            
            var ownerData = Actor_Manager.GetActor_Data(OwnerID);
            var owner     = Actor_Manager.GetActor_Component(OwnerID);
            
            owner.ActorMaterial.material = Resources.Load<Material>("Materials/Material_Yellow");

            // And change all affected things, like perks, job settings, etc.
        }

        public void SetOwner(uint ownerID)
        {
            OwnerID = ownerID;
        }

        public uint GetNewOwner()
        {
            uint newOwnerID = _findEmployeeFromJobsite(EmployeePositionName.Owner);

            if (newOwnerID != 0)
            {
                return OwnerID = newOwnerID;
            }

            newOwnerID = _findEmployeeFromCity(EmployeePositionName.Owner);

            if (newOwnerID != 0)
            {
                return OwnerID = newOwnerID;
            }
            
            FillEmptyJobsitePositions();

            GetNewOwner();

            if (OwnerID != 0)
            {
                return OwnerID;
            }

            Debug.Log("Couldn't generate new owner.");
            return 0;
        }

        protected uint _findEmployeeFromJobsite(EmployeePositionName positionName)
        {
            if (AllEmployeeIDs is null || !AllEmployeeIDs.Any())
            {
                //Debug.Log("No employees found in the jobsite.");
                return 0;
            }

            var employeeID = AllEmployeeIDs.FirstOrDefault(); 
            // For now, just get the first. Later, use inheritance or the greatest combined skills or governor approval.
            
            if (employeeID is 0)
            {
                //Debug.LogWarning($"No suitable employee found for position: {position} in the jobsite.");
                return 0;
            }

            var actor = Actor_Manager.GetActor_Component(actorID: employeeID);
            
            return actor?.ActorData?.ActorID ?? 0;
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
                                          Actor_Manager.GetActor_Data(c)?.CareerData.JobsiteID == 0 &&
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

            city.CityData.Population.AddCitizen(actor.ActorData.ActorID);
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
            if (AllEmployeeIDs.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} already exists in employee list.");
                return;
            }

            AllEmployeeIDs.Add(employeeID);
            Actor_Manager.GetActor_Data(employeeID).CareerData.SetJobsiteID(JobSiteID);
        }

        public void HireEmployee(uint employeeID)
        {
            AddEmployeeToJobsite(employeeID);

            // And then apply relevant relation buff
        }

        public void RemoveEmployeeFromJobsite(uint employeeID)
        {
            if (!AllEmployeeIDs.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} is not in employee list.");
                return;
            }

            AllEmployeeIDs.Remove(employeeID);
            Actor_Manager.GetActor_Data(employeeID).CareerData.SetJobsiteID(0);

            // Remove employee job from employee job component.
        }

        public void FireEmployee(uint employeeID)
        {
            RemoveEmployeeFromJobsite(employeeID);

            // And then apply relation debuff.
        }

        public bool AddEmployeeToStation(uint employeeID, uint stationID)
        {
            RemoveEmployeeFromStation(employeeID);

            if (!AllStationIDs.Contains(stationID))
            {
                Debug.Log($"StationID: {stationID} does not exist in AllStationData");
                return false;
            }

            var station = Station_Manager.GetStation_Data(stationID);

            if (station.CurrentOperatorIDs.Contains(employeeID))
            {
                Debug.Log($"EmployeeID: {employeeID} is already an operator at StationID: {stationID}");
                return false;
            }

            if (!station.AddOperatorToStation(employeeID))
            {
                Debug.Log($"Couldn't add employee to station: {stationID}");
                return false;
            }

            Actor_Manager.GetActor_Data(employeeID)?.CareerData.CurrentJob.SetStationID(stationID);

            return true;
        }

        public bool RemoveEmployeeFromStation(uint employeeID)
        {
            var actorData = Actor_Manager.GetActor_Data(employeeID);
            
            if (actorData == null)
            {
                //Debug.Log($"ActorData for employeeID: {employeeID} does not exist.");
                return false;
            }
            
            var actorCareer = actorData.CareerData;

            if (actorCareer == null)
            {
                //Debug.Log($"Employee does not have a career.");
                return false;
            }
            
            if (actorCareer.CurrentJob == null)
            {
                //Debug.Log($"Employee does not have a current job.");
                return false;
            }
            
            var stationID = actorCareer.CurrentJob.StationID;

            if (!AllStationIDs.Contains(stationID))
            {
                //Debug.Log($"StationID: {stationID} does not exist in AllStationIDs");
                return false;
            }

            var station = Station_Manager.GetStation_Data(stationID);

            if (station == null)
            {
                //Debug.Log($"Station does not exist.");
                return false;
            }

            if (!station.CurrentOperatorIDs.Contains(employeeID))
            {
                //Debug.Log($"EmployeeID: {employeeID} is not an operator at StationID: {station.StationID}");
                return false;
            }

            if (!station.RemoveOperatorFromStation(employeeID))
            {
                //Debug.Log($"Couldn't remove employee from station: {station.StationID}");
                return false;
            }

            Actor_Manager.GetActor_Data(employeeID)?.CareerData.CurrentJob.SetStationID(0);

            return true;
        }

        public List<uint> GetAllOperators() => AllStationIDs.SelectMany(stationID => Station_Manager.GetStation_Data(stationID).CurrentOperatorIDs).ToList();

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

        public void FillEmptyJobsitePositions()
        {
            var prosperityRatio      = ProsperityData.GetProsperityPercentage();
            var maxOperatorCount     = AllStationIDs.SelectMany(stationID => Station_Manager.GetStation_Component(stationID).AllOperatingAreasInStation).ToList().Count;
            var currentOperatorCount = AllStationIDs.SelectMany(stationID => Station_Manager.GetStation_Data(stationID).CurrentOperatorIDs).ToList().Count;
            var desiredOperatorCount = Mathf.RoundToInt(maxOperatorCount * prosperityRatio);

            if (currentOperatorCount >= maxOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than MaxOperatorCount: {maxOperatorCount}.");
                return;
            }

            if (currentOperatorCount >= desiredOperatorCount)
            {
                //Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than DesiredOperatorCount: {desiredOperatorCount}.");
                return;
            }

            int iteration = 0;

            //Debug.Log($"Trying to hire {desiredOperatorCount - currentOperatorCount} employees, curr: {currentOperatorCount} des: {desiredOperatorCount} max: {maxOperatorCount}");

            while (iteration < desiredOperatorCount - currentOperatorCount)
            {
                bool allPositionsFilled = true;

                foreach (var stationID in AllStationIDs)
                {
                    if (iteration >= desiredOperatorCount - currentOperatorCount)
                    {
                        break;
                    }

                    var station = Station_Manager.GetStation_Component(stationID);

                    if (station == null)
                    {
                        //Debug.Log($"StationID: {stationID} does not exist in Manager_Station.");
                        continue;
                    }

                    if (Station_Manager.GetStation_Data(stationID).CurrentOperatorIDs.Count >= station.AllOperatingAreasInStation.Count)
                    {
                        //Debug.Log($"All operating areas are already filled for StationID: {stationID}");
                        continue;
                    }
                    else
                    {
                        allPositionsFilled = false;

                        var newEmployeeID = _findEmployeeFromJobsite(station.CoreEmployeePositionName);

                        if (newEmployeeID == 0)
                        {
                            //Debug.Log($"Couldn't find employee from Jobsite for position: {station.CoreEmployeePosition}");
                        }

                        newEmployeeID = _findEmployeeFromCity(station.CoreEmployeePositionName);

                        if (newEmployeeID == 0)
                        {
                            //Debug.Log($"Couldn't find employee from City for position: {station.CoreEmployeePosition}");
                            newEmployeeID = _generateNewEmployee(station.CoreEmployeePositionName);
                        }

                        var actorData = Actor_Manager.GetActor_Data(newEmployeeID);

                        if (!AddEmployeeToStation(actorData.ActorID, stationID))
                        {
                            //Debug.Log($"Couldn't add employee to station: {stationID}");
                            continue;
                        }

                        actorData.CareerData.SetEmployeePosition(station.CoreEmployeePositionName);

                        iteration++;
                    }
                }

                if (allPositionsFilled)
                {
                    //Debug.Log("All necessary positions are filled.");
                    break;
                }
            }
        }
    }
}