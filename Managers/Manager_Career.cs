using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

public class Manager_Career
{
    public static List<Career> AllCareers = new();

    public static void Initialise()
    {
        _wanderer();
        _smith();
        _lumberJack();
    }

    static void _wanderer()
    {
        AllCareers.Add(new Career(
            careerName: CareerName.None,
            careerDescription: "Functionally unemployed",
            new List<JobName>()

            ));
    }

    static void _smith()
    {
        AllCareers.Add(new Career(
            careerName: CareerName.Smith,
            careerDescription: "A smith",
            new List<JobName> 
            {
                
            }
            ));
    }

    static void _lumberJack()
    {
        AllCareers.Add(new Career(
            careerName: CareerName.Lumberjack,
            careerDescription: "A lumberjack",
            new List<JobName>
            {
                JobName.Lumberjack,
            }
            ));
    }

    public static Career GetCareer(CareerName careerName)
    {
        if (!AllCareers.Any(c => c.CareerName == careerName)) throw new ArgumentException($"Career: {careerName} is not in AllCareers List.");

        return AllCareers.FirstOrDefault(c => c.CareerName == careerName);
    }
}

public enum CareerName 
{
    None,
    Lumberjack,
    Smith
}



[Serializable]
public class Career
{
    public CareerName CareerName;
    public string CareerDescription;
    public ActivityPeriod ActivityPeriod;

    public List<JobName> CareerJobs = new();

    public Career(CareerName careerName, string careerDescription, List<JobName> careerJobs)
    {
        if (Manager_Career.AllCareers.Any(c => c.CareerName == careerName)) throw new ArgumentException("CareerName already exists.");

        CareerName = careerName;
        CareerDescription = careerDescription;
        CareerJobs = careerJobs;
    }
}

public enum ActivityPeriodName { Cathemeral, Nocturnal, Diurnal, Crepuscular }

public class ActivityPeriod
{
    public ActivityPeriodName PeriodName;
}
