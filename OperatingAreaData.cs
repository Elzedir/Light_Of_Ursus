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

    public ActorData CurrentOperator;
    public bool IsOperatorMovingToOperatingArea = false;

    public void InitialiseOperatingAreaData(int stationID)
    {
        Manager_OperatingArea.GetOperatingArea(OperatingAreaID).Initialise(stationID);
    }

    public bool AddOperatorToOperatingArea(ActorData operatorData)
    {
        if (CurrentOperator.ActorID != 0) Debug.Log($"OperatingArea: {OperatingAreaID} replaced operator: {CurrentOperator.ActorID}: {CurrentOperator.ActorName.GetName()}with new Operator {operatorData.ActorID}: {operatorData.ActorName.GetName()}");

        CurrentOperator = operatorData;
        CurrentOperator.CareerAndJobs.OperatingAreaID = OperatingAreaID;
        return true;
    }

    public bool RemoveOperatorFromOperatingArea()
    {
        if (CurrentOperator == null)
        {
            Debug.Log($"OperatingArea does not have current operator.");
            return false;
        }


        CurrentOperator.CareerAndJobs.OperatingAreaID = 0;
        CurrentOperator = null;
        IsOperatorMovingToOperatingArea = false;
        return true;
    }

    public bool HasOperator()
    {
        return CurrentOperator.ActorID != 0;
    }
}
