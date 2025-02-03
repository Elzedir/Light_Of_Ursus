using System.Collections;
using System.Collections.Generic;
using Actor;
using ActorActions;
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
        public ulong              StationID => Station_Data.StationID;
        public JobSite_Component JobSite   => Station_Data.JobSite_Component;

        public Station_Data Station_Data;
        public abstract StationName  StationName { get; }
        public abstract StationType  StationType { get; }
        List<Job> _defaultStationJobs;
        public List<Job> DefaultStationJobs => _defaultStationJobs ??= _getDefaultStationJobs();
        
        protected abstract List<Job> _getDefaultStationJobs();

        public float InteractRange { get; private set; }

        public abstract JobName           CoreJobName           { get; }
        public abstract RecipeName        DefaultProduct        { get; }
        public abstract List<RecipeName>  DefaultAllowedRecipes { get; }
        public abstract List<ulong>        AllowedStoredItemIDs  { get; }
        public abstract List<ulong>        DesiredStoredItemIDs  { get; }

        Priority_Data_Station        _priorityData;
        public Priority_Data_Station PriorityData => _priorityData ??= new Priority_Data_Station();

        BoxCollider        _boxCollider;
        public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();

        // public void Update()
        // {
        //     Debug.Log($"Station: {StationName} JobSiteID: {Station_Data.JobSiteID}");
        // }

        public void SetStationData(Station_Data stationData) => Station_Data = stationData;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseStations += _initialise;
        }

        void _initialise()
        {
            var stationData = Station_Manager.GetStation_DataFromName(this);
            
            if (stationData is null)
            {
                Debug.LogWarning($"Station with name {name} not found in Station_SO.");
                return;
            }
            
            SetStationData(stationData);

            Station_Data.InitialiseStationData();

            SetInteractRange();
            _initialiseStartingInventory();
        }

        protected abstract void _initialiseStartingInventory();

        public List<Item> GetInventoryItems(ActorActionName actorAction)
        {
            return actorAction switch
            {
                ActorActionName.Haul_Fetch => Station_Data.InventoryData.GetInventoryItemsToFetchFromStation(),
                ActorActionName.Haul_Deliver => Station_Data.InventoryData.GetInventoryItemsToDeliverFromOtherStations(),
                ActorActionName.Chop_Wood => Station_Data.InventoryData.GetInventoryItemsToFetchFromStation(),
                _ => new List<Item>()
            };
        }

        public List<Item> GetInventoryItemsToDeliverFromInventory(InventoryData inventory)
        {
            if (inventory != null) return Station_Data.InventoryData.GetInventoryItemsToDeliverFromInventory(inventory);
            
            Debug.LogError("Inventory is null.");
            return new List<Item>();
        }
        
        public List<Item> GetInventoryItemsToFetchFromStation()
        {
            return Station_Data.InventoryData.GetInventoryItemsToFetchFromStation();
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