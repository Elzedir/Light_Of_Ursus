using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class Manager_StateAndCondition
{
    public static Dictionary<StateName, State> AllStates = new();
    public static Dictionary<ConditionName, Condition_Master> AllConditions = new();

    public static void Initialise()
    {

    }

    static void _addState(State state)
    {
        if (state == null || AllStates.ContainsKey(state.StateName))
        {
            Debug.LogError($"State: {state} is null or exists in AllStates.");
            return;
        }

        AllStates.Add(state.StateName, state);
    }

    static void _addCondition(Condition_Master condition)
    {
        if (condition == null || AllConditions.ContainsKey(condition.ConditionName))
        {
            Debug.LogError($"Condition: {condition} is null or exists in AllConditions.");
            return;
        }

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

    public static Condition_Master GetCondition_Master(ConditionName conditionName)
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
        Actor.ActorData.StatsAndAbilities.Actor_Conditions.Tick();
    }
}

[Serializable]
public class State // Permanent or Perpetual thing
{
    public StateName StateName;
}

public class Condition_Master
{
    public ConditionName ConditionName;
    public float DefaultConditionDuration;
}

[Serializable]
public class Condition // Temprary and tickable thing
{
    public ConditionName ConditionName;
    public float ConditionDuration;
}

public enum ConditionName
{
    None,

    // Health
    Inspired,
    
    // Movement

    // Social
    Drunk,
    High,
    
    // Combat
    Bleeding,
    Drowning,
    Poisoned,
    Stunned,
    Paralysed,
    Blinded,
    Deafened,
    Silenced,
    Cursed,
    Charmed,
    Enraged,
    Frightened,
    Panicked,
    Confused,
    Dazed,
    Distracted,
    Dominated,
    Burning,
}

[Serializable]
public class Actor_Conditions : PriorityData
{
    public Actor_Conditions(uint actorID) : base(actorID, ComponentType.Actor) { }
    public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
    public override PriorityComponent PriorityComponent { get => _priorityComponent ??= ActorReference.Actor.PriorityComponent; }


    Dictionary<ConditionName, float> _currentConditions = new();
    public Dictionary<ConditionName, float> CurrentConditions 
    { 
        get { return _currentConditions; } set { _currentConditions = value; _priorityChangeCheck(DataChanged.ChangedCondition); } 
    }

    public void Tick()
    {
        foreach (var condition in CurrentConditions)
        {
            if (condition.Value <= 0)
            {
                RemoveCondition(condition.Key);
                continue;
            }

            CurrentConditions[condition.Key] -= 1;
        }
    }

    public void AddCondition(ConditionName conditionName)
    {
        if (CurrentConditions.ContainsKey(conditionName))
        {
            // CurrentConditions[conditionName] = 0; For now do nothing, but later can add to the total condition duration.
            return;
        }

        var condition_Master = Manager_StateAndCondition.GetCondition_Master(conditionName);

        if (condition_Master == null) return;

        CurrentConditions[conditionName] = condition_Master.DefaultConditionDuration;
    }

    public void SetConditionTimer(ConditionName conditionName, float timer)
    {
        CurrentConditions[conditionName] = timer;
    }

    public void RemoveCondition(ConditionName conditionName)
    {
        if (!CurrentConditions.ContainsKey(conditionName)) return;

        CurrentConditions.Remove(conditionName);
    }
    protected override bool _priorityChangeNeeded(object conditionName) => (ConditionName)conditionName != ConditionName.None;

    protected override Dictionary<DataChanged, List<PriorityParameter>> _priorityParameterList { get; set; } = new()
    {
        
    };
}

public enum StateName
{
    None, 

    Alive,

    CanBeDepressed,
    IsDepressed,

    CanDrown,
    IsDrowning,

    CanSuffocate,
    IsSuffocating,

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

public class Actor_States : PriorityData
{
    public Actor_States(uint actorID) : base(actorID, ComponentType.Actor) { }
    public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
    public override PriorityComponent PriorityComponent { get => _priorityComponent ??= ActorReference.Actor.PriorityComponent; }

    public Dictionary<StateName, bool> CurrentStates = new();    

    public void SetState(StateName stateName, bool state)
    {
        CurrentStates[stateName] = state;

        _priorityChangeCheck(DataChanged.ChangedState);
    }

    protected override bool _priorityChangeNeeded(object dataChanged) => (StateName)dataChanged != StateName.None;

    protected override Dictionary<DataChanged, List<PriorityParameter>> _priorityParameterList { get; set; } = new()
    {
        
    };
}