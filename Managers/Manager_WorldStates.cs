using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_WorldStates
{
    public static List<WorldState_Data_SO> AllWorldStates = new();

    public static WorldStateStatus CheckWorldState(WorldState_Data_SO worldStateCheck = null)
    {
        foreach (WorldState_Data_SO worldState in AllWorldStates)
        {
            if (worldStateCheck == worldState || worldStateCheck == null)
            {
                return worldState.Status;
            }
        }

        return WorldStateStatus.Default;
    }

    public static List<Action> AllWorldStateEvents = new();

    public static void GetWorldStateEvent(string name)
    {
        
    }

    public static void Initialise()
    {

    }

    static void _testWorldStateEvent()
    {

    }
}

public enum WorldStateStatus 
{ 
    Default,
    Alive, Dead,
    Free, Captured,
    Intact, Destroyed,
    Repaired, Damaged
}

[CreateAssetMenu(fileName = "WorldStateData", menuName = "WorldStateData", order = 0)]

public class WorldState_Data_SO : ScriptableObject
{
    public string WorldStateName;
    public int WorldstateID;
    public WorldStateStatus Status;

    public List<Action> WorldStateEvents = new();
}
