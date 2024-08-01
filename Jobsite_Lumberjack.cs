using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Jobsite_Lumberjack : Jobsite_Base
{
    public List<Interactable_Lumberjack> AllStations;

    public override void Initialise(Actor_Base owner, Dictionary<Actor_Base, EmployeePosition> employeeList)
    {
        base.Initialise(owner, employeeList);

        AllStations = _getStationsInArea();
    }

    public void Awake()
    {
        JobsiteArea = GetComponent<BoxCollider>();
        AllStations = _getStationsInArea();

        Initialise(null, new Dictionary<Actor_Base, EmployeePosition>());
    }

    List<Interactable_Lumberjack> _getStationsInArea()
    {
        return Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<Interactable_Lumberjack>())
        .Where(lumberjack => lumberjack != null)
        .ToList();
    }
}
