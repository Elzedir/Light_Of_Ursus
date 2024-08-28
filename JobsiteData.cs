using System;
using System.Collections.Generic;
using System.Linq;
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
    public int JobsiteID;
    public JobsiteName JobsiteName;
    public int JobsiteFactionID;
    public int CityID;

    public bool JobsiteIsActive = true;
    public void SetJobsiteIsActive(bool jobsiteIsActive) => JobsiteIsActive = jobsiteIsActive;
    public string JobsiteDescription;
    public int OwnerID;

    public List<ActorData> AllEmployees;

    public ProsperityData ProsperityData;

    public List<StationData> AllStationData;

    public void InitialiseJobsiteData(int cityID)
    {
        CityID = cityID;

        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        jobsite.Initialise();

        ProsperityData = new ProsperityData(jobsite.gameObject);

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (!AllStationData.Any(s => s.StationID == station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationName} with ID: {station.StationData.StationID} was not in AllStationData");
                AllStationData.Add(station.StationData);
            }

            station.SetStationData(Manager_Station.GetStationData(jobsiteID: JobsiteID, stationID: station.StationData.StationID));
        }

        for (int i = 0; i < AllStationData.Count; i++)
        {
            AllStationData[i].InitialiseStationData(JobsiteID);
        }

        Manager_Initialisation.OnInitialiseJobsiteDatas += _initialiseJobsiteData;
    }

    void _initialiseJobsiteData()
    {
        CheckOwner();
        //AllocateEmployeesToStations();
        FillEmptyJobsitePositions();
    }

    public void CheckOwner()
    {
        if (OwnerID == 0) GetNewOwner();

        // And change all affected things, like perks, job settings, etc.
    }

    public void SetOwner(int ownerID)
    {
        OwnerID = ownerID;
    }
    
    public int GetNewOwner()
    {
        if (_findEmployeeFromJobsite(EmployeePosition.Owner, out int newOwnerID))
        {
            OwnerID = newOwnerID;
        }

        if (_findEmployeeFromCity(EmployeePosition.Owner, out newOwnerID))
        {
            OwnerID = newOwnerID;
        }

        for (int i = 0; i < 3; i++)
        {
            if (OwnerID != 0)
            {
                break;
            }

            OwnerID = _generateNewEmployee(EmployeePosition.Owner);
        }

        if (OwnerID == 0) Debug.Log("Couldn't generate new owner.");

        return OwnerID;
    }

    protected bool _findEmployeeFromJobsite(EmployeePosition position, out int actorID)
    {
        actorID = 0;
        var employee = AllEmployees.FirstOrDefault(); // For now, just get the first. Later, use inheritance or the greatest combined skills or governer approval.

        if (employee == null)
        {
            return false;
        }

        Debug.Log($"Found employee: {employee.ActorID}: {employee.ActorName.GetName()} for position: {position}");

        var actor = Manager_Actor.GetActor(actorID: employee.ActorID, out _, factionID: JobsiteFactionID, generateActorIfNotFound: true);
        actorID = actor.ActorData.ActorID;
        return actor.ActorData.ActorID > 0;
    }

    protected bool _findEmployeeFromCity(EmployeePosition position, out int actorID)
    {
        actorID = 0;
        var citizen = Manager_City.GetCity(CityID).CityData.Population.AllCitizens
            .FirstOrDefault(c => c.CareerAndJobs.JobsiteID == -1
                && _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(position)));

        // Eventually change from FirstOrDefault to either random selection or highest score, maybe depending on ruler's personality or succession.

        if (citizen == null)
        {
            return false;
        }

        Debug.Log($"Found citizen: {citizen.ActorID}: {citizen.ActorName.GetName()} for position: {position}");

        var actor = Manager_Actor.GetActor(actorID: citizen.ActorID, out _, factionID: JobsiteFactionID, generateActorIfNotFound: true);
        actorID = actor.ActorData.ActorID;
        return actor.ActorData.ActorID > 0;
    }

    protected int _generateNewEmployee(EmployeePosition position)
    {
        CityComponent city = Manager_City.GetCity(CityID);

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position); // Will add to optional parameters

        var actor = Manager_Actor.SpawnActor(city.CitySpawnZone.transform.position);

        city.CityData.Population.AddCitizen(actor.ActorData);
        AddEmployeeToJobsite(actor.ActorData);

        return actor.ActorData.ActorID;
    }

    protected bool _hasMinimumVocationRequired(ActorData citizen, List<VocationRequirement> vocationRequirements)
    {
        foreach(var vocation in vocationRequirements)
        {
            if (vocation.VocationName == VocationName.None) continue;

            if (citizen.VocationData.GetVocationExperience(vocation.VocationName) < vocation.MinimumVocationExperience)
            {
                return false;
            }
        }

        return true;
    }

    protected List<VocationRequirement> _getVocationAndMinimumExperienceRequired(EmployeePosition position)
    {
        return new List<VocationRequirement> { new VocationRequirement(VocationName.None, 0) };
    }

    public void AddEmployeeToJobsite(ActorData employeeData)
    {
        if (AllEmployees.Any(e => e.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} already exists in employee list.");
            return;
        }

        AllEmployees.Add(employeeData);
        employeeData.CareerAndJobs.SetJobsiteID(JobsiteID);
    }

    public void HireEmployee(ActorData employeeData)
    {
        AddEmployeeToJobsite(employeeData);

        // And then apply relevant relation buff
    }

    public void RemoveEmployeeFromJobsite(ActorData employeeData)
    {
        if (!AllEmployees.Any(e => e.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is not in employee list.");
            return;
        }

        AllEmployees.Remove(AllEmployees.FirstOrDefault(e => e.ActorID == employeeData.ActorID));
        employeeData.CareerAndJobs.SetJobsiteID(-1);

        // Remove employee job from employee job component.
    }

    public void FireEmployee(ActorData employeeData)
    {
        RemoveEmployeeFromJobsite(employeeData);

        // And then apply relation debuff.
    }

    public bool AddEmployeeToStation(ActorData employeeData, int stationID)
    {
        RemoveEmployeeFromStation(employeeData);

        var station = AllStationData.FirstOrDefault(s => s.StationID == stationID);
        if (station == null)
        {
            Debug.Log($"StationID: {stationID} does not exist in AllStationData");
            return false;
        }

        if (station.CurrentOperators.Any(e => e.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is already an operator at StationID: {stationID}");
            return false;
        }

        if (!station.AddOperatorToStation(employeeData))
        {
            Debug.Log($"Couldn't add employee to station: {stationID}");
            return false;
        }

        AllEmployees.FirstOrDefault(e => e.ActorID == employeeData.ActorID)?.CareerAndJobs.SetStationID(stationID);

        return true;
    }

    public bool RemoveEmployeeFromStation(ActorData employeeData)
    {
        var stationID = AllEmployees.FirstOrDefault(e => e.ActorID == employeeData.ActorID)?.CareerAndJobs.StationID;

        if (stationID == null)
        {
            Debug.Log($"Employee has not been assigned a station.");
            return false;
        }

        var station = AllStationData.FirstOrDefault(s => s.StationID == stationID);

        if (station == null)
        {
            Debug.Log($"Employee has not been assigned a station.");
            return false;
        }

        if (!station.CurrentOperators.Any(e => e.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is not an operator at StationID: {station.StationID}");
            return false;
        }

        if (!station.RemoveOperatorFromStation(employeeData))
        {
            Debug.Log($"Couldn't remove employee from station: {station.StationID}");
            return false;
        }

        AllEmployees.FirstOrDefault(e => e.ActorID == employeeData.ActorID)?.CareerAndJobs.SetStationID(-1);

        return true;
    }

    public List<ActorData> GetAllOperators()
    {
        var allOperators = new List<ActorData>();

        foreach (var station in AllStationData)
        {
            allOperators.AddRange(station.CurrentOperators);
        }

        return allOperators;
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

    public void FillEmptyJobsitePositions()
    {
        var prosperityRatio = ProsperityData.GetProsperityPercentage();
        var maxOperatorCount = AllStationData.SelectMany(station => Manager_Station.GetStation(station.StationID).AllOperatingAreasInStation).ToList().Count;
        var currentOperatorCount = AllStationData.SelectMany(station => station.CurrentOperators).ToList().Count;
        var desiredOperatorCount = Mathf.RoundToInt(maxOperatorCount * prosperityRatio);

        if (currentOperatorCount >= maxOperatorCount)
        {
            Debug.Log("All jobsite positions are already filled.");
            return;
        }

        if (currentOperatorCount >= desiredOperatorCount)
        {
            Debug.Log($"CurrentOperatorCount {currentOperatorCount} is higher than DesiredOperatorCount: {desiredOperatorCount}.");
            return;
        }

        int iteration = 0;

        Debug.Log($"Trying to hire {desiredOperatorCount - currentOperatorCount} employees, curr: {currentOperatorCount} des: {desiredOperatorCount} max: {maxOperatorCount}");

        while (iteration < desiredOperatorCount - currentOperatorCount)
        {
            bool allPositionsFilled = true;

            foreach (var stationData in AllStationData)
            {
                if (iteration >= desiredOperatorCount - currentOperatorCount)
                {
                    break;
                }

                var station = Manager_Station.GetStation(stationData.StationID);

                if (station == null)
                {
                    Debug.Log($"StationID: {stationData.StationID} does not exist in Manager_Station.");
                    continue;
                }

                if (stationData.CurrentOperators.Count >= station.AllOperatingAreasInStation.Count)
                {
                    Debug.Log($"All operating areas are already filled for StationID: {stationData.StationID}");
                    continue;
                }
                else
                {
                    allPositionsFilled = false;

                    if (!_findEmployeeFromJobsite(station.NecessaryEmployeePosition, out int actorID))
                    {
                        Debug.Log($"Couldn't find employee from Jobsite for position: {station.NecessaryEmployeePosition}");
                    }

                    if (!_findEmployeeFromCity(station.NecessaryEmployeePosition, out actorID))
                    {
                        Debug.Log($"Couldn't find employee from City for position: {station.NecessaryEmployeePosition}");
                        actorID = _generateNewEmployee(station.NecessaryEmployeePosition);
                    }

                    var actorData = Manager_Actor.GetActor(actorID: actorID, out _, factionID: JobsiteFactionID).ActorData;

                    if (!AddEmployeeToStation(actorData, stationData.StationID))
                    {
                        Debug.Log($"Couldn't add employee to station: {stationData.StationID}");
                        continue;
                    }

                    actorData.CareerAndJobs.EmployeePosition = station.NecessaryEmployeePosition;

                    iteration++;
                }
            }

            if (allPositionsFilled)
            {
                Debug.Log("All necessary positions are filled.");
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