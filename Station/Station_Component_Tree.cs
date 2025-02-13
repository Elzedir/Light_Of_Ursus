using System.Collections;
using System.Collections.Generic;
using Actors;
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

        public override RecipeName       DefaultProduct       => RecipeName.Log;
        public override HashSet<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Log };
        public override HashSet<ulong>       AllowedStoredItemIDs { get; } = new();
        public override HashSet<ulong>       DesiredStoredItemIDs { get; } = new();

        protected override void _initialiseStartingInventory() { }

        public override bool CanCraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.Crafting.KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                return false;
            }

            if (!DefaultAllowedRecipes.Contains(recipeName))
            {
                Debug.Log($"AllowedRecipes does not contain RecipeName: {recipeName}");
                return false;
            }

            var recipeData = Recipe_Manager.GetRecipe_Data(recipeName);
            
            var yield = GetYield(recipeData.RecipeProducts, actor);

            if (!actor.ActorData.InventoryData.HasSpaceForAllItemList(yield))
            {
                Debug.Log($"Inventory does not have space for yield items.");
                return false;
            }
            
            //* Have another system where the tree loses durability instead or something.
            //* Later allow it to partially remove logs to chop the tree down completely.
            
            //* Visually, have the logs accumulate at a collection point next to the tree, like a collection WorkPost or something.
            Station_Data.InventoryData.AddToInventory(yield);

            return true;
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Tree.");
            yield return null;
        }
        
        public override Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong>();

            // Base resource cost on actor relevant skill
        }

        public override Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> products, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong> { { 1100, 1 } }; // For now

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