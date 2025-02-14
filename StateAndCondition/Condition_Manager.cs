using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Inventory;
using Pathfinding;
using Priorities;
using Priority;
using TickRates;
using Tools;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class Condition_Manager
    {
        const string _condition_SOPath = "ScriptableObjects/Condition_SO";

        static Condition_SO s_allConditions;
        static Condition_SO S_AllConditions => s_allConditions ??= _getCondition_SO();
        
        public static ObservableDictionary<ConditionName, float> InitialiseDefaultConditions(ObservableDictionary<ConditionName, float> existingConditions) =>
            S_AllConditions.InitialiseDefaultConditions(existingConditions);

        public static Condition_Data GetCondition_Data(ConditionName conditionName)
        {
            return S_AllConditions.GetCondition_Data(conditionName).Data_Object;
        }

        public static Condition GetCondition(ConditionName conditionName, ulong conditionDuration)
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

        public static void ClearSOData()
        {
            S_AllConditions.ClearSOData();
        }
    }

    [Serializable]
    public class Actor_Data_Conditions : Priority_Class
    {
        public Actor_Data_Conditions(ulong actorID, ObservableDictionary<ConditionName, float> currentConditions) : base(actorID, ComponentType.Actor)
        {
            _currentConditions                   =  currentConditions;
            CurrentConditions.DictionaryChanged += OnConditionChanged;
        }
        public          ComponentReference_Actor ActorReference    => Reference as ComponentReference_Actor;

        ObservableDictionary<ConditionName, float> _currentConditions;
        public ObservableDictionary<ConditionName, float> CurrentConditions => _currentConditions ??= Condition_Manager.InitialiseDefaultConditions(null);

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
            PriorityData.RegenerateAllPriorities(DataChangedName.ChangedCondition);
        }

        public void OnTick()
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
        
        public void SetConditionTimer(ConditionName conditionName, float setTimer = 0, float addTimer = 0, bool overruleMaxDuration = false)
        {
            if (setTimer != 0 && addTimer != 0)
            {
                Debug.LogError("Both set and add timer set.");
                return;
            }
            
            if (!CurrentConditions.ContainsKey(conditionName))
            {
                Debug.LogError($"Condition {conditionName} not found.");
                return;
            }
            
            var condition_Data = Condition_Manager.GetCondition_Data(conditionName);

            CurrentConditions[conditionName] = overruleMaxDuration
                ? setTimer != 0
                    ? setTimer
                    : CurrentConditions[conditionName] + addTimer
                : setTimer != 0
                    ? Math.Min(setTimer, condition_Data.MaxConditionDuration)
                    : Math.Min(CurrentConditions[conditionName] + addTimer, condition_Data.MaxConditionDuration);
        }

        public void RemoveCondition(ConditionName conditionName)
        {
            if (!CurrentConditions.ContainsKey(conditionName))
            {
                Debug.LogError($"Condition {conditionName} not found.");
                return;
            }

            SetConditionTimer(conditionName);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Restrict or allow actions depending on conditions that you have. For example, frozen or burning.
            return new List<ActorActionName>();
        }
        
        public (List<MoverType> EnabledMoverTypes, List<MoverType> DisabledMoverTypes) GetMoverTypes()
        {
            var enableMoverTypes = new List<MoverType>();
            var disableMoverTypes = new List<MoverType>();

            foreach (var state in CurrentConditions)
            {
                //* Add in a dictionary somewhere of what each condition will enable or disable.
            }

            return (enableMoverTypes, disableMoverTypes);
        }
    }
}

