using System.Collections.Generic;
using System.Linq;
using Buildings;
using Jobs;
using UnityEngine;

namespace JobSites
{
    public abstract class Building_Manager
    {
        const  string     _jobSite_SOPath = "ScriptableObjects/Building_SO";
        
        static Building_SO s_building_SO;
        static Building_SO BuildingSO => s_building_SO ??= _getJobSite_SO();
        
        public static Building_Data GetJobSite_Data(ulong jobSiteID)
        {
            return BuildingSO.GetJobSite_Data(jobSiteID).Data_Object;
        }
        
        public static Building_Data GetJobSite_DataFromName(Building_Component building_Component)
        {
            return BuildingSO.GetDataFromName(building_Component.name)?.Data_Object;
        }
        
        public static Building_Component GetJobSite_Component(ulong jobSiteID)
        {
            return BuildingSO.GetJobSite_Component(jobSiteID);
        }
        
        public static List<ulong> GetAllJobSiteIDs() => BuildingSO.GetAllDataIDs();
        
        static Building_SO _getJobSite_SO()
        {
            var jobSite_SO = Resources.Load<Building_SO>(_jobSite_SOPath);
            
            if (jobSite_SO is not null) return jobSite_SO;
            
            Debug.LogError("JobSite_SO not found. Creating temporary JobSite_SO.");
            jobSite_SO = ScriptableObject.CreateInstance<Building_SO>();
            
            return jobSite_SO;
        }

        public static Building_Component GetNearestJobSite(Vector3 position, BuildingName buildingName)
        {
            // Change so that you either pass through a city, or if not, then it will check nearest region, and give you nearest 
            // Region => City => Jobsite. Maybe flash a BoxCollider at increasing distances and check if it hits a city or region and
            // use that to calculate the nearest one.
            
            Building_Component nearestBuilding = null;

            var nearestDistance = float.PositiveInfinity;

            foreach (var jobSite in BuildingSO.JobSite_Components.Values.Where(j => j.BuildingName == buildingName))
            {
                var distance = Vector3.Distance(position, jobSite.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestBuilding  = jobSite;
                nearestDistance = distance;
            }

            return nearestBuilding;
        }
        
        public static Dictionary<BuildingName, List<JobName>> EmployeeCanUseList = new()
        {
            {BuildingName.Lumber_Yard, new List<JobName>
            {
                JobName.Logger,
                JobName.Sawyer,
                
            }},
            {BuildingName.Smithy, new List<JobName>
            {
                JobName.Miner,
                JobName.Smith,
            }}
        };
        
        public static void ClearSOData()
        {
            BuildingSO.ClearSOData();
        }
    }
    
    public enum BuildingName
    {
        None,

        Lumber_Yard,
        Smithy
    }
}
