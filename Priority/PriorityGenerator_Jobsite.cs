using System.Collections.Generic;
using System.Linq;
using Actors;
using Jobs;
using UnityEngine;

namespace Priority
{
    public class PriorityGenerator_Jobsite : PriorityGenerator
    {
        public List<float> GeneratePriority(JobTaskName                         jobTaskName,
                                            Dictionary<PriorityParameterName, object> existingPriorityParameters) =>
            _generatePriorities((uint)jobTaskName, existingPriorityParameters
                                                       .Select(x => x)
                                                       .ToDictionary(x => 
                                                           (uint)x.Key, x => x.Value));

        protected override List<float> _generatePriority(uint priorityID,
                                                         Dictionary<uint, object>
                                                             existingPriorityParameters)
        {
            switch (priorityID)
            {
                case (uint)JobTaskName.Fetch_Items:
                    return _generateStockpilePriority(existingPriorityParameters) ?? new List<float>();
                default:
                    Debug.LogError($"ActionName: {priorityID} not found.");
                    return null;
            }
        }
        
        List<float> _generateStockpilePriority(Dictionary<uint, object> existingPriorityParameters)
        {
            return new List<float>
            {
                1
            };
        }
    }
}
