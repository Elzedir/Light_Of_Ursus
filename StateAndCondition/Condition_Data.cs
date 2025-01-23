using System;
using System.Collections.Generic;
using Tools;

namespace StateAndCondition
{
    [Serializable]
    public class Condition_Data : Data_Class
    {
        public ConditionName ConditionName;
        public float DefaultConditionDuration;
        public float MaxConditionDuration;

        public Condition_Data(ConditionName conditionName, float defaultConditionDuration, float maxConditionDuration)
        {
            ConditionName = conditionName;
            DefaultConditionDuration = defaultConditionDuration;
            MaxConditionDuration = maxConditionDuration;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Condition Name", $"{ConditionName}" },
                { "Default Duration", $"{DefaultConditionDuration}" },
                { "Max Duration", $"{MaxConditionDuration}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Condition Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }

    //= Temporary and tickable condition with a duration
    [Serializable]
    public class Condition : Data_Class
    {
        public ConditionName ConditionName;
        public float ConditionDuration;

        public Condition(ConditionName conditionName, float conditionDuration)
        {
            ConditionName = conditionName;
            ConditionDuration = conditionDuration;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Condition Name", $"{ConditionName}" },
                { "Duration", $"{ConditionDuration}" }
            };
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Condition Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
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
}