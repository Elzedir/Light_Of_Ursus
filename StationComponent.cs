using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationComponent : MonoBehaviour, IInteractable
{
    public StationData StationData;
    public BoxCollider StationArea;
    
    public bool IsBeingUsed;

    public float InteractRange {  get; private set; }

    public GameObject GameObject { get; protected set; }

    public List<EmployeePosition> AllowedEmployeePositions;

    public List<BoxCollider> AllOperatingAreasInStation;

    void Awake()
    {
        StationArea = GetComponent<BoxCollider>();
    }

    public void Initialise()
    {
        InitialiseOperatingAreas();
        SetInteractRange();
        InitialiseAllowedEmployeePositions();
    }

    public void InitialiseOperatingAreas()
    {
        foreach(Transform child in transform)
        {
            if (!child.name.Contains("OperatingArea")) continue;

            AllOperatingAreasInStation.Add(child.GetComponent<BoxCollider>());
        }
    }

    public void InitialiseAllowedEmployeePositions()
    {
        throw new ArgumentException("Cannot use base class.");
        
        //AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Lumberjack, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
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
        .OrderBy(area => Vector3.Distance(position, area.bounds.center))
        .FirstOrDefault().bounds.center;
    }
}
