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
        const string  _career_SOPath = "ScriptableObjects/Career_SO";
        
        static Career_SO _careerSO;
        static Career_SO Career_SO => _careerSO ??= _getCareer_SO();

        public static Career_Master GetCareer_Master(CareerName careerName) => Career_SO.GetCareer_Master(careerName);
        
        public static void PopulateAllCareers()
        {
            Career_SO.PopulateDefaultCareers();
            // Then populate custom careers.
        }
        
        static Career_SO _getCareer_SO()
        {
            var career_SO = Resources.Load<Career_SO>(_career_SOPath);
            
            if (career_SO is not null) return career_SO;
            
            Debug.LogError("Career_SO not found. Creating temporary Career_SO.");
            career_SO = ScriptableObject.CreateInstance<Career_SO>();
            
            return career_SO;
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