using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Inventory;
using Pathfinding;
using Priorities;
using Tools;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class State_Manager
    {
        const string _state_SOPath = "ScriptableObjects/State_SO";

        static State_SO _allStates;
        static State_SO AllStates => _allStates ??= _getState_SO();

        public static ObservableDictionary<StateName, bool> InitialiseDefaultStates(
            ObservableDictionary<StateName, bool> existingStates) =>
            AllStates.InitialiseDefaultStates(existingStates);

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

        public static void ClearSOData()
        {
            AllStates.ClearSOData();
        }
    }

    public class Actor_Data_States : Priority_Class
    {
        public Actor_Data_States(ulong actorID, ObservableDictionary<StateName, bool> initialisedStates = null) :
            base(actorID, ComponentType.Actor)
        {
            _currentStates = State_Manager.InitialiseDefaultStates(initialisedStates);
            CurrentStates.SetDictionaryChanged(_onStateChanged);
        }

        ObservableDictionary<StateName, bool> _currentStates;

        public ObservableDictionary<StateName, bool> CurrentStates =>
            _currentStates ??= State_Manager.InitialiseDefaultStates(null);

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

        void _onStateChanged(StateName stateName)
        {
            PriorityData.RegenerateAllPriorities(DataChangedName.ChangedState);
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

        //* Later will also have to remove states that are added in earlier, like inCombat will disable crafting, so this
        //* needs to be put after crafting, or find a better way to initialise it.
        public override List<ActorActionName> GetAllowedActions()
        {
            return CurrentStates.Where(state => state.Value)
                .SelectMany(state =>
                {
                    if (ActorAction_List.S_ActorActionStateDictionary.TryGetValue(state.Key, out var actions))
                        return actions
                            .Where(action => action.Value == state.Value)
                            .Select(action => action.Key);

                    //Debug.LogError($"State: {state.Key} has no actions.");
                    return Enumerable.Empty<ActorActionName>();
                }).ToList();
        }
        
        public (List<MoverType> EnabledMoverTypes, List<MoverType> DisabledMoverTypes) GetMoverTypes()
        {
            var enableMoverTypes = new List<MoverType>();
            var disableMoverTypes = new List<MoverType>();

            foreach (var state in CurrentStates)
            {
                //* Add in a dictionary somewhere of what each state will enable or disable.
            }

            return (enableMoverTypes, disableMoverTypes);
        }
    }
}