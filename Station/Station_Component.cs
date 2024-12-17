using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using EmployeePosition;
using Initialisation;
using Inventory;
using Items;
using Jobs;
using JobSite;
using Priority;
using Recipe;
using TickRates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using WorkPosts;

namespace Station
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Station_Component : MonoBehaviour, IInteractable
    {
        public uint             StationID => StationData.StationID;
        public JobSite_Component JobSite   => StationData.JobSite_Component;
        bool                    _initialised;

        public          Station_Data StationData;
        public abstract StationName  StationName            { get; }
        public abstract StationType  StationType            { get; }
        bool                         _isStationBeingOperated => AllOperatingAreasInStation.Any(oa => oa.WorkPostData.CurrentOperatorID != 0);

        public float InteractRange { get; private set; }
    
        public abstract         EmployeePositionName       CoreEmployeePositionName { get; }
        protected abstract      RecipeName                 _defaultProduct          { get; }
        public abstract         List<RecipeName>           AllowedRecipes           { get; }
        public abstract         List<uint>                 AllowedStoredItemIDs     { get; }
        public abstract         List<uint>                 DesiredStoredItemIDs     { get; }
        protected abstract      uint                       _operatingAreaCount       { get; }
        public abstract         List<EmployeePositionName> AllowedEmployeePositions { get; }
        public         abstract List<JobName>              AllowedJobs              { get; }

        const    float      _baseProgressRatePerHour = 5;
        readonly List<Item> _currentProductsCrafted  = new();
    
        PriorityComponent_Station        _priorityComponent;
        public PriorityComponent_Station PriorityComponent => _priorityComponent ??= new PriorityComponent_Station(); 

        BoxCollider        _boxCollider;
        public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();

        // Temporary
        public Transform CollectionPoint;
        
        public void SetStationData(Station_Data stationData)
        {
            StationData = stationData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseStations += _initialise;
        }

        void _initialise()
        {
            StationData.InitialiseStationData();
        
            var employeeOperatingAreaPairs = from operatingArea in AllOperatingAreasInStation
                                             from employeeID in StationData.CurrentOperatorIDs
                                             let actorData = Actor_Manager.GetActor_Data(employeeID)
                                             where actorData.CareerData.CurrentJob.OperatingAreaID == operatingArea.WorkPostData.WorkPostID
                                             select new { operatingArea, employeeID };

            foreach (var pair in employeeOperatingAreaPairs)
            {
                pair.operatingArea.WorkPostData.AddOperatorToOperatingArea(pair.employeeID);
            }

            SetInteractRange();
            _initialiseStartingInventory();

            _setTickRate(TickRateName.OneSecond, false);

            _initialised = true;
        }

        public WorkPost_Component GetRelevantOperatingArea(Actor_Component actor)
        {
            var relevantOperatingArea = AllOperatingAreasInStation.FirstOrDefault(oa => !oa.HasOperator());

            if (relevantOperatingArea is not null)
            {
                return relevantOperatingArea;
            }

            Debug.Log("No relevant operating area found for actor.");
            return null;
        }

        public void AddOperatorToArea(uint operatorID)
        {
            var openOperatingArea = AllOperatingAreasInStation.FirstOrDefault(area => !area.WorkPostData.HasOperator());
        
            if (openOperatingArea is not null)
            {
                openOperatingArea.WorkPostData.AddOperatorToOperatingArea(operatorID);
            }
            else
            {
                Debug.Log("No open operating areas found in all operating areas.");
            }
        }
    
        public void RemoveAllOperatorsFromStation()
        {
            foreach (var operatingArea in AllOperatingAreasInStation)
            {
                operatingArea.WorkPostData.RemoveCurrentOperatorFromOperatingArea();
            }
        }

        public void RemoveOperatorFromArea(uint operatorID)
        {
            if (AllOperatingAreasInStation.Any(area => area.WorkPostData.CurrentOperatorID == operatorID))
            {
                AllOperatingAreasInStation.FirstOrDefault(area => area.WorkPostData.CurrentOperatorID == operatorID)?
                    .WorkPostData.RemoveCurrentOperatorFromOperatingArea();
            }
            else
            {
                Debug.Log($"Operator {operatorID} not found in operating areas.");
            }
        }

        public void RemoveAllOperators()
        {
            foreach(var operatingArea in AllOperatingAreasInStation)
            {
                operatingArea.WorkPostData.RemoveCurrentOperatorFromOperatingArea();
            }
        }
    
        TickRateName _currentTickRateName;

        void _setTickRate(TickRateName tickRateName, bool unregister = true)
        {
            if (_currentTickRateName == tickRateName) return;
            
            if (unregister) Manager_TickRate.UnregisterTicker(TickerTypeName.Station, _currentTickRateName, StationID);
            
            Manager_TickRate.RegisterTicker(TickerTypeName.Station, tickRateName, StationID, _onTick);
            _currentTickRateName = tickRateName;
        }

        void _onTick()
        {
            if (!_initialised) return;

            _operateStation();
        }

        void _operateStation()
        {
            if (!_passesStationChecks()) return;

            foreach (var operatingArea in AllOperatingAreasInStation)
            {
                var progressMade = operatingArea.Operate(_baseProgressRatePerHour,
                    StationData.StationProgressData.CurrentProduct);
                
                var itemCrafted = StationData.StationProgressData.Progress(progressMade);
                
                Debug.Log(3);

                if (!itemCrafted) continue;

                // For now is the final person who adds the last progress, but change to a cumulative system later.
                _craftItem(
                    StationData.StationProgressData.CurrentProduct.RecipeName,
                    Actor_Manager.GetActor_Component(operatingArea.WorkPostData.CurrentOperatorID)
                );
            }
        }

        bool _passesStationChecks()
        {
            if (!StationData.StationIsActive) 
                return false;
        
            if (!_isStationBeingOperated) 
                return false;
            
            Debug.Log(1);

            var success = false;

            while (!success)
            {
                try
                {
                    success = StationData.InventoryData.InventoryContainsAllItems(
                        StationData.StationProgressData.CurrentProduct.RequiredIngredients);
                    break;
                }
                catch (Exception e)
                {
                    StationData.StationProgressData.CurrentProduct ??= Recipe_Manager.GetRecipe_Master(_defaultProduct);
                    
                    Debug.Log(2);

                    if (StationData.StationProgressData.CurrentProduct.RecipeName != RecipeName.None ||
                        _defaultProduct                                            == RecipeName.None)
                    {
                        break;
                    }
                
                    Console.WriteLine(e);
                }    
            }
        
            return success;
        }

        public WorkPost_Component GetOperatingArea(int operatingAreaID)
        {
            return AllOperatingAreasInStation.FirstOrDefault(oa => oa.WorkPostData.WorkPostID == operatingAreaID);
        }

        List<WorkPost_Component> _getAllWorkPostsInStation()
        {
            foreach(Transform child in transform)
            {
                if (child.name.Contains("OperatingArea") || child.name.Contains("CollectionPoint"))
                {
                    Destroy(child);
                }
            }

            var operatingAreas = new List<WorkPost_Component>();

            for (uint i = 1; i <= _operatingAreaCount; i++)
            {
                operatingAreas.Add(_createOperatingArea(i));
            }

            CollectionPoint = new GameObject("CollectionPoint").transform;
            CollectionPoint.SetParent(transform);
            CollectionPoint.localPosition = new Vector3(0, 0, -2);

            return operatingAreas;
        }

        protected abstract WorkPost_Component _createOperatingArea(uint operatingAreaID);

        protected abstract void _initialiseStartingInventory();
        public List<Item> GetInventoryItemsToFetch()
        {
            return StationData.InventoryData.GetInventoryItemsToFetch();
        }

        public List<Item> GetInventoryItemsToDeliver(InventoryData inventory)
        {
            return StationData.InventoryData.GetInventoryItemsToDeliver(inventory);
        }

        public void SetInteractRange(float interactRange = 2)
        {
            InteractRange = interactRange;
        }

        public bool WithinInteractRange(Actor_Component interactor)
        {
            return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
        }

        public abstract IEnumerator Interact(Actor_Component actor);

        protected abstract void _craftItem(RecipeName recipeName, Actor_Component actor);

        protected abstract List<Item> _getCost(List<Item> ingredients, Actor_Component actor);

        protected abstract List<Item> _getYield(List<Item> ingredients, Actor_Component actor);

        protected void _onCraftItem(List<Item> craftedItems)
        {
            _currentProductsCrafted.AddRange(craftedItems);
        }

        public List<Item> GetActualProductionRatePerHour()
        {
            var currentProductsCrafted = new List<Item>(_currentProductsCrafted);
            return currentProductsCrafted;
        }

        public List<Item> GetEstimatedProductionRatePerHour()
        {
            float totalProductionRate = 0;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var currentOperatorID in StationData.CurrentOperatorIDs)
            {
                var individualProductionRate = _baseProgressRatePerHour;

                foreach(var vocation in StationData.StationProgressData.CurrentProduct.RequiredVocations)
                {
                    individualProductionRate *= Actor_Manager.GetActor_Data(currentOperatorID).VocationData.GetProgress(vocation);
                }

                totalProductionRate += individualProductionRate;
                // Don't forget to add in estimations for travel time.
            }

            float requiredProgress         = StationData.StationProgressData.CurrentProduct.RequiredProgress;
            var estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

            var estimatedProductionItems = new List<Item>();

            for (var i = 0; i < Mathf.FloorToInt(estimatedProductionCount); i++)
            {
                foreach(var item in StationData.StationProgressData.CurrentProduct.RecipeProducts)
                {
                    estimatedProductionItems.Add(new Item(item));
                }
            }

            return estimatedProductionItems;
        }
    }
}