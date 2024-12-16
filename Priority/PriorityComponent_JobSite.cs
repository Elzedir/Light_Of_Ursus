using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Jobs;
using JobSite;
using Managers;
using Station;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_JobSite : PriorityComponent
    {
        public PriorityComponent_JobSite(uint jobsiteID)
        {
            _jobsiteReferences = new ComponentReference_Jobsite(jobsiteID);
        }

        readonly ComponentReference_Jobsite _jobsiteReferences;

        public uint                     JobsiteID     => _jobsiteReferences.JobsiteID;
        JobSite_Component                _jobSite      => _jobsiteReferences.JobSite;
        protected override PriorityType _priorityType => PriorityType.JobTask;

        protected override List<uint> _getPermittedPriorities(List<uint> priorityIDs)
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

        protected override void _regeneratePriority(uint priorityID)
        {
            Debug.LogWarning("Cannot regenerate priority for Jobsite.");
        }

        protected override List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID)
        {
            return priorityIDs.Where(priorityID =>
                Station_Manager.GetStation_Component(limiterID).AllowedJobs.Contains((JobName)priorityID)).ToList();
        }

        protected override Dictionary<PriorityParameterName, object> _getPriorityParameters(
            uint priorityID, Dictionary<PriorityParameterName, object> requiredParameters)
        {
            return JobTask_Manager.GetTaskParameters((JobTaskName)priorityID, requiredParameters);
        }

        protected override Dictionary<uint, PriorityElement> _getRelevantPriorities(ActorPriorityState actorPriorityState)
        {
            return PriorityQueue.PeekAll().ToDictionary(priority => priority.PriorityID);
        }

        protected override Dictionary<PriorityUpdateTrigger, List<uint>> _priorityIDsToUpdateOnDataChange { get; } = new();
    }
}