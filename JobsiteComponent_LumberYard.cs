using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobsiteComponent_LumberYard : JobsiteComponent
{
    protected override bool _compareProductionOutput()
    {
        var producedItems = AllStationsInJobsite
        .SelectMany(s => s.StationData.ProductionData.ActualProductionRatePerHour)
        .ToList();

        var mergedItems = producedItems
        .GroupBy(item => item.CommonStats_Item.ItemID)
        .Select(group => Manager_Item.GetItem(group.Key, group.Sum(item => item.CommonStats_Item.CurrentStackSize)))
        .ToList();

        var duplicateItems = producedItems
        .GroupBy(item => item.CommonStats_Item.ItemID)
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)
        .ToList();

        foreach (var itemId in duplicateItems)
        {
            Debug.Log($"Item {itemId} were not merged correctly.");
        }

        float logProduction = mergedItems.FirstOrDefault(item => item.CommonStats_Item.ItemID == 1100)?.CommonStats_Item.CurrentStackSize ?? 0;
        float plankProduction = mergedItems.FirstOrDefault(item => item.CommonStats_Item.ItemID == 2300)?.CommonStats_Item.CurrentStackSize ?? 0;

        float currentRatio = logProduction / plankProduction;
        float idealRatio = 3f;

        float percentageDifference = Mathf.Abs(((currentRatio / idealRatio) * 100) - 100);

        Debug.Log($"Log Average: {logProduction}, Plank Average: {plankProduction}, Percentage Difference: {percentageDifference}%");

        bool isBalanced = percentageDifference <= PermittedProductionInequality;

        if (!isBalanced)
        {
            _adjustProduction(logProduction, plankProduction, idealRatio);
        }

        return isBalanced;
    }

    protected void _adjustProduction(float logProduction, float plankProduction, float idealRatio)
    {
        var allEmployees = new List<ActorData>(JobsiteData.AllEmployees);
        var bestCombination = new List<ActorData>();
        float bestRatioDifference = float.MaxValue;

        var allCombinations = _getAllCombinations(allEmployees);
        int i = 0;

        foreach (var combination in allCombinations)
        {
            _assignEmployeesToStations(combination);

            var estimatedProduction = AllStationsInJobsite
                .SelectMany(s => s.StationData.ProductionData.GetEstimatedProductionRatePerHour())
                .ToList();

            var mergedEstimatedProduction = estimatedProduction
            .GroupBy(item => item.CommonStats_Item.ItemID)
            .Select(group => Manager_Item.GetItem(group.Key, group.Sum(item => item.CommonStats_Item.CurrentStackSize)))
            .ToList();

            float estimatedLogProduction = mergedEstimatedProduction.FirstOrDefault(item => item.CommonStats_Item.ItemID == 1100)?.CommonStats_Item.CurrentStackSize ?? 0;
            float estimatedPlankProduction = mergedEstimatedProduction.FirstOrDefault(item => item.CommonStats_Item.ItemID == 2300)?.CommonStats_Item.CurrentStackSize ?? 0;

            float estimatedRatio = estimatedLogProduction / estimatedPlankProduction;
            float ratioDifference = Mathf.Abs(estimatedRatio - idealRatio);

            i++;

            Debug.Log($"Combination {i} has eL: {estimatedLogProduction} eP: {estimatedPlankProduction} eR: {estimatedRatio} and rDif: {ratioDifference}");

            if (ratioDifference < bestRatioDifference)
            {
                Debug.Log($"Combination {i} the is best ratio");

                bestRatioDifference = ratioDifference;
                bestCombination = new List<ActorData>(combination);
            }
        }

        _assignEmployeesToStations(bestCombination);

        Debug.Log("Adjusted production to balance the ratio.");
    }

    protected override VocationName _getRelevantVocation(EmployeePosition position)
    {
        switch (position)
        {
            case EmployeePosition.Logger:
            case EmployeePosition.Assistant_Logger:
                return VocationName.Logger;
            case EmployeePosition.Sawyer:
            case EmployeePosition.Assistant_Sawyer:
                return VocationName.Sawyer;
            default:
                return VocationName.None;
        }
    }
}