using System.Collections.Generic;
using Actors;
using Jobs;
using Jobsite;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_Jobsite : PriorityComponent
    {
        PriorityGenerator_Jobsite _priorityGenerator;
        PriorityGenerator_Jobsite PriorityGenerator => _priorityGenerator ??= new PriorityGenerator_Jobsite();

        protected override List<uint> _canPeek(List<uint> priorityIDs)
        {
            var allowedPriorities = new List<uint>();

            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (uint)JobTaskName.Idle)
                {
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
                    continue;
                }

                allowedPriorities.Add(priorityID);
            }

            return allowedPriorities;
        }
        
        protected override PriorityElement _createPriorityElement(uint priorityID,
                                                                  Dictionary<PriorityParameterName, object>
                                                                      priorityParameters)
        {
            return new PriorityElement(priorityID,
                priorityParameters ?? Manager_JobTask.GetDefaultTaskParameters((JobTaskName)priorityID));
        }

        protected override void _regeneratePriority(uint priorityQueueID, uint priorityID)
        {
            if (!_priorityExists(priorityQueueID, priorityID, out var existingPriorityParameters)) return;

            var newPriorities = ;

            AllPriorities[priorityQueueID].Update(priorityID, newPriorities);
        }

        protected override ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(
            PriorityState priorityState)
        {
            return AllPriorities;
        }

        public PriorityComponent_Jobsite(uint jobsiteID)
        {
            _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
        }

        readonly ComponentReference_Jobsite _jobsiteReferences;

        public uint      JobsiteID => _jobsiteReferences.JobsiteID;
        JobsiteComponent _jobsite  => _jobsiteReferences.Jobsite;

        protected override Dictionary<DataChanged, List<PriorityToChange>> _prioritiesToChange { get; } = new()
        {
            {
                DataChanged.ChangedInventory, new List<PriorityToChange>
                {
                    new((uint)JobTaskName.Fetch_Items, PriorityImportance.High),
                    new((uint)JobTaskName.Deliver_Items, PriorityImportance.High),
                }
            },
        };
    }
}