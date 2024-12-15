using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Tools;
using UnityEngine;

namespace Careers
{
    [Serializable]
    public class Career_Data : Data_Class
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
                    data: CareerJobs.Select(job => $"{(uint)job}: {job}").ToList()));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return new Data_Display(
                title: $"{(uint)CareerName}: {CareerName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
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