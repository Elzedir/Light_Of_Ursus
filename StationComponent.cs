using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationComponent : MonoBehaviour, IInteractable
{
    public StationData StationData;
    public BoxCollider StationArea;

    public Actor_Base CurrentOperator;
    public bool IsBeingOperated = false;
    public float CurrentProgress = 0;
    public float ProgressRequired = 100;
    public float ProgressPercentageChance = 80;
    public float ProgressCooldownTime = 1;
    public float ProgressCooldownCheck;

    public float InteractRange {  get; protected set; }

    public GameObject GameObject { get; protected set; }

    public List<EmployeePosition> AllowedEmployeePositions;

    public Dictionary<BoxCollider, bool> AllOperatingAreasInStation;

    protected void Awake()
    {
        StationArea = GetComponent<BoxCollider>();
    }

    protected void Update()
    {
        //StationName is resetting on start
        //Station data is null

        if (StationData.StationIsActive && IsBeingOperated)
        {
            _operateStation();
        }
    }

    protected void _operateStation()
    {
        // Use actor data to check how fast the process will be completed and the outcome of the process.
        // Use an accumulation to determine outcome. If one actor with 80 skill used it, then 80 * 100 = 800. 
        // If you have someone who did 40% of the work at 40 skill, then 40 * 40 + 80 * 60 = 160 + 480 = 640.

        // if () inventory !has the necessary items, return.

        ProgressCooldownCheck += UnityEngine.Time.deltaTime;

        if (ProgressCooldownCheck > ProgressCooldownTime)
        {
            if (UnityEngine.Random.Range(0, 100) < ProgressPercentageChance)
            {
                CurrentProgress += 1;
            }

            if (CurrentProgress >= ProgressRequired)
            {
                // Create the item.
                return;
            }

            ProgressCooldownCheck = 0;
        }
    }

    public void Initialise()
    {
        InitialiseStationName();
        InitialiseOperatingAreas();
        SetInteractRange();
        InitialiseAllowedEmployeePositions();
    }

    public virtual void InitialiseStationName()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public void InitialiseOperatingAreas()
    {
        foreach(Transform child in transform)
        {
            if (!child.name.Contains("OperatingArea")) continue;

            AllOperatingAreasInStation.Add(child.GetComponent<BoxCollider>(), false);
        }
    }

    public virtual void InitialiseAllowedEmployeePositions()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public void SetStationData(StationData stationData)
    {
        StationData = stationData;
    }

    public void SetInteractRange(float interactRange = 2)
    {
        InteractRange = interactRange;
    }

    public bool WithinInteractRange(Actor_Base interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }

    public virtual IEnumerator Interact(Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual bool EmployeeCanUse(EmployeePosition employeePosition)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public Vector3 GetNearestOperatingAreaInStation(Vector3 position)
    {
        return AllOperatingAreasInStation
        .OrderBy(area => Vector3.Distance(position, area.Key.bounds.center))
        .FirstOrDefault().Key.bounds.center;
    }
}
