using System.Collections.Generic;
using System.Linq;
using Actors;
using Jobs;
using UnityEngine;

namespace Priority
{
    public class PriorityGenerator_Jobsite : PriorityGenerator
    {
        public Dictionary<PriorityParameterName, float> GeneratePriority(JobTaskName                         jobTaskName,
                                                                         Dictionary<PriorityParameterName, object> existingPriorityParameters) =>
            _generatePriorities((uint)jobTaskName, existingPriorityParameters
                                                       .Select(x => x)
                                                       .ToDictionary(x => 
                                                           (uint)x.Key, x => x.Value));

        protected override Dictionary<PriorityParameterName, float> _generatePriority(uint priorityID,
                                                         Dictionary<uint, object>
                                                             existingPriorityParameters)
        {
            switch (priorityID)
            {
                case (uint)JobTaskName.Fetch_Items:
                    return _generateStockpilePriority(existingPriorityParameters) ?? new Dictionary<PriorityParameterName, float>();
                default:
                    Debug.LogError($"ActionName: {priorityID} not found.");
                    return null;
            }
        }
        
        Dictionary<PriorityParameterName, float> _generateStockpilePriority(Dictionary<uint, object> existingPriorityParameters)
        {
            return new Dictionary<PriorityParameterName, float>
            {
                {PriorityParameterName.DefaultPriority, 1},
            };
        }
    }
}
