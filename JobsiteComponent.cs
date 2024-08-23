using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EmployeePosition
{
    None,

    Owner,

    Shopkeeper,

    Chief_Logger,
    Logger,
    Assistant_Logger,

    Chief_Sawyer,
    Sawyer,
    Assistant_Sawyer,

    Assistant_Smith,
}

[Serializable]
[RequireComponent(typeof(BoxCollider))]
public class JobsiteComponent : MonoBehaviour, ITickable
{
    public JobsiteData JobsiteData;
    public BoxCollider JobsiteArea;

    public ProsperityComponent ProsperityComponent;

    public List<StationComponent> AllStationsInJobsite;

    public bool JobsiteOpen = true;

    public void Initialise()
    {
        JobsiteArea = GetComponent<BoxCollider>();

        ProsperityComponent = new ProsperityComponent(JobsiteData.Prosperity.CurrentProsperity);
        GetAllStationsInJobsite();

        Manager_TickRate.RegisterTickable(this);
    }

    public virtual void OnTick()
    {
        throw new ArgumentException("Cannot use base class");
    }

    public virtual TickRate GetTickRate()
    {
        throw new ArgumentException("Cannot use base class");
    }

    protected virtual bool _compareProductionOutput()
    {
        throw new ArgumentException("Cannot use base class");
    }

    protected virtual void _redistributeEmployees()
    {
        throw new ArgumentException("Cannot use base class");
    }

    public void SetJobsiteData(JobsiteData jobsiteData)
    {
        JobsiteData = jobsiteData;
    }

    public void RefreshJobsite()
    {
        // Refresh all stations in jobsitedata
    }

    public void GetAllStationsInJobsite()
    {
        AllStationsInJobsite = Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<StationComponent>()).Where(sc => sc != null).ToList();
    }

    public StationComponent GetNearestStationInJobsite(Vector3 position, StationName stationName)
    {
        return AllStationsInJobsite
        .Where(station => station.StationData.StationName == stationName)
        .OrderBy(station => Vector3.Distance(position, station.transform.position))
        .FirstOrDefault();
    }
}
