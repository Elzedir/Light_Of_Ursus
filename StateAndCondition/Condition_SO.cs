using System;
using System.Collections.Generic;
using System.Linq;
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
            GetData((ulong)conditionName);
        
        
        public ObservableDictionary<ConditionName, float> InitialiseDefaultConditions(ObservableDictionary<ConditionName, float> existingConditions)
        {
            existingConditions ??= new ObservableDictionary<ConditionName, float>();

            foreach (var condition in Conditions)
            {
                //* For some reason, conditions and states are not initialising properly if we don't have the SO selected, compared to the others, which
                //* can have problems initialising when they ARE selected, due to scene objects. Maybe fix this tomorrow.
                if (condition?.DataTitle is null)
                {
                    Debug.LogError($"ConditionID: {condition?.DataID} has no DataTitle.");
                    continue;   
                }
                
                if (existingConditions.ContainsKey(condition.Data_Object.ConditionName))
                    continue;
                
                existingConditions.Add(condition.Data_Object.ConditionName, condition.Data_Object.DefaultConditionDuration);
            }

            return existingConditions;
        }

        public void UpdateCondition(ulong conditionID, Condition_Data condition_Data) =>
            UpdateData(conditionID, condition_Data);

        public void UpdateAllConditions(Dictionary<ulong, Condition_Data> allConditions) =>
            UpdateAllData(allConditions);

        protected override Dictionary<ulong, Data<Condition_Data>> _getDefaultData() => 
            _convertDictionaryToData(Condition_List.DefaultConditions);

        protected override Data<Condition_Data> _convertToData(Condition_Data data)
        {
            return new Data<Condition_Data>(
                dataID: (ulong)data.ConditionName,
                data_Object: data, 
                dataTitle: $"{(ulong)data.ConditionName}: {data.ConditionName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Condition_SO))]
    public class Condition_SOEditor : Data_SOEditor<Condition_Data>
    {
        public override Data_SO<Condition_Data> SO => _so ??= (Condition_SO)target;
    }
}