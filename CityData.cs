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
    public DisplayProsperity Prosperity;

    public List<JobsiteData> AllJobsiteData;

    public void InitialiseCityData(int regionID)
    {
        RegionID = regionID;

        var city = Manager_City.GetCity(CityID);
            
        city.Initialise();

        foreach (var jobsite in city.AllJobsitesInCity)
        {
            if (!AllJobsiteData.Any(j => j.JobsiteID == jobsite.JobsiteData.JobsiteID))
            {
                Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteID} with ID: {jobsite.JobsiteData.JobsiteID} was not in AllJobsiteData");
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
public class DisplayProsperity
{
    public float CurrentProsperity;
}

[Serializable]
public class DisplayPopulation
{
    public float CurrentPopulation;
    public float MaxPopulation;
    public float ExpectedPopulation;

    public List<DisplayCitizen> AllCitizens;

    public void DisplayCurrentPopulation()
    {
        CurrentPopulation = AllCitizens.Count;
    }

    public void CalculateExpectedPopulation()
    {
        // Calculate expected population
    }

    public void AddCitizen(DisplayCitizen citizen)
    {
        if (AllCitizens.Contains(citizen)) throw new ArgumentException($"Citizen: {citizen.CitizenName} already exists in AllCitizens.");

        AllCitizens.Add(citizen);
        DisplayCurrentPopulation();
    }

    public void RemoveCitizen(int actorID)
    {
        if (!AllCitizens.Any(c => c.CitizenID == actorID)) throw new ArgumentException($"CitizenID: {actorID} does not exist in AllCitizens.");

        AllCitizens.Remove(AllCitizens.FirstOrDefault(c => c.CitizenID == actorID));
        DisplayCurrentPopulation();
    }
}

[Serializable]
public class DisplayCitizen
{
    public int CitizenID;
    public string CitizenName;
    public ActorData CitizenData;

    public DisplayCitizen(ActorData citizenData)
    {
        CitizenData = citizenData;

        if (CitizenData == null) throw new ArgumentException("ActorData is null");

        CitizenID = CitizenData.ActorID;
        CitizenName = CitizenData.ActorName.GetName();
    }

    public void UpdateDisplayCitizen(Actor_Base actor)
    {
        CitizenID = actor.ActorData.ActorID;
        CitizenName = actor.ActorData.ActorName.GetName();
    }
}

[CustomPropertyDrawer(typeof(DisplayCitizen))]
public class DisplayCitizen_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var citizenName = property.FindPropertyRelative("CitizenName");
        label.text = !string.IsNullOrEmpty(citizenName.stringValue) ? citizenName.stringValue : "Unnamed Citizen";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}