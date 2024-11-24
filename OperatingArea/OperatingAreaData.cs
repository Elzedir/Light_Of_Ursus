using System;
using Actors;
using Jobsite;
using Managers;
using UnityEngine;

namespace OperatingArea
{
    [Serializable]
    public class OperatingAreaData
    {
        #region ID Info
        public uint             OperatingAreaID;
        public uint             StationID;
        StationComponent        _station;
        public StationComponent Station => _station ??= Manager_Station.GetStation(StationID);
        public uint             JobsiteID;
        JobsiteComponent        _jobsite;
        public JobsiteComponent Jobsite => _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID);
        #endregion

        #region Operator
        public uint           CurrentOperatorID;
        ActorComponent        _currentOperator;
        public ActorComponent CurrentOperator => _currentOperator ??= Manager_Actor.GetActor(CurrentOperatorID);
        public bool           HasOperator()   => CurrentOperatorID != 0;
        public bool           IsOperatorMovingToOperatingArea;
        #endregion

        public OperatingAreaData(uint operatingAreaID, uint stationID)
        {
            OperatingAreaID   = operatingAreaID;
            StationID         = stationID;
            JobsiteID         = Station.StationData.JobsiteID;
            CurrentOperatorID = 0;
        }

        public bool AddOperatorToOperatingArea(uint operatorID)
        {
            if (CurrentOperatorID != 0) Debug.Log($"OperatingArea: {OperatingAreaID} replaced operator: {CurrentOperatorID} with new Operator {operatorID}");

            CurrentOperatorID = operatorID;
            Manager_Actor.GetActorData(CurrentOperatorID).CareerData.CurrentActorJob.SetOperatingAreaID(OperatingAreaID);
            return true;
        }

        public bool RemoveOperatorFromOperatingArea()
        {
            if (CurrentOperatorID == 0)
            {
                Debug.Log($"OperatingArea does not have current operator.");
                return false;
            }

            Manager_Actor.GetActorData(CurrentOperatorID).CareerData.CurrentActorJob.SetOperatingAreaID(0);
            CurrentOperatorID                                                           = 0;
            IsOperatorMovingToOperatingArea                                             = false;
            return true;
        }
    }
}
