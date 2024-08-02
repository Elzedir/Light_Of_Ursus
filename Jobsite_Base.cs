using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EmployeePosition
{
    None,

    Owner,

    Shopkeeper,

    Chief_Lumberjack,
    Logger,
    Assistant_Logger,

    Sawyer,
    Assistant_Sawyer,

    Assistant_Smith,
}

public class Jobsite_Base : MonoBehaviour
{
    public Actor_Base Owner;
    public CityComponent City;
    
    public bool IsActive = true;

    public Dictionary<Actor_Base, EmployeePosition> AllEmployees;
    public Dictionary<EmployeePosition, List<Actor_Base>> AllJobPositions;
    public BoxCollider JobsiteArea;

    void Awake()
    {
        Manager_Initialisation.OnInitialiseJobsites += Initialise;
    }

    public virtual void Initialise()
    {
        JobsiteArea = GetComponent<BoxCollider>();
        AllEmployees = new();
        AllJobPositions = new();

        StartCoroutine(TestInitialiseCity());
    }

    public void UnpackJobsiteData()
    {
        //Manager_Jobsites.GetJobsiteData(City);
    }

    protected virtual IEnumerator TestInitialiseCity()
    {
        yield return new WaitForSeconds(0.5f);
        City = Manager_Region.GetNearestCity(transform.position);

        UnpackJobsiteData();

        SetOwner(Owner);
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
        if (Owner != null) throw new ArgumentException($"Already has owner: {Owner.ActorData.BasicIdentification.ActorID} - {Owner.ActorData.BasicIdentification.ActorName} ");
        
        for (int i = 0; i < City.CityData.Population.AllCitizens.Count; i++)
        {
            // For now

            City.CityData.Population.AllCitizens.Clear();
        }

        if (_findEmployeeFromCity(EmployeePosition.Owner, out Actor_Base newOwner))
        {
            Owner = newOwner;
        }

        for (int i = 0; i < 3; i++)
        {
            if (Owner != null)
            {
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

        var citizen = City.CityData.Population.AllCitizens
            .FirstOrDefault(c => Manager_Actor.GetActor(c.CitizenActorID).ActorData != null &&
                Manager_Actor.GetActor(c.CitizenActorID).ActorData.AttributesCareerAndPersonality.ActorCareer == CareerName.None
                && _hasMinimumVocationRequired(
                    Manager_Actor.GetActor(c.CitizenActorID),
                    vocationAndExperience.Vocation,
                    vocationAndExperience.minimumExperienceRequired));

        if (citizen != null)
        {
            actor =  Manager_Actor.GetActor(citizen.CitizenActorID);
            return true;
        }

        return false;
    }

    protected Actor_Base _generateNewEmployee(EmployeePosition position)
    {
        var vocationAndExperience = _getVocationAndMinimumExperienceRequired(position);

        var actor = Manager_Actor.InitialiseNewActorOnGO(City.CityEntranceSpawnZone.transform.position);
        Manager_Actor.GenerateNewActorData(actor);

        actor.transform.parent.name = $"{actor.ActorData.BasicIdentification.ActorName.Name}Body";
        actor.transform.name = $"{actor.ActorData.BasicIdentification.ActorName.Name}";

        City.CityData.Population.AddCitizen(new DisplayCitizen(
            citizenActorID: actor.ActorData.BasicIdentification.ActorID, 
            citizenName: actor.ActorData.BasicIdentification.ActorName.GetName()
            ));

        return actor;
    }

    protected bool _hasMinimumVocationRequired(Actor_Base actor, Vocation vocation, float minimumExperienceRequired)
    {
        // for now

        return true;

        if (actor.VocationComponent.Vocations[vocation] < minimumExperienceRequired)
        {
            return false;
        }

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
            if (AllEmployees[employee] == position) throw new ArgumentException($"Employee: {employee.ActorData.BasicIdentification.ActorName} already exists in employee list at same position.");
            else
            {
                AllEmployees[employee] = position;
                return;
            }
        }

        AllEmployees.Add(employee, position);
    }

    public void HireEmployee(Actor_Base employee, EmployeePosition position)
    {
        AddEmployee(employee, position);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(Actor_Base employee)
    {
        if (employee == null) throw new ArgumentException($"Employee is null.");

        if (!AllEmployees.ContainsKey(employee)) throw new ArgumentException($"Employee: {employee.ActorData.BasicIdentification.ActorName} is not in employee list.");

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

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
