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
    public class Station_Component_Tree : Station_Component
    {
        public override StationName      StationName          => StationName.Tree;
        public override StationType      StationType          => StationType.Resource;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Logger;

        protected override RecipeName       _defaultProduct       => RecipeName.Log;
        public override List<RecipeName> AllowedRecipes       { get; } = new() { RecipeName.Log };
        public override List<uint>       AllowedStoredItemIDs { get; } = new();
        public override List<uint>       DesiredStoredItemIDs { get; } = new();

        public override List<EmployeePositionName> AllowedEmployeePositions { get; } = new()
        {
            EmployeePositionName.Intern
        };

        public override List<JobName> AllowedJobs { get; } = new()
        {
            JobName.Logger,
            JobName.Sawmiller,
            JobName.Hauler,
            JobName.Vendor
        };

        protected override uint _operatingAreaCount => 4;

        protected override WorkPost_Component _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent =
                new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<WorkPost_Component>();
            operatingAreaComponent.transform.SetParent(transform);

            switch (operatingAreaID)
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

        protected override void _initialiseStartingInventory()
        {
            if (StationData.InventoryData.AllInventoryItems.Count == 0)
            {
                StationData.InventoryData.AddToInventory(new List<Item>());
            }
        }

        protected override void _craftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.CraftingData.KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                return;
            }

            if (!AllowedRecipes.Contains(recipeName))
            {
                Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}");
                return;
            }

            Recipe_Data recipeData = Recipe_Manager.GetRecipe_Master(recipeName);

            var cost  = _getCost(recipeData.RequiredIngredients, actor);
            var yield = _getYield(recipeData.RecipeProducts, actor);

            if (!StationData.InventoryData.InventoryContainsAllItems(cost))
            {
                Debug.Log($"Inventory does not contain cost items.");
                return;
            }

            if (!StationData.InventoryData.HasSpaceForItems(yield))
            {
                Debug.Log($"Inventory does not have space for yield items.");
                return;
            }

            StationData.InventoryData.RemoveFromInventory(cost);
            // Have another system where the tree loses durability instead or something.
            // Later allow it to partially remove logs to chop the tree down completely.
            StationData.InventoryData.AddToInventory(yield);

            _onCraftItem(yield);
        }

        protected override List<Item> _getCost(List<Item> ingredients, Actor_Component actor)
        {
            return new List<Item>();

            // Base resource cost on actor relevant skill
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Tree.");
            yield return null;
        }

        protected override List<Item> _getYield(List<Item> products, Actor_Component actor)
        {
            return new List<Item> { new Item(1100, 1) }; // For now

            // Base resource yield on actor relevant skill
        }
    }
}