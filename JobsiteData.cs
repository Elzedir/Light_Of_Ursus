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

    public List<EmployeeData> AllEmployees;

    public DisplayProsperity Prosperity;

    public List<StationData> AllStationData;

    public void InitialiseJobsiteData(int cityID)
    {
        CityID = cityID;

        var jobsite = Manager_Jobsite.GetJobsite(JobsiteID);

        jobsite.Initialise();

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

        Manager_Initialisation.OnInitialiseJobsites += _initialiseJobs;
    }

    void _initialiseJobs()
    {
        SetOwner(null);
        FillEmptyJobsitePositions();
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

        foreach(var cit in Manager_City.GetCity(CityID).CityData.Population.AllCitizens)
        {
            Debug.Log($"City Citizens: {cit.ActorID}: {cit.ActorName.GetName()}");
        }

        var citizen = Manager_City.GetCity(CityID).CityData.Population.AllCitizens
            .FirstOrDefault(c => c.CareerAndJobs.JobsiteID == -1
                && _hasMinimumVocationRequired(c, _getVocationAndMinimumExperienceRequired(position)));

        // Eventually change from FirstOrDefault to either random selection or highest score, maybe depending on ruler's personality or succession.

        if (citizen != null)
        {
            Debug.Log($"Found citizen: {citizen.ActorID}: {citizen.ActorName.GetName()} for position: {position}");

            return Manager_Actor.GetActor(actorID: citizen.ActorID, out actor, factionID: JobsiteFactionID) != null;
        }

        return false;
    }

    protected Actor_Base _generateNewEmployee(EmployeePosition position)
    {
        CityComponent city = Manager_City.GetCity(CityID);

        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var actor = Manager_Actor.SpawnActor(city.CitySpawnZone.transform.position);

        city.CityData.Population.AddCitizen(actor.ActorData);
        AddEmployee(actor.ActorData);

        return actor;
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

    public void AddEmployee(ActorData employeeData)
    {
        if (AllEmployees.Any(e => e.ActorData.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} already exists in employee list.");
            return;
        }

        AllEmployees.Add(new EmployeeData(employeeData, null));
    }

    public void HireEmployee(ActorData employeeData)
    {
        AddEmployee(employeeData);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(ActorData employeeData)
    {
        if (!AllEmployees.Any(e => e.ActorData.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is not in employee list.");
            return;
        }

        AllEmployees.Remove(AllEmployees.FirstOrDefault(e => e.ActorData.ActorID == employeeData.ActorID));

        // Remove employee job from employee job component.
    }

    public void FireEmployee(ActorData employeeData)
    {
        RemoveEmployee(employeeData);

        // And then apply relation debuff.
    }

    public void AddEmployeeToStation(ActorData employeeData, int stationID)
    {
        var station = AllStationData.FirstOrDefault(s => s.StationID == stationID);
        if (station == null)
        {
            Debug.Log($"StationID: {stationID} does not exist in AllStationData");
            return;
        }

        if (station.CurrentOperators.Any(e => e.ActorData.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is already an operator at StationID: {stationID}");
            return;
        }

        station.AddOperatorToStation(employeeData);
    }

    public void RemoveEmployeeFromStation(ActorData employeeData, int stationID)
    {
        var station = AllStationData.FirstOrDefault(s => s.StationID == stationID);
        if (station == null)
        {
            Debug.Log($"StationID: {stationID} does not exist in AllStationData");
            return;
        }

        if (!station.CurrentOperators.Any(e => e.ActorData.ActorID == employeeData.ActorID))
        {
            Debug.Log($"EmployeeID: {employeeData} is not an operator at StationID: {stationID}");
            return;
        }

        station.RemoveOperatorFromStation(employeeData);
    }

    public List<OperatorData> GetAllOperators()
    {
        var allOperators = new List<OperatorData>();

        foreach (var station in AllStationData)
        {
            allOperators.AddRange(station.CurrentOperators);
        }

        return allOperators;
    }

    public void SetJobsiteIsActive(bool jobsiteIsActive)
    {
        JobsiteIsActive = jobsiteIsActive;
    }

    public void FillEmptyJobsitePositions()
    {
        AllStationData
            .SelectMany(station => Manager_Station.GetStation(station.StationID)?.AllowedEmployeePositions.Select(position => new { station, position }))
            .Where(sp => !sp.station.CurrentOperators.Any(e =>e.ActorData.CareerAndJobs.EmployeePosition == sp.position))
            .ToList()
            .ForEach(sp =>
            {
                if (!_findEmployeeFromCity(sp.position, out Actor_Base actor))
                {
                    Debug.Log($"Couldn't find employee for position: {sp.position}");
                    actor = _generateNewEmployee(sp.position);
                }

                actor.ActorData.CareerAndJobs.EmployeePosition = sp.position;
                AddEmployeeToStation(actor.ActorData, sp.station.StationID);
            });
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
    public ActorData ActorData;
    public StationComponent CurrentStation;

    public EmployeeData(ActorData actorData, StationComponent currentStation)
    {
        ActorData = actorData;
        CurrentStation = currentStation;
    }

    public void SetStation(StationComponent currentStation)
    {
        CurrentStation = currentStation;
    }

    public void RemoveStation()
    {
        CurrentStation = null;
    }
}

[CustomPropertyDrawer(typeof(EmployeeData))]
public class EmployeeData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var actorDataProp = property.FindPropertyRelative("ActorData");
        var actorIDProp = actorDataProp.FindPropertyRelative("ActorID");
        var actorNameProp = actorDataProp.FindPropertyRelative("ActorName");

        var nameProp = actorNameProp.FindPropertyRelative("Name");
        var name = "Nameless";

        if (nameProp != null)
        {
            var surnameProp = actorNameProp.FindPropertyRelative("Surname");

            if (surnameProp != null)
            {
                name = $"{nameProp.stringValue} {surnameProp.stringValue}";
            }
            else
            {
                name = $"{nameProp.stringValue}";
            }
        }

        label.text = $"{actorIDProp.intValue}: {name}";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}