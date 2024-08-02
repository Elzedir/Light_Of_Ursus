using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Jobsite_Lumberjack : Jobsite_Base
{
    public List<Interactable_Lumberjack> AllStations;

    public override void Initialise()
    {
        base.Initialise();
    }

    protected override IEnumerator TestInitialiseCity()
    {
        yield return base.TestInitialiseCity();

        AllStations = _getStationsInArea();

        _initialiseJobs();
    }

    void _initialiseJobs()
    {
        AllocateJobPositions();
        FillJobPositions();
    }

    List<Interactable_Lumberjack> _getStationsInArea()
    {
        return Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<Interactable_Lumberjack>()).Where(lumberjack => lumberjack != null).ToList();
    }

    public void AllocateJobPositions()
    {
        foreach (var position in AllStations.SelectMany(station => station.EmployeePositions).Where(position => !AllJobPositions.ContainsKey(position)).Distinct())
        {
            AllJobPositions[position] = new();
        }
    }

    public void FillJobPositions()
    {
        foreach (var position in AllJobPositions.Where(position => position.Value.Count == 0).ToList())
        {
            if (!_findEmployeeFromCity(position.Key, out Actor_Base actor)) actor = _generateNewEmployee(position.Key);

            if (!AllJobPositions.ContainsKey(position.Key))
            {
                AllJobPositions[position.Key] = new List<Actor_Base>();
            }

            AllJobPositions[position.Key].Add(actor);
            actor.JobComponent.AddJob(JobName.Lumberjack);
        }
    }
}
