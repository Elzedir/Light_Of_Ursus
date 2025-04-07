using System.Collections.Generic;
using UnityEngine;

namespace Cities
{
    public abstract class City_Manager
    {
        const string _city_SOPath = "ScriptableObjects/City_SO";
        
        static City_SO _allCities;
        static City_SO AllCities => _allCities ??= _getCity_SO();
        
        public static Barony_Data GetCity_Data(ulong cityID)
        {
            return AllCities.GetCity_Data(cityID).Data_Object;
        }
        
        public static Barony_Data GetCity_DataFromName(Barony_Component barony_Component)
        {
            return AllCities.GetDataFromName(barony_Component.name)?.Data_Object;
        }
        
        public static Barony_Component GetCity_Component(ulong cityID)
        {
            return AllCities.GetCity_Component(cityID);
        }
        
        public static List<ulong> GetAllCityIDs() => AllCities.GetAllDataIDs();
        
        static City_SO _getCity_SO()
        {
            var city_SO = Resources.Load<City_SO>(_city_SOPath);
            
            if (city_SO is not null) return city_SO;
            
            Debug.LogError("City_SO not found. Creating temporary City_SO.");
            city_SO = ScriptableObject.CreateInstance<City_SO>();
            
            return city_SO;
        }

        public static Barony_Component GetNearestCity(Vector3 position)
        {
            Barony_Component nearestBarony = null;

            var nearestDistance = float.MaxValue;

            foreach (var city in AllCities.City_Components.Values)
            {
                var distance = Vector3.Distance(position, city.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestBarony  = city;
                nearestDistance = distance;
            }

            return nearestBarony;
        }
        
        public static void ClearSOData()
        {
            AllCities.ClearSOData();
        }
    }
    
    public enum BaronyName
    {
        None,
        
        Metropolis, City, Town, Village, Hamlet, // Economic (Trade)
        
        Citadel, Fortress, Castle, Outpost, Watchtower, // Military defensive
        
        Fort, Encampment, Garrison, Barracks, Camp, // Military offensive
        
        Grand_Estate, Estate, Manor, Farmstead, // Production (food)
        
        Ore_Camp, Mine, Quarry, Dig_Site, // Production (Mineral)
        
        Grand_Port, Port, Harbour, Dock,  // Economic and production (Trade and food)
    }
}
