using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Initialisation;
using Inventory;
using Items;
using Jobs;
using JobSites;
using Recipes;
using UnityEngine;

namespace Station
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Station_Component : MonoBehaviour, IInteractable
    {
        public Station_Data Station_Data;
        
        public ulong              StationID => Station_Data.StationID;
        public JobSite_Component JobSite   => Station_Data.JobSite_Component;
        
        public abstract StationName  StationName { get; }
        public abstract StationType  StationType { get; }

        public float InteractRange { get; private set; }

        //* Remove this, and instead add all allowedJobs to be determined by workPosts, not stations, since there could be different
        //* jobs on different workPosts.
        public abstract JobName           DefaultJobName           { get; }
        public abstract RecipeName        DefaultProduct        { get; }
        public abstract HashSet<RecipeName>  DefaultAllowedRecipes { get; }
        public HashSet<ActorActionName> AllowedActions => Station_Data.AllWorkPosts.Values.SelectMany(workPost => workPost.Job.JobActions).ToHashSet();
        public abstract HashSet<ulong>        AllowedStoredItemIDs  { get; }
        public abstract HashSet<ulong>        DesiredStoredItemIDs  { get; }
        
        BoxCollider        _boxCollider;
        public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();

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
            
            Station_Data = stationData;

            Station_Data.InitialiseStationData();

            SetInteractRange();
            _initialiseStartingInventory();
        }

        public void OnTick()
        {
            Station_Data.OnTick();
        }

        protected abstract void _initialiseStartingInventory();

        public Dictionary<ulong, ulong> GetInventoryItemsToFetch() => 
            Station_Data.InventoryData.GetItemsToFetchFromStation();

        public Dictionary<ulong, Dictionary<ulong, ulong>> GetInventoryItemsToDeliver() =>
            Station_Data.InventoryData.GetItemsToDeliverFromOtherStations();

        public Dictionary<ulong, ulong> GetInventoryItemsToDeliverFromActor(InventoryData actor)
        {
            if (actor != null) return Station_Data.InventoryData.GetItemsToDeliverFromActor(actor);
            
            Debug.LogError("Actor is null.");
            return new Dictionary<ulong, ulong>();
        }
        
        public Dictionary<ulong, ulong> GetInventoryItemsToFetchFromStation() =>
            Station_Data.InventoryData.GetItemsToFetchFromStation();

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

        public abstract Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor);

        public abstract Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> ingredients, Actor_Component actor);
    }
}