using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class CityData
{
    public void SaveData()
    {
        Population.SaveData();
    }

    public void LoadData()
    {
        Population.LoadData();
    }

    public int CityID;
    public string CityName;
    public int CityFactionID;
    public int RegionID;

    public string CityDescription;

    public PopulationData Population;
    public ProsperityData ProsperityData;
    public List<int> AllJobsiteIDs;

    public void InitialiseCityData()
    {
        var city = Manager_City.GetCity(CityID);
            
        city.Initialise();

        foreach (var jobsite in city.AllJobsitesInCity)
        {
            if (!AllJobsiteIDs.Contains(jobsite.JobsiteData.JobsiteID))
            {
                Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteID}: {jobsite.JobsiteData.JobsiteName} was not in AllJobsiteIDs");
                AllJobsiteIDs.Add(jobsite.JobsiteData.JobsiteID);
            }
        }
    }
}


[CustomPropertyDrawer(typeof(CityData))]
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
public class PopulationData
{
    public void SaveData()
    {
        AllCitizenIDList = AllCitizenIDs.ToList();
    }

    public void LoadData()
    {
        AllCitizenIDs = new HashSet<int>(AllCitizenIDList);
    }

    public float CurrentPopulation;
    public float MaxPopulation;
    public float ExpectedPopulation;
    public List<int> AllCitizenIDList;

    public HashSet<int> AllCitizenIDs = new();

    public void DisplayCurrentPopulation()
    {
        CurrentPopulation = AllCitizenIDs.Count;
    }

    public void CalculateExpectedPopulation()
    {
        // Calculate expected population
    }

    public void AddCitizen(int citizenID)
    {
        if (AllCitizenIDs.Contains(citizenID))
        {
            Debug.Log($"CitizenID: {citizenID} already exists in AllCitizens.");
            return;
        }

        AllCitizenIDs.Add(citizenID);
        DisplayCurrentPopulation();
    }

    public void RemoveCitizen(int actorID)
    {
        if (!AllCitizenIDs.Contains(actorID))
        {
            Debug.Log($"CitizenID: {actorID} does not exist in AllCitizens.");
            return;
        }

        AllCitizenIDs.Remove(actorID);
        DisplayCurrentPopulation();
    }
}