using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class CityData
{
    public int CityID;
    public string CityName;
    public int CityFactionID;
    public int RegionID;

    public string CityDescription;

    public DisplayPopulation Population;
    public ProsperityData ProsperityData;

    public List<JobsiteData> AllJobsiteData;

    public void InitialiseCityData(int regionID)
    {
        RegionID = regionID;

        var city = Manager_City.GetCity(CityID);
            
        city.Initialise();

        ProsperityData = new ProsperityData(city.gameObject);

        foreach (var jobsite in city.AllJobsitesInCity)
        {
            if (!AllJobsiteData.Any(j => j.JobsiteID == jobsite.JobsiteData.JobsiteID))
            {
                Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteName} with ID: {jobsite.JobsiteData.JobsiteID} was not in AllJobsiteData");
                AllJobsiteData.Add(jobsite.JobsiteData);
            }

            jobsite.SetJobsiteData(Manager_Jobsite.GetJobsiteData(jobsiteID: jobsite.JobsiteData.JobsiteID, cityID: CityID));
        }

        for (int i = 0; i < AllJobsiteData.Count; i++)
        {
            AllJobsiteData[i].InitialiseJobsiteData(CityID);
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
public class DisplayPopulation
{
    public float CurrentPopulation;
    public float MaxPopulation;
    public float ExpectedPopulation;

    public List<ActorData> AllCitizens;

    public void DisplayCurrentPopulation()
    {
        CurrentPopulation = AllCitizens.Count;
    }

    public void CalculateExpectedPopulation()
    {
        // Calculate expected population
    }

    public void AddCitizen(ActorData citizenData)
    {
        if (AllCitizens.Any(c => c.ActorID == citizenData.ActorID)) { Debug.Log($"Citizen: {citizenData.ActorID}: {citizenData.ActorName.GetName()} already exists in AllCitizens."); return; }

        AllCitizens.Add(citizenData);
        DisplayCurrentPopulation();
    }

    public void RemoveCitizen(int actorID)
    {
        if (!AllCitizens.Any(c => c.ActorID == actorID)) { Debug.Log($"CitizenID: {actorID} does not exist in AllCitizens."); return; }

        AllCitizens.Remove(AllCitizens.FirstOrDefault(c => c.ActorID == actorID));
        DisplayCurrentPopulation();
    }
}