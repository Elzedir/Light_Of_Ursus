using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum EmployeePosition
{
    None,

    Owner,

    Shopkeeper,
    Assistant_Shopkeeper,

    Chief_Logger,
    Logger,
    Assistant_Logger,

    Chief_Sawyer,
    Sawyer,
    Assistant_Sawyer,

    Chief_Smith,
    Smith,
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
    public List<EmployeePosition> AllNecessaryEmployeePositions;

    public bool JobsiteOpen = true;

    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Initialise()
    {
        JobsiteArea = GetComponent<BoxCollider>();

        ProsperityComponent = new ProsperityComponent(JobsiteData.Prosperity.CurrentProsperity);
        GetAllStationsInJobsite();

        Manager_TickRate.RegisterTickable(this);
    }

    public virtual void OnTick()
    {
        foreach (var product in AllStationsInJobsite.Select(s => s.StationData.ProductionData))
        {
            product.GetEstimatedProductionRatePerHour();
            product.GetActualProductionRatePerHour();
        }

        _compareProductionOutput();
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneGameHour;
    }

    protected virtual bool _compareProductionOutput()
    {
        throw new ArgumentException("Cannot use base class");
    }

    protected virtual void _adjustProduction()
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

    public List<EmployeePosition> GetMinimumEmployeePositions()
    {
        HashSet<EmployeePosition> employeePositions = new();

        foreach (var station in AllStationsInJobsite)
        {
            foreach (var position in station.AllowedEmployeePositions)
            {
                employeePositions.Add(position);
            }
        }

        return employeePositions.ToList();
    }
}

[Serializable]
public class ProductionData
{
    public List<Item> AllProducedItems;
    public List<Item> EstimatedProductionRatePerHour;
    public List<Item> ActualProductionRatePerHour;
    public int StationID;
    private StationComponent _station;

    public StationComponent Station
    {
        get
        {
            if (_station == null)
            {
                _station = Manager_Station.GetStation(StationID);
            }
            return _station;
        }
        set
        {
            _station = value;
        }
    }

    public ProductionData(List<Item> allProducedItems, int stationID)
    {
        AllProducedItems = allProducedItems;
        StationID = stationID;
    }

    public List<Item> GetActualProductionRatePerHour()
    {
        return ActualProductionRatePerHour = Station.GetActualProductionRatePerHour();
    }

    public List<Item> GetEstimatedProductionRatePerHour()
    {
        return EstimatedProductionRatePerHour = Station.GetEstimatedProductionRatePerHour();
    }
}