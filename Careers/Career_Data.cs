using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Recipes;
using Tools;

namespace Careers
{
    [Serializable]
    public class Career_Data : Data_Class
    {
        public CareerName                          CareerName;
        public string                              CareerDescription;
        public List<JobName>                       CareerBaseJobs;
        public Dictionary<JobName, JobRequirement> CareerSpecialistJobs;

        public Career_Data(CareerName careerName, string careerDescription, List<JobName> careerBaseJobs, Dictionary<JobName, JobRequirement> careerSpecialistJobs)
        {
            CareerName           = careerName;
            CareerDescription    = careerDescription;
            CareerBaseJobs       = careerBaseJobs;
            CareerSpecialistJobs = careerSpecialistJobs;
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Base Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(ref _dataToDisplay,
                title: "Career Base Jobs",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: CareerBaseJobs.ToDictionary(
                    job => $"{(uint)job}",
                    job => $"{job}"));
            
            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Career ID", $"{(uint)CareerName}" },
                { "Career Name", $"{CareerName}" },
                { "Career Description", CareerDescription }
            };
        }
    }
    
    [Serializable]
    public class Career
    {
        public CareerName CareerName;
        
        Career_Data _career_Data;
        public Career_Data Career_Data => _career_Data ??= Career_Manager.GetCareer_Master(CareerName);
        
        public List<JobName> CareerJobs => Career_Data.CareerBaseJobs.ToList();
        
        public Career (CareerName careerName)
        {
            CareerName = careerName;
        }
    }

    public class JobRequirement
    {
        public VocationRequirement VocationRequirement;
        
        public JobRequirement(VocationRequirement vocationRequirement)
        {
            VocationRequirement = vocationRequirement;
        }
    }
}