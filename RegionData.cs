using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class RegionData
{
    public int RegionID;
    public string RegionName;

    public bool OverwriteDataInRegion = true;

    public string RegionDescription;

    public DisplayProsperity Prosperity;

    public FactionName Faction;
    public List<CityData> AllCityData;

    public void InitialiseRegionData()
    {
        var region = Manager_Region.GetRegion(RegionID);

        region.Initialise();

        foreach (var city in region.AllCitiesInRegion)
        {
            if (!AllCityData.Any(c => c.CityID == city.CityData.CityID))
            {
                Debug.Log($"City: {city.CityData.CityName} with ID: {city.CityData.CityID} was not in AllCityData");
                AllCityData.Add(city.CityData);
            }

            city.SetCityData(Manager_City.GetCityDataFromID(RegionID, city.CityData.CityID));
        }

        for (int i = 0; i < AllCityData.Count; i++)
        {
            AllCityData[i].InitialiseCityData(RegionID);
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