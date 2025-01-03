using System.Collections;
using System.Collections.Generic;
using Actor;
using Items;
using Jobs;
using Recipes;
using UnityEngine;
using WorkPosts;

namespace Station
{
    public class Station_Component_Sawmill : Station_Component
    {
        public override StationName      StationName          => StationName.Sawmill;
        public override StationType      StationType          => StationType.Crafter;
        public override JobName CoreJobName => JobName.Sawyer;
        public          float            PercentageStorageFilled    = 0;
        public          float            PercentageStorageThreshold = 50; // The percent at which you should transfer products to storage.

        public override RecipeName       DefaultProduct       => RecipeName.Plank;
        public override List<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Plank };
        public override List<uint>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<uint>       DesiredStoredItemIDs { get; } = new() { 1100 };
        public override List<JobTaskName> AllowedJobTasks { get; } = new()
        {
            JobTaskName.Process_Logs,
            JobTaskName.Fetch_Items,
            JobTaskName.Deliver_Items
        };

        protected override void _initialiseStartingInventory() { }

        public override IEnumerator Interact(Actor_Component actor)
        {
            yield break;
            // Open inventory
        }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.CraftingDataPreset.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
            if (!DefaultAllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

            var recipeMaster = Recipe_Manager.GetRecipe_Master(recipeName);

            var cost  = GetCost(recipeMaster.RequiredIngredients, actor);
            var yield = GetYield(recipeMaster.RecipeProducts, actor);
        
            if (!Station_Data.InventoryDataPreset.InventoryContainsAllItems(cost)) { Debug.Log("Station does not have required items."); return; }
            if (!Station_Data.InventoryDataPreset.HasSpaceForItems(yield)) { Debug.Log("Station does not have space for yield items."); return; }

            Station_Data.InventoryDataPreset.RemoveFromInventory(cost);
            Station_Data.InventoryDataPreset.AddToInventory(yield);
        }

        public override List<Item> GetCost(List<Item> ingredients, Actor_Component actor)
        {
            return ingredients;

            // Base resource cost on actor relevant skill
        }

        public override List<Item> GetYield(List<Item> products, Actor_Component actor)
        {
            return products; // For now

            // Base resource yield on actor relevant skill
        }
    }
}
