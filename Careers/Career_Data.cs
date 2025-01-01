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
        public HashSet<JobName>                    CareerBaseJobs;
        public Dictionary<JobName, JobRequirement> CareerSpecialistJobs;

        public Career_Data(CareerName careerName, string careerDescription, HashSet<JobName> careerBaseJobs, Dictionary<JobName, JobRequirement> careerSpecialistJobs)
        {
            CareerName                = careerName;
            CareerDescription         = careerDescription;
            CareerBaseJobs            = careerBaseJobs;
            CareerSpecialistJobs = careerSpecialistJobs;
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);
            
            try
            {
                dataObjects["Career Data"] = new Data_Display(
                    title: "Career Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Career ID", $"{(uint)CareerName}" },
                        { "Career Name", CareerName.ToString() },
                        { "Career Description", CareerDescription }
                    });
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects["Career Jobs"] = new Data_Display(
                    title: "Career Jobs",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: CareerBaseJobs.ToDictionary(job => $"{(uint)job}:", job => $"{job}"));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return dataSO_Object = new Data_Display(
                title: $"{(uint)CareerName}: {CareerName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
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