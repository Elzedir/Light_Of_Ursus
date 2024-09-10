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

    Hauler,
}

public class JobsiteComponent : MonoBehaviour, ITickable
{
    public JobsiteData JobsiteData;
    public void SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
    public void SetCityID(int cityID) => JobsiteData.CityID = cityID;

    public List<StationComponent> AllStationsInJobsite;
    public List<EmployeePosition> AllCoreEmployeePositions;

    public bool JobsiteOpen = true;

    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Awake()
    {
        
    }

    public void Initialise()
    {
        AllStationsInJobsite = GetAllStationsInJobsite();

        AllStationsInJobsite.ForEach(station => station.SetJobsiteID(JobsiteData.JobsiteID));

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

    public List<StationComponent> GetAllStationsInJobsite() => GetComponentsInChildren<StationComponent>().ToList();

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
            foreach (var position in station.AllRequiredEmployeePositions)
            {
                employeePositions.Add(position);
            }
        }

        return employeePositions.ToList();
    }

    protected virtual void _assignEmployeesToStations(List<int> employeeIDs)
    {
        foreach (var station in AllStationsInJobsite)
        {
            station.RemoveAllOperators();
        }

        var employees = employeeIDs.Select(employeeID => Manager_Actor.GetActorData(employeeID)).ToList();

        foreach (var station in AllStationsInJobsite)
        {
            var allowedPositions = station.AllRequiredEmployeePositions;
            var employeesForStation = employees
                .Where(e => allowedPositions.Contains(e.CareerAndJobs.EmployeePosition))
                .OrderByDescending(e => e.CareerAndJobs.EmployeePosition)
                .ThenByDescending(e => e.VocationData.GetVocationExperience(_getRelevantVocation(e.CareerAndJobs.EmployeePosition)))
                .ToList();

            foreach (var employee in employeesForStation)
            {
                JobsiteData.AddEmployeeToStation(employee.ActorID, station.StationData.StationID);
                employees.Remove(employee);
            }
        }
    }

    protected virtual List<List<int>> _getAllCombinations(List<int> employees)
    {
        var result = new List<List<int>>();
        int combinationCount = (int)Mathf.Pow(2, employees.Count);

        for (int i = 1; i < combinationCount; i++)
        {
            var combination = new List<int>();
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