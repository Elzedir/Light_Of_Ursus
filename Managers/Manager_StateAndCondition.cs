using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Priority;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public abstract class Manager_StateAndCondition
    {
        static readonly Dictionary<PrimaryStateName, State>                _allStates     = new();
        static readonly Dictionary<ConditionName, Condition_Master> _allConditions = new();

        public static void Initialise()
        {
            _initialiseStates();
            _initialiseConditions();
        }

        static void _initialiseStates()
        {
            _addState(new State { PrimaryStateName = PrimaryStateName.IsAlive });
        }
        
        static void _initialiseConditions()
        {
            _addCondition(new Condition_Master(ConditionName.Inspired, 100, 300));   
        }

        static void _addState(State state)
        {
            if (state != null && _allStates.TryAdd(state.PrimaryStateName, state)) return;
            
            Debug.LogError($"State: {state} is null or exists in AllStates.");
        }

        static void _addCondition(Condition_Master condition)
        {
            if (condition != null && _allConditions.TryAdd(condition.ConditionName, condition)) return;
            
            Debug.LogError($"Condition: {condition} is null or exists in AllConditions.");
        }

        public static State GetState(PrimaryStateName primaryStateName)
        {
            if (_allStates.TryGetValue(primaryStateName, out var state)) return state;
            
            Debug.LogError($"State: {primaryStateName} is not in AllStates list");
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
        public readonly PrimaryStateName PrimaryStateName;
        public readonly bool      DefaultState;
        
        public State_Master(PrimaryStateName primaryStateName, bool defaultState)
        {
            PrimaryStateName    = primaryStateName;
            DefaultState = defaultState;
        }
    }

    [Serializable]
    public class State // Permanent or Perpetual thing
    {
        public PrimaryStateName PrimaryStateName;
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
    
    public class StatesAndConditionsData
    {
        public StatesAndConditionsData(Actor_States actorStates, Actor_Conditions actorConditions)
        {
            Actor_States     = actorStates;
            Actor_Conditions = actorConditions;
        }
        
        public StatesAndConditionsData(StatesAndConditionsData statesAndConditionsData)
        {
            Actor_States     = statesAndConditionsData.Actor_States;
            Actor_Conditions = statesAndConditionsData.Actor_Conditions;
        }
        
        public void SetActorStatesAndConditions (StatesAndConditionsData statesAndConditionsData)
        {
            SetActorStates(statesAndConditionsData.Actor_States);
            SetActorConditions(statesAndConditionsData.Actor_Conditions);
        }
        
        public Actor_States     Actor_States;
        public void SetActorStates(Actor_States actorStates) => Actor_States = actorStates;
        
        public Actor_Conditions Actor_Conditions;
        public void SetActorConditions(Actor_Conditions actorConditions) => Actor_Conditions = actorConditions;
        
        
    }

    [Serializable]
    public class Actor_Conditions : PriorityData
    {
        public Actor_Conditions(uint actorID, ObservableDictionary<ConditionName, float> currentConditions) : base(actorID, ComponentType.Actor)
        {
            CurrentConditions                   =  currentConditions;
            CurrentConditions.DictionaryChanged += OnConditionChanged;
            
            Manager_TickRate.RegisterTicker(TickerType.Actor_StateAndCondition, TickRate.OneSecond, ActorReference.ActorID, _onTick);
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;


        public ObservableDictionary<ConditionName, float> CurrentConditions;
        
        void OnConditionChanged(ConditionName conditionName)
        {
            _priorityChangeCheck(PriorityUpdateTrigger.ChangedCondition);
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

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList
        {
            get;
            set;
        } = new();
    }

    public enum PrimaryStateName
    {
        None, 

        IsAlive,
        
        CanIdle,
        CanCombat,
        CanMove,
        CanTalk,
    }

    public enum SubStateName
    {
        None, 
        
        IsIdle,
        
        CanJump,
        IsJumping,
        
        Alerted,
        Hostile,
        IsInCombat,
        CanDodge,
        IsDodging,
        CanBlock,
        IsBlocking,
        CanBerserk,
        IsBerserking, 
        
        CanGetPregnant,
        IsPregnant,
        
        CanBeDepressed,
        IsDepressed,
        
        CanDrown,
        IsDrowning,
        CanSuffocate,
        IsSuffocating,
        
        CanReanimate,
        IsReanimated,
        
        InFire,
        OnFire,
        
        IsTalking,
    }

    public class Actor_States : PriorityData
    {
        public Actor_States(uint actorID, ObservableDictionary<PrimaryStateName, bool> initialisedStates = null) : base(actorID, ComponentType.Actor)
        {
            _currentPrimaryStates                   =  initialisedStates ?? _defaultPrimaryStates;
            _currentSubStates                       =  new ObservableDictionary<SubStateName, bool>();
            
            _setSubStates();
            
            _currentPrimaryStates.SetDictionaryChanged(_onPrimaryStateChanged);
            _currentSubStates.SetDictionaryChanged(_onSubStateChanged);
        }

        ComponentReference_Actor _actorReference    => Reference as ComponentReference_Actor;

        readonly ObservableDictionary<PrimaryStateName, bool> _currentPrimaryStates;
        readonly ObservableDictionary<SubStateName, bool>     _currentSubStates;

        void _setSubStates()
        {
            foreach (var primaryState in _currentPrimaryStates)
            {
                if (!_defaultSubStates.TryGetValue(primaryState.Key, out var subStates)) continue;

                foreach (var subState in subStates
                             .Where(subState => !_currentSubStates.TryAdd(subState.Key, subState.Value)))
                {
                    SetSubState(subState.Key, subState.Value);
                }
            }
        }
        
        void _onPrimaryStateChanged(PrimaryStateName primaryStateName)
        {
            _priorityChangeCheck(PriorityUpdateTrigger.ChangedPrimaryState);
        }
        
        void _onSubStateChanged(SubStateName subStateName)
        {
            _priorityChangeCheck(PriorityUpdateTrigger.ChangedSubState);
        }

        public void SetPrimaryState(PrimaryStateName primaryStateName, bool actorState)
        {
            if (!_currentPrimaryStates.TryAdd(primaryStateName, actorState)) _currentPrimaryStates[primaryStateName] = actorState;
        }
        
        public void SetSubState(SubStateName subStateName, bool actorState)
        {
            if (!_currentSubStates.TryAdd(subStateName, actorState)) _currentSubStates[subStateName] = actorState;
        }
        
        public bool GetPrimaryState(PrimaryStateName primaryStateName)
        {
            if (_currentPrimaryStates.TryGetValue(primaryStateName, out var state)) return state;

            if (_defaultPrimaryStates.TryGetValue(primaryStateName, out var defaultState))
            {
                Debug.LogWarning($"PrimaryState: {primaryStateName} not found in CurrentStates. Setting to default value: {defaultState}");
                
                SetPrimaryState(primaryStateName, defaultState);

                if (_currentPrimaryStates.TryGetValue(primaryStateName, out state)) return state;
                
                Debug.LogError($"PrimaryState: {primaryStateName} still not found in CurrentPrimaryStates after setting.");
                return false;
            }
            
            Debug.LogError($"PrimaryState: {primaryStateName} not found in DefaultPrimaryStates.");
            return false;
        }
        
        public bool GetSubState(SubStateName subStateName)
        {
            if (_currentSubStates.TryGetValue(subStateName, out var state)) return state;

            if (!_subStatePrimaryStateMap.TryGetValue(subStateName, out var primaryStateName))
            {
                Debug.LogError($"SubState: {subStateName} not found in SubStatePrimaryStateMap.");
                return false;
            }

            if (!_currentPrimaryStates.ContainsKey(primaryStateName))
            {
                if (!_defaultPrimaryStates.TryGetValue(primaryStateName, out var defaultPrimaryState))
                {
                    Debug.LogError($"PrimaryState: {primaryStateName} not found in DefaultPrimaryStates.");
                    return false;
                }    
                
                Debug.LogWarning($"PrimaryState: {primaryStateName} not found in CurrentStates. Setting to default value: {defaultPrimaryState}");
                
                SetPrimaryState(primaryStateName, defaultPrimaryState);                
            }
            
            if (!_defaultSubStates.TryGetValue(primaryStateName, out var defaultSubStates))
            {
                Debug.LogError($"PrimaryState: {primaryStateName} not found in DefaultSubStates.");
                return false;
            }

            if (defaultSubStates.TryGetValue(subStateName, out var defaultState))
            {
                SetSubState(subStateName, defaultState);

                if (_currentSubStates.TryGetValue(subStateName, out state)) return state;
                
                Debug.LogError($"SubState: {subStateName} still not found in CurrentSubStates after setting.");
                return false;
            }
            
            Debug.LogError($"State: {subStateName} not found in DefaultSubStates.");
            return false;
        }
        
        protected override bool _priorityChangeNeeded(object dataChanged) => (PrimaryStateName)dataChanged != PrimaryStateName.None;

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList
        {
            get;
            set;
        } = new();

        static readonly ObservableDictionary<PrimaryStateName, bool> _defaultPrimaryStates = new()
        {
            { PrimaryStateName.IsAlive, true},
            { PrimaryStateName.CanIdle, true},
            { PrimaryStateName.CanCombat, false},
        };
        
        static readonly ObservableDictionary<PrimaryStateName, Dictionary<SubStateName, bool>> _defaultSubStates = new()
        {
            { PrimaryStateName.CanCombat, new Dictionary<SubStateName, bool>()
            {
                {SubStateName.IsInCombat, false},
            }},
        };
        
        static readonly Dictionary<SubStateName, PrimaryStateName> _subStatePrimaryStateMap = new()
        {
            {SubStateName.IsInCombat, PrimaryStateName.CanCombat},
        };
        
        static readonly Dictionary<PrimaryStateName, ActionMap> _stateActionMap = new()
        {
            {PrimaryStateName.CanCombat, new ActionMap
            {
                EnabledGroupActions = new HashSet<ActorActionGroup> {ActorActionGroup.Combat},
                DisabledGroupActions = new HashSet<ActorActionGroup> { ActorActionGroup.Work, ActorActionGroup.Recreation, }
            }},
        }; 
    }

    public class ActionMap
    {
        public HashSet<ActorActionGroup> EnabledGroupActions;
        public HashSet<ActorActionName>  EnabledIndividualActions;
        public HashSet<ActorActionGroup>  DisabledGroupActions;
        public HashSet<ActorActionName>  DisabledIndividualActions;
    }
}

