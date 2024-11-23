using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Managers;
using UnityEditor;
using UnityEngine;

namespace Careers
{
    public abstract class Careers
    {
        const string  _allJobsSOPath = "ScriptableObjects/AllCareers_SO";
        
        static AllCareers_SO _allCareers;
        static AllCareers_SO AllCareers => _allCareers ??= _getOrCreateAllCareersSO();

        public static Career_Master GetCareer_Master(CareerName careerName) => AllCareers.GetCareer_Master(careerName);
        
        public static void PopulateAllCareers()
        {
            AllCareers.PopulateDefaultCareers();
            // Then populate custom jobs.
        }
        
        static AllJobs_SO _getOrCreateAllCareersSO()
        {
            var allCareersSO = Resources.Load<AllCareers_SO>(_allJobsSOPath);
            
            if (allCareersSO is not null) return allCareersSO;
            
            allCareersSO = ScriptableObject.CreateInstance<AllCareers_SO>();
            AssetDatabase.CreateAsset(allCareersSO, $"Assets/Resources/{_allJobsSOPath}");
            AssetDatabase.SaveAssets();
            
            return allCareersSO;
        }

        static void _wanderer()
        {
            AllCareers.Add(new Career_Master(
                careerName: CareerName.None,
                careerDescription: "Functionally unemployed",
                new List<JobName>()

            ));
        }

        static void _smith()
        {
            AllCareers.Add(new Career_Master(
                careerName: CareerName.Smith,
                careerDescription: "A smith",
                new List<JobName> 
                {
                
                }
            ));
        }

        static void _lumberJack()
        {
            AllCareers.Add(new Career_Master(
                careerName: CareerName.Lumberjack,
                careerDescription: "A lumberjack",
                new List<JobName>
                {
                    JobName.Lumberjack,
                }
            ));
        }

        public static Career_Master GetCareer(CareerName careerName)
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
        
    }

    [Serializable]
    public class Career_Master
    {
        public CareerName     CareerName;
        public string         CareerDescription;
        public ActivityPeriod ActivityPeriod;

        public List<JobName> CareerJobs = new();

        public Career_Master(CareerName careerName, string careerDescription, List<JobName> careerJobs)
        {
            if (Careers.AllCareers.Any(c => c.CareerName == careerName)) throw new ArgumentException("CareerName already exists.");

            CareerName        = careerName;
            CareerDescription = careerDescription;
            CareerJobs        = careerJobs;
        }
    }

    public enum ActivityPeriodName { Cathemeral, Nocturnal, Diurnal, Crepuscular }

    public class ActivityPeriod
    {
        public ActivityPeriodName PeriodName;
    }
}