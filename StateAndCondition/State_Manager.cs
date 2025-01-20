using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Priority;
using Tools;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class State_Manager
    {
        const string _state_SOPath = "ScriptableObjects/State_SO";

        static State_SO _allStates;
        static State_SO AllStates => _allStates ??= _getState_SO();

        public static State GetState(StateName stateName)
        {
            return AllStates.GetState(stateName);
        }

        static State_SO _getState_SO()
        {
            var state_SO = Resources.Load<State_SO>(_state_SOPath);

            if (state_SO is not null) return state_SO;

            Debug.LogError("State_SO not found. Creating temporary State_SO.");
            state_SO = ScriptableObject.CreateInstance<State_SO>();

            return state_SO;
        }

        public static uint GetUnusedStateID()
        {
            return AllStates.GetUnusedStateID();
        }

        public static void ClearSOData()
        {
            AllStates.ClearSOData();
        }
    }

    public class Actor_Data_States : Priority_Updater
    {
        public Actor_Data_States(uint actorID, ObservableDictionary<StateName, bool> initialisedStates = null) :
            base(actorID, ComponentType.Actor)
        {
            _currentStates = initialisedStates ?? new ObservableDictionary<StateName, bool>();
            _currentStates.SetDictionaryChanged(_onStateChanged);
        }

        readonly ObservableDictionary<StateName, bool> _currentStates;

        public override Dictionary<string, string> GetStringData()
        {
            return _currentStates.ToDictionary(
                state => $"{state.Key}",
                state => $"{state.Value}");
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "States",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        void _onStateChanged(StateName primaryStateName)
        {
            _priorityChangeCheck(PriorityUpdateTrigger.ChangedState);
        }

        public void SetState(StateName stateName, bool state)
        {
            if (!_currentStates.TryAdd(stateName, state))
                _currentStates[stateName] = state;
        }
        
        //* Currently, getState does not take into account parent state, maybe include that.

        public bool GetState(StateName stateName)
        {
            if (_currentStates.TryGetValue(stateName, out var state)) return state;
            
            var defaultState = State_Manager.GetState(stateName);

            if (defaultState.StateName != StateName.None)
            {
                SetState(defaultState.StateName, defaultState.CurrentState);

                if (_currentStates.TryGetValue(stateName, out state)) return state;

                Debug.LogError(
                    $"PrimaryState: {stateName} still not found in CurrentPrimaryStates after setting.");
                return false;
            }

            Debug.LogError($"PrimaryState: {stateName} not found in DefaultStates.");
            return false;
        }

        protected override bool _priorityChangeNeeded(object dataChanged) =>
            (StateName)dataChanged != StateName.None;

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }
}