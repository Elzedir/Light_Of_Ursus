using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Buildings;
using Priorities.Priority_Queues;
using Station;
using Tools;
using UnityEngine;

namespace Priorities
{
    public class Priority_Data_Building : Priority_Data
    {
        public Priority_Data_Building(ulong buildingID)
        {
            _buildingReferences = new ComponentReference_Building(buildingID);
        }

        readonly ComponentReference_Building _buildingReferences;

        public ulong                     BuildingID     => _buildingReferences.BuildingID;
        Building_Component                Building      => _buildingReferences.Building;
        
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
            var priorityParameters = _getPriorityParameters((ActorActionName)priorityID);
            var priorityValue = Priority_Generator.GeneratePriority(priorityID, priorityParameters);

            PriorityQueueMaxHeap.Update(new Priority_Element<ActorAction_Data>(priorityID, priorityValue, null));
        }

        protected override void _setActorID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.ActorID_Source = 0;
        }

        protected override void _setBuildingID_Source(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.BuildingID_Source = BuildingID;
        }

        protected override void _setActorID_Target(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.ActorID_Target = 0;
        }

        protected override void _setBuildingID_Target(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.BuildingID_Target = 0;
        }

        protected override HashSet<ActorActionName> _getAllowedActions() => Building.Building_Data.AllowedActions;

        public override Dictionary<string, string> GetStringData()
        {
            var highestPriority = PeekHighestPriority();
            
            return new Dictionary<string, string>
            {
                { "BuildingID", $"{BuildingID}" },
                { "Building", $"{Building.Building_Data.Name}" },
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
                allSubData: _convertUlongIDToStringID(PriorityQueueMaxHeap?.GetDataToDisplay(toggleMissingDataDebugs)));

            return DataToDisplay;
        }

        protected override string _getPriorityID(string iteration, ulong priorityID) =>
            $"PriorityID({iteration}) - {(ActorActionName)priorityID}({priorityID})";
    }
}