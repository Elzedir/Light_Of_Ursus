using System.Collections;
using System.Collections.Generic;
using Actors;
using Recipes;
using UnityEngine;
using WorkPosts;

namespace Station
{
    public class Station_Component_IdleTemp : Station_Component
    {
        public override StationName      StationName          => StationName.IdleTemp;
        public override StationType      StationType          => StationType.Recreation;

        public override RecipeName       DefaultProduct       => RecipeName.No_Recipe;
        public override HashSet<RecipeName> DefaultAllowedRecipes       { get; } = new();
        public override HashSet<ulong>       AllowedStoredItemIDs { get; } = new();
        public override HashSet<ulong>       DesiredStoredItemIDs { get; } = new();

        protected override void _initialiseStartingInventory() { }

        public override bool CanCraftItem(RecipeName recipeName, Actor_Component actor)
        {
            Debug.LogError("Idle Temp does not craft items.");
            return false;
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Idle Temp.");
            yield return null;
        }

        public override Dictionary<ulong, ulong> GetCost(Dictionary<ulong, ulong> ingredients, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong>();
        }

        public override Dictionary<ulong, ulong> GetYield(Dictionary<ulong, ulong> products, Actor_Component actor)
        {
            return new Dictionary<ulong, ulong>();
        }

        protected override float _produce(WorkPost_Component workPost, float baseProgressRate, Recipe_Data recipe)
        {
            return _isAtWorkPost(workPost) ? 0 : 0;
        }
    }
}