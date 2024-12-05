using System.Collections;
using System.Collections.Generic;
using Actor;
using EmployeePosition;
using Items;
using Jobs;
using OperatingArea;
using Recipes;
using ScriptableObjects;
using UnityEngine;

namespace Station
{
    public class StationComponent_Tree : Station_Component
    {
        public override StationName      StationName          => StationName.Tree;
        public override StationType      StationType          => StationType.Resource;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Logger;

        public override RecipeName       DefaultProduct       => RecipeName.Log;
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

        public override uint OperatingAreaCount => 4;

        protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent =
                new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
            operatingAreaComponent.transform.SetParent(transform);

            switch (operatingAreaID)
            {
                case 1:
                    operatingAreaComponent.transform.localPosition = new Vector3(1.5f, -0.8f,  0);
                    operatingAreaComponent.transform.localScale    = new Vector3(1,    0.333f, 1);
                    break;
                case 2:
                    operatingAreaComponent.transform.localPosition = new Vector3(0, -0.8f,  1.5f);
                    operatingAreaComponent.transform.localScale    = new Vector3(1, 0.333f, 1);
                    break;
                case 3:
                    operatingAreaComponent.transform.localPosition = new Vector3(-1.5f, -0.8f,  0);
                    operatingAreaComponent.transform.localScale    = new Vector3(1,     0.333f, 1);
                    break;
                case 4:
                    operatingAreaComponent.transform.localPosition = new Vector3(0, -0.8f,  -1.5f);
                    operatingAreaComponent.transform.localScale    = new Vector3(1, 0.333f, 1);
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

        public override void InitialiseStartingInventory()
        {
            if (StationData.InventoryData.AllInventoryItems.Count == 0)
            {
                StationData.InventoryData.AddToInventory(new List<Item>());
            }
        }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
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

            Recipe_Master recipeMaster = Manager_Recipe.GetRecipe_Master(recipeName);

            var cost  = _getCost(recipeMaster.RequiredIngredients, actor);
            var yield = _getYield(recipeMaster.RecipeProducts, actor);

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