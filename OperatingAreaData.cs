using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Managers;
using UnityEngine;

[Serializable]
public class OperatingAreaData
{
    #region ID Info
    public uint OperatingAreaID;
    public uint StationID;
    StationComponent _station;
    public StationComponent Station { get { return _station ??= Manager_Station.GetStation(StationID); } }
    public uint JobsiteID;
    JobsiteComponent _jobsite;
    public JobsiteComponent Jobsite { get { return _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID); } }
    #endregion

    public bool OperatingAreaIsActive = true;

    #region Operator
    public uint CurrentOperatorID;
    ActorComponent _currentOperator;
    public ActorComponent CurrentOperator => _currentOperator ??= Manager_Actor.GetActor(CurrentOperatorID);
    public bool HasOperator() => CurrentOperatorID != 0;
    public bool IsOperatorMovingToOperatingArea = false;
    #endregion

    public OperatingAreaData(uint operatingAreaID, uint stationID)
    {
        OperatingAreaID = operatingAreaID;
        StationID = stationID;
        JobsiteID = Station.StationData.JobsiteID;
        CurrentOperatorID = 0;
    }

    public bool AddOperatorToOperatingArea(uint operatorID)
    {
        if (CurrentOperatorID != 0) Debug.Log($"OperatingArea: {OperatingAreaID} replaced operator: {CurrentOperatorID} with new Operator {operatorID}");

        CurrentOperatorID = operatorID;
        Manager_Actor.GetActorData(CurrentOperatorID).CareerAndJobs.SetOperatingAreaID(OperatingAreaID);
        return true;
    }

    public bool RemoveOperatorFromOperatingArea()
    {
        if (CurrentOperatorID == 0)
        {
            Debug.Log($"OperatingArea does not have current operator.");
            return false;
        }

        Manager_Actor.GetActorData(CurrentOperatorID).CareerAndJobs.OperatingAreaID = 0;
        CurrentOperatorID = 0;
        IsOperatorMovingToOperatingArea = false;
        return true;
    }
}
