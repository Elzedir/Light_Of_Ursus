using System;
using System.Collections.Generic;

public class StationComponent_Storage : StationComponent
{
    public virtual List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        throw new ArgumentException("Cannot use base class.");
    }
}
