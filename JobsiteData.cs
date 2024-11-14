using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Managers;
using UnityEditor;
using UnityEngine;

public enum JobsiteName
{
    None,

    Lumber_Yard,
    Smithy
}

[Serializable]
public class JobsiteData
{
    public uint JobsiteID;
    public JobsiteName JobsiteName;
    public uint JobsiteFactionID;
    public uint CityID;

    public bool JobsiteIsActive = true;
    public void SetJobsiteIsActive(bool jobsiteIsActive) => JobsiteIsActive = jobsiteIsActive;
    public string JobsiteDescription;
    public uint OwnerID;

    public List<uint> AllEmployeeIDs;
    Dictionary<uint, ActorComponent> _allEmployees;
    public Dictionary<uint, ActorComponent> AllEmployees { get => _allEmployees ??= _populateAllEmployees(); }
    Dictionary<uint, ActorComponent> _populateAllEmployees()
    {
        var allEmployees = new Dictionary<uint, ActorComponent>();

        foreach (var employeeID in AllEmployeeIDs)
        {
            var employee = Manager_Actor.GetActor(employeeID);

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

    public List<uint> AllStationIDs;
    public Dictionary<(uint ActorID, uint OrderID), Order_Base> AllOrders = new();

    // Work out how to do quotas and set production rate

    public void InitialiseJobsiteData()
    {
        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        jobsite.Initialise();

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (!AllStationIDs.Contains(station.StationData.StationID))
            {
                //Debug.Log($"Station: {station.StationData.StationID}: {station.StationName}  was not in AllStationIDs");
                AllStationIDs.Add(station.StationData.StationID);
            }
        }

        Manager_Initialisation.OnInitialiseJobsiteDatas += _initialiseJobsiteData;
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

        if (OwnerID != 0) 
        {
            var ownerData = Manager_Actor.GetActorData(OwnerID);
            var owner = Manager_Actor.GetActor(OwnerID);
            
            owner.ActorMaterial.material = Resources.Load<Material>("Materials/Material_Yellow");
        }

        // And change all affected things, like perks, job settings, etc.
    }

    public void SetOwner(uint ownerID)
    {
        OwnerID = ownerID;
    }

    public uint GetNewOwner()
    {
        uint newOwnerID = _findEmployeeFromJobsite(EmployeePosition.Owner);

        if (newOwnerID != 0)
        {
            return OwnerID = newOwnerID;
        }

        newOwnerID = _findEmployeeFromCity(EmployeePosition.Owner);

        if (newOwnerID != 0)
        {
            return OwnerID = newOwnerID;
        }

        for (int i = 0; i < 3; i++)
        {
            if (OwnerID != 0)
            {
                break;
            }

            OwnerID = _generateNewEmployee(EmployeePosition.Owner);
        }

        if (OwnerID != 0)
        {
            return OwnerID;
        }

        Debug.Log("Couldn't generate new owner.");
        return 0;
    }

    protected uint _findEmployeeFromJobsite(EmployeePosition position)
    {
        if (AllEmployeeIDs == null || !AllEmployeeIDs.Any())
        {
            //Debug.Log("No employees found in the jobsite.");
            return 0;
        }

        uint employeeID = AllEmployeeIDs.FirstOrDefault(); // For now, just get the first. Later, use inheritance or the greatest combined skills or governer approval.
        if (employeeID == 0)
        {
            //Debug.LogWarning($"No suitable employee found for position: {position} in the jobsite.");
            return 0;
        }

        var actor = Manager_Actor.GetActor(actorID: employeeID, generateActorIfNotFound: true);
        if (actor == null || actor.ActorData == null)
        {
            //Debug.Log($"Failed to get or generate actor for employee ID {employeeID}.");
            return 0;
        }

        //Debug.Log($"Found employee: {employeeID} for position: {position}");
        return actor.ActorData.ActorID;
    }

    protected uint _findEmployeeFromCity(EmployeePosition position)
    {
        var city = Manager_City.GetCity(CityID);
        if (city == null)
        {
            //Debug.Log($"City with ID {CityID} not found.");
            return 0;
        }

        var cityData = city.CityData;
        if (cityData == null)
        {
            //Debug.Log($"CityData for city with ID {CityID} is null.");
            return 0;
        }

        var population = cityData.Population;
        if (population == null)
        {
            //Debug.Log($"Population data for city with ID {CityID} is null.");
            return 0;
        }

        if (population.AllCitizenIDs == null || !population.AllCitizenIDs.Any())
        {
            //Debug.Log("No citizens found in the city.");
            return 0;
        }

        var citizenID = population.AllCitizenIDs
            .FirstOrDefault(c =>
            Manager_Actor.GetActorData(c)?.CareerAndJobs.JobsiteID == -1 &&
                _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(position))
            );

        if (citizenID == 0)
        {
            //Debug.LogWarning($"No suitable citizen found for position: {position} in city with ID {CityID}.");
            return 0;
        }

        var actor = Manager_Actor.GetActor(actorID: citizenID, generateActorIfNotFound: true);
        if (actor == null || actor.ActorData == null)
        {
            //Debug.Log($"Failed to get or generate actor for citizen ID {citizenID}.");
            return 0;
        }

        //Debug.Log($"Found citizen: {citizenID} for position: {position}");
        return actor.ActorData.ActorID;
    }

    protected uint _generateNewEmployee(EmployeePosition position)
    {
        CityComponent city = Manager_City.GetCity(CityID);

        var employeeMaster = AllEmployeeTypesManager.AllEmployeeTypes_SO.GetEmployeeType(position);

        if (employeeMaster == null) throw new Exception($"EmployeeMaster for position: {position} is null.");

        var actorGenerationParameters = employeeMaster.ActorGenerationParameters;

        if (actorGenerationParameters == null) throw new Exception($"ActorGenerationParameters for position: {position} is null.");

        var actor = Manager_Actor.SpawnNewActor(city.CitySpawnZone.transform.position, actorGenerationParameters);

        city.CityData.Population.AddCitizen(actor.ActorData.ActorID);
        AddEmployeeToJobsite(actor.ActorData.ActorID);

        return actor.ActorData.ActorID;
    }

    protected bool _hasMinimumVocationRequired(uint citizenID, List<VocationRequirement> vocationRequirements)
    {
        var actorData = Manager_Actor.GetActorData(citizenID);

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

    protected List<VocationRequirement> _getVocationAndMinimumExperienceRequired(EmployeePosition position)
    {
        var vocationRequirements = new List<VocationRequirement>();

        switch(position)
        {
            case EmployeePosition.Owner:
                vocationRequirements = new List<VocationRequirement>();
                break;
            case EmployeePosition.Logger:
                vocationRequirements = new List<VocationRequirement>
                {
                    new VocationRequirement(VocationName.Logging, 1000)
                };
                break;
            case EmployeePosition.Sawyer:
                vocationRequirements = new List<VocationRequirement>
                {
                    new VocationRequirement(VocationName.Sawying, 1000)
                };
                break;
            case EmployeePosition.Hauler:
                vocationRequirements = new List<VocationRequirement>();
                break;
            default:
                Debug.Log($"EmployeePosition: {position} not recognised.");
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
        Manager_Actor.GetActorData(employeeID).CareerAndJobs.SetJobsiteID(JobsiteID);
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
        Manager_Actor.GetActorData(employeeID).CareerAndJobs.SetJobsiteID(0);

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

        var station = Manager_Station.GetStationData(stationID);

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

        Manager_Actor.GetActorData(employeeID)?.CareerAndJobs.SetStationID(stationID);

        return true;
    }

    public bool RemoveEmployeeFromStation(uint employeeID)
    {
        var stationID = Manager_Actor.GetActorData(employeeID)?.CareerAndJobs.StationID;

        if (stationID == null)
        {
            //Debug.Log($"Employee has not been assigned a station.");
            return false;
        }

        if (!AllStationIDs.Contains(stationID.Value))
        {
            //Debug.Log($"StationID: {stationID} does not exist in AllStationIDs");
            return false;
        }

        var station = Manager_Station.GetStationData(stationID.Value);

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

        Manager_Actor.GetActorData(employeeID)?.CareerAndJobs.SetStationID(0);

        return true;
    }

    public List<uint> GetAllOperators() => AllStationIDs.SelectMany(stationID => Manager_Station.GetStationData(stationID).CurrentOperatorIDs).ToList();

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
        var prosperityRatio = ProsperityData.GetProsperityPercentage();
        var maxOperatorCount = AllStationIDs.SelectMany(stationID => Manager_Station.GetStation(stationID).AllOperatingAreasInStation).ToList().Count;
        var currentOperatorCount = AllStationIDs.SelectMany(stationID => Manager_Station.GetStationData(stationID).CurrentOperatorIDs).ToList().Count;
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

                var station = Manager_Station.GetStation(stationID);

                if (station == null)
                {
                    //Debug.Log($"StationID: {stationID} does not exist in Manager_Station.");
                    continue;
                }

                if (Manager_Station.GetStationData(stationID).CurrentOperatorIDs.Count >= station.AllOperatingAreasInStation.Count)
                {
                    //Debug.Log($"All operating areas are already filled for StationID: {stationID}");
                    continue;
                }
                else
                {
                    allPositionsFilled = false;

                    var newEmployeeID = _findEmployeeFromJobsite(station.CoreEmployeePosition);

                    if (newEmployeeID == 0)
                    {
                        //Debug.Log($"Couldn't find employee from Jobsite for position: {station.CoreEmployeePosition}");
                    }

                    newEmployeeID = _findEmployeeFromCity(station.CoreEmployeePosition);

                    if (newEmployeeID == 0)
                    {
                        //Debug.Log($"Couldn't find employee from City for position: {station.CoreEmployeePosition}");
                        newEmployeeID = _generateNewEmployee(station.CoreEmployeePosition);
                    }

                    var actorData = Manager_Actor.GetActorData(newEmployeeID);

                    if (!AddEmployeeToStation(actorData.ActorID, stationID))
                    {
                        //Debug.Log($"Couldn't add employee to station: {stationID}");
                        continue;
                    }

                    actorData.CareerAndJobs.SetEmployeePosition(station.CoreEmployeePosition);

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

[CustomPropertyDrawer(typeof(JobsiteData))]
public class JobsiteData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var jobsiteNameProp = property.FindPropertyRelative("JobsiteName");
        string jobsiteName = ((JobsiteName)jobsiteNameProp.enumValueIndex).ToString();

        label.text = !string.IsNullOrEmpty(jobsiteName) ? jobsiteName : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}