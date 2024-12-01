using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using UnityEditor;
using UnityEngine;

namespace Career
{
    public abstract class Career_Manager
    {
        const string  _allCareersSOPath = "ScriptableObjects/Career_SO";
        
        static Career_SO _careerSO;
        static Career_SO Career_SO => _careerSO ??= _getOrCreateAllCareersSO();

        public static Career_Master GetCareer_Master(CareerName careerName) => Career_SO.GetCareer_Master(careerName);
        
        public static void PopulateAllCareers()
        {
            Career_SO.PopulateDefaultCareers();
            // Then populate custom careers.
        }
        
        static Career_SO _getOrCreateAllCareersSO()
        {
            var allCareersSO = Resources.Load<Career_SO>(_allCareersSOPath);
            
            if (allCareersSO is not null) return allCareersSO;
            
            allCareersSO = ScriptableObject.CreateInstance<Career_SO>();
            AssetDatabase.CreateAsset(allCareersSO, $"Assets/Resources/{_allCareersSOPath}");
            AssetDatabase.SaveAssets();
            
            return allCareersSO;
        }
    }

    public enum CareerName 
    {
        None,
        Wanderer,
        Lumberjack,
        Smith
    }

    [Serializable]
    public class Career
    {
        public CareerName CareerName;
        
        Career_Master _career_Master;
        Career_Master Career_Master => _career_Master ??= Career_Manager.GetCareer_Master(CareerName);
        
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