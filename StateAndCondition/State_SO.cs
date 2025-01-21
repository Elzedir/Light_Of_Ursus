using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace StateAndCondition
{
    [CreateAssetMenu(fileName = "State_SO", menuName = "SOList/State_SO")]
    [Serializable]
    public class State_SO : Data_SO<State_Data>
    {
        public Data<State_Data>[] States => Data;

        public Data<State_Data> GetState_Data(StateName stateName) =>
            GetData((uint)stateName);

        public State GetState(StateName stateName)
        {
            var stateData = GetState_Data(stateName).Data_Object;
            
            return new State(stateData.StateName, stateData.DefaultState);
        }

        public ObservableDictionary<StateName, bool> InitialiseDefaultStates(ObservableDictionary<StateName, bool> existingStates)
        {
            var defaultStates = new ObservableDictionary<StateName, bool>();

            foreach (var state in States)
            {
                if (existingStates.ContainsKey(state.Data_Object.StateName))
                    continue;
                
                defaultStates.Add(state.Data_Object.StateName, state.Data_Object.DefaultState);
            }

            return defaultStates;
        }
        

        //* None doesn't show up in the interactable list in the inspector.
        public override uint GetDataID(int id) =>
            (uint)States[id].Data_Object.StateName;   

        public void UpdateState(uint stateID, State_Data state_Data) =>
            UpdateData(stateID, state_Data);

        public void UpdateAllStates(Dictionary<uint, State_Data> allStates) =>
            UpdateAllData(allStates);

        protected override Dictionary<uint, Data<State_Data>> _getDefaultData() => 
            _convertDictionaryToData(State_List.DefaultStates);

        protected override Data<State_Data> _convertToData(State_Data data) =>
            new ( dataID: (uint)data.StateName,
                data_Object: data,
                dataTitle: $"{(uint)data.StateName}: {data.StateName}",
                getDataToDisplay: data.GetDataToDisplay);

        static uint _lastUnusedStateID = 1;

        public uint GetUnusedStateID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedStateID))
            {
                _lastUnusedStateID++;
            }

            return _lastUnusedStateID;
        }
    }

    [CustomEditor(typeof(State_SO))]
    public class State_SOEditor : Data_SOEditor<State_Data>
    {
        public override Data_SO<State_Data> SO => _so ??= (State_SO)target;
    }
}