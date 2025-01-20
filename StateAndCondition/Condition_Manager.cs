using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Priority;
using TickRates;
using Tools;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class StateAndCondition_Manager
    {
        const string _condition_SOPath = "ScriptableObjects/Condition_SO";

        static Condition_SO _allConditions;
        static Condition_SO AllConditions => _allConditions ??= _getCondition_SO();

        public static Condition_Data GetCondition_Data(ConditionName conditionName)
        {
            return AllConditions.GetCondition_Data(conditionName).Data_Object;
        }

        public static Condition GetCondition(ConditionName conditionName, uint conditionLevel)
        {
            return AllConditions.GetCondition(conditionName, conditionLevel);
        }

        static Condition_SO _getCondition_SO()
        {
            var condition_SO = Resources.Load<Condition_SO>(_condition_SOPath);

            if (condition_SO is not null) return condition_SO;

            Debug.LogError("Condition_SO not found. Creating temporary Condition_SO.");
            condition_SO = ScriptableObject.CreateInstance<Condition_SO>();

            return condition_SO;
        }

        public static uint GetUnusedConditionID()
        {
            return AllConditions.GetUnusedConditionID();
        }

        public static void ClearSOData()
        {
            AllConditions.ClearSOData();
        }
        
        .....
        
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
    
    public class Actor_Data_StatesAndConditions : Priority_Updater
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public Actor_Data_StatesAndConditions(uint actorID, Actor_States states, Actor_Conditions conditions) : base (actorID, ComponentType.Actor)
        {
            States     = states;
            Conditions = conditions;
        }
        
        public Actor_Data_StatesAndConditions(Actor_Data_StatesAndConditions actorDataStatesAndConditions) : base (actorDataStatesAndConditions.ActorReference.ActorID, ComponentType.Actor)
        {
            States     = actorDataStatesAndConditions.States;
            Conditions = actorDataStatesAndConditions.Conditions;
        }
        
        public void SetActorStatesAndConditions (Actor_Data_StatesAndConditions actorDataStatesAndConditions)
        {
            SetActorStates(actorDataStatesAndConditions.States);
            SetActorConditions(actorDataStatesAndConditions.Conditions);
        }
        
        public Actor_States     States;
        public void SetActorStates(Actor_States actorStates) => States = actorStates;
        
        public Actor_Conditions Conditions;
        public void SetActorConditions(Actor_Conditions actorConditions) => Conditions = actorConditions;
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor States", $"{States}" },
                { "Actor Conditions", $"{Conditions}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "States And Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Conditions.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "States",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: States.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }
        
        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Actor_Conditions : Priority_Updater
    {
        public Actor_Conditions(uint actorID, ObservableDictionary<ConditionName, float> currentConditions) : base(actorID, ComponentType.Actor)
        {
            CurrentConditions                   =  currentConditions;
            CurrentConditions.DictionaryChanged += OnConditionChanged;
            
            Manager_TickRate.RegisterTicker(TickerTypeName.Actor_StateAndCondition, TickRateName.OneSecond, ActorReference.ActorID, _onTick);
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;


        public ObservableDictionary<ConditionName, float> CurrentConditions;

        public override Dictionary<string, string> GetStringData()
        {
            var data = new Dictionary<string, string>();

            foreach (var condition in CurrentConditions)
            {
                data.Add(condition.Key.ToString(), condition.Value.ToString());
            }

            return data;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            return DataToDisplay;
        }

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
                CurrentConditions[conditionName] += StateAndCondition_Manager.GetCondition_Master(conditionName).DefaultConditionDuration;
                return;
            }

            var condition_Master = StateAndCondition_Manager.GetCondition_Master(conditionName);

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
            
            var conditionMaster = StateAndCondition_Manager.GetCondition_Master(conditionName);

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

    public class Actor_States : Priority_Updater
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

        public override Dictionary<string, string> GetStringData()
        {
            var data = new Dictionary<string, string>();

            foreach (var primaryState in _currentPrimaryStates)
            {
                data.Add(primaryState.Key.ToString(), primaryState.Value.ToString());
            }

            foreach (var subState in _currentSubStates)
            {
                data.Add(subState.Key.ToString(), subState.Value.ToString());
            }

            return data;
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Primary States",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            return DataToDisplay;
        }

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
                
                //Debug.LogWarning($"PrimaryState: {primaryStateName} not found in CurrentStates. Setting to default value: {defaultPrimaryState}");
                
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
            { PrimaryStateName.CanCombat, true},
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
                EnabledGroupActions = new HashSet<ActorBehaviourName> {ActorBehaviourName.Combat},
                DisabledGroupActions = new HashSet<ActorBehaviourName> { ActorBehaviourName.Work, ActorBehaviourName.Recreation, }
            }},
        }; 
    }

    public class ActionMap
    {
        public HashSet<ActorBehaviourName> EnabledGroupActions;
        public HashSet<ActorActionName>  EnabledIndividualActions;
        public HashSet<ActorBehaviourName>  DisabledGroupActions;
        public HashSet<ActorActionName>  DisabledIndividualActions;
    }
}

