using System.Collections;
using System.Collections.Generic;
using Actor;
using EmployeePosition;
using Initialisation;
using Inventory;
using Items;
using Jobs;
using JobSite;
using Priority;
using Recipes;
using UnityEngine;

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

        public float InteractRange { get; private set; }
    
        public abstract         EmployeePositionName       CoreEmployeePositionName { get; }
        public abstract      RecipeName                 DefaultProduct          { get; }
        public abstract         List<RecipeName>           DefaultAllowedRecipes           { get; }
        public abstract         List<uint>                 AllowedStoredItemIDs     { get; }
        public abstract         List<uint>                 DesiredStoredItemIDs     { get; }
        public         abstract List<JobName>              AllowedJobs              { get; }
    
        Priority_Data_Station        _priorityData;
        public Priority_Data_Station PriorityData => _priorityData ??= new Priority_Data_Station(); 

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

            SetInteractRange();
            _initialiseStartingInventory();
        }

        protected abstract void _initialiseStartingInventory();
        public List<Item> GetInventoryItemsToFetch()
        {
            return Station_Data.InventoryUpdater.GetInventoryItemsToFetch();
        }

        public List<Item> GetInventoryItemsToDeliver(InventoryUpdater inventory)
        {
            return Station_Data.InventoryUpdater.GetInventoryItemsToDeliver(inventory);
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
    }
}