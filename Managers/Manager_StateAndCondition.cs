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
            _initialiseStates();
            _initialiseConditions();
        }

        static void _initialiseStates()
        {
            _addState(new State { StateName = StateName.Alive });
        }
        
        static void _initialiseConditions()
        {
            _addCondition(new Condition_Master(ConditionName.Inspired, 100, 300));   
        }

        static void _addState(State state)
        {
            if (state != null && _allStates.TryAdd(state.StateName, state)) return;
            
            Debug.LogError($"State: {state} is null or exists in AllStates.");
        }

        static void _addCondition(Condition_Master condition)
        {
            if (condition != null && _allConditions.TryAdd(condition.ConditionName, condition)) return;
            
            Debug.LogError($"Condition: {condition} is null or exists in AllConditions.");
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

    public class State_Master
    {
        public readonly StateName StateName;
        public readonly bool      DefaultState;
        
        public State_Master(StateName stateName, bool defaultState)
        {
            StateName    = stateName;
            DefaultState = defaultState;
        }
    }

    [Serializable]
    public class State // Permanent or Perpetual thing
    {
        public StateName StateName;
    }

    public class Condition_Master
    {
        public readonly ConditionName ConditionName;
        public readonly float         DefaultConditionDuration;
        public readonly float         MaxConditionDuration;

        public Condition_Master(ConditionName conditionName, float defaultConditionDuration, float maxConditionDuration)
        {
            ConditionName            = conditionName;
            DefaultConditionDuration = defaultConditionDuration;
            MaxConditionDuration     = maxConditionDuration;
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
    
    public class StatesAndConditions
    {
        public StatesAndConditions(uint actorID)
        {
            Actor_States     = new Actor_States(actorID);
            
            Actor_Conditions = new Actor_Conditions(actorID);
        }
        
        public Actor_States     Actor_States;
        public void SetActorStates(Actor_States actorStates) => Actor_States = actorStates;
        
        public Actor_Conditions Actor_Conditions;
        public void SetActorConditions(Actor_Conditions actorConditions) => Actor_Conditions = actorConditions;
    }

    [Serializable]
    public class Actor_Conditions : PriorityData
    {
        public Actor_Conditions(uint actorID) : base(actorID, ComponentType.Actor)
        {
            CurrentConditions                   =  new ObservableDictionary<ConditionName, float>();
            CurrentConditions.DictionaryChanged += OnConditionChanged;
            
            Manager_TickRate.RegisterTickable(_onTick, TickRate.OneSecond);
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;
        public override PriorityComponent        PriorityComponent => _priorityComponent ??= ActorReference.Actor.PriorityComponent;


        public ObservableDictionary<ConditionName, float> CurrentConditions;
        
        void OnConditionChanged(ConditionName conditionName)
        {
            _priorityChangeCheck(DataChanged.ChangedCondition);
        }

        void _onTick()
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
                CurrentConditions[conditionName] += Manager_StateAndCondition.GetCondition_Master(conditionName).DefaultConditionDuration;
                return;
            }

            var condition_Master = Manager_StateAndCondition.GetCondition_Master(conditionName);

            if (condition_Master == null) return;

            CurrentConditions[conditionName] = condition_Master.DefaultConditionDuration;
        }
        
        void _updateExistingCondition(ConditionName conditionName)
        {
            if (!CurrentConditions.ContainsKey(conditionName))
            {
                Debug.LogError($"Cannot call this function for a condition that doesn't exist.");
                return;
            }
            
            var conditionMaster = Manager_StateAndCondition.GetCondition_Master(conditionName);

            CurrentConditions[conditionName] += Math.Min(
                CurrentConditions[conditionName] + conditionMaster.DefaultConditionDuration,
                conditionMaster.MaxConditionDuration);
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

        protected override Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList
        {
            get;
            set;
        } = new();
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
        public Actor_States(uint actorID) : base(actorID, ComponentType.Actor)
        {
            _currentStates                   =  new ObservableDictionary<StateName, bool>();
            _currentStates.DictionaryChanged += OnStateChanged;
        }

        ComponentReference_Actor _actorReference    => Reference as ComponentReference_Actor;
        public override PriorityComponent        PriorityComponent => _priorityComponent ??= _actorReference.Actor.PriorityComponent;

        readonly ObservableDictionary<StateName, bool> _currentStates;
        
        void OnStateChanged(StateName stateName)
        {
            _priorityChangeCheck(DataChanged.ChangedState);
        }

        public void SetState(StateName stateName, bool state)
        {
            if (!_currentStates.TryAdd(stateName, state)) _currentStates[stateName] = state;
        }
        
        public bool GetState(StateName stateName)
        {
            if (_currentStates.TryGetValue(stateName, out var state)) return state;
            
            Debug.LogError($"State: {stateName} not found in CurrentStates.");
            return false;
        }
        
        protected override bool _priorityChangeNeeded(object dataChanged) => (StateName)dataChanged != StateName.None;

        protected override Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList
        {
            get;
            set;
        } = new();
    }
}