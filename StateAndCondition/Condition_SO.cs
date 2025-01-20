using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace StateAndCondition
{
    [CreateAssetMenu(fileName = "Condition_SO", menuName = "SOList/Condition_SO")]
    [Serializable]
    public class Condition_SO : Data_SO<Condition_Data>
    {
        public Data<Condition_Data>[] Conditions => Data;

        public Data<Condition_Data> GetCondition_Data(ConditionName conditionName) =>
            GetData((uint)conditionName);

        public override uint GetDataID(int id) => (uint)Conditions[id].Data_Object.ConditionName;

        public void UpdateCondition(uint conditionID, Condition_Data condition_Data) =>
            UpdateData(conditionID, condition_Data);

        public void UpdateAllConditions(Dictionary<uint, Condition_Data> allConditions) =>
            UpdateAllData(allConditions);

        protected override Dictionary<uint, Data<Condition_Data>> _getDefaultData() => 
            _convertDictionaryToData(Condition_List.DefaultConditions);

        protected override Data<Condition_Data> _convertToData(Condition_Data data)
        {
            return new Data<Condition_Data>(
                dataID: (uint)data.ConditionName,
                data_Object: data, 
                dataTitle: $"{(uint)data.ConditionName}: {data.ConditionName}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        static uint _lastUnusedConditionID = 1;

        public uint GetUnusedConditionID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedConditionID))
            {
                _lastUnusedConditionID++;
            }

            return _lastUnusedConditionID;
        }
    }

    [CustomEditor(typeof(Condition_SO))]
    public class Condition_SOEditor : Data_SOEditor<Condition_Data>
    {
        public override Data_SO<Condition_Data> SO => _so ??= (Condition_SO)target;
    }
}