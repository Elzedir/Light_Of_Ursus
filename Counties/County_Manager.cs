using System;
using System.Collections.Generic;
using DateAndTime;
using UnityEngine;

namespace Counties
{
    public class County_Manager : MonoBehaviour
    {
        const string c_county_SOPath = "ScriptableObjects/County_SO";

        static County_SO s_allCounties;
        static County_SO AllCounties => s_allCounties ??= _getCounty_SO();
        
        public static County_Data GetCounty_Data(ulong countyID)
        {
            return AllCounties.GetCounty_Data(countyID).Data_Object;
        }

        public static County_Data GetCounty_DataFromName(County_Component county_Component)
        {
            return AllCounties.GetDataFromName(county_Component.name)?.Data_Object;
        }
        
        public static County_Component GetCounty_Component(ulong countyID)
        {
            return AllCounties.GetCounty_Component(countyID);
        }
        
        public static List<ulong> GetAllCountyIDs() => AllCounties.GetAllDataIDs(); 
        
        static County_SO _getCounty_SO()
        {
            var county_SO = Resources.Load<County_SO>(c_county_SOPath);
            
            if (county_SO is not null) return county_SO;
            
            Debug.LogError("County_SO not found. Creating temporary County_SO.");
            county_SO = ScriptableObject.CreateInstance<County_SO>();
            
            return county_SO;
        }

        public static County_Component GetNearestCounty(Vector3 position)
        {
            County_Component nearestCounty = null;

            var nearestDistance = float.PositiveInfinity;

            foreach (var county in AllCounties.CountyComponents.Values)
            {
                var distance = Vector3.Distance(position, county.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestCounty  = county;
                nearestDistance = distance;
            }

            return nearestCounty;
        }
        
        public static void ClearSOData()
        {
            AllCounties.ClearSOData();
        }

        public static void RegisterOnProgressDay()
        {
            Manager_DateAndTime.OnProgressDay += _onProgressDay;
        }

        public void OnDestroy()
        {
            Manager_DateAndTime.OnProgressDay -= _onProgressDay;
        }

        static void _onProgressDay()
        {
            foreach (var county in AllCounties.CountyComponents.Values)
            {
                county.County_Data.OnProgressDay();
            }
        }
    }
}