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
    Lumberjack,
    Assistant_Lumberjack,

    Assistant_Smith,
}

public class Jobsite_Base : MonoBehaviour
{
    public Actor_Base Owner;
    public CityComponent City;

    public bool IsActive = true;

    public Dictionary<Actor_Base, EmployeePosition> EmployeeList = new();
    public BoxCollider JobsiteArea;

    public virtual void Initialise(Actor_Base owner, Dictionary<Actor_Base, EmployeePosition> employeeList)
    {
        JobsiteArea = GetComponent<BoxCollider>();
        SetOwner(owner);
        EmployeeList = employeeList;

        StartCoroutine(TestInitialiseCity());
    }

    IEnumerator TestInitialiseCity()
    {
        yield return new WaitForSeconds(0.5f);
        City = Manager_Region.GetNearestCity(transform.position);
    }

    public void SetOwner(Actor_Base owner)
    {
        Owner = owner;

        if (Owner == null)
        {
            StartCoroutine(GetNewOwnerAfterOneSecond());
        }

        // And change all affected things, like perks, job settings, etc.
    }

    public IEnumerator GetNewOwnerAfterOneSecond()
    {
        yield return new WaitForSeconds(2);
        GetNewOwner();
    }

    public void GetNewOwner()
    {
        if (Owner != null) throw new ArgumentException($"Already has owner: {Owner.ActorData.BasicIdentification.ActorID} - {Owner.ActorData.BasicIdentification.ActorName} ");

        var newOwner = _findEmployeeFromCity(City, EmployeePosition.Owner);

        if (newOwner != null)
        {
            Owner = newOwner;
        }

        for (int i = 0; i < 3; i++)
        {
            if (Owner != null)
            {
                return;
            }

            Owner = _generateNewEmployee(City, EmployeePosition.Owner);
        }

        Debug.Log("Couldn't generate new owner.");

    }

    Actor_Base _findEmployeeFromCity(CityComponent city, EmployeePosition position)
    {
        var vocationAndExperience = _getVocationAndExperienceFromPosition(position);

        var citizen = city.CityData.Population.AllCitizens
            .FirstOrDefault(c =>
                Manager_Actor.GetActor(c.CitizenActorID).ActorData.AttributesCareerAndPersonality.ActorCareer == CareerName.None
                && _hasMinimumVocationRequired(
                    Manager_Actor.GetActor(c.CitizenActorID),
                    vocationAndExperience.Vocation,
                    vocationAndExperience.minimumExperienceRequired));

        if (citizen != null)
        {
            return Manager_Actor.GetActor(citizen.CitizenActorID);
        }

        return null;
    }

    Actor_Base _generateNewEmployee(CityComponent city, EmployeePosition position)
    {
        var vocationAndExperience = _getVocationAndExperienceFromPosition(position);

        var actor = Manager_Actor.InitialiseNewActorOnGO(city.CityEntranceSpawnZone.transform.position);
        Manager_Actor.GenerateNewActorData(actor);

        actor.transform.parent.name = $"{actor.ActorData.BasicIdentification.ActorName.Name}Body";
        actor.transform.name = $"{actor.ActorData.BasicIdentification.ActorName.Name}";

        return actor;
    }

    bool _hasMinimumVocationRequired(Actor_Base actor, Vocation vocation, float minimumExperienceRequired)
    {
        if (actor.VocationComponent.Vocations[vocation] < minimumExperienceRequired)
        {
            return false;
        }

        return true;
    }

    (Vocation Vocation, float minimumExperienceRequired) _getVocationAndExperienceFromPosition(EmployeePosition position)
    {
        return (null, 0);
    }

    public void AddEmployee(Actor_Base employee, EmployeePosition position)
    {
        if (employee == null) throw new ArgumentException($"Employee: {employee} or employee position {position} is null.");

        if (EmployeeList.ContainsKey(employee))
        {
            if (EmployeeList[employee] == position) throw new ArgumentException($"Employee: {employee.ActorData.BasicIdentification.ActorName} already exists in employee list at same position.");
            else
            {
                EmployeeList[employee] = position;
                return;
            }
        }

        EmployeeList.Add(employee, position);
    }

    public void HireEmployee(Actor_Base employee, EmployeePosition position)
    {
        AddEmployee(employee, position);

        // And then apply relevant relation buff
    }

    public void RemoveEmployee(Actor_Base employee)
    {
        if (employee == null) throw new ArgumentException($"Employee is null.");

        if (!EmployeeList.ContainsKey(employee)) throw new ArgumentException($"Employee: {employee.ActorData.BasicIdentification.ActorName} is not in employee list.");

        EmployeeList.Remove(employee);

        // Remove employee job from employee job component.
    }

    public void FireEmployee(Actor_Base employee)
    {
        RemoveEmployee(employee);

        // And then apply relation debuff.
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
