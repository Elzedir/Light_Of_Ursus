using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Sawmill : StationComponent_Crafter
{
    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Sawmill);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Lumberjack, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        yield break;
        // Open inventory
    }

    public override bool EmployeeCanUse(EmployeePosition employeePosition)
    {
        return new List<EmployeePosition> 
        { 
            EmployeePosition.Sawyer, EmployeePosition.Assistant_Sawyer 
        }
        .Contains(employeePosition);
    }

    public override IEnumerator CraftItem(Actor_Base actor)
    {
        yield break;
    }

    public override IEnumerator CraftItemAll(Actor_Base actor)
    {
        yield break;
    }
}
