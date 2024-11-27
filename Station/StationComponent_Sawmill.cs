using System.Collections;
using System.Collections.Generic;
using Actors;
using Items;
using Jobs;
using OperatingArea;
using Recipes;
using UnityEngine;

namespace Station
{
    public class StationComponent_Sawmill : StationComponent
    {
        public override StationName      StationName          => StationName.Sawmill;
        public override StationType      StationType          => StationType.Crafter;
        public override EmployeePosition CoreEmployeePosition => EmployeePosition.Sawyer;
        public          float            PercentageStorageFilled    = 0;
        public          float            PercentageStorageThreshold = 50; // The percent at which you should transfer products to storage.

        public override RecipeName       DefaultProduct       => RecipeName.Plank;
        public override List<RecipeName> AllowedRecipes       { get; } = new() { RecipeName.Plank };
        public override List<uint>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<uint>       DesiredStoredItemIDs { get; } = new() { 1100 };
        public override List<EmployeePosition> AllowedEmployeePositions { get; } = new()
        {
            EmployeePosition.None
        };
        public override List<JobName> AllowedJobs { get; } = new()
        {
            JobName.Sawmiller,
            JobName.Hauler
        };
        public override uint OperatingAreaCount => 4;

        protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
            operatingAreaComponent.transform.SetParent(transform);
        
            switch(operatingAreaID)
            {
                case 1:
                    operatingAreaComponent.transform.localPosition = new Vector3(0.75f,  0f, 0);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.333f, 1f, 1);
                    break;
                case 2:
                    operatingAreaComponent.transform.localPosition = new Vector3(0,      0f, 1f);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.333f, 1f, 1);
                    break;
                case 3:
                    operatingAreaComponent.transform.localPosition = new Vector3(-0.75f, 0f, 0);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.333f, 1f, 1);
                    break;
                case 4:
                    operatingAreaComponent.transform.localPosition = new Vector3(0,      0f, -1f);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.333f, 1f, 1);
                    break;
                default:
                    Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {OperatingAreaCount}.");
                    break;
            }

            var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
            operatingArea.isTrigger = true;
            operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

            return operatingAreaComponent;
        }

        public override void InitialiseStartingInventory() { }

        public override IEnumerator Interact(ActorComponent actor)
        {
            yield break;
            // Open inventory
        }

        public override void CraftItem(RecipeName recipeName, ActorComponent actor)
        {
            if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
            if (!AllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

            var recipeMaster = Manager_Recipe.GetRecipe_Master(recipeName);

            var cost  = _getCost(recipeMaster.RequiredIngredients, actor);
            var yield = _getYield(recipeMaster.RecipeProducts, actor);
        
            if (!StationData.InventoryData.InventoryContainsAllItems(cost)) { Debug.Log("Station does not have required items."); return; }
            if (!StationData.InventoryData.HasSpaceForItems(yield)) { Debug.Log("Station does not have space for yield items."); return; }

            StationData.InventoryData.RemoveFromInventory(cost);
            StationData.InventoryData.AddToInventory(yield);

            _onCraftItem(yield);
        }

        protected override List<Item> _getCost(List<Item> ingredients, ActorComponent actor)
        {
            return ingredients;

            // Base resource cost on actor relevant skill
        }

        protected override List<Item> _getYield(List<Item> products, ActorComponent actor)
        {
            return products; // For now

            // Base resource yield on actor relevant skill
        }
    }
}
