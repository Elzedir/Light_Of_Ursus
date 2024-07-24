using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Career
{
    public static List<Career> AllCareers = new();

    public static void Initialise()
    {
        _smith();
        _lumberJack();
    }

    static void _smith()
    {
        AllCareers.Add(new Career(
            careerName: CareerName.Smith,
            careerDescription: "A smith",
            new List<Job> 
            {
                
            }
            ));
    }

    static void _lumberJack()
    {
        AllCareers.Add(new Career(
            careerName: CareerName.Lumberjack,
            careerDescription: "A lumberjack",
            new List<Job>
            {
                Manager_Jobs.GetJob(JobName.Lumberjack)
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

    public List<Job> CareerJobs = new();

    public Career(CareerName careerName, string careerDescription, List<Job> careerJobs)
    {
        if (Manager_Career.AllCareers.Any(c => c.CareerName == careerName)) throw new ArgumentException("CareerName already exists.");

        CareerName = careerName;
        CareerDescription = careerDescription;
        CareerJobs = careerJobs;
    }
}
