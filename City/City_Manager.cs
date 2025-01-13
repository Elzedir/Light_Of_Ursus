using UnityEngine;

namespace City
{
    public abstract class City_Manager
    {
        const string _city_SOPath = "ScriptableObjects/City_SO";
        
        static City_SO _allCities;
        static City_SO AllCities => _allCities ??= _getCity_SO();
        
        public static City_Data GetCity_Data(uint cityID)
        {
            return AllCities.GetCity_Data(cityID).Data_Object;
        }
        
        public static City_Component GetCity_Component(uint cityID)
        {
            return AllCities.GetCity_Component(cityID);
        }
        
        static City_SO _getCity_SO()
        {
            var city_SO = Resources.Load<City_SO>(_city_SOPath);
            
            if (city_SO is not null) return city_SO;
            
            Debug.LogError("City_SO not found. Creating temporary City_SO.");
            city_SO = ScriptableObject.CreateInstance<City_SO>();
            
            return city_SO;
        }

        public static City_Component GetNearestCity(Vector3 position)
        {
            City_Component nearestCity = null;

            var nearestDistance = float.MaxValue;

            foreach (var city in AllCities.City_Components.Values)
            {
                var distance = Vector3.Distance(position, city.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestCity  = city;
                nearestDistance = distance;
            }

            return nearestCity;
        }

        public static uint GetUnusedCityID()
        {
            return AllCities.GetUnusedCityID();
        }
        
        public static void ClearSOData()
        {
            AllCities.ClearSOData();
        }
    }
}
