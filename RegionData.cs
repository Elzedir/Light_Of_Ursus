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

    public FactionName Faction;
    public List<CityData> AllCityData;

    public void InitialiseRegionData()
    {
        foreach (var city in GetAllCitiesWithinBounds())
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

    public List<CityComponent> GetAllCitiesWithinBounds()
    {
        BoxCollider regionArea = Manager_Region.GetRegion(RegionID).RegionArea;
        Vector3 center = regionArea.transform.TransformPoint(regionArea.center);
        Vector3 halfExtents = regionArea.size * 0.5f;
        Quaternion orientation = regionArea.transform.rotation;

        List<CityComponent> cityComponents = new List<CityComponent>();

        foreach (var collider in Physics.OverlapBox(center, halfExtents, orientation))
        {
            CityComponent cityComponent = collider.GetComponent<CityComponent>();
            if (cityComponent != null)
            {
                cityComponents.Add(cityComponent);
            }
        }

        return cityComponents;
    }
}

[CustomPropertyDrawer(typeof(RegionData))]
public class RegionData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var cityNameProp = property.FindPropertyRelative("RegionName");
        label.text = !string.IsNullOrEmpty(cityNameProp.stringValue) ? cityNameProp.stringValue : "No Name";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

    }
}