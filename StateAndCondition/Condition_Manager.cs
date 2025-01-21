using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorAction;
using Inventory;
using Priority;
using TickRates;
using Tools;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class Condition_Manager
    {
        const string _condition_SOPath = "ScriptableObjects/Condition_SO";

        static Condition_SO _allConditions;
        static Condition_SO AllConditions => _allConditions ??= _getCondition_SO();

        public static Condition_Data GetCondition_Data(ConditionName conditionName)
        {
            return AllConditions.GetCondition_Data(conditionName).Data_Object;
        }

        public static Condition GetCondition(ConditionName conditionName, uint conditionDuration)
        {
            return new Condition(conditionName, conditionDuration);
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
    }

    [Serializable]
    public class Actor_Data_Conditions : Priority_Updater
    {
        public Actor_Data_Conditions(uint actorID, ObservableDictionary<ConditionName, float> currentConditions) : base(actorID, ComponentType.Actor)
        {
            CurrentConditions                   =  currentConditions;
            CurrentConditions.DictionaryChanged += OnConditionChanged;
            
            Manager_TickRate.RegisterTicker(TickerTypeName.Actor_StateAndCondition, TickRateName.OneSecond, ActorReference.ActorID, _onTick);
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;


        public ObservableDictionary<ConditionName, float> CurrentConditions;

        public override Dictionary<string, string> GetStringData()
        {
            return CurrentConditions.ToDictionary(
                condition => $"{condition.Key}", 
                condition => $"{condition.Value}");
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
                CurrentConditions[conditionName] += Condition_Manager.GetCondition_Data(conditionName).DefaultConditionDuration;
                return;
            }

            var condition_Master = Condition_Manager.GetCondition_Data(conditionName);

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
            
            var conditionMaster = Condition_Manager.GetCondition_Data(conditionName);

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
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Restrict or allow actions depending on conditions that you have. For example, frozen or burning.
            return new List<ActorActionName>();
        }
        
        protected override bool _priorityChangeNeeded(object conditionName) => (ConditionName)conditionName != ConditionName.None;

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList
        {
            get;
            set;
        } = new();
    }
}

