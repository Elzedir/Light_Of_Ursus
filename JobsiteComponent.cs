using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class JobsiteComponent : MonoBehaviour, ITickable
{
    public uint JobsiteID { get { return JobsiteData.JobsiteID; } }
    public JobsiteData JobsiteData;
    public void SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
    public void SetCityID(uint cityID) => JobsiteData.CityID = cityID;

    public List<StationComponent> AllStationsInJobsite;
    public List<EmployeePosition> AllCoreEmployeePositions;

    public PriorityComponent_Jobsite PriorityComponent;

    public bool JobsiteOpen = true;

    public float IdealRatio;
    public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Initialise()
    {
        PriorityComponent = new PriorityComponent_Jobsite(JobsiteID);

        AllStationsInJobsite = GetAllStationsInJobsite();

        AllStationsInJobsite.ForEach(station => station.StationData.SetJobsiteID(JobsiteID));

        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneGameHour);
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
    .Where(station => station.StationName == stationName)
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

    protected virtual void _assignEmployeesToStations(List<uint> employeeIDs)
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

    protected virtual List<List<uint>> _getAllCombinations(List<uint> employees)
    {
        var result = new List<List<uint>>();
        int combinationCount = (int)Mathf.Pow(2, employees.Count);

        for (int i = 1; i < combinationCount; i++)
        {
            var combination = new List<uint>();
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

    protected void _prioritiseAllStationsToHaulFrom() => PriorityComponent.FullPriorityUpdate(AllStationsInJobsite.Cast<object>().ToList());

    public (StationComponent Station, List<Item> Items) GetStationToHaulFrom(ActorComponent hauler)
    {
        return PriorityComponent.GetStationToFetchFrom(hauler);
    }

    public (StationComponent Station, List<Item> Items) GetStationToHaulTo(ActorComponent hauler)
    {
        return PriorityComponent.GetStationToDeliverTo(hauler);
    }

    public List<StationComponent> GetRelevantStations(ActionName actionName, InventoryData inventoryData)
    {
        switch(actionName)
        {
            case ActionName.Fetch:
                return _relevantStations_Fetch();
            case ActionName.Deliver:
                return _relevantStations_Deliver(inventoryData);
            default:
                return new List<StationComponent>();
        }
    }

    List<StationComponent> _relevantStations_Fetch()
    {
        var allStationsCanFetch = AllStationsInJobsite.Where(station => station.GetInventoryItemsToFetch().Count > 0).ToList();
        return allStationsCanFetch;
    }

    List<StationComponent> _relevantStations_Deliver(InventoryData inventoryData)
    {
        var allStationsCanDeliver = AllStationsInJobsite.Where(station => station.GetInventoryItemsToDeliver(inventoryData).Count > 0).ToList();

        a
        // find a way to prioritise stations using MasterItem.PriorityStats, check the highest priority List<Station> that is in allStationsCanDeliver and return those stations. If there are multiple stations with the same priority, return all of them.

        if (!allStationsCanDeliver.Any()) return allStationsCanDeliver;
        else
        {
            for(int i = 0; i < allStationsCanDeliver.Count; i++)
            {
                if (allStationsCanDeliver[i])
                {
                    allStationsCanDeliver.RemoveAt(i);
                }
            }
        }

        return allStationsCanDeliver;
    }
}