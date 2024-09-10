using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StationComponent_LogPile : StationComponent
{
    public override EmployeePosition CoreEmployeePosition => EmployeePosition.Hauler;

    public override int OperatingAreaCount => 4;
    protected override OperatingAreaComponent _createOperatingArea(int operatingAreaID)
    {
        var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
        operatingAreaComponent.transform.SetParent(transform);
        
        switch(operatingAreaID)
        {
            case 1:
                operatingAreaComponent.transform.localPosition = new Vector3(0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 2:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, 0.75f);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 3:
                operatingAreaComponent.transform.localPosition = new Vector3(-0.75f, 0f, 0);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            case 4:
                operatingAreaComponent.transform.localPosition = new Vector3(0, 0f, -0.75f);
                operatingAreaComponent.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                break;
            default:
                Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {OperatingAreaCount}.");
                break;
        }

        var operatingArea = operatingAreaComponent.AddComponent<BoxCollider>();
        operatingArea.isTrigger = true;
        operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

        return operatingAreaComponent;
    }
    public bool SalesEnabled = false;

    public override void InitialiseStationName()
    {
        StationData.SetStationName(StationName.Log_Pile);
    }

    public override void InitialiseRequiredEmployeePositions()
    {
        AllRequiredEmployeePositions = new() { EmployeePosition.None };
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
