using System.Collections.Generic;

namespace Buildings
{
    public class Building_List
    {
        static Dictionary<BuildingType, Building_DefaultData> s_defaultBuildings;
        public static Dictionary<BuildingType, Building_DefaultData> S_DefaultBuilding => s_defaultBuildings ??= _initialiseDefaultBuilding();
        
        static Dictionary<BuildingType, Building_DefaultData> _initialiseDefaultBuilding()
        {
            return new Dictionary<BuildingType, Building_DefaultData>
            {
                {
                    BuildingType.Lumber_Yard, new Building_DefaultData()
                    {
                        // Initialise the Lumber Yard blueprint here
                    } 
                }
            };  
        }
    }
}