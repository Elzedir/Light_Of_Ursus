using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Recipes;
using Tools;
using UnityEngine;

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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Career Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    data: new Dictionary<string, string>());
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Career Data", out var careerData))
                {
                    dataSO_Object.SubData["Career Data"] = new Data_Display(
                        title: "Career Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        data: new Dictionary<string, string>());
                }
                
                if (careerData is not null)
                {
                    careerData.Data = new Dictionary<string, string>
                    {
                        { "Career ID", $"{(uint)CareerName}" },
                        { "Career Name", CareerName.ToString() },
                        { "Career Description", CareerDescription }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Career Jobs", out var careerJobs))
                {
                    dataSO_Object.SubData["Career Jobs"] = new Data_Display(
                        title: "Career Jobs",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        data: CareerBaseJobs.ToDictionary(job => $"{(uint)job}:", job => $"{job}"));
                }
                
                if (careerJobs is not null)
                {
                    careerJobs.Data = CareerBaseJobs.ToDictionary(job => $"{(uint)job}:", job => $"{job}");
                }
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return dataSO_Object;
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