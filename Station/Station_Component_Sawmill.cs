using System.Collections;
using System.Collections.Generic;
using Actor;
using EmployeePosition;
using Items;
using Jobs;
using Recipe;
using UnityEngine;
using WorkPosts;

namespace Station
{
    public class Station_Component_Sawmill : Station_Component
    {
        public override StationName      StationName          => StationName.Sawmill;
        public override StationType      StationType          => StationType.Crafter;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Sawyer;
        public          float            PercentageStorageFilled    = 0;
        public          float            PercentageStorageThreshold = 50; // The percent at which you should transfer products to storage.

        protected override RecipeName       _defaultProduct       => RecipeName.Plank;
        public override List<RecipeName> AllowedRecipes       { get; } = new() { RecipeName.Plank };
        public override List<uint>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<uint>       DesiredStoredItemIDs { get; } = new() { 1100 };
        public override List<EmployeePositionName> AllowedEmployeePositions { get; } = new()
        {
            EmployeePositionName.Intern
        };
        public override List<JobName> AllowedJobs { get; } = new()
        {
            JobName.Sawmiller,
            JobName.Hauler
        };

        protected override uint _operatingAreaCount => 4;

        protected override WorkPost_Component _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<WorkPost_Component>();
            operatingAreaComponent.transform.SetParent(transform);
        
            switch(operatingAreaID)
            {
                
                default:
                    Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {_operatingAreaCount}.");
                    break;
            }

            var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
            operatingArea.isTrigger = true;
            operatingAreaComponent.Initialise(new WorkPost_Data(operatingAreaID, StationData.StationID), operatingArea);

            return operatingAreaComponent;
        }

        protected override void _initialiseStartingInventory() { }

        public override IEnumerator Interact(Actor_Component actor)
        {
            yield break;
            // Open inventory
        }

        protected override void _craftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
            if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

            var recipeMaster = Recipe_Manager.GetRecipe_Master(recipeName);

            var cost  = _getCost(recipeMaster.RequiredIngredients, actor);
            var yield = _getYield(recipeMaster.RecipeProducts, actor);
        
            if (!StationData.InventoryData.InventoryContainsAllItems(cost)) { Debug.Log("Station does not have required items."); return; }
            if (!StationData.InventoryData.HasSpaceForItems(yield)) { Debug.Log("Station does not have space for yield items."); return; }

            StationData.InventoryData.RemoveFromInventory(cost);
            StationData.InventoryData.AddToInventory(yield);

            _onCraftItem(yield);
        }

        protected override List<Item> _getCost(List<Item> ingredients, Actor_Component actor)
        {
            return ingredients;

            // Base resource cost on actor relevant skill
        }

        protected override List<Item> _getYield(List<Item> products, Actor_Component actor)
        {
            return products; // For now

            // Base resource yield on actor relevant skill
        }
    }
}
