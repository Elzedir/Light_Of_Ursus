using System;
using System.Collections.Generic;
using UnityEngine;

public enum StateName
{
    Alive,

    CanReanimate,
    IsReanimated,
    
    Alerted,
    Hostile,

    CanJump,
    IsJumping,

    CanBerserk,
    IsBerserking, 
    
    InFire,
    OnFire,

    CanTalk,
    IsTalking,

    CanDodge,
    IsDodging,

    CanBlock,
    IsBlocking,

    CanGetPregnant,
    IsPregnant,
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
        if (state == null || AllStates.ContainsKey(state.StateName)) throw new ArgumentException($"Title: {state} is null or exists in AllTitles.");

        AllStates.Add(state.StateName, state);
    }

    static void _addCondition(Condition condition)
    {
        if (condition == null || AllConditions.ContainsKey(condition.ConditionName)) throw new ArgumentException($"Title: {condition} is null or exists in AllTitles.");

        AllConditions.Add(condition.ConditionName, condition);
    }

    public static State GetState(StateName stateName)
    {
        if (!AllStates.ContainsKey(stateName))
        {
            Debug.LogError($"State: {stateName} is not in AllStates list");
            return null;
        }
        return AllStates[stateName];
    }

    public static Condition GetCondition(ConditionName conditionName)
    {
        if (!AllConditions.ContainsKey(conditionName))
        {
            Debug.LogError($"Condition: {conditionName} is not in AllConditions list");
            return null;
        }

        return AllConditions[conditionName];
    }
}

public class StateAndConditionComponent : ITickable
{
    public uint ActorID;
    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }

    public StateAndConditionComponent(uint actorID) => ActorID = actorID;

    public void Initialise()
    {
        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneSecond);
    }

    public void OnTick()
    {
        Actor.ActorData.StatsAndAbilities.Actor_StatesAndConditions.Tick();
    }
}

[Serializable]
public class State // Permanent or Perpetual thing
{
    public StateName StateName;
}

[Serializable]
public class Condition // Temprary and tickable thing
{
    public ConditionName ConditionName;
}

[Serializable]
public class Actor_StatesAndConditions
{
    public uint ActorID;
    public Actor_StatesAndConditions(uint actorID) => ActorID = actorID;

    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }

    PriorityQueue _actorPriorityQueue;
    public PriorityQueue ActorPriorityQueue { get => _actorPriorityQueue ??=Actor.TaskComponent.PriorityQueue; }

    public Dictionary<StateName, bool> CurrentStates = new();
    public Dictionary<ConditionName, float> CurrentConditions = new();

    public void SetState(StateName stateName, bool state)
    {
        CurrentStates[stateName] = state;
    }

    public void AddCondition(ConditionName conditionName)
    {
        if (CurrentConditions.ContainsKey(conditionName))
        {
            CurrentConditions[conditionName] = 0;
            return;
        }

        var condition = Manager_StateAndCondition.GetCondition(conditionName);

        if (condition == null) return;

        CurrentConditions[conditionName] = 0;
    }

    public void SetCondition(ConditionName conditionName, float timer)
    {
        CurrentConditions[conditionName] = timer;
    }

    public void RemoveCondition(ConditionName conditionName)
    {
        if (!CurrentConditions.ContainsKey(conditionName)) return;

        CurrentConditions.Remove(conditionName);

        Modify priority
    }

    public void Tick()
    {
        foreach(var condition in CurrentConditions)
        {
            if (condition.Value <= 0)
            {
                RemoveCondition(condition.Key);
                continue;
            }

            CurrentConditions[condition.Key] -= 1;
        }
    }
}