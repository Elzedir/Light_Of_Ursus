using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using JobSite;
using Station;
using Tools;
using UnityEngine;

namespace Priority
{
    public class Priority_Data_JobSite : Priority_Data
    {
        public Priority_Data_JobSite(uint jobSiteID)
        {
            _jobSiteReferences = new ComponentReference_Jobsite(jobSiteID);
        }

        readonly ComponentReference_Jobsite _jobSiteReferences;

        public uint                     JobSiteID     => _jobSiteReferences.JobsiteID;
        JobSite_Component                _jobSite      => _jobSiteReferences.JobSite;

        protected override List<uint> _getPermittedPriorities(List<uint> priorityIDs)
        {
            var allowedPriorities = new List<uint>();

            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (uint)ActorActionName.Idle)
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
                foreach (var actorAction in AllowedActions)
                {
                    _regeneratePriority((uint)actorAction);
                }
                
                return;
            }
            
            foreach (ActorActionName jobTask in Enum.GetValues(typeof(ActorActionName)))
            {
                _regeneratePriority((uint)jobTask);
            }
        }

        protected override void _regeneratePriority(uint priorityID)
        {
            if (priorityID == (uint)ActorActionName.Idle)
            {
                PriorityQueue.Update(priorityID, 1);
                return;
            }
            
            var priorityParameters = _getPriorityParameters(priorityID);

            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override List<uint> _getRelevantPriorityIDs(List<uint> priorityIDs, uint limiterID)
        {
            if (limiterID != 0)
                return priorityIDs.Where(priorityID =>
                    Station_Manager.GetStation_Component(limiterID).AllowedJobTasks.Contains((ActorActionName)priorityID)).ToList();// This should be ActorActionName
            
            return priorityIDs;
        }

        protected override Dictionary<PriorityParameterName, object> _getPriorityParameters(uint priorityID)
        {
            var actorAction_Data = ActorAction_Manager.GetActorAction_Data((ActorActionName)priorityID);
            var parameters = actorAction_Data.RequiredParameters.ToDictionary(parameter => parameter, _getParameter);
            
            return ActorAction_Manager.PopulateActionParameters((ActorActionName)priorityID, parameters);
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

        protected override List<ActorActionName> _getAllowedActions() =>
            _jobSite.BaseJobActions;

        protected override Dictionary<PriorityUpdateTrigger, List<uint>> _priorityIDsToUpdateOnDataChange { get; } = new();

        public override Dictionary<string, string> GetStringData()
        {
            var highestPriority = PeekHighestPriority();
            
            return new Dictionary<string, string>
            {
                { "JobSiteID", $"{JobSiteID}" },
                { "JobSite", $"{_jobSite.JobSiteData.JobSiteName}" },
                { "Next Highest Priority", highestPriority?.PriorityID != null 
                    ? $"{(ActorActionName)highestPriority.PriorityID}({highestPriority.PriorityID}) - {highestPriority.PriorityValue}" 
                    : "No Highest Priority" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Priority Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Priority Queue",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: _convertUintIDToStringID(PriorityQueue?.GetDataToDisplay(toggleMissingDataDebugs)));

            return DataToDisplay;
        }

        protected override string _getPriorityID(string iteration, uint priorityID) =>
            $"PriorityID({iteration}) - {(ActorActionName)priorityID}";
    }
}