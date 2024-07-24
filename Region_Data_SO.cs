using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DisplayCitySOData
{
    public string CityName;
    public City_Data_SO CityData;
}

[Serializable]
public class DisplayWorldStateSOData
{
    public WorldStateName CurrentWorldState;
    public WorldState_Data_SO WorldStateData;
}

[CreateAssetMenu(fileName = "New Region", menuName = "Region/Region Data")]
public class Region_Data_SO : ScriptableObject
{
    public string RegionID;
    public string RegionName;
    public string RegionDescription;

    public FactionName Faction;

    public List<DisplayWorldStateSOData> WorldStates;

    public List<DisplayCitySOData> CityList = new();
}

[CreateAssetMenu(fileName = "New City", menuName = "Region/City Data")]
public class City_Data_SO : ScriptableObject
{
    public string CityID;
    public string CityName;
    public string CityDescription;

    public int CurrentPopulation;
    public int ExpectedPopulation;

    public List<DisplayWorldStateSOData> WorldStates;

    public List<Career> CityCareers = new();
}