using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum EmployeePosition
{
    None,

    Owner,

    Shopkeeper,

    Chief_Lumberjack,
    Logger,
    Assistant_Logger,

    Sawyer,
    Assistant_Sawyer,

    Assistant_Smith,
}

[Serializable]
public class JobsiteComponent : MonoBehaviour
{
    public JobsiteData JobsiteData;
    public BoxCollider JobsiteArea;

    public ProsperityComponent ProsperityComponent;

    public List<StationComponent_Crafter> AllCraftingStationsInJobsite;
    public List<StationComponent_Resource> AllResourceStationsInJobsite;

    public void Initialise()
    {
        JobsiteArea = GetComponent<BoxCollider>();

        ProsperityComponent = new ProsperityComponent(JobsiteData.Prosperity.CurrentProsperity);
        GetAllStationsInJobsite();
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
        AllCraftingStationsInJobsite = Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<StationComponent_Crafter>()).Where(sc => sc != null).ToList();

        AllResourceStationsInJobsite = Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<StationComponent_Resource>()).Where(sc => sc != null).ToList();
    }

    public StationComponent_Crafter GetNearestCraftingStationInJobsite(Vector3 position, StationName stationName)
    {
        return AllCraftingStationsInJobsite
        .Where(station => station.StationData.StationName == stationName)
        .OrderBy(station => Vector3.Distance(position, station.transform.position))
        .FirstOrDefault();
    }

    public StationComponent_Resource GetNearestResourceStationInJobsite(Vector3 position, StationName stationName)
    {
        return AllResourceStationsInJobsite
        .Where(station => station.StationData.StationName == stationName)
        .OrderBy(station => Vector3.Distance(position, station.transform.position))
        .FirstOrDefault();
    }

    public StationComponent GetNearestDropOffStationInJobsite(Vector3 position, StationName stationName)
    {
        // Change for dropoff
        return AllResourceStationsInJobsite
        .Where(station => station.StationData.StationName == stationName)
        .OrderBy(station => Vector3.Distance(position, station.transform.position))
        .FirstOrDefault();
    }
}
