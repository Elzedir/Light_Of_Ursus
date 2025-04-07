using System.Collections;
using System.Collections.Generic;
using Actors;
using Buildings;
using Initialisation;
using Inventory;
using Jobs;
using Recipes;
using UnityEngine;
using WorkPosts;

namespace Station
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Station_Component : MonoBehaviour, IInteractable
    {
        public Station_Data Station_Data;
        
        public ulong              StationID => Station_Data.StationID;
        public Building_Component Building   => Station_Data.Building;
        
        public abstract StationName  StationName { get; }
        public abstract StationType  StationType { get; }

        public float InteractRange { get; private set; }

        //* Remove this, and instead add all allowedJobs to be determined by workPosts, not stations, since there could be different
        //* jobs on different workPosts.
        public abstract RecipeName        DefaultProduct        { get; }
        public abstract HashSet<RecipeName>  DefaultAllowedRecipes { get; }
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

        protected abstract void _initialiseStartingInventory();

        public Dictionary<ulong, ulong> GetItemsToFetchFromThisStation() => 
            Station_Data.InventoryData.GetItemsToFetchFromThisInventory();

        public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToThisStationFromAllStations() =>
            Station_Data.InventoryData.GetItemsToDeliverToThisInventoryFromAllStations();

        public Dictionary<ulong, ulong> GetItemsToDeliverToThisStation(InventoryData otherInventory)
        {
            return otherInventory != null
                ? Station_Data.InventoryData.GetItemsToDeliverToThisInventory(otherInventory)
                : new Dictionary<ulong, ulong>();
        }
        public void SetInteractRange(float interactRange = 2)
        {
            InteractRange = interactRange;
        }

        public bool WithinInteractRange(Actor_Component interactor)
        {
            return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
        }

        public void Operate()
        {
            var haulers = new List<Job>();
            
            foreach (var workPost in Station_Data.AllWorkPosts.Values)
            {
                if (workPost.Job.ActorID == 0) continue;

                if (workPost.Job.Station.StationType == StationType.Storage)
                {
                    haulers.Add(workPost.Job);
                    continue;
                }
                
                var progressMade = _produce(workPost, Station_Data.BaseProgressRatePerHour,
                    Station_Data.StationProgressData.CurrentProduct);

                if (!Station_Data.StationProgressData.ItemCrafted(progressMade)) continue;
                
                if (CanCraftItem(
                        Station_Data.StationProgressData.CurrentProduct.RecipeName, workPost.Job.Actor))
                    Station_Data.StationProgressData.ResetProgress();
            }
            
            Building.Building_Data.Haul(haulers);
        }

        protected bool _isAtWorkPost(WorkPost_Component workPost)
        {
            if (workPost.WorkPostCollider.bounds.Contains(workPost.Job.Actor.transform.position)) return true;
            
            if (!workPost.Job.IsWorkerMovingToWorkPost)
                StartCoroutine(workPost.MoveWorkerToWorkPost(workPost.CurrentWorker, workPost.transform.position));

            return false;
        }
        protected abstract float _produce(WorkPost_Component workPost, float baseProgressRate, Recipe_Data recipe);

        public abstract IEnumerator Interact(Actor_Component actor);

        public abstract bool CanCraftItem(RecipeName recipeName, Actor_Component actor);

        public abstract Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor);

        public abstract Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> ingredients, Actor_Component actor);
    }
}