using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Tools;
using UnityEditor;
using UnityEngine;

namespace City
{
    [Serializable]
    public class City_Data : Data_Class
    {
        public uint   CityID;
        public string CityName;
        public uint   CityFactionID;
        public uint   RegionID;
        public string CityDescription;

        public PopulationData Population;
        
        public ProsperityData ProsperityData;
        
        public List<uint>     AllJobsiteIDs;

        public void InitialiseCityData()
        {
            var city = City_Manager.GetCity_Component(CityID);

            foreach (var jobsite in city.AllJobSitesInCity)
            {
                if (!AllJobsiteIDs.Contains(jobsite.JobSiteData.JobSiteID))
                {
                    //Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteID}: {jobsite.JobsiteData.JobsiteName} was not in AllJobsiteIDs");
                    AllJobsiteIDs.Add(jobsite.JobSiteData.JobSiteID);
                }
            }
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "City Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"City ID: {CityID}",
                        $"City Name: {CityName}",
                        $"City Faction ID: {CityFactionID}",
                        $"Region ID: {RegionID}",
                        $"City Description: {CityDescription}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Population Data",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    subData: Population.DataSO_Object(toggleMissingDataDebugs).SubData));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return new Data_Display(
                title: $"{CityID}: {CityName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
        }
    }


    [CustomPropertyDrawer(typeof(City_Data))]
    public class CityData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var cityName = property.FindPropertyRelative("CityName");
            label.text = !string.IsNullOrEmpty(cityName.stringValue) ? cityName.stringValue : "Unnamed City";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    [Serializable]
    public class PopulationData : Data_Class
    {
        public float      CurrentPopulation;
        public float      MaxPopulation;
        public float      ExpectedPopulation;
        public List<uint> AllCitizenIDList;

        public HashSet<uint> AllCitizenIDs = new();
        
        public PopulationData(List<uint> allCitizenIDList, float maxPopulation, float expectedPopulation)
        {
            foreach (var citizenID in allCitizenIDList)
            {
                AllCitizenIDs.Add(citizenID);
            }
            
            CurrentPopulation = allCitizenIDList.Count;
            MaxPopulation     = maxPopulation;
            ExpectedPopulation = expectedPopulation;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Population Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Current Population: {CurrentPopulation}",
                        $"Max Population: {MaxPopulation}",
                        $"Expected Population: {ExpectedPopulation}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Citizen IDs",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: AllCitizenIDs.Select(citizenID => citizenID.ToString()).ToList()));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return new Data_Display(
                title: "Population Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
        }
    }
}