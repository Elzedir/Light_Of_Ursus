using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Jobs;
using JobSite;
using Station;
using Tools;
using UnityEngine;

namespace Priority
{
    public class Priority_Data_JobSite : Priority_Data
    {
        public Priority_Data_JobSite(uint jobsiteID)
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
        
        public override void RegenerateAllPriorities(bool includeOptionalPriorities = false)
        {
            if (!includeOptionalPriorities)
            {
                foreach (var jobTask in Priority_Manager.BasePriorityJobTasks)
                {
                    _regeneratePriority((uint)jobTask);
                }
                
                return;
            }
            
            foreach (JobTaskName jobTask in Enum.GetValues(typeof(JobTaskName)))
            {
                _regeneratePriority((uint)jobTask);
            }
        }

        protected override void _regeneratePriority(uint priorityID)
        {
            if (priorityID == (uint)JobTaskName.Idle)
            {
                PriorityQueue.Update(priorityID, 1);
                return;
            }
            
            var priorityParameters = _getPriorityParameters(priorityID);
            
            if (priorityParameters == null)
            {
                Debug.LogWarning($"PriorityParameters not found for: {(JobTaskName)priorityID}.");
            }

            var priorityValue = Priority_Generator.GeneratePriority(_priorityType, priorityID, priorityParameters);

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID)
        {
            if (limiterID != 0)
                return priorityIDs.Where(priorityID =>
                    Station_Manager.GetStation_Component(limiterID).AllowedJobTasks.Contains((JobTaskName)priorityID)).ToList();// This should be JobTaskName
            
            return priorityIDs;
        }

        protected override Dictionary<PriorityParameterName, object> _getPriorityParameters(uint priorityID)
        {
            var jobTask_Master = JobTask_Manager.GetJobTask_Master((JobTaskName)priorityID);
            var parameters = jobTask_Master.RequiredParameters.ToDictionary(parameter => parameter, _getParameter);
            
            return JobTask_Manager.PopulateTaskParameters((JobTaskName)priorityID, parameters);
        }
        
        object _getParameter(PriorityParameterName parameter)
        {
            return parameter switch
            {
                // PriorityParameterName.Target_Component => find a way to see which target we'd be talking about.
                PriorityParameterName.Jobsite_Component => _jobSite,
                //PriorityParameterName.Worker_Component  => _actor,
                _                                       => null
            };
        }

        protected override Dictionary<uint, PriorityElement> _getRelevantPriorities(ActorPriorityState actorPriorityState)
        {
            return PriorityQueue.PeekAll().ToDictionary(priority => priority.PriorityID);
        }

        protected override Dictionary<PriorityUpdateTrigger, List<uint>> _priorityIDsToUpdateOnDataChange { get; } = new();

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "JobSiteID", $"{JobsiteID}" },
                { "JobSite", $"{_jobSite.JobSiteData.JobSiteName}" },
                { "PriorityType", $"{_priorityType}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs, DataToDisplay dataToDisplay)
        {
            _updateDataDisplay(ref dataToDisplay,
                title: "Base Priority Data",
                stringData: GetStringData());
            
            _updateDataDisplay(ref dataToDisplay,
                title: "Priority Queue",
                subData: PriorityQueue.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);

            return dataToDisplay;
        }
    }
}