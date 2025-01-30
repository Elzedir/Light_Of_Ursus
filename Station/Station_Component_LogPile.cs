using System.Collections;
using System.Collections.Generic;
using Actor;
using ActorActions;
using Items;
using Jobs;
using Recipes;
using UnityEngine;
using WorkPosts;

namespace Station
{
    public class Station_Component_LogPile : Station_Component
    {
        public override StationName      StationName          => StationName.Log_Pile;
        public override StationType      StationType          => StationType.Storage;
        public override JobName CoreJobName => JobName.Sawyer;

        public override RecipeName       DefaultProduct       => RecipeName.None; // Fix hauling so that it doesn't need a recipe.
        public override List<RecipeName> DefaultAllowedRecipes       { get; } = new();
        public override List<ulong>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<ulong>       DesiredStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<ActorActionName> AllowedJobTasks { get; } = new()
        {
            ActorActionName.Haul_Fetch,
            ActorActionName.Haul_Deliver
        };

        protected override void _initialiseStartingInventory() { }

        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
        {
            Debug.LogError("Log Pile does not craft items.");
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Log Pile.");
            yield return null;
        }

        public override List<Item> GetCost(List<Item> ingredients, Actor_Component actor)
        {
            return new List<Item>(); // For now

            // Base resource cost on actor relevant skill
        }

        public override List<Item> GetYield(List<Item> products, Actor_Component actor)
        {
            return new List<Item>(); // For now

            // Base resource yield on actor relevant skill
        }
    }
}
