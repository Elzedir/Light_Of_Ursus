using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Sawmill : StationComponent_Crafter
{
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

    }

    public override IEnumerator CraftItemAll(Actor_Base actor)
    {
        
    }
}
