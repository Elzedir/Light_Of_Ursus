using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OperatingAreaData
{
    public int OperatingAreaID;
    public int StationID;

    public bool OperatingAreaIsActive = true;

    public int CurrentOperatorID;
    public bool HasOperator() => CurrentOperatorID != 0;
    public bool IsOperatorMovingToOperatingArea = false;

    public OperatingAreaData(int operatingAreaID, int stationID)
    {
        OperatingAreaID = operatingAreaID;
        StationID = stationID;
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
