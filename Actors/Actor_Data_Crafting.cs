using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Inventory;
using Items;
using Priority;
using Recipes;
using Tools;
using UnityEngine;

namespace Actor
{
    [Serializable]
    public class Actor_Data_Crafting : Priority_Updater
    {
        public Actor_Data_Crafting(uint actorID, List<RecipeName> knownRecipes = null) : base(actorID, ComponentType.Actor)
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
            return KnownRecipes.ToDictionary(recipe => $"{(uint)recipe}", recipe => $"{recipe}");
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
            var recipe = Recipe_Manager.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActor_Data(ActorReference.ActorID);

            while (_inventoryContainsAllIngredients(actorData, recipe.RequiredIngredients))
            {
                yield return CraftItem(recipeName);
            }
        }

        bool _inventoryContainsAllIngredients(Actor_Data actorData, List<Item> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                var inventoryItem = actorData.InventoryData.GetItemFromInventory(ingredient.ItemID);

                if (inventoryItem == null || inventoryItem.ItemAmount < ingredient.ItemAmount)
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerator CraftItem(RecipeName recipeName)
        {
            if (!KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                yield break;
            }

            Recipe_Data recipeData = Recipe_Manager.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActor_Data(ActorReference.ActorID);

            if (!_inventoryContainsAllIngredients(actorData, recipeData.RequiredIngredients))
            {
                Debug.Log("Inventory does not contain all ingredients.");
                yield break;
            }

            if (!actorData.InventoryData.HasSpaceForItems(recipeData.RecipeProducts))
            {
                Debug.Log("Inventory does not have space for produced items.");
                yield break;
            }

            actorData.InventoryData.RemoveFromInventory(recipeData.RequiredIngredients);
            actorData.InventoryData.AddToInventory(recipeData.RecipeProducts);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Change so that some things will prevent you from crafting, like being in combat.
            return new List<ActorActionName>
            {
                ActorActionName.Process,
                ActorActionName.Craft
            };
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }
}