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
    public void SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
    public BoxCollider JobsiteArea;

    public List<StationComponent> AllStationsInJobsite;
    public List<EmployeePosition> AllNecessaryEmployeePositions;

    public bool JobsiteOpen = true;

    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Awake()
    {
        JobsiteArea = GetComponent<BoxCollider>();
        
        if (!JobsiteArea.isTrigger)
        {
            Debug.Log($"Set IsTrigger to true for {name}");
            JobsiteArea.isTrigger = true;
        }
    }

    public void Initialise()
    {
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

    protected virtual bool _compareProductionOutput() { throw new ArgumentException("Cannot use base class"); }

    protected virtual void _adjustProduction() { throw new ArgumentException("Cannot use base class"); }

    protected virtual void _redistributeEmployees() { throw new ArgumentException("Cannot use base class"); }
    protected virtual VocationName _getRelevantVocation(EmployeePosition position) { throw new ArgumentException("Cannot use base class"); }

    public void RefreshJobsite()
    {
        // Refresh all stations in jobsitedata
    }

    public void GetAllStationsInJobsite() =>
    AllStationsInJobsite = Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
    .Select(collider => collider
    .GetComponent<StationComponent>())
    .Where(sc => sc != null)
    .ToList();

    public StationComponent GetNearestStationInJobsite(Vector3 position, StationName stationName)
    => AllStationsInJobsite
    .Where(station => station.StationData.StationName == stationName)
    .OrderBy(station => Vector3.Distance(position, station.transform.position))
    .FirstOrDefault();


    public List<EmployeePosition> GetMinimumEmployeePositions()
    {
        HashSet<EmployeePosition> employeePositions = new();

        foreach (var station in AllStationsInJobsite)
        {
            foreach (var position in station.AllAllowedEmployeePositions)
            {
                employeePositions.Add(position);
            }
        }

        return employeePositions.ToList();
    }

    protected virtual void _assignEmployeesToStations(List<ActorData> employees)
    {
        foreach (var station in AllStationsInJobsite)
        {
            station.RemoveAllOperators();
        }

        foreach (var station in AllStationsInJobsite)
        {
            var allowedPositions = station.AllAllowedEmployeePositions;
            var employeesForStation = employees
                .Where(e => allowedPositions.Contains(e.CareerAndJobs.EmployeePosition))
                .OrderByDescending(e => e.CareerAndJobs.EmployeePosition)
                .ThenByDescending(e => e.VocationData.GetVocationExperience(_getRelevantVocation(e.CareerAndJobs.EmployeePosition)))
                .ToList();

            foreach (var employee in employeesForStation)
            {
                JobsiteData.AddEmployeeToStation(employee, station.StationData.StationID);
                employees.Remove(employee);
            }
        }
    }

    protected virtual List<List<ActorData>> _getAllCombinations(List<ActorData> employees)
    {
        var result = new List<List<ActorData>>();
        int combinationCount = (int)Mathf.Pow(2, employees.Count);

        for (int i = 1; i < combinationCount; i++)
        {
            var combination = new List<ActorData>();
            for (int j = 0; j < employees.Count; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    combination.Add(employees[j]);
                }
            }
            result.Add(combination);
        }

        return result;
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