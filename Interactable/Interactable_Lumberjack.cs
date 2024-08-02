using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Lumberjack : Interactable_Base
{
    public bool IsActive = true;
    public List<EmployeePosition> EmployeePositions;

    public GameObject GameObject { get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
