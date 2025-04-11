using System.Collections;
using System.Collections.Generic;
using Actors;
using Inventory;
using Jobs;
using Recipes;
using UnityEngine;

namespace Station
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Station_Component : MonoBehaviour, IInteractable
    {
        [SerializeField] Station_Data _station_Data;
        
        public ulong              ID;

        public float InteractRange { get; private set; }

        //* Remove this, and instead add all allowedJobs to be determined by workPosts, not stations, since there could be different
        //* jobs on different workPosts.
        public abstract RecipeName        DefaultProduct        { get; }
        public abstract HashSet<RecipeName>  DefaultAllowedRecipes { get; }
        public abstract HashSet<ulong>        AllowedStoredItemIDs  { get; }
        public abstract HashSet<ulong>        DesiredStoredItemIDs  { get; }
        
        BoxCollider        _boxCollider;
        
        public Station_Data Station_Data => _station_Data ??= Station_Manager.GetStation_DataFromName(this);
        public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();
        
        public void Initialise()
        {
            if (Station_Data is null)
            {
                Debug.LogWarning($"Station with name {name} not found in Station_SO.");
                return;
            }

            Station_Data.InitialiseStationData(ID);

            SetInteractRange();
        }

        public Dictionary<ulong, ulong> GetItemsToFetchFromThisStation() => 
            _station_Data.InventoryData.GetItemsToFetchFromThisInventory();

        public Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToThisStationFromAllStations() =>
            _station_Data.InventoryData.GetItemsToDeliverToThisInventoryFromAllStations();

        public Dictionary<ulong, ulong> GetItemsToDeliverToThisStation(InventoryData otherInventory)
        {
            return otherInventory != null
                ? _station_Data.InventoryData.GetItemsToDeliverToThisInventory(otherInventory)
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
            var haulers = new List<Job_Data>();
            
            foreach (var job_Data in _station_Data.Jobs.Values)
            {
                if (job_Data.Actor_Data == null) continue;

                if (job_Data.Station.Station_Data.StationType == StationType.Storage)
                {
                    haulers.Add(job_Data);
                    continue;
                }
                
                var progressMade = _produce(job_Data, _station_Data.BaseProgressRatePerHour,
                    _station_Data.StationProgressData.CurrentProduct);

                if (!_station_Data.StationProgressData.ItemCrafted(progressMade)) continue;
                
                if (CanCraftItem(
                        _station_Data.StationProgressData.CurrentProduct.RecipeName, job_Data.Actor_Data))
                    _station_Data.StationProgressData.ResetProgress();
            }
            
            Station_Data.Building.Building_Data.Haul(haulers);
        }

        protected bool _isAtWorkPost(Job_Component job)
        {
            if (job.JobCollider.bounds.Contains(job.Job_Data.Actor_Data.Actor.transform.position)) return true;
            
            if (!job.Job_Data.IsWorkerMovingToJob)
                StartCoroutine(job.MoveWorkerToJob(job.Job_Data.Actor_Data.Actor, job.transform.position));

            return false;
        }
        protected abstract float _produce(Job_Data job, float baseProgressRate, Recipe_Data recipe);

        public abstract IEnumerator Interact(Actor_Component actor);

        public abstract bool CanCraftItem(RecipeName recipeName, Actor_Data actor);

        public abstract Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor);

        public abstract Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> ingredients, Actor_Component actor);
    }
}