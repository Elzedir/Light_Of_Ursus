using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationComponent_LogPile : StationComponent
{
    public bool SalesEnabled = false;

    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Log_Pile);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        NecessaryEmployeePosition = EmployeePosition.None;
        AllAllowedEmployeePositions = new() { EmployeePosition.None };
    }

    public override void InitialiseAllowedRecipes()
    {
        
    }

    public override List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.GetInventoryData().AllInventoryItems.Where(i => i.ItemID == 2300)
        .Select(i => new Item(i.ItemID, i.ItemAmount)).ToList();
    }
}
