using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using UnityEditor;
using UnityEngine;

namespace Careers
{
    public abstract class Manager_Career
    {
        const string  _allCareersSOPath = "ScriptableObjects/AllCareers_SO";
        
        static AllCareers_SO _allCareers;
        static AllCareers_SO AllCareers => _allCareers ??= _getOrCreateAllCareersSO();

        public static Career_Master GetCareer_Master(CareerName careerName) => AllCareers.GetCareer_Master(careerName);
        
        public static void PopulateAllCareers()
        {
            AllCareers.PopulateDefaultCareers();
            // Then populate custom careers.
        }
        
        static AllCareers_SO _getOrCreateAllCareersSO()
        {
            var allCareersSO = Resources.Load<AllCareers_SO>(_allCareersSOPath);
            
            if (allCareersSO is not null) return allCareersSO;
            
            allCareersSO = ScriptableObject.CreateInstance<AllCareers_SO>();
            AssetDatabase.CreateAsset(allCareersSO, $"Assets/Resources/{_allCareersSOPath}");
            AssetDatabase.SaveAssets();
            
            return allCareersSO;
        }
    }

    public enum CareerName 
    {
        Wanderer,
        Lumberjack,
        Smith
    }

    [Serializable]
    public class Career
    {
        public CareerName CareerName;
        
        Career_Master _career_Master;
        Career_Master Career_Master => _career_Master ??= Manager_Career.GetCareer_Master(CareerName);
        
        public List<JobName> CareerJobs => Career_Master.CareerJobs.ToList();
        
        public Career (CareerName careerName)
        {
            CareerName = careerName;
        }
    }

    [Serializable]
    public class Career_Master
    {
        public CareerName     CareerName;
        public string         CareerDescription;
        public HashSet<JobName> CareerJobs;

        public Career_Master(CareerName careerName, string careerDescription, HashSet<JobName> careerJobs)
        {
            CareerName        = careerName;
            CareerDescription = careerDescription;
            CareerJobs        = careerJobs;
        }
    }
}