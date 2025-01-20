using System.Collections.Generic;

namespace StateAndCondition
{
    public class Condition_List
    {
        static Dictionary<uint, Condition_Data> _defaultConditions;

        public static Dictionary<uint, Condition_Data> DefaultConditions =>
            _defaultConditions ??= _initialiseDefaultConditions();

        static Dictionary<uint, Condition_Data> _initialiseDefaultConditions()
        {
            return new Dictionary<uint, Condition_Data>
            {
                {
                    (uint)ConditionName.Inspired, new Condition_Data(
                        conditionName: ConditionName.Inspired, 
                        defaultConditionDuration: 100, 
                        maxConditionDuration: 300)
                }
            };
        }
    }
}