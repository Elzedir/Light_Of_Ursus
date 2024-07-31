using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New City", menuName = "Region/City Data")]
public class City_Data_SO : ScriptableObject
{
    public string CityID;
    public string CityName;
    public string CityDescription;

    public DisplayPopulation Population;
    public DisplayProsperity Prosperity;

    public List<DisplayWorldStateSOData> WorldStates;

    public List<DisplayCityCareers> RequiredCityCareers = new();
}

[Serializable]
public class DisplayCityCareers
{
    public CareerName CareerName;
    public List<DisplayCitizen> CareerCitizens = new();
    public int CareerQuantity;
}

[Serializable]
public class DisplayCitizen
{
    public int CitizenActorID;
    public string CitizenName;

    public void UpdateDisplayCitizen(Actor_Base actor)
    {
        CitizenActorID = actor.ActorData.ActorID;
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
    public List<DisplayCitizen> AllCitizens = new();

    public float CurrentPopulation;
    public float MaxPopulation;
    public float ExpectedPopulation;

    public void DisplayCurrentPopulation()
    {
        CurrentPopulation = AllCitizens.Count;
    }

    public void CalculateExpectedPopulation()
    {
        // Calculate expected population
    }
}