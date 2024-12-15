using System;
using System.Collections.Generic;
using Managers;
using Relationships;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [Serializable]
    public class Region_Data : Data_Class
    {
        public uint   RegionID;
        public string RegionName;
        public int    RegionFactionID;

        public string RegionDescription;

        public ProsperityData ProsperityData;

        public FactionName Faction;
        public List<uint>  AllCityIDs;
        
        public Region_Data(uint regionID, string regionName, string regionDescription, int regionFactionID, ProsperityData prosperityData = null)
        {
            RegionID          = regionID;
            RegionName        = regionName;
            RegionDescription = regionDescription;
            RegionFactionID   = regionFactionID;

            ProsperityData = new ProsperityData(prosperityData);
            AllCityIDs     = new List<uint>();
        }

        public void InitialiseRegionData()
        {
            var region = Region_Manager.GetRegion_Component(RegionID);

            foreach (var city in region.AllCitiesInRegion)
            {
                if (!AllCityIDs.Contains(city.CityData.CityID))
                {
                    //Debug.Log($"City: {city.CityData.CityID}: {city.CityData.CityName} was not in AllCityData");
                    AllCityIDs.Add(city.CityData.CityID);
                }
            }
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Region Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Region ID: {RegionID}",
                        $"Region Name: {RegionName}",
                        $"Region Faction ID: {RegionFactionID}",
                        $"Region Description: {RegionDescription}",
                        $"Prosperity Data: {ProsperityData}",
                        $"Faction: {Faction}",
                        $"All City IDs: {string.Join(", ", AllCityIDs)}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Base Region Data not found.");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Region Prosperity",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Current Prosperity: {ProsperityData.CurrentProsperity}",
                        $"Max Prosperity: {ProsperityData.MaxProsperity}",
                        $"Base Prosperity Growth: {ProsperityData.BaseProsperityGrowthPerDay}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: ProsperityData.");
            }

            return new Data_Display(
                title: "Base Region Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
        }
    }

    [CustomPropertyDrawer(typeof(Region_Data))]
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