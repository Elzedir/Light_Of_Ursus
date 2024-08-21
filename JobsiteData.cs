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

    public string JobsiteDescription;

    public string OwnerName;
    Actor_Base _owner;

    public List<int> AllEmployees;
    public Dictionary<EmployeePosition, HashSet<int>> AllJobPositions = new();
    public Dictionary<int, int> EmployeeStationAllocation = new();

    public DisplayProsperity Prosperity;

    public List<StationData> AllStationData;

    public void InitialiseJobsiteData(int cityID)
    {
        CityID = cityID;

        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        jobsite.Initialise();

        // For some reason we are not getting any StationComponents returning

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (!AllStationData.Any(s => s.JobsiteID == station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationName} with ID: {station.StationData.StationID} was not in AllStationData");
                AllStationData.Add(station.StationData);
            }

            station.SetStationData(Manager_Station.GetStationData(JobsiteID, station.StationData.StationID));
        }

        for (int i = 0; i < AllStationData.Count; i++)
        {
            AllStationData[i].InitialiseStationData(JobsiteID);
        }

        Manager_Initialisation.OnInitialiseJobsites += _initialiseJobs;
    }

    void _initialiseJobs()
    {
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

        // Instead of using a career.none, use an appropriate career, and then use careerNone.

        Debug.Log(string.Join(", ", Manager_City.GetCity(CityID).CityData.Population.AllCitizens.Select(c => $"{c.CitizenID}: {c.CitizenName}")));

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var citizen = Manager_City.GetCity(CityID).CityData.Population.AllCitizens
            .FirstOrDefault(c => Manager_Actor.GetActorData(JobsiteFactionID, c.CitizenID, out ActorData actorData)?.CareerAndJobs.ActorCareer == CareerName.None
                && _hasMinimumVocationRequired(
                    c,
                    vocationAndExperience.Vocation,
                    vocationAndExperience.minimumExperienceRequired));

        // Eventually change from FirstOrDefault to either random selection or highest score, maybe depending on ruler's personality or succession.

        if (citizen != null)
        {
            Debug.Log($"Found citizen: {citizen.CitizenName} for position: {position}");

            return Manager_Actor.GetActor(JobsiteFactionID, citizen.CitizenID, out actor) != null;
        }

        return false;
    }

    protected Actor_Base _generateNewEmployee(EmployeePosition position)
    {
        CityComponent city = Manager_City.GetCity(CityID);

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var actor = Manager_Actor.SpawnActor(city.CitySpawnZone.transform.position);

        city.CityData.Population.AddCitizen(new Citizen(actor.ActorData.ActorID, actor.ActorData.ActorName.GetName(), actor.ActorData.ActorFactionID));
        AddEmployee(actor.ActorData.ActorID);

        return actor;
    }

    protected bool _hasMinimumVocationRequired(Citizen citizen, Vocation vocation, float minimumExperienceRequired)
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

    public void AddEmployee(int employeeID)
    {
        if (AllEmployees.Contains(employeeID))
        {
            Debug.Log($"EmployeeID: {employeeID} already exists in employee list.");
            return;
        }

        AllEmployees.Add(employeeID);
    }

    public void HireEmployee(int employeeID)
    {
        AddEmployee(employeeID);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(int employeeID)
    {
        if (!AllEmployees.Contains(employeeID))
        {
            Debug.Log($"EmployeeID: {employeeID} is not in employee list.");
            return;
        }

        AllEmployees.Remove(employeeID);

        // Remove employee job from employee job component.
    }

    public void FireEmployee(int employeeID)
    {
        RemoveEmployee(employeeID);

        // And then apply relation debuff.
    }

    public void AddEmployeeToJob(int employeeID, EmployeePosition employeePosition)
    {
        if (!AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"New position: {employeePosition} does not exist in AllJobPositions");
        if (AllJobPositions[employeePosition].Contains(employeeID)) throw new ArgumentException($"EmployeeID: {employeeID} already has position {employeePosition}");

        AllJobPositions[employeePosition].Add(employeeID);

    }

    public void RemoveEmployeeFromJob(int employeeID, EmployeePosition employeePosition)
    {
        if (!AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"New position: {employeePosition} does not exist in AllJobPositions");
        if (!AllJobPositions[employeePosition].Contains(employeeID)) throw new ArgumentException($"EmployeeID: {employeeID} does not have position {employeePosition}");

        AllJobPositions[employeePosition].Remove(employeeID);
    }

    public void AddJobToJobsite(EmployeePosition employeePosition, HashSet<int> employeeList)
    {
        if (AllJobPositions.ContainsKey(employeePosition)) throw new ArgumentException($"Position: {employeePosition} already exists in AllJobPositions");

        AllJobPositions.Add(employeePosition, new HashSet<int>(employeeList));
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
        .ForEach(position => AllJobPositions[position] = new HashSet<int>());
    }

    public void FillAllJobsitePositions()
    {
        foreach (var position in AllJobPositions.Where(position => position.Value.Count == 0).ToList())
        {
            if (!_findEmployeeFromCity(position.Key, out Actor_Base actor))
            {
                Debug.Log($"Couldnt find employee {actor}");
                actor = _generateNewEmployee(position.Key);
            }

            //var actor = !_findEmployeeFromCity(position.Key, out Actor_Base foundActor) ? _generateNewEmployee(position.Key) : foundActor;

            AllJobPositions[position.Key].Add(actor.ActorData.ActorID);
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