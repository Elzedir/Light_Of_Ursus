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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Career Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Career ID: {(uint)CareerName}",
                        $"Career Name: {CareerName}",
                        $"Career Description: {CareerDescription}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Career Jobs",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: CareerBaseJobs.Select(job => $"{(uint)job}: {job}").ToList()));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return new Data_Display(
                title: $"{(uint)CareerName}: {CareerName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects),
                selectedIndex: dataSO_Object?.SelectedIndex ?? -1,
                showData: dataSO_Object?.ShowData           ?? false);
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