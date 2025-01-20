using System;
using System.Collections.Generic;
using Tools;

namespace StateAndCondition
{
    [Serializable]
    public class State_Data : Data_Class
    {
        public StateName StateName;
        public bool DefaultState;
        public StateName ParentState;
        
        public State_Data(StateName stateName, bool defaultState, StateName parentState)
        {
            StateName    = stateName;
            DefaultState = defaultState;
            ParentState = parentState;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "State Name", $"{StateName}" },
                { "Default State", $"{DefaultState}" },
                { "Parent State", $"{ParentState}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: StateName.ToString(),
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            return DataToDisplay;
        }
    }

    //= Permanent or perpetual state with no ticking duration.
    [Serializable]
    public class State : Data_Class
    {
        public readonly StateName StateName;
        public readonly StateName ParentState;
        
        public bool      CurrentState;
        
        public State(StateName stateName, StateName parentState, bool currentState)
        {
            StateName = stateName;
            CurrentState     = currentState;
            ParentState = parentState;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Primary State Name", $"{StateName}" },
                { "Current State", $"{CurrentState}" }
            };
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: StateName.ToString(),
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            return DataToDisplay;
        }
    }
}