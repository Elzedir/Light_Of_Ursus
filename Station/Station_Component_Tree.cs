using System.Collections;
using System.Collections.Generic;
using Actor;
using EmployeePosition;
using Items;
using Jobs;
using Recipes;
using UnityEngine;
using WorkPosts;

namespace Station
{
    public class Station_Component_Tree : Station_Component
    {
        public override StationName      StationName          => StationName.Tree;
        public override StationType      StationType          => StationType.Resource;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Logger;

        public override RecipeName       DefaultProduct       => RecipeName.Log;
        public override List<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Log };
        public override List<uint>       AllowedStoredItemIDs { get; } = new();
        public override List<uint>       DesiredStoredItemIDs { get; } = new();

        public override List<JobName> AllowedJobs { get; } = new()
        {
            JobName.Logger,
            JobName.Sawmiller,
            JobName.Hauler,
            JobName.Vendor
        };

        protected override void _initialiseStartingInventory()
        {
            if (Station_Data.InventoryUpdater.AllInventoryItems.Count == 0)
            {
                Station_Data.InventoryUpdater.AddToInventory(new List<Item>());
            }
        }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.CraftingUpdater.KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                return;
            }

            if (!DefaultAllowedRecipes.Contains(recipeName))
            {
                Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}");
                return;
            }

            var recipeData = Recipe_Manager.GetRecipe_Master(recipeName);

            var cost  = GetCost(recipeData.RequiredIngredients, actor);
            var yield = GetYield(recipeData.RecipeProducts, actor);

            if (!Station_Data.InventoryUpdater.InventoryContainsAllItems(cost))
            {
                Debug.Log($"Inventory does not contain cost items.");
                return;
            }

            if (!Station_Data.InventoryUpdater.HasSpaceForItems(yield))
            {
                Debug.Log($"Inventory does not have space for yield items.");
                return;
            }

            Station_Data.InventoryUpdater.RemoveFromInventory(cost);
            // Have another system where the tree loses durability instead or something.
            // Later allow it to partially remove logs to chop the tree down completely.
            Station_Data.InventoryUpdater.AddToInventory(yield);
        }

        public override List<Item> GetCost(List<Item> ingredients, Actor_Component actor)
        {
            return new List<Item>();

            // Base resource cost on actor relevant skill
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Tree.");
            yield return null;
        }

        public override List<Item> GetYield(List<Item> products, Actor_Component actor)
        {
            return new List<Item> { new Item(1100, 1) }; // For now

            // Base resource yield on actor relevant skill
        }
    }
}