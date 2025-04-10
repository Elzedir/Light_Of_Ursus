using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Careers;
using Initialisation;
using Jobs;
using Priorities;
using Settlements;
using Station;
using TickRates;
using UnityEngine;
using Station_Component = Station.Station_Component;

namespace Buildings
{
    public class Building_Component : MonoBehaviour
    {
        //* Change this to be a plot of land rather.
        
        public Building_Data Building_Data;

        public ulong ID;

        Settlement_Component _settlement;
        
        public Settlement_Component Settlement => _settlement ??= GetComponentInParent<Settlement_Component>();

        void Awake()
        {
            Manager_Initialisation.OnInitialiseBuildings += _initialise;
            Manager_Initialisation.OnInitialiseBuildingData += InitialiseBuildingData;
        }

        void OnDestroy()
        {
            Manager_TickRate.UnregisterTicker(TickerTypeName.Building, TickRateName.OneSecond, ID);
            Manager_TickRate.UnregisterTicker(TickerTypeName.Building, TickRateName.TenSeconds, ID);
            
            Manager_Initialisation.OnInitialiseBuildings -= _initialise;
            Manager_Initialisation.OnInitialiseBuildingData -= InitialiseBuildingData;
        }

        void _initialise()
        {
            _setTickers();
        }

        void InitialiseBuildingData()
        {
            Building_Data.InitialiseBuildingData();
        }

        void _setTickers()
        {
            Manager_TickRate.RegisterTicker(TickerTypeName.Building, TickRateName.OneSecond, ID, OnTickOneSecond);
            Manager_TickRate.RegisterTicker(TickerTypeName.Building, TickRateName.TenSeconds, ID, OnTickTenSeconds);
        }

        public void BuildNewBuilding(BuildingType buildingType)
        {
            
        }

        public void BuildCustomBuilding(Building_Data building_Data)
        {
            
        }

        public void OnTickOneSecond()
        {
            Building_Data.OnTickOneSecond();
        }

        public void OnTickTenSeconds()
        {
            Building_Data.Production.GetEstimatedProductionRatePerHour();

            _compareProductionOutput();
            
            Building_Data.Priorities.RegenerateAllPriorities(DataChangedName.None);

            //* Temporary fix to move people from recreation to another job.
            foreach (var worker in Building_Data.Jobs.AllJobs.Values
                         .Where(job => job.Station.StationType == StationType.Recreation && job.ActorID != 0))
            {
                Building_Data.AssignActorToNewCurrentJob(worker.Actor);
            }
            
            //* Eventually change to once per day.
            Building_Data.Jobs.FillEmptyBuildingPositions();
        }

        // To make more efficient search for best combinations:
        // Implement a heuristic algorithm to guide the search towards the best combination.
        // Implement a percentage threshold to the ideal ratio to end the search early if a combination within the threshold is found.
        // Implement a minimum skill cap either determined by the crafted item skill requirement or a mean average of all employee skills to ensure that the employees assigned to the stations are skilled enough to operate them.

        protected void _assignAllEmployeesToStations(Dictionary<ulong, Actor_Component> allEmployees)
        {
            Building_Data.RemoveAllWorkersFromAllJobs();

            var tempEmployees = allEmployees.Select(employee => employee.Value).ToList();

            var employeesForStation = tempEmployees
                .OrderByDescending(actor =>
                    actor.ActorData.Career.CurrentJob != null
                        ? actor.ActorData.Vocation.GetVocationExperience(
                            _getRelevantVocation(actor.ActorData.Career.CurrentJob.JobName))
                        : 0)
                .ToList();

            foreach (var employee in employeesForStation)
            {
                if (!Building_Data.AssignActorToNewCurrentJob(employee))
                {
                    Debug.LogError($"Could not assign actor {employee.ActorID} to a job.");
                    continue;
                }
                    
                tempEmployees.Remove(employee);
            }

            if (tempEmployees.Count > 0)
            {
                Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
            }
        }

        protected List<Dictionary<ulong, Actor_Component>> _getAllCombinations(
            Dictionary<ulong, Actor_Component> employees)
        {
            var result = new List<Dictionary<ulong, Actor_Component>>();
            var employeeKeys = new List<ulong>(employees.Keys);
            var combinationCount = (int)Mathf.Pow(2, employeeKeys.Count);

            for (var i = 1; i < combinationCount; i++)
            {
                var combination = employeeKeys.Where((_, j) => (i & (1 << j)) != 0)
                    .ToDictionary(key => key, key => employees[key]);

                result.Add(combination);
            }

            return result;
        }
        
        public HashSet<StationName> GetStationNames() => Building_Data.Jobs.AllJobs.Values.Select(job => job.Station.StationName).ToHashSet();

        public void GetRelevantStations(ActorActionName actorActionName, Priority_Parameters priority_Parameters)
        {
            if (!RelevantStations.TryGetValue(actorActionName, out var relevantStations))
                throw new ArgumentException($"ActorActionName: {actorActionName} not recognised.");
            
            relevantStations(priority_Parameters);
        }

        Dictionary<ActorActionName, Action<Priority_Parameters>> _relevantStations;
        public Dictionary<ActorActionName, Action<Priority_Parameters>> RelevantStations => _getRelevantStations();

        Dictionary<ActorActionName, Action<Priority_Parameters>> _getRelevantStations()
        {
            return new Dictionary<ActorActionName, Action<Priority_Parameters>>
            {
                { ActorActionName.Idle, _relevantStations_Idle },
                { ActorActionName.Haul, _relevantStations_Haul },
                { ActorActionName.Chop_Wood, _relevantStations_Chop_Wood },
                { ActorActionName.Process_Logs, _relevantStations_Process_Logs },
                { ActorActionName.Wander, _relevantStations_Wander }
            };
        }
        
        void _relevantStations_Idle(Priority_Parameters priority_Parameters)
        {
            //* Later, replace with recreational stations    
        }

        void _relevantStations_Wander(Priority_Parameters priority_Parameters)
        {
            //* This wouldn't make sense, I think, you do not wander between stations, that would be idling?
        }


        void _relevantStations_Haul(Priority_Parameters priority_Parameters)
        {
            var allFetchStations = new Dictionary<ulong, Station_Component>();
            var allFetchItems = new Dictionary<ulong, ulong>();
            var allDeliverStations = new Dictionary<ulong, Station_Component>();

            foreach (var job in Building_Data.Jobs.AllJobs.Values)
            {
                foreach (var itemToFetch in job.Station.GetItemsToFetchFromThisStation())
                {
                    allFetchStations.TryAdd(job.StationID, job.Station);

                    if (!allFetchItems.TryAdd(itemToFetch.Key, itemToFetch.Value))
                        allFetchItems[itemToFetch.Key] += itemToFetch.Value;
                }
            }

            foreach (var job in Building_Data.Jobs.AllJobs.Values)
            {
                foreach (var itemToFetch in allFetchItems)
                {
                    if (!job.Station.DesiredStoredItemIDs.Contains(itemToFetch.Key)) continue;
                    
                    allDeliverStations.TryAdd(job.StationID, job.Station);
                }
            }
            
            priority_Parameters.AllStation_Sources = allFetchStations.Values.ToList();
            priority_Parameters.AllStation_Targets = allDeliverStations.Values.ToList();
        }
        
        //* Fix these

        void _relevantStations_Chop_Wood(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.AllStation_Sources = Building_Data.Jobs.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Tree &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }

        void _relevantStations_Process_Logs(Priority_Parameters priority_Parameters)
        {
            priority_Parameters.AllStation_Sources = Building_Data.Jobs.AllJobs.Values
                .Where(job => job.Station.StationName == StationName.Sawmill &&
                                  job.Station.Station_Data.GetOpenWorkPost() is not null)
                .Select(job => job.Station).ToList();
        }
        
        public float IdealRatio;
        public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
        public int PermittedProductionInequality = 10;
        
        bool _compareProductionOutput()
        {
            //* Also, this is designed for a Lumber_Yard, so change that.
            
            // Temporary, maybe change to cost of items over product of items
            SetIdealRatio(3f);

            var producedItems = Building_Data.Production.GetEstimatedProductionRatePerHour();

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

        void _adjustProduction(float idealRatio)
        {
            var   allEmployees        = new Dictionary<ulong, Actor_Component>();
            
            //* Improve this
            //* Also, this is designed for a Lumber_Yard, so change that.

            foreach (var job in Building_Data.Jobs.AllJobs.Values)
            {
                if (job.Actor is null) continue;
                
                allEmployees.Add(job.Actor.ActorID, job.Actor);
            }
            
            var   bestCombination     = new Dictionary<ulong, Actor_Component>();
            var bestRatioDifference = float.PositiveInfinity;

            var allCombinations = _getAllCombinations(allEmployees);
            var i               = 0;

            foreach (var combination in allCombinations)
            {
                _assignAllEmployeesToStations(combination);

                var estimatedProduction = Building_Data.GetEstimatedProductionRatePerHour();

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

        static VocationName _getRelevantVocation(JobName positionName)
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

        Dictionary<BuildingType, CareerName> _defaultCareers;
        public Dictionary<BuildingType, CareerName> DefaultCareers => _defaultCareers ??= new Dictionary<BuildingType, CareerName>
        {
            {
                BuildingType.Lumber_Yard,
                CareerName.Lumberjack
            },
            {
                BuildingType.Smithy,
                CareerName.Smith
            }
        };
    }
}