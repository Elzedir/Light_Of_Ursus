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
using Recipes;
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
        public uint             StationID => Station_Data.StationID;
        public JobSite_Component JobSite   => Station_Data.JobSite_Component;

        public Station_Data Station_Data;
        public abstract                              StationName  StationName             { get; }
        public abstract                              StationType  StationType             { get; }

        //Maybe change this so that it checks if there is a worker in the station rather than just assigned to it,
        // since the worker could be away from the post.
        public bool IsStationBeingOperated =>
            Station_Data.AllWorkPost_Data.Values.Any(workPost => workPost.CurrentWorkerID != 0);

        public float InteractRange { get; private set; }
    
        public abstract         EmployeePositionName       CoreEmployeePositionName { get; }
        protected abstract      RecipeName                 _defaultProduct          { get; }
        public abstract         List<RecipeName>           DefaultAllowedRecipes           { get; }
        public abstract         List<uint>                 AllowedStoredItemIDs     { get; }
        public abstract         List<uint>                 DesiredStoredItemIDs     { get; }
        public         abstract List<JobName>              AllowedJobs              { get; }

        const    float      _baseProgressRatePerHour = 5;
        readonly List<Item> _currentProductsCrafted  = new();
    
        PriorityComponent_Station        _priorityComponent;
        public PriorityComponent_Station PriorityComponent => _priorityComponent ??= new PriorityComponent_Station(); 

        BoxCollider        _boxCollider;
        public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();
        
        public void SetStationData(Station_Data stationData)
        {
            Station_Data = stationData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseStations += _initialise;
        }

        void _initialise()
        {
            Station_Data.InitialiseStationData();

            _populateWorkPlace_Components();

            SetInteractRange();
            _initialiseStartingInventory();
        }
    
        public void RemoveAllWorkersFromStation()
        {
            foreach (var workPost in Station_Data.AllWorkPost_Components.Values)
            {
                workPost.WorkPostData.RemoveCurrentWorkerFromWorkPost();
            }
        }

        public void RemoveWorkerFromWorkPost(uint workerID)
        {
            if (Station_Data.AllWorkPost_Data.Values.Any(area => area.CurrentWorkerID == workerID))
            {
                Station_Data.AllWorkPost_Data.Values.FirstOrDefault(area => area.CurrentWorkerID == workerID)?.RemoveCurrentWorkerFromWorkPost();
                return;
            }

            Debug.Log($"Operator {workerID} not found in operating areas.");
        }

        protected abstract void _initialiseStartingInventory();
        public List<Item> GetInventoryItemsToFetch()
        {
            return Station_Data.InventoryData.GetInventoryItemsToFetch();
        }

        public List<Item> GetInventoryItemsToDeliver(InventoryData inventory)
        {
            return Station_Data.InventoryData.GetInventoryItemsToDeliver(inventory);
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

        public abstract void CraftItem(RecipeName recipeName, Actor_Component actor);

        public abstract List<Item> GetCost(List<Item> ingredients, Actor_Component actor);

        public abstract List<Item> GetYield(List<Item> ingredients, Actor_Component actor);

        protected void _onCraftItem(List<Item> craftedItems)
        {
            _currentProductsCrafted.AddRange(craftedItems);
        }

        public List<Item> GetActualProductionRatePerHour()
        {
            var currentProductsCrafted = new List<Item>(_currentProductsCrafted);
            return currentProductsCrafted;
        }
    }
}