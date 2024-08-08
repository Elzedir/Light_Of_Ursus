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

    public bool OverwriteDataInCity = false;

    public string CityDescription;

    public DisplayPopulation Population;
    public DisplayProsperity Prosperity;

    public DisplayCareers Careers;
}

[Serializable]
public class DisplayCareers
{
    public List<DisplayCareer> AllCareers;

    public void AddToCareerList(DisplayCareer career)
    {
        if (AllCareers.Any(c => c.CareerName == career.CareerName)) throw new ArgumentException($"Career: {career.CareerName} already exists in AllCareers.");

        AllCareers.Add(career);
    }

    public void RemoveFromCareerList(DisplayCareer career)
    {
        if (!AllCareers.Any(c => c.CareerName == career.CareerName)) throw new ArgumentException($"Career: {career.CareerName} does not exist in AllCareers.");

        AllCareers.Remove(career);
    }
}

[Serializable]
public class DisplayCareer
{
    public CareerName CareerName;
    public List<DisplayJobsite> Jobsites;

    public DisplayCareer(CareerName careerName, List<DisplayJobsite> jobsites)
    {
        CareerName = careerName;
        Jobsites = jobsites;
    }
}

[Serializable]
public class DisplayJobsite
{
    public Jobsite_Base Jobsite;
    public List<DisplayCitizen> Employees;

    public DisplayJobsite(Jobsite_Base jobsite, List<DisplayCitizen> employees)
    {
        Jobsite = jobsite;
        Employees = employees;
    }
}

[Serializable]
public class DisplayCitizen
{
    public int ActorID;
    public string CitizenName;
    public ActorData ActorData;

    public DisplayCitizen(ActorData actorData)
    {
        ActorData = actorData;

        if (ActorData == null) throw new ArgumentException("ActorData is null");

        ActorID = ActorData.ActorID;
        CitizenName = ActorData.ActorName.GetName();
    }

    public void UpdateDisplayCitizen(Actor_Base actor)
    {
        ActorID = actor.ActorData.ActorID;
        CitizenName = actor.ActorData.ActorName.GetName();
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
        if (AllCitizens.Contains(citizen)) throw new ArgumentException($"Citizen: {citizen.CitizenName} is already in AllCitizens.");

        AllCitizens.Add(citizen);
    }
}

[CustomPropertyDrawer(typeof(CityData))]
public class CityData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var cityNameProp = property.FindPropertyRelative("CityName");
        label.text = !string.IsNullOrEmpty(cityNameProp.stringValue) ? cityNameProp.stringValue : "No Name";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

[CustomPropertyDrawer(typeof(DisplayCitizen))]
public class DisplayCitizen_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var cityNameProp = property.FindPropertyRelative("CitizenName");
        label.text = !string.IsNullOrEmpty(cityNameProp.stringValue) ? cityNameProp.stringValue : "No Name";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}