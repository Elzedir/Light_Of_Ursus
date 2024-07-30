using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New City", menuName = "Region/City Data")]
public class City_Data_SO : ScriptableObject
{
    public string CityID;
    public string CityName;
    public string CityDescription;

    public DisplayProsperity Prosperity;
    public int ExpectedPopulation;
    public List<DisplayCitizen> Citizens = new();

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

