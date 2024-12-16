using System.Collections;
using System.Collections.Generic;
using Actor;
using EmployeePosition;
using Items;
using Jobs;
using Recipe;
using UnityEngine;

namespace Station
{
    public class Station_Component_LogPile : Station_Component
    {
        public override StationName      StationName          => StationName.Log_Pile;
        public override StationType      StationType          => StationType.Storage;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Hauler;

        protected override RecipeName       _defaultProduct       => RecipeName.None; // Fix hauling so that it doesn't need a recipe.
        public override List<RecipeName> AllowedRecipes       { get; } = new();
        public override List<uint>       AllowedStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<uint>       DesiredStoredItemIDs { get; } = new() { 1100, 2300 };
        public override List<EmployeePositionName> AllowedEmployeePositions { get; } = new()
        {
            EmployeePositionName.Intern
        };
        public override List<JobName> AllowedJobs { get; } = new()
        {
            JobName.Logger,
            JobName.Hauler
        };

        protected override uint _operatingAreaCount => 4;
        protected override WorkPost_Component _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<WorkPost_Component>();
            operatingAreaComponent.transform.SetParent(transform);
        
            switch(operatingAreaID)
            {
                
                default:
                    Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {_operatingAreaCount}.");
                    break;
            }

            var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
            operatingArea.isTrigger = true;
            operatingAreaComponent.Initialise(new WorkPost_Data(operatingAreaID, StationData.StationID), operatingArea);

            return operatingAreaComponent;
        }

        protected override void _initialiseStartingInventory() { }

        protected override void _craftItem(RecipeName recipeName, Actor_Component actor)
        {
            Debug.LogError("Log Pile does not craft items.");
        }

        public override IEnumerator Interact(Actor_Component actor)
        {
            Debug.LogError("No Interact method implemented for Log Pile.");
            yield return null;
        }

        protected override List<Item> _getCost(List<Item> ingredients, Actor_Component actor)
        {
            return new List<Item>(); // For now

            // Base resource cost on actor relevant skill
        }

        protected override List<Item> _getYield(List<Item> products, Actor_Component actor)
        {
            return new List<Item>(); // For now

            // Base resource yield on actor relevant skill
        }
    }
}
