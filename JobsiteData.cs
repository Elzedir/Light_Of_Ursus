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

    public Actor_Base Owner;

    public Dictionary<Actor_Base, List<EmployeePosition>> AllEmployees;
    public Dictionary<EmployeePosition, List<Actor_Base>> AllJobPositions;

    public DisplayPopulation Population;
    public DisplayProsperity Prosperity;

    public List<StationData> AllStationData;

    public bool OverwriteDataInJobsiteFromEditor = false;

    public void InitialiseJobsiteData(int cityID)
    {
        CityID = cityID;

        var jobsite = Manager_Jobsites.GetJobsite(JobsiteID);

        jobsite.Initialise();

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (!AllStationData.Any(s => s.JobsiteID == station.StationData.StationID))
            {
                Debug.Log($"Station: {station.StationData.StationID} with ID: {station.StationData.StationID} was not in AllStationData");
                AllStationData.Add(station.StationData);
            }

            station.SetStationData(Manager_Station.GetStationDataFromID(JobsiteID, station.StationData.StationID));
        }

        for (int i = 0; i < AllStationData.Count; i++)
        {
            AllStationData[i].InitialiseStationData(JobsiteID);
        }
    }

    public void SetOwner(Actor_Base owner)
    {
        Owner = owner;

        if (Owner == null)
        {
            GetNewOwner();
        }

        // And change all affected things, like perks, job settings, etc.
    }
    
    public void GetNewOwner()
    {
        if (Owner != null) throw new ArgumentException($"Already has owner: {Owner.ActorData.ActorID} - {Owner.ActorData.ActorName} ");

        if (_findEmployeeFromCity(EmployeePosition.Owner, out Actor_Base newOwner))
        {
            Debug.Log("Found owner");
            Owner = newOwner;
        }

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"{Owner}");

            if (Owner != null)
            {
                Debug.Log($"Returned");
                return;
            }

            Owner = _generateNewEmployee(EmployeePosition.Owner);
        }

        Debug.Log("Couldn't generate new owner.");
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

        // Eventually change from FirstOrDefault to either random selection or highest score, maybe depending on ruler.

        if (citizen != null)
        {
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

        if (AllEmployees.ContainsKey(employee))
        {
            if (AllEmployees[employee].Contains(position)) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} already exists in employee list at same position.");
            else
            {
                AllEmployees[employee].Add(position);
                return;
            }
        }

        AllEmployees.Add(employee, new List<EmployeePosition> { position });
    }

    public void HireEmployee(Actor_Base employee, EmployeePosition position)
    {
        AddEmployee(employee, position);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(Actor_Base employee)
    {
        if (employee == null) throw new ArgumentException($"Employee is null.");

        if (!AllEmployees.ContainsKey(employee)) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} is not in employee list.");

        AllEmployees.Remove(employee);

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

    public void AllocateJobPositions()
    {
        //foreach (var position in AllStationData.SelectMany(station => station.EmployeePositions).Where(position => !AllJobPositions.ContainsKey(position)).Distinct())
        //{
        //    AllJobPositions[position] = new();
        //}
    }

    public void FillJobPositions()
    {
        foreach (var position in AllJobPositions.Where(position => position.Value.Count == 0).ToList())
        {
            if (!_findEmployeeFromCity(position.Key, out Actor_Base actor))
            {
                Debug.Log($"Actor {actor} is null and therefore new employee generated");
                actor = _generateNewEmployee(position.Key);
            }

            if (!AllJobPositions.ContainsKey(position.Key))
            {
                AllJobPositions[position.Key] = new List<Actor_Base>();
            }

            AllJobPositions[position.Key].Add(actor);
            actor.ActorData.CareerAndJobs.AddJob(JobName.Lumberjack, Manager_Jobsites.GetJobsite(JobsiteID));
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