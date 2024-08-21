using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobsiteComponent_LumberYard : JobsiteComponent
{
    public int PermittedProductionInequality = 10;
    public Queue<int> LogInputs = new();
    public Queue<int> PlankOutputs = new();
    public int LogInputSum = 0;
    public int PlankOutputSum = 0;
    public int OutputBatchCount = 5;
    public bool CustomerPresent = false;

    public override void OnTick()
    {
        _getStationProduction(out int logsProduced, out int planksProduced);

        LogInputs.Enqueue(logsProduced);
        PlankOutputs.Enqueue(planksProduced);

        LogInputSum += logsProduced;
        PlankOutputSum += planksProduced;

        if (LogInputs.Count > OutputBatchCount)
        {
            LogInputSum -= LogInputs.Dequeue();
        }

        if (PlankOutputs.Count > OutputBatchCount)
        {
            PlankOutputSum -= PlankOutputs.Dequeue();
        }

        _compareProductionOutput();
    }

    public override TickRate GetTickRate()
    {
        return TickRate.OneHundredSeconds;
    }

    protected override bool _compareProductionOutput()
    {
        if (PlankOutputSum == 0) return false;

        float logAverage = LogInputSum / (float)OutputBatchCount;
        float plankAverage = PlankOutputSum / (float)OutputBatchCount;

        float expectedRatio = logAverage / plankAverage;
        float idealRatio = 3f;

        float percentageDifference = Mathf.Abs(((expectedRatio / idealRatio) * 100) - 100);

        Debug.Log($"Log Average: {logAverage}, Plank Average: {plankAverage}, Percentage Difference: {percentageDifference}%");

        bool isBalanced = percentageDifference <= PermittedProductionInequality;

        if (!isBalanced)
        {
            AdjustProduction(expectedRatio, idealRatio);
        }

        return isBalanced;
    }

    private void AdjustProduction(float actualRatio, float idealRatio)
    {
        if (actualRatio > idealRatio)
        {
            _redistributeEmployees();
            Debug.Log("Increase sawmill efficiency or assign more workers to balance production.");
        }
        else
        {
            _redistributeEmployees();
            Debug.Log("Increase logging efficiency or assign more workers to balance production.");
        }
    }

    protected override void _redistributeEmployees()
    {
        foreach (var station in AllStationsInJobsite)
        {
            station.RemoveOperator();
        }


        // For each station, rank each employee (weighting) based on position and its relevance vs seniority, then select one and move onto another station type
        // and find another employee, and then go until all employees are used. If you run out of stations with single employees, then start allocating multiple employees
        // to the same stations.
    }

    void _getStationProduction(out int logsProduced, out int planksProduced)
    {
        List<Item> producedItems = new();

        foreach(var station in AllStationsInJobsite)
        {
            foreach(var item in station.GetProductedItems())
            {
                if (!producedItems.Any(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID)) producedItems.Add(item);
                else
                {
                    producedItems.FirstOrDefault(i => 
                    i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).CommonStats_Item.CurrentStackSize += item.CommonStats_Item.CurrentStackSize;
                }
            }
        }

        logsProduced = 0;
        planksProduced = 0;

        foreach(var item in producedItems)
        {
            if (item.CommonStats_Item.ItemID == 1100)
            {
                logsProduced = item.CommonStats_Item.CurrentStackSize;
            }
            else if (item.CommonStats_Item.ItemID == 2300)
            {
                planksProduced = item.CommonStats_Item.CurrentStackSize;
            }
            else
            {
                Debug.Log($"Somehow collected item: {item.CommonStats_Item.ItemID}: {item.CommonStats_Item.ItemName} from stations");
            }
        }
    }
}