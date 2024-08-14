using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum JobsiteName
{
    None,

    Logging_Yard,
    Smithy
}

[Serializable]
public class JobsiteData
{
    public int JobsiteID;
    public JobsiteName JobsiteName;
    public int CityID;

    public bool JobsiteIsActive = true;

    public string JobsiteDescription;

    public string OwnerName;
    Actor_Base _owner;

    public List<EmployeeData> AllEmployees;
    public Dictionary<EmployeePosition, List<Actor_Base>> AllJobPositions = new();

    public DisplayProsperity Prosperity;

    public List<StationData> AllStationData;

    public bool OverwriteDataInJobsiteFromEditor = false;

    public void InitialiseJobsiteData(int cityID)
    {
        CityID = cityID;

        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        jobsite.Initialise();

        // For some reason we are not getting any StationComponents returning

        foreach (var station in jobsite.AllCraftingStationsInJobsite)
        {
            if (!AllStationData.Any(s => s.JobsiteID == station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationID} with ID: {station.StationData.StationID} was not in AllStationData");
                AllStationData.Add(station.StationData);
            }

            station.SetStationData(Manager_Station.GetStationData(JobsiteID, station.StationData.StationID));
        }

        foreach (var station in jobsite.AllResourceStationsInJobsite)
        {
            if (!AllStationData.Any(s => s.JobsiteID == station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationID} with ID: {station.StationData.StationID} was not in AllStationData");
                AllStationData.Add(station.StationData);
            }

            station.SetStationData(Manager_Station.GetStationData(JobsiteID, station.StationData.StationID));
        }

        for (int i = 0; i < AllStationData.Count; i++)
        {
            AllStationData[i].InitialiseStationData(JobsiteID);
        }

        SetOwner(null);
        GetAllJobsitePositions();
        FillAllJobsitePositions();
    }

    public void SetOwner(Actor_Base owner)
    {
        _owner = owner ?? GetNewOwner();

        OwnerName = _owner.ActorData.ActorName.GetName();

        // And change all affected things, like perks, job settings, etc.
    }
    
    public Actor_Base GetNewOwner()
    {
        if (_owner != null) throw new ArgumentException($"Already has owner: {_owner.ActorData.ActorID} - {_owner.ActorData.ActorName} ");

        if (_findEmployeeFromCity(EmployeePosition.Owner, out Actor_Base newOwner))
        {
            _owner = newOwner;
        }

        for (int i = 0; i < 3; i++)
        {
            if (_owner != null)
            {
                break;
            }

            _owner = _generateNewEmployee(EmployeePosition.Owner);
        }

        if (_owner == null) Debug.Log("Couldn't generate new owner.");

        return _owner;
    }

    protected bool _findEmployeeFromCity(EmployeePosition position, out Actor_Base actor)
    {
        actor = null;

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var citizen = Manager_City.GetCity(CityID).CityData.Population.AllCitizens
            .FirstOrDefault(c => c.ActorData.CareerAndJobs.ActorCareer == CareerName.None
                && _hasMinimumVocationRequired(
                    c.ActorData,
                    vocationAndExperience.Vocation,
                    vocationAndExperience.minimumExperienceRequired));

        // Eventually change from FirstOrDefault to either random selection or highest score, maybe depending on ruler's personality or succession.

        if (citizen != null)
        {
            Debug.Log($"Found citizen: {citizen.CitizenName} for position: {position}");

            return Manager_Actor.GetActor(citizen.ActorID, out actor) != null;
        }

        return false;
    }

    protected Actor_Base _generateNewEmployee(EmployeePosition position)
    {
        CityComponent city = Manager_City.GetCity(CityID);

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var actor = Manager_Actor.SpawnActor(city.CitySpawnZone.transform.position);

        city.CityData.Population.AddCitizen(new DisplayCitizen(actorData: actor.ActorData));
        AddEmployee(actor, position);

        return actor;
    }

    protected bool _hasMinimumVocationRequired(ActorData actorData, Vocation vocation, float minimumExperienceRequired)
    {
        // for now

        return true;

        //if (actorData.VocationComponent.Vocations[vocation] < minimumExperienceRequired)
        //{
        //    return false;
        //}

        return true;
    }

    protected (Vocation Vocation, float minimumExperienceRequired) _getVocationAndMinimumExperienceRequired(EmployeePosition position)
    {
        return (null, 0);
    }

    public void AddEmployee(Actor_Base employee, EmployeePosition position)
    {
        if (employee == null) throw new ArgumentException($"Employee: {employee} or employee position {position} is null.");

        if (AllEmployees.Any(e => e.ActorID == employee.ActorData.ActorID))
        {
            if (AllEmployees.FirstOrDefault(e => e.ActorID == employee.ActorData.ActorID).EmployeePositions.Contains(position)) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} already exists in employee list at same position.");
            else
            {
                AllEmployees.FirstOrDefault(e => e.ActorID == employee.ActorData.ActorID).EmployeePositions.Add(position);
                return;
            }
        }

        AllEmployees.Add(new EmployeeData(employee.ActorData.ActorID, employee.ActorData.ActorName.GetName(), new List<EmployeePosition> { position }));
    }

    public void HireEmployee(Actor_Base employee, EmployeePosition position)
    {
        AddEmployee(employee, position);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(Actor_Base employee)
    {
        if (employee == null) throw new ArgumentException($"Employee is null.");

        if (!AllEmployees.Any(e => e.ActorID == employee.ActorData.ActorID)) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} is not in employee list.");

        AllEmployees.Remove(AllEmployees.FirstOrDefault(e => e.ActorID == employee.ActorData.ActorID));

        // Remove employee job from employee job component.
    }

    public void FireEmployee(Actor_Base employee)
    {
        RemoveEmployee(employee);

        // And then apply relation debuff.
    }

    public void AddEmployeeToJob(Actor_Base actor, EmployeePosition employeePosition)
    {
        if (!AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"New position: {employeePosition} does not exist in AllJobPositions");
        if (AllJobPositions[employeePosition].Contains(actor)) throw new ArgumentException($"Emplyee {actor.name} already has position {employeePosition}");

        AllJobPositions[employeePosition].Add(actor);

    }

    public void RemoveEmployeeFromJob(Actor_Base actor, EmployeePosition employeePosition)
    {
        if (!AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"New position: {employeePosition} does not exist in AllJobPositions");
        if (!AllJobPositions[employeePosition].Contains(actor)) throw new ArgumentException($"Emplyee {actor.name} does not have position {employeePosition}");

        AllJobPositions[employeePosition].Remove(actor);
    }

    public void AddJobToJobsite(EmployeePosition employeePosition, List<Actor_Base> employeeList)
    {
        if (AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"Position: {employeePosition} already exists in AllJobPositions");

        AllJobPositions.Add(employeePosition, new List<Actor_Base>(employeeList));
    }

    public void RemoveJobFromJobsite(EmployeePosition employeePosition)
    {
        if (!AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"Position: {employeePosition} does not exist in AllJobPositions");

        AllJobPositions.Remove(employeePosition);
    }

    public void SetJobsiteIsActive(bool jobsiteIsActive)
    {
        JobsiteIsActive = jobsiteIsActive;
    }

    public void GetAllJobsitePositions()
    {
        AllStationData
        .SelectMany(station => Manager_Station.GetStation(station.StationID)?.AllowedEmployeePositions ?? new List<EmployeePosition> { EmployeePosition.None })
        .Distinct()
        .Where(position => !AllJobPositions.ContainsKey(position))
        .ToList()
        .ForEach(position => AllJobPositions[position] = new List<Actor_Base>());
    }

    public void FillAllJobsitePositions()
    {
        foreach (var position in AllJobPositions.Where(position => position.Value.Count == 0).ToList())
        {
            var actor = !_findEmployeeFromCity(position.Key, out Actor_Base foundActor) ? _generateNewEmployee(position.Key) : foundActor;

            AllJobPositions[position.Key].Add(actor);
            actor.ActorData.CareerAndJobs.AddJob(JobName.Lumberjack, JobsiteID);
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

[Serializable]
public class EmployeeData
{
    public int ActorID;
    public string EmployeeName;
    public List<EmployeePosition> EmployeePositions;

    public EmployeeData(int actorID, string employeeName, List<EmployeePosition> employeePositions)
    {
        ActorID = actorID;
        EmployeeName = employeeName;
        EmployeePositions = employeePositions;
    }
}

[CustomPropertyDrawer(typeof(EmployeeData))]
public class EmployeeData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var citizenName = property.FindPropertyRelative("EmployeeName");
        label.text = !string.IsNullOrEmpty(citizenName.stringValue) ? citizenName.stringValue : "Unnamed Employee";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}