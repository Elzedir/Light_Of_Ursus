using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Region", menuName = "Region/Region Data")]
public class Region_Data_SO : ScriptableObject
{
    public string RegionID;
    public string RegionName;
    public string RegionDescription;

    public FactionName Faction;

    public List<DisplayWorldStateSOData> WorldStates;

    public List<DisplayCitySOData> AllCities = new();
}

[Serializable]
public class DisplayCitySOData
{
    public string CityName;
    public City_Data_SO CityData;
}

[Serializable]
public class DisplayWorldStateSOData
{
    public string WorldStateName;
    public WorldStateStatus WorldStateStatus;
    public WorldState_Data_SO WorldStateData;
}