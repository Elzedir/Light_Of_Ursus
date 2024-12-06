using System.Collections.Generic;
using System.Linq;
using EmployeePosition;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace JobSite
{
    public class JobSite_Component_LumberYard : JobSite_Component
    {
        public override JobSiteName JobSiteName => JobSiteName.Lumber_Yard;

        protected override bool _compareProductionOutput()
        {
            // Temporary
            SetIdealRatio(3f);

            var producedItems = AllStationsInJobSite.Values
                                .SelectMany(s => s.StationData.ProductionData.ActualProductionRatePerHour)
                                .ToList();

            var mergedItems = producedItems
                              .GroupBy(item => item.ItemID)
                              .Select(group => new Item(group.Key, (uint)group.Sum(item => item.ItemAmount)))
                              .ToList();

            var duplicateItems = producedItems
                                 .GroupBy(item => item.ItemID)
                                 .Where(group => group.Count() > 1)
                                 .Select(group => group.Key)
                                 .ToList();

            foreach (var itemId in duplicateItems)
            {
                Debug.Log($"Item {itemId} were not merged correctly.");
            }

            float logProduction   = mergedItems.FirstOrDefault(item => item.ItemID == 1100)?.ItemAmount ?? 0;
            float plankProduction = mergedItems.FirstOrDefault(item => item.ItemID == 2300)?.ItemAmount ?? 0;

            float currentRatio = logProduction / plankProduction;

            float percentageDifference = Mathf.Abs(((currentRatio / IdealRatio) * 100) - 100);

            Debug.Log($"Log Average: {logProduction}, Plank Average: {plankProduction}, Percentage Difference: {percentageDifference}%");

            bool isBalanced = percentageDifference <= PermittedProductionInequality;

            if (!isBalanced)
            {
                _adjustProduction(IdealRatio);
            }

            return isBalanced;
        }

        // To make more efficient search for best combinations:
        // Implement a heuristic algorithm to guide the search towards the best combination.
        // Implement a percentage threshold to the ideal ratio to end the search early if a combination within the threshold is found.
        // Implement a minimum skill cap either determined by the crafted item skill requirement or a mean average of all employee skills to ensure that the employees assigned to the stations are skilled enough to operate them.

        protected override void _adjustProduction(float idealRatio)
        {
            var   allEmployees        = new List<uint>(JobSiteData.AllEmployeeIDs);
            var   bestCombination     = new List<uint>();
            float bestRatioDifference = float.MaxValue;

            var allCombinations = _getAllCombinations(allEmployees);
            int i               = 0;

            foreach (var combination in allCombinations)
            {
                _assignAllEmployeesToStations(combination);

                var estimatedProduction = AllStationsInJobSite.Values
                                          .SelectMany(s => s.StationData.ProductionData.GetEstimatedProductionRatePerHour())
                                          .ToList();

                var mergedEstimatedProduction = estimatedProduction
                                                .GroupBy(item => item.ItemID)
                                                .Select(group => new Item(group.Key, (uint)group.Sum(item => item.ItemAmount)))
                                                .ToList();

                float estimatedLogProduction   = mergedEstimatedProduction.FirstOrDefault(item => item.ItemID == 1100)?.ItemAmount ?? 0;
                float estimatedPlankProduction = mergedEstimatedProduction.FirstOrDefault(item => item.ItemID == 2300)?.ItemAmount ?? 0;

                float estimatedRatio  = estimatedLogProduction / estimatedPlankProduction;
                float ratioDifference = Mathf.Abs(estimatedRatio - idealRatio);

                i++;

                Debug.Log($"Combination {i} has eL: {estimatedLogProduction} eP: {estimatedPlankProduction} eR: {estimatedRatio} and rDif: {ratioDifference}");

                if (ratioDifference < bestRatioDifference)
                {
                    Debug.Log($"Combination {i} the is best ratio");

                    bestRatioDifference = ratioDifference;
                    bestCombination     = new List<uint>(combination);
                }
            }

            _assignAllEmployeesToStations(bestCombination);

            Debug.Log("Adjusted production to balance the ratio.");
        }

        protected override VocationName _getRelevantVocation(EmployeePositionName positionName)
        {
            switch (positionName)
            {
                case EmployeePositionName.Logger:
                    return VocationName.Logging;
                case EmployeePositionName.Sawyer:
                    return VocationName.Sawying;
                default:
                    Debug.Log($"EmployeePosition: {positionName} does not have a relevant vocation.");
                    return VocationName.None;
            }
        }
    }
}