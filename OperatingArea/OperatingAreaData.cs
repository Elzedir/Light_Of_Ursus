using System;
using Actor;
using Jobsite;
using Managers;
using Station;
using UnityEngine;

namespace OperatingArea
{
    [Serializable]
    public class OperatingAreaData
    {
        #region ID Info
        public uint              OperatingAreaID;
        public uint              StationID;
        Station_Component        _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);
        public uint              JobsiteID;
        JobsiteComponent         _jobsite;
        public JobsiteComponent  Jobsite => _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID);
        #endregion

        #region Operator
        public uint           CurrentOperatorID;
        Actor_Component        _currentOperator;
        public Actor_Component CurrentOperator => _currentOperator ??= Actor_Manager.GetActor(CurrentOperatorID);
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
            
            if (Actor_Manager.GetActorData(operatorID).CareerData.GetNewCurrentJob(StationID))
            {
                CurrentOperatorID = operatorID;
                return true;
            }

            Debug.Log($"OperatingArea: {OperatingAreaID} failed to add operator: {operatorID} to Station: {StationID}");
            return false;
        }

        public bool RemoveOperatorFromOperatingArea()
        {
            if (CurrentOperatorID == 0)
            {
                Debug.Log($"OperatingArea does not have current operator.");
                return false;
            }

            Actor_Manager.GetActorData(CurrentOperatorID).CareerData.StopCurrentJob();
            CurrentOperatorID               = 0;
            IsOperatorMovingToOperatingArea = false;
            return true;
        }
    }
}
