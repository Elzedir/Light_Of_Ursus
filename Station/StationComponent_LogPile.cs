using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using EmployeePosition;
using Items;
using Jobs;
using OperatingArea;
using Recipes;
using ScriptableObjects;
using UnityEngine;

namespace Station
{
    public class StationComponent_LogPile : StationComponent
    {
        public override StationName      StationName          => StationName.Log_Pile;
        public override StationType      StationType          => StationType.Storage;
        public override EmployeePositionName CoreEmployeePositionName => EmployeePositionName.Hauler;

        public override RecipeName       DefaultProduct       => RecipeName.None; // Fix hauling so that it doesn't need a recipe.
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
    
        public override uint OperatingAreaCount => 4;
        protected override OperatingAreaComponent _createOperatingArea(uint operatingAreaID)
        {
            var operatingAreaComponent = new GameObject($"OperatingArea_{operatingAreaID}").AddComponent<OperatingAreaComponent>();
            operatingAreaComponent.transform.SetParent(transform);
        
            switch(operatingAreaID)
            {
                case 1:
                    operatingAreaComponent.transform.localPosition = new Vector3(0.75f, 0f, 0);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.5f,  1f, 0.5f);
                    break;
                case 2:
                    operatingAreaComponent.transform.localPosition = new Vector3(0,    0f, 0.75f);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.5f, 1f, 0.5f);
                    break;
                case 3:
                    operatingAreaComponent.transform.localPosition = new Vector3(-0.75f, 0f, 0);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.5f,   1f, 0.5f);
                    break;
                case 4:
                    operatingAreaComponent.transform.localPosition = new Vector3(0,    0f, -0.75f);
                    operatingAreaComponent.transform.localScale    = new Vector3(0.5f, 1f, 0.5f);
                    break;
                default:
                    Debug.Log($"OperatingAreaID: {operatingAreaID} greater than OperatingAreaCount: {OperatingAreaCount}.");
                    break;
            }

            var operatingArea = operatingAreaComponent.gameObject.AddComponent<BoxCollider>();
            operatingArea.isTrigger = true;
            operatingAreaComponent.Initialise(new OperatingAreaData(operatingAreaID, StationData.StationID), operatingArea);

            return operatingAreaComponent;
        }

        public override void InitialiseStartingInventory() { }
        
        public override void CraftItem(RecipeName recipeName, Actor_Component actor)
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
