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
        return inventoryOwner.GetInventoryData().InventoryItems.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }
}
