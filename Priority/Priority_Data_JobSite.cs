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
            
            a
                
                // Re-look at priority system and make sure we can generate priority in the beginning with no parameters as on initialisation, we need
                // to have some parameters to generate some priority to assign initial jobs to the actors.

            var priorityValue =
                Priority_Generator.GeneratePriority(_priorityType, priorityID, _getPriorityParameters(priorityID, null));
            
            Debug.Log($"PriorityID: {priorityID}, PriorityValue: {priorityValue}");

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID)
        {
            if (limiterID != 0)
                return priorityIDs.Where(priorityID =>
                    Station_Manager.GetStation_Component(limiterID).AllowedJobs.Contains((JobName)priorityID)).ToList();
            
            return priorityIDs;
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Priority Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"JobsiteID: {JobsiteID}",
                        $"JobSite: {_jobSite}",
                        $"PriorityType: {_priorityType}",
                    }));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in Base Priority Data");
                }
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "All Priorities",
                    dataDisplayType: DataDisplayType.SelectableList,
                    subData: PriorityQueue.DataSO_Object(toggleMissingDataDebugs).SubData));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogError("Error in Base Priority Data");
                }
            }

            return new Data_Display(
                title: "Priority Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
        }
    }
}