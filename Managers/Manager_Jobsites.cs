using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Jobsites
{
    public static Dictionary<CityComponent, ExistingJobsiteData> AllExistingJobsites;

    public static void Initialise()
    {
        // Initialise before Jobsite_Base
    }

    static void _testCity()
    {
        var testCity = GameObject.Find("Test_City").GetComponent<CityComponent>();

        AllExistingJobsites.Add(testCity, 
            new ExistingJobsiteData(
                city: testCity,
                ownerActorID: -1,
                allEmployees: new()
                ));
    }

    public static ExistingJobsiteData GetJobsiteData(CityComponent city)
    {
        if (!AllExistingJobsites.ContainsKey(city)) throw new ArgumentException($"City: {city.name} does not exist in AllExistingJobsites.");

        return AllExistingJobsites[city];
    }
}

public class ExistingJobsiteData
{
    public CityComponent City;
    public int OwnerActorID;
    public Dictionary<Actor_Base, EmployeePosition> AllEmployees;

    public ExistingJobsiteData(CityComponent city, int ownerActorID, Dictionary<Actor_Base, EmployeePosition> allEmployees)
    {
        City = city;
        OwnerActorID = ownerActorID;
        AllEmployees = allEmployees;
    }
}
