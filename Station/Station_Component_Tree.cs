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
    public class Station_Component_Tree : Station_Component
    {
        public override StationName      StationName          => StationName.Tree;
        public override StationType      StationType          => StationType.Resource;
        
        public override JobName CoreJobName => JobName.Logger;

        public override RecipeName       DefaultProduct       => RecipeName.Log;
        public override List<RecipeName> DefaultAllowedRecipes       { get; } = new() { RecipeName.Log };
        public override List<ulong>       AllowedStoredItemIDs { get; } = new();
        public override List<ulong>       DesiredStoredItemIDs { get; } = new();

        protected override void _initialiseStartingInventory()
        {
            if (Station_Data.InventoryData.AllInventoryItems.Count == 0)
                Station_Data.InventoryData.AddToInventory(new List<Item>());
        }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
        {
            if (!actor.ActorData.Crafting.KnownRecipes.Contains(recipeName))
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

            if (!Station_Data.InventoryData.InventoryContainsAllItems(cost))
            {
                Debug.Log($"Inventory does not contain cost items.");
                return;
            }

            if (!Station_Data.InventoryData.HasSpaceForItems(yield))
            {
                Debug.Log($"Inventory does not have space for yield items.");
                return;
            }

            //Station_Data.InventoryData.RemoveFromInventory(cost);
            Station_Data.InventoryData.RemoveFromInventory(new List<Item>());
            
            // Have another system where the tree loses durability instead or something.
            // Later allow it to partially remove logs to chop the tree down completely.

            foreach (var item in yield)
            {
                Debug.Log($"Adding {item.ItemName} to inventory.");
            }
            
            actor.ActorData.InventoryData.AddToInventory(yield);
            
            foreach (var item in actor.ActorData.InventoryData.AllInventoryItems)
            {
                Debug.Log($"Actor inventory contains {item.Value.ItemName}({item.Key}) - {item.Value.ItemAmount}.");
            }
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