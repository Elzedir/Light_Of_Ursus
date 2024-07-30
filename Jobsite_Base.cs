using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EmployeePosition
{
    None,

    Shopkeeper,

    Chief_Lumberjack,
    Lumberjack,
    Assistant_Lumberjack,

    Assistant_Smith,
}

public class Jobsite_Base : MonoBehaviour
{
    public Actor_Base Owner;

    public bool IsActive = true;

    public Dictionary<Actor_Base, EmployeePosition> EmployeeList = new();
    public BoxCollider JobsiteArea;

    public virtual void Initialise(Actor_Base owner, Dictionary<Actor_Base, EmployeePosition> employeeList)
    {
        JobsiteArea = GetComponent<BoxCollider>();
        SetOwner(owner);
        EmployeeList = employeeList;
    }

    public void SetOwner(Actor_Base owner)
    {
        Owner = owner;

        // And change all affected things, like perks, job settings, etc.
    }

    public void AddEmployee(Actor_Base employee, EmployeePosition position)
    {
        if (employee == null) throw new ArgumentException($"Employee: {employee} or employee position {position} is null.");

        if (EmployeeList.ContainsKey(employee))
        {
            if (EmployeeList[employee] == position) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} already exists in employee list at same position.");
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

        if (!EmployeeList.ContainsKey(employee)) throw new ArgumentException($"Employee: {employee.ActorData.ActorName} is not in employee list.");

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
