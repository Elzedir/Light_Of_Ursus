using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Items;
using Jobs;
using Recipes;
using UnityEngine;

namespace Station
{
    public class Station_Component_Sawmill : Station_Component
    {
        public override StationName      StationName          => StationName.Sawmill;
        public override StationType      StationType          => StationType.Crafter;
        
        public override JobName DefaultJobName => JobName.Sawyer;
        public          float            PercentageStorageFilled    = 0;
        //= The percent at which you should transfer products to storage.
        public          float            PercentageStorageThreshold = 50; 

        public override RecipeName       DefaultProduct       => RecipeName.Plank;
        public override List<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Plank };
        public override List<ulong>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<ulong>       DesiredStoredItemIDs { get; } = new() { 1100 };

        protected override void _initialiseStartingInventory() { }

        public override IEnumerator Interact(Actor_Component actor)
        {
            yield break;
            // Open inventory
        }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.Crafting.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return; }
            if (!DefaultAllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return; }

            var recipeMaster = Recipe_Manager.GetRecipe_Master(recipeName);

            var cost  = GetCost(recipeMaster.RequiredIngredients, actor);
            var yield = GetYield(recipeMaster.RecipeProducts, actor);
        
            if (!Station_Data.InventoryData.InventoryContainsAllItems(cost)) { Debug.Log("Station does not have required items."); return; }
            if (!Station_Data.InventoryData.HasSpaceForItems(yield)) { Debug.Log("Station does not have space for yield items."); return; }

            Station_Data.InventoryData.RemoveFromInventory(cost);
            Station_Data.InventoryData.AddToInventory(yield);
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
