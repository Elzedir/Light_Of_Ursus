using System;
using System.Collections.Generic;
using Actors;
using Priority;
using Tools;
using UnityEngine;

namespace Managers
{
    public abstract class Manager_StateAndCondition
    {
        static readonly Dictionary<StateName, State>                _allStates     = new();
        static readonly Dictionary<ConditionName, Condition_Master> _allConditions = new();

        public static void Initialise()
        {

        }

        static void _addState(State state)
        {
            if (state != null && _allStates.TryAdd(state.StateName, state)) return;
            
            Debug.LogError($"State: {state} is null or exists in AllStates.");
            return;
        }

        static void _addCondition(Condition_Master condition)
        {
            if (condition != null && _allConditions.TryAdd(condition.ConditionName, condition)) return;
            
            Debug.LogError($"Condition: {condition} is null or exists in AllConditions.");
            return;
        }

        public static State GetState(StateName stateName)
        {
            if (_allStates.TryGetValue(stateName, out var state)) return state;
            
            Debug.LogError($"State: {stateName} is not in AllStates list");
            return null;
        }

        public static Condition_Master GetCondition_Master(ConditionName conditionName)
        {
            if (_allConditions.TryGetValue(conditionName, out var master)) return master;
            
            Debug.LogError($"Condition: {conditionName} is not in AllConditions list");
            return null;

        }
    }

    public class StateAndConditionComponent
    {
        readonly uint         _actorID;
        ActorComponent        _actor;
        public ActorComponent Actor => _actor ??= Manager_Actor.GetActor(_actorID);

        public StateAndConditionComponent(uint actorID) => _actorID = actorID;

        public void Initialise()
        {
            Manager_TickRate.RegisterTickable(_onTick, TickRate.OneSecond);
        }

        void _onTick()
        {
            Actor.ActorData.StatsAndAbilities.Actor_Conditions.Tick();
        }
    }

    [Serializable]
    public class State // Permanent or Perpetual thing
    {
        public StateName StateName;
    }

    public abstract class Condition_Master
    {
        public readonly ConditionName ConditionName;
        public readonly float         DefaultConditionDuration;

        protected Condition_Master(ConditionName conditionName, float defaultConditionDuration)
        {
            ConditionName                 = conditionName;
            DefaultConditionDuration = defaultConditionDuration;
        }
    }

    [Serializable]
    public class Condition // Temporary and tickable thing
    {
        public ConditionName ConditionName;
        public float         ConditionDuration;

        public Condition(ConditionName conditionName, float conditionDuration)
        {
            ConditionName     = conditionName;
            ConditionDuration = conditionDuration;
        }
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
        public Actor_Conditions(uint actorID) : base(actorID, ComponentType.Actor)
        {
            CurrentConditions                   =  new ObservableDictionary<ConditionName, float>();
            CurrentConditions.DictionaryChanged += OnConditionChanged;
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;
        public override PriorityComponent        PriorityComponent => _priorityComponent ??= ActorReference.Actor.PriorityComponent;


        public ObservableDictionary<ConditionName, float> CurrentConditions;
        
        void OnConditionChanged()
        {
            _priorityChangeCheck(DataChanged.ChangedCondition);
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

        protected override Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList { get; set; } = new()
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
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;
        public override PriorityComponent        PriorityComponent { get => _priorityComponent ??= ActorReference.Actor.PriorityComponent; }

        public Dictionary<StateName, bool> CurrentStates = new();    

        public void SetState(StateName stateName, bool state)
        {
            CurrentStates[stateName] = state;

            _priorityChangeCheck(DataChanged.ChangedState);
        }

        protected override bool _priorityChangeNeeded(object dataChanged) => (StateName)dataChanged != StateName.None;

        protected override Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList { get; set; } = new()
        {
        
        };
    }
}