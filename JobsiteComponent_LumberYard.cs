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
        var allEmployees = new List<EmployeeData>(JobsiteData.AllEmployees);
        var bestCombination = new List<EmployeeData>();
        float bestRatioDifference = float.MaxValue;

        var allCombinations = GetAllCombinations(allEmployees);
        int i = 0;

        foreach (var combination in allCombinations)
        {
            AssignEmployeesToStations(combination);

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
                bestCombination = new List<EmployeeData>(combination);
            }
        }

        AssignEmployeesToStations(bestCombination);

        Debug.Log("Adjusted production to balance the ratio.");
    }

    private void AssignEmployeesToStations(List<EmployeeData> employees)
    {
        foreach (var station in AllStationsInJobsite)
        {
            station.RemoveAllOperators();
        }

        foreach (var station in AllStationsInJobsite)
        {
            var allowedPositions = station.AllowedEmployeePositions;
            var employeesForStation = employees
                .Where(e => allowedPositions.Contains(e.ActorData.CareerAndJobs.EmployeePosition))
                .OrderByDescending(e => e.ActorData.CareerAndJobs.EmployeePosition)
                .ThenByDescending(e => e.ActorData.VocationData.GetVocationExperience(GetRelevantVocation(e.ActorData.CareerAndJobs.EmployeePosition)))
                .ToList();

            foreach (var employee in employeesForStation)
            {
                station.StationData.AddOperatorToStation(employee.ActorData);
                employees.Remove(employee);
            }
        }
    }

    private List<List<EmployeeData>> GetAllCombinations(List<EmployeeData> employees)
    {
        var result = new List<List<EmployeeData>>();
        int combinationCount = (int)Mathf.Pow(2, employees.Count);

        for (int i = 1; i < combinationCount; i++)
        {
            var combination = new List<EmployeeData>();
            for (int j = 0; j < employees.Count; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    combination.Add(employees[j]);
                }
            }
            result.Add(combination);
        }

        return result;
    }

    private VocationName GetRelevantVocation(EmployeePosition position)
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