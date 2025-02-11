using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Actors;
using Items;
using Jobs;
using UnityEngine;

namespace JobSites
{
    public class JobSite_Component_LumberYard : JobSite_Component
    {
        public override JobSiteName JobSiteName => JobSiteName.Lumber_Yard;
        
        protected override bool _compareProductionOutput()
        {
            // Temporary, maybe change to cost of items over product of items
            SetIdealRatio(3f);

            var producedItems = JobSite_Data.ProductionData.GetEstimatedProductionRatePerHour();

            //* Later, add a general application of this, rather than typing it out every time.
            float logProduction = producedItems.FirstOrDefault(item => item.ItemID == 1100)?.ItemAmount ?? 0;
            float plankProduction = producedItems.FirstOrDefault(item => item.ItemID == 2300)?.ItemAmount ?? 0;

            if (plankProduction == 0)
            {
                Debug.Log("Plank production is 0.");
                return false;
            }
            
            var currentRatio = logProduction / plankProduction;

            var percentageDifference = Mathf.Abs(((currentRatio / IdealRatio) * 100) - 100);

            Debug.Log($"Log Average: {logProduction}, Plank Average: {plankProduction}, Percentage Difference: {percentageDifference}%");

            var isBalanced = percentageDifference <= PermittedProductionInequality;

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
            var   allEmployees        = new Dictionary<ulong, Actor_Component>();
            
            //* Improve this

            foreach (var job in JobSite_Data.AllJobs.Values)
            {
                if (job.Actor is null) continue;
                
                allEmployees.Add(job.Actor.ActorID, job.Actor);
            }
            
            var   bestCombination     = new Dictionary<ulong, Actor_Component>();
            var bestRatioDifference = float.MaxValue;

            var allCombinations = _getAllCombinations(allEmployees);
            var i               = 0;

            foreach (var combination in allCombinations)
            {
                _assignAllEmployeesToStations(combination);

                var estimatedProduction = JobSite_Data.GetEstimatedProductionRatePerHour();

                var mergedEstimatedProduction = estimatedProduction
                                                .GroupBy(item => item.ItemID)
                                                .Select(group => new Item(group.Key, (ulong)group.Sum(item => (int)item.ItemAmount)))
                                                .ToList();

                float estimatedLogProduction   = mergedEstimatedProduction.FirstOrDefault(item => item.ItemID == 1100)?.ItemAmount ?? 0;
                float estimatedPlankProduction = mergedEstimatedProduction.FirstOrDefault(item => item.ItemID == 2300)?.ItemAmount ?? 0;

                var estimatedRatio  = estimatedLogProduction / estimatedPlankProduction;
                var ratioDifference = Mathf.Abs(estimatedRatio - idealRatio);

                i++;

                Debug.Log($"Combination {i} has eL: {estimatedLogProduction} eP: {estimatedPlankProduction} eR: {estimatedRatio} and rDif: {ratioDifference}");

                if (!(ratioDifference < bestRatioDifference)) continue;
                
                Debug.Log($"Combination {i} the is best ratio");

                bestRatioDifference = ratioDifference;
                bestCombination     = new Dictionary<ulong, Actor_Component>(combination);
            }

            _assignAllEmployeesToStations(bestCombination);

            Debug.Log("Adjusted production to balance the ratio.");
        }

        protected override VocationName _getRelevantVocation(JobName positionName)
        {
            switch (positionName)
            {
                case JobName.Logger:
                    return VocationName.Logging;
                case JobName.Sawyer:
                    return VocationName.Sawying;
                default:
                    Debug.Log($"EmployeePosition: {positionName} does not have a relevant vocation.");
                    return VocationName.None;
            }
        }
    }
}