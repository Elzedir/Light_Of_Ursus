using System.Collections.Generic;
using Initialisation;
using UnityEditor;
using UnityEngine;

namespace City
{
    public class City_Manager : IDataPersistence
    {
        const         string                           _city_SOPath   = "ScriptableObjects/City_SO";

        public static Dictionary<uint, City_Data>      AllCityData       = new();
        static        uint                                _lastUnusedCityID = 1;
        public static Dictionary<uint, City_Component> AllCityComponents = new();
        
        static City_SO _city_SO;
        static City_SO City_SO => _city_SO ??= _getOrCreateCity_SO();

        public void SaveData(SaveData saveData) =>
            saveData.SavedCityData = new SavedCityData(City_SO.Save_SO());

        public void LoadData(SaveData saveData)
        {
            if (saveData is null)
            {
                //Debug.Log("No SaveData found in LoadData.");
                return;
            }

            if (saveData.SavedCityData == null)
            {
                //Debug.Log("No SavedCityData found in SaveData.");
                return;
            }

            if (saveData.SavedCityData.AllCityData == null)
            {
                //Debug.Log("No AllCityData found in SavedCityData.");
                return;
            }

            if (saveData.SavedCityData.AllCityData.Length == 0)
            {
                //Debug.Log("AllCityData count is 0.");
                return;
            }

            City_SO.Load_SO(saveData.SavedCityData.AllCityData);
        }

        public static void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerCity += _initialise;
        }

        static void _initialise()
        {
            City_SO.PopulateSceneCities();
        }
        
        public static City_Data GetCity_Data(uint cityID)
        {
            return City_SO.GetCity_Data(cityID);
        }
        
        public static City_Component GetCity_Component(uint cityID)
        {
            return City_SO.GetCity_Component(cityID);
        }
        
        static City_SO _getOrCreateCity_SO()
        {
            var city_SO = Resources.Load<City_SO>(_city_SOPath);
            
            if (city_SO is not null) return city_SO;
            
            city_SO = ScriptableObject.CreateInstance<City_SO>();
            AssetDatabase.CreateAsset(city_SO, $"Assets/Resources/{_city_SOPath}");
            AssetDatabase.SaveAssets();
            
            return city_SO;
        }

        public static City_Component GetNearestCity(Vector3 position)
        {
            City_Component nearestCity = null;

            var nearestDistance = float.MaxValue;

            foreach (var city in City_SO.Cities)
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
            return City_SO.GetUnusedCityID();
        }
    }
}
