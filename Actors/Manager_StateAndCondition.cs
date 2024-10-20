using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class Manager_StateAndCondition
{
    public static Dictionary<StateName, State> AllStates = new();
    public static Dictionary<ConditionName, Condition> AllConditions = new();

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

    static void _addCondition(Condition condition)
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
        Actor.ActorData.StatsAndAbilities.Actor_Conditions.Tick();
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
    public float DefaultConditionDuration;
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
public class Actor_Conditions : DataSubClass
{
    public Actor_Conditions(uint actorID) : base(actorID) { }

    public Dictionary<ConditionName, float> CurrentConditions = new();
    protected override bool _priorityChangeNeeded(object conditionName) => (ConditionName)conditionName != ConditionName.None;

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

        var condition = Manager_StateAndCondition.GetCondition(conditionName);

        if (condition == null) return;

        CurrentConditions[conditionName] = condition.DefaultConditionDuration;

        _priorityChangeCheck(conditionName);
    }

    public void SetConditionTimer(ConditionName conditionName, float timer)
    {
        CurrentConditions[conditionName] = timer;

        _priorityChangeCheck(conditionName);
    }

    public void RemoveCondition(ConditionName conditionName)
    {
        if (!CurrentConditions.ContainsKey(conditionName)) return;

        CurrentConditions.Remove(conditionName);

        _priorityChangeCheck(conditionName);
    }

    protected override void _setOnDataChange() => OnDataChange = (Dictionary<ActionName, Dictionary<PriorityParameter, object>> actionsToChange)
    => Actor.PriorityComponent.OnConditionChange(actionsToChange);

    protected override Dictionary<ActionName, Dictionary<PriorityParameter, object>> _getActionsToChange(object dataChanged)
    {
        var conditionName = (ConditionName)dataChanged;

        if (!ActionsAndParameters.TryGetValue(conditionName, out var actionsAndImportance))
        {
            Debug.LogError($"Condition: {conditionName} is not in ActionsToUpdateOnDataChange list");
            return null;
        }

        var actionsToChange = new Dictionary<ActionName, Dictionary<PriorityParameter, object>>();

        foreach (var action in actionsAndImportance)
        {
            actionsToChange.Add(action.Key, action.Value);
        }

        return actionsToChange;
    }

    public static Dictionary<ConditionName, Dictionary<ActionName, Dictionary<PriorityParameter, object>>> ActionsAndParameters = new()
    {
        { ConditionName.Paralysed, new Dictionary<ActionName, Dictionary<PriorityParameter, object>>
            {
                { ActionName.Move, new Dictionary<PriorityParameter, object>
                    {
                        { PriorityParameter.PriorityImportance, PriorityImportance.Critical }
                    }
                },
            }
        },
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

public class Actor_States : DataSubClass
{
    public Actor_States(uint actorID) : base(actorID) { }
    public Dictionary<StateName, bool> CurrentStates = new();
    StateName _changedStateName;

    public void SetState(StateName stateName, bool state)
    {
        CurrentStates[stateName] = state;

        _stateChanged(stateName);
    }

    void _stateChanged(StateName conditionName)
    {
        _changedStateName = conditionName;

        _priorityChangeCheck();

        _changedStateName = StateName.None;
    }

    protected override bool _priorityChangeNeeded() => _changedStateName != StateName.None;

    public static Dictionary<StateName, List<ActionName>> ActionsToUpdateOnChange = new()
    {
        { StateName.Hostile, new List<ActionName>
            {
                ActionName.Move
            }
        },
    };
}