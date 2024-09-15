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

public abstract class JobsiteComponent : MonoBehaviour, ITickable
{
    public JobsiteData JobsiteData;
    public void SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
    public void SetCityID(int cityID) => JobsiteData.CityID = cityID;

    public List<StationComponent> AllStationsInJobsite;
    public List<EmployeePosition> AllCoreEmployeePositions;

    public bool JobsiteOpen = true;

    public float IdealRatio;
    public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Initialise()
    {
        AllStationsInJobsite = GetAllStationsInJobsite();

        AllStationsInJobsite.ForEach(station => station.StationData.JobsiteID = JobsiteData.JobsiteID);

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

    protected abstract bool _compareProductionOutput();

    protected abstract void _adjustProduction(float idealRatio);
    protected abstract VocationName _getRelevantVocation(EmployeePosition position);

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
            foreach (var position in station.AllowedEmployeePositions)
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

        var tempEmployees = employeeIDs.Select(employeeID => Manager_Actor.GetActorData(employeeID)).ToList();

        foreach (var station in AllStationsInJobsite)
        {
            var allowedPositions = station.AllowedEmployeePositions;
            var employeesForStation = tempEmployees
                .Where(e => allowedPositions.Contains(e.CareerAndJobs.EmployeePosition))
                .OrderByDescending(e => e.CareerAndJobs.EmployeePosition)
                .ThenByDescending(e => e.VocationData.GetVocationExperience(_getRelevantVocation(e.CareerAndJobs.EmployeePosition)))
                .ToList();

            foreach (var employee in employeesForStation)
            {
                JobsiteData.AddEmployeeToStation(employee.ActorID, station.StationData.StationID);
                tempEmployees.Remove(employee);
            }

            if (tempEmployees.Count > 0)
            {
                Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
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