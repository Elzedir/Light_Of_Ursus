using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Inventory;
using Priorities;
using Recipes;
using Tools;
using UnityEngine;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Crafting : Priority_Class
    {
        public Actor_Data_Crafting(ulong actorID, List<RecipeName> knownRecipes = null) : base(actorID, ComponentType.Actor)
        {
            KnownRecipes = knownRecipes ?? new List<RecipeName>();
        }

        public Actor_Data_Crafting(Actor_Data_Crafting actorDataCrafting) : base(actorDataCrafting.ActorReference.ActorID,
            ComponentType.Actor)
        {
            KnownRecipes = new List<RecipeName>(actorDataCrafting.KnownRecipes);
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Crafting Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return KnownRecipes.ToDictionary(recipe => $"{(ulong)recipe}", recipe => $"{recipe}");
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public List<RecipeName> KnownRecipes;

        public bool AddRecipe(RecipeName recipeName)
        {
            if (KnownRecipes.Contains(recipeName)) return false;

            KnownRecipes.Add(recipeName);

            return true;
        }

        public IEnumerator CraftItemAll(RecipeName recipeName)
        {
            var recipe_Data = Recipe_Manager.GetRecipe_Data(recipeName);

            var actorData = Actor_Manager.GetActor_Data(ActorReference.ActorID);

            while (actorData.InventoryData.InventoryContainsAllItems(recipe_Data.RequiredIngredients))
            {
                yield return CraftItem(recipeName);
            }
        }

        public IEnumerator CraftItem(RecipeName recipeName)
        {
            if (!KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                yield break;
            }

            var recipe_Data = Recipe_Manager.GetRecipe_Data(recipeName);
            var actor_Data = Actor_Manager.GetActor_Data(ActorReference.ActorID);

            if (!actor_Data.InventoryData.InventoryContainsAllItems(recipe_Data.RequiredIngredients))
            {
                Debug.Log("Inventory does not contain all ingredients.");
                yield break;
            }

            if (!actor_Data.InventoryData.HasSpaceForItemList(recipe_Data.RecipeProducts))
            {
                Debug.Log("Inventory does not have space for produced items.");
                yield break;
            }

            actor_Data.InventoryData.RemoveFromInventory(recipe_Data.RequiredIngredients);
            actor_Data.InventoryData.AddToInventory(recipe_Data.RecipeProducts);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Change so that some things will prevent you from crafting, like being in combat.
            return new List<ActorActionName>
            {
                // ActorActionName.Process,
                // ActorActionName.Craft
            };
        }
    }
}