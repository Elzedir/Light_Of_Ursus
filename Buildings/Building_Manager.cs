using System.Collections.Generic;
using System.Linq;
using Jobs;
using UnityEngine;

namespace Buildings
{
    public abstract class Building_Manager
    {
        const  string     c_building_SOPath = "ScriptableObjects/Building_SO";
        
        static Building_SO s_building_SO;
        static Building_SO BuildingSO => s_building_SO ??= _getBuilding_SO();
        
        public static Building_Data GetBuilding_Data(ulong buildingID)
        {
            return BuildingSO.GetBuilding_Data(buildingID).Data_Object;
        }
        
        public static Building_Data GetBuilding_DataFromName(Building_Component building_Component)
        {
            return BuildingSO.GetDataFromName(building_Component.name)?.Data_Object;
        }
        
        public static Building_Component GetBuilding_Component(ulong buildingID)
        {
            return BuildingSO.GetBuilding_Component(buildingID);
        }
        
        public static List<ulong> GetAllBuildingIDs() => BuildingSO.GetAllDataIDs();
        
        static Building_SO _getBuilding_SO()
        {
            var building_SO = Resources.Load<Building_SO>(c_building_SOPath);
            
            if (building_SO is not null) return building_SO;
            
            Debug.LogError("Building_SO not found. Creating temporary Building_SO.");
            building_SO = ScriptableObject.CreateInstance<Building_SO>();
            
            return building_SO;
        }

        public static Building_Component GetNearestBuilding(Vector3 position, BuildingType buildingType)
        {
            // Change so that you either pass through a city, or if not, then it will check nearest region, and give you nearest 
            // Region => City => Jobsite. Maybe flash a BoxCollider at increasing distances and check if it hits a city or region and
            // use that to calculate the nearest one.
            
            Building_Component nearestBuilding = null;

            var nearestDistance = float.PositiveInfinity;

            foreach (var building in BuildingSO.Building_Components.Values.Where(j => j.Building_Data.BuildingType == buildingType))
            {
                var distance = Vector3.Distance(position, building.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestBuilding  = building;
                nearestDistance = distance;
            }

            return nearestBuilding;
        }
        
        public static Dictionary<BuildingType, List<JobName>> EmployeeCanUseList = new()
        {
            {BuildingType.Lumber_Yard, new List<JobName>
            {
                JobName.Logger,
                JobName.Sawyer,
                
            }},
            {BuildingType.Smithy, new List<JobName>
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
    
    public enum BuildingType
    {
        None,

        Grand_Timber_Consortium, Timber_Depot, Lumber_Yard, Logging_Cabin, Woodcutters_Hut,
        
        Grand_Metalworks, Smithing_Hall, Smithy, Blacksmith_Hut, Forge,
        
        Grand_Cathedral, Cathedral, Abbey, Monastery, Church, Chapel, Shrine,
        
        Grand_Exchange, Merchant_District, Trade_Hub, Market, Fairground,
        
        Guild_Headquarters, Guild_Hall, Artisan_Hall, Workshop,
        
        Extermination_Camp, Hunting_Lodge, Hunting_Cabin, Hunting_Hut,
    }
}
