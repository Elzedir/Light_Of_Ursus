using System;
using System.Collections.Generic;
using Actor;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [Serializable]
    public class RegionData
    {
        public uint   RegionID;
        public string RegionName;
        public int    RegionFactionID;

        public string RegionDescription;

        public ProsperityData ProsperityData;

        public FactionName Faction;
        public List<uint>  AllCityIDs;

        public void InitialiseRegionData()
        {
            var region = Region_Manager.GetRegion(RegionID);

            region.Initialise();

            foreach (var city in region.AllCitiesInRegion)
            {
                if (!AllCityIDs.Contains(city.CityData.CityID))
                {
                    //Debug.Log($"City: {city.CityData.CityID}: {city.CityData.CityName} was not in AllCityData");
                    AllCityIDs.Add(city.CityData.CityID);
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(RegionData))]
    public class RegionData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var regionName = property.FindPropertyRelative("RegionName");
            label.text = !string.IsNullOrEmpty(regionName.stringValue) ? regionName.stringValue : "Unnamed Region";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }
}