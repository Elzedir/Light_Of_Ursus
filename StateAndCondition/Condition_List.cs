using System.Collections.Generic;

namespace StateAndCondition
{
    public class Condition_List
    {
        static Dictionary<ulong, Condition_Data> _defaultConditions;

        public static Dictionary<ulong, Condition_Data> DefaultConditions =>
            _defaultConditions ??= _initialiseDefaultConditions();

        static Dictionary<ulong, Condition_Data> _initialiseDefaultConditions()
        {
            return new Dictionary<ulong, Condition_Data>
            {
                {
                    (ulong)ConditionName.Inspired, new Condition_Data(
                        conditionName: ConditionName.Inspired, 
                        defaultConditionDuration: 100, 
                        maxConditionDuration: 300)
                }
            };
        }
    }
}