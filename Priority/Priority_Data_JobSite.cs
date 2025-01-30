using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using JobSite;
using Station;
using Tools;
using UnityEngine;

namespace Priority
{
    public class Priority_Data_JobSite : Priority_Data
    {
        public Priority_Data_JobSite(ulong jobSiteID)
        {
            _jobSiteReferences = new ComponentReference_Jobsite(jobSiteID);
        }

        readonly ComponentReference_Jobsite _jobSiteReferences;

        public ulong                     JobSiteID     => _jobSiteReferences.JobsiteID;
        JobSite_Component                _jobSite      => _jobSiteReferences.JobSite;

        protected override List<ulong> _getPermittedPriorities(List<ulong> priorityIDs)
        {
            var allowedPriorities = new List<ulong>();

            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (ulong)ActorActionName.Idle)
                {
                    Debug.LogError(
                        $"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
                    continue;
                }

                allowedPriorities.Add(priorityID);
            }

            return allowedPriorities;
        }
        
        public override void RegenerateAllPriorities(DataChangedName dataChangedName, bool forceRegenerateAll = false)
        {
            if (!forceRegenerateAll)
            {
                foreach (var actorAction in AllowedActions)
                {
                    _regeneratePriority((ulong)actorAction);
                }
                
                return;
            }
            
            foreach (ActorActionName jobTask in Enum.GetValues(typeof(ActorActionName)))
            {
                _regeneratePriority((ulong)jobTask);
            }
        }

        protected override void _regeneratePriority(ulong priorityID)
        {
            if (priorityID == (ulong)ActorActionName.Idle)
            {
                PriorityQueue.Update(priorityID, 1);
                return;
            }
            
            var priorityParameters = _getPriorityParameters((ActorActionName)priorityID);

            //* If it works, change the highest priority thing to be named right.
            priorityParameters = ActorAction_Manager.GetHighestPriorityStation(priorityParameters, (ActorActionName)priorityID);

            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            PriorityQueue.Update(priorityID, priorityValue);
        }

        protected override Priority_Parameters _getPriorityParameters(ActorActionName actorActionName)
        {
            return new Priority_Parameters
            (
                jobSiteID_Source: JobSiteID
            );
        }

        protected override List<ulong> _getRelevantPriorityIDs(List<ulong> priorityIDs, ulong limiterID)
        {
            if (limiterID != 0)
                return priorityIDs.Where(priorityID =>
                    Station_Manager.GetStation_Component(limiterID).AllowedJobTasks.Contains((ActorActionName)priorityID)).ToList();
            
            return priorityIDs;
        }

        protected override HashSet<ActorActionName> _getAllowedActions() =>
            _jobSite.BaseJobActions;

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
                allSubData: _convertUlongIDToStringID(PriorityQueue?.GetDataToDisplay(toggleMissingDataDebugs)));

            return DataToDisplay;
        }

        protected override string _getPriorityID(string iteration, ulong priorityID) =>
            $"PriorityID({iteration}) - {(ActorActionName)priorityID}";
    }
}