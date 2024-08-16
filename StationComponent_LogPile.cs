using System.Collections.Generic;
using System.Linq;

public class StationComponent_LogPile : StationComponent
{
    public bool SalesEnabled = false;

    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Log_Pile);
    }

    public override void InitialiseAllowedEmployeePositions()
    {
        AllowedEmployeePositions = new() { EmployeePosition.None };
    }

    public override void InitialiseAllowedRecipes()
    {
        
    }

    public override List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        return inventoryOwner.InventoryData.InventoryItems.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }
}
