using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Tree : StationComponent_Resource
{
    public override void InitialiseStationName()
    {
        StationData._stationName = StationName.Tree;
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Lumberjack, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
    }

    protected override IEnumerator _gather()
    {
        yield return new WaitForSeconds(1);
    }

    protected override List<Item> _getResourceYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(1100, 3) }; // For now

        // Base resource yield on actor relevant skill
    }
}
