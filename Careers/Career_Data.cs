using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;

namespace Career
{
    [Serializable]
    public class Career_Data
    {
        public CareerName       CareerName;
        public string           CareerDescription;
        public HashSet<JobName> CareerJobs;

        public Career_Data(CareerName careerName, string careerDescription, HashSet<JobName> careerJobs)
        {
            CareerName        = careerName;
            CareerDescription = careerDescription;
            CareerJobs        = careerJobs;
        }
    }
    
    [Serializable]
    public class Career
    {
        public CareerName CareerName;
        
        Career_Data _career_Data;
        Career_Data Career_Data => _career_Data ??= Career_Manager.GetCareer_Master(CareerName);
        
        public List<JobName> CareerJobs => Career_Data.CareerJobs.ToList();
        
        public Career (CareerName careerName)
        {
            CareerName = careerName;
        }
    }
}