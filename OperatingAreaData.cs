using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OperatingAreaData
{
    public int OperatingAreaID;
    public int StationID;

    public bool StationIsActive = true;

    public int CurrentOperatorID;
    public bool HasOperator() => CurrentOperatorID != -1;
    public bool IsOperatorMovingToOperatingArea = false;

    public void InitialiseOperatingAreaData()
    {
        Manager_OperatingArea.GetOperatingArea(OperatingAreaID).Initialise();
    }

    public bool AddOperatorToOperatingArea(int operatorData)
    {
        if (CurrentOperatorID != 0) Debug.Log($"OperatingArea: {OperatingAreaID} replaced operator: {CurrentOperatorID} with new Operator {operatorData}");

        CurrentOperatorID = operatorData;
        Manager_Actor.GetActorData(CurrentOperatorID).CareerAndJobs.OperatingAreaID = OperatingAreaID;
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
