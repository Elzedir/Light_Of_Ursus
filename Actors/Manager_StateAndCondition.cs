using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public enum StateName
{
    Alive,
    Dead,
    
    Alerted,
    Hostile,

    Jumping,

    Berserk, 
    
    OnFire,
    InFire,

    Talkable,
    Talking,

    DodgeAvailable,
    Dodging,

    BlockAvailable,
    Blocking,
}

public enum ConditionName
{
    None,
    Depressed,
    Inspired,
}

public class Manager_StateAndCondition
{
    public static Dictionary<StateName, State> AllStates = new();
    public static Dictionary<ConditionName, Condition> AllConditions = new();

    public static void Initialise()
    {

    }

    static void _addState(State state)
    {
        if (state == null && AllStates.ContainsKey(state.StateName)) throw new ArgumentException($"Title: {state} is null or exists in AllTitles.");

        AllStates.Add(state.StateName, state);
    }

    static void _addCondition(Condition condition)
    {
        if (condition == null && AllConditions.ContainsKey(condition.ConditionName)) throw new ArgumentException($"Title: {condition} is null or exists in AllTitles.");

        AllConditions.Add(condition.ConditionName, condition);
    }

    public static void GetState(StateName stateName, out State state)
    {
        AllStates.TryGetValue(stateName, out state);
    }

    public static void GetCondition(ConditionName conditionName, out Condition condition)
    {
        AllConditions.TryGetValue(conditionName, out condition);
    }
}

public class StateAndConditionComponent : ITickable
{
    public Dictionary<StateName, float> CurrentStates = new();
    public Dictionary<ConditionName, float> CurrentConditions = new();

    public bool CanGetPregnant { get; private set; } // FInd where to put, maybe actorData

    public void SetState(StateName stateName, float timer)
    {
        CurrentStates[stateName] = timer;
    }

    public void SetCondition(ConditionName conditionName, float timer)
    {
        CurrentConditions[conditionName] = timer;
    }

    public void OnTick()
    {
        for (int i = 0; i < CurrentStates.Count; i++)
        {
            //if (CurrentStates[i] <= 0) continue;
            //CurrentStates[i]--;
            // And execute state effect
        }

        for (int i = 0; i < CurrentConditions.Count; i++)
        {
            //if (CurrentConditions[i] <= 0) continue;
            //CurrentStates[i]--;
            // And execute state effect
        }
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneSecond;
    }
}

[Serializable]
public class State
{
    public StateName StateName;
}

[Serializable]
public class Condition
{
    public ConditionName ConditionName;
}

[Serializable]
public class Actor_States_And_Conditions
{
    
}