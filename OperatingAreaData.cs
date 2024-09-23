using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OperatingAreaData
{
    #region ID Info
    public int OperatingAreaID;
    public int StationID;
    StationComponent _station;
    public StationComponent Station { get { return _station ??= Manager_Station.GetStation(StationID); } }
    public int JobsiteID;
    JobsiteComponent _jobsite;
    public JobsiteComponent Jobsite { get { return _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID); } }
    #endregion

    public bool OperatingAreaIsActive = true;

    #region Operator
    public int CurrentOperatorID;
    ActorComponent _currentOperator;
    public ActorComponent CurrentOperator { get { return _currentOperator ??= Manager_Actor.GetActor(CurrentOperatorID); } }
    public bool HasOperator() => CurrentOperatorID != 0;
    public bool IsOperatorMovingToOperatingArea = false;
    #endregion

    public OperatingAreaData(int operatingAreaID, int stationID)
    {
        OperatingAreaID = operatingAreaID;
        StationID = stationID;
        JobsiteID = Station.StationData.JobsiteID;
        CurrentOperatorID = 0;
    }

    public bool AddOperatorToOperatingArea(int operatorID)
    {
        if (CurrentOperatorID != 0) Debug.Log($"OperatingArea: {OperatingAreaID} replaced operator: {CurrentOperatorID} with new Operator {operatorID}");

        CurrentOperatorID = operatorID;
        Manager_Actor.GetActorData(CurrentOperatorID).CareerAndJobs.SetOperatingAreaID(OperatingAreaID);
        return true;
    }

    public bool RemoveOperatorFromOperatingArea()
    {
        if (CurrentOperatorID == -1)
        {
            Debug.Log($"OperatingArea does not have current operator.");
            return false;
        }

        Manager_Actor.GetActorData(CurrentOperatorID).CareerAndJobs.OperatingAreaID = 0;
        CurrentOperatorID = -1;
        IsOperatorMovingToOperatingArea = false;
        return true;
    }
}
