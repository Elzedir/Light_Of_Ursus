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
    public class Station_Component_LogPile : Station_Component
    {
        public override StationName      StationName          => StationName.Log_Pile;
        public override StationType      StationType          => StationType.Storage;

        public override RecipeName       DefaultProduct       => RecipeName.No_Recipe; // Fix hauling so that it doesn't need a recipe.
        public override HashSet<RecipeName> DefaultAllowedRecipes       { get; } = new();
        public override HashSet<ulong>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override HashSet<ulong>       DesiredStoredItemIDs { get; } = new() { 1100, 2300 };

        protected override void _initialiseStartingInventory() { }

        public override bool CanCraftItem(RecipeName recipeName, Actor_Component actor)
        {
            Debug.LogError("Log Pile does not craft items.");
            return false;
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Log Pile.");
            yield return null;
        }

        public override Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong>(); // For now

            // Base resource cost on actor relevant skill
        }

        public override Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> products, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong>(); // For now

            // Base resource yield on actor relevant skill
        }

        public override float Produce(WorkPost_Component workPost, float baseProgressRate, Recipe_Data recipe)
        {
            JobSite.JobSite_Data.Haul(workPost.Job);
            
            return 0;
        }
    }
}
