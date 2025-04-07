using System.Collections.Generic;

namespace Buildings
{
    public class Building_List
    {
        static Dictionary<BuildingName, Building_DefaultData> s_defaultBuildings;
        public static Dictionary<BuildingName, Building_DefaultData> S_DefaultBuilding => s_defaultBuildings ??= _initialiseDefaultBuilding();
        
        static Dictionary<BuildingName, Building_DefaultData> _initialiseDefaultBuilding()
        {
            return new Dictionary<BuildingName, Building_DefaultData>
            {
                {
                    BuildingName.Lumber_Yard, new Building_DefaultData()
                    {
                        // Initialise the Lumber Yard blueprint here
                    } 
                }
            };  
        }
    }
}