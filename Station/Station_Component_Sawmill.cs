using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
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
        public          float            PercentageStorageFilled    = 0;
        //= The percent at which you should transfer products to storage.
        public          float            PercentageStorageThreshold = 50; 

        public override RecipeName       DefaultProduct       => RecipeName.Plank;
        public override HashSet<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Plank };
        public override HashSet<ulong>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override HashSet<ulong>       DesiredStoredItemIDs { get; } = new() { 1100 };

        protected override void _initialiseStartingInventory() { }

        public override IEnumerator Interact(Actor_Component actor)
        {
            yield break;
            // Open inventory
        }

        public override bool CanCraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.Crafting.KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); return false; }
            if (!DefaultAllowedRecipes.Contains(recipeName)) { Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}"); return false; }

            var recipeMaster = Recipe_Manager.GetRecipe_Data(recipeName);

            var cost  = GetCost(recipeMaster.RequiredIngredients, actor);
            var yield = GetYield(recipeMaster.RecipeProducts, actor);
        
            if (!Station_Data.InventoryData.InventoryContainsAllItems(cost)) { Debug.Log("Station does not have required items."); return false; }
            if (!Station_Data.InventoryData.HasSpaceForAllItemList(yield)) { Debug.Log("Station does not have space for yield items."); return false; }

            Station_Data.InventoryData.RemoveFromInventory(cost);
            Station_Data.InventoryData.AddToInventory(yield);

            return true;
        }

        public override Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor)
        {
            return ingredients;

            // Base resource cost on actor relevant skill
        }

        public override Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> products, Actor_Component actor)
        {
            return products; // For now

            // Base resource yield on actor relevant skill
        }
        
        protected override float _produce(WorkPost_Component workPost, float baseProgressRate, Recipe_Data recipe)
        {
            if (_isAtWorkPost(workPost)) return 0;
            
            var productionRate = baseProgressRate;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            if (recipe.RecipeName is RecipeName.None) return productionRate;

            foreach (var vocation in recipe.RequiredVocations)
            {
                productionRate *= workPost.CurrentWorker.ActorData.Vocation.GetProgress(vocation.Value);
            }

            return productionRate;
        }
    }
}
