using System;
using System.Collections.Generic;
using Actor;
using Career;
using Recipe;
using UnityEditor;
using UnityEngine;

namespace EmployeePosition
{
    public abstract class EmployeePosition_Manager : MonoBehaviour
    {
        const string _employeePosition_SOPath = "ScriptableObjects/EmployeePosition_SO";

        static EmployeePosition_SO _EmployeePositions;

        static EmployeePosition_SO EmployeePositions =>
            _EmployeePositions ??= _getEmployeePosition_SO();

        public static EmployeePosition_Master GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            EmployeePositions.GetEmployeePosition_Master(employeePositionName);
        
        public static EmployeePosition_Requirements GetEmployeeMinimumRequirements(EmployeePositionName employeePositionName) =>
            EmployeePositions.GetEmployeeMinimumRequirements(employeePositionName);

        public static void PopulateAllEmployeePositions()
        {
            EmployeePositions.PopulateDefaultEmployeePositions();
            // Then populate custom EmployeePositions.
        }

        static EmployeePosition_SO _getEmployeePosition_SO()
        {
            var employeePosition_SO = Resources.Load<EmployeePosition_SO>(_employeePosition_SOPath);

            if (employeePosition_SO is not null) return employeePosition_SO;

            Debug.LogError("EmployeePosition_SO not found. Creating temporary EmployeePosition_SO.");
            employeePosition_SO = ScriptableObject.CreateInstance<EmployeePosition_SO>();

            return employeePosition_SO;
        }
    }

    [Serializable]
    public class EmployeePosition_Master
    {
        public readonly EmployeePositionName EmployeePositionName;
        public readonly ActorDataPresetName  EmployeeDataPreset;

        public EmployeePosition_Master(EmployeePositionName employeePositionName,
                                       ActorDataPresetName  employeeDataPreset)
        {
            EmployeePositionName = employeePositionName;
            EmployeeDataPreset   = employeeDataPreset;
        }
    }

    [Serializable]
    public class EmployeePosition_Requirements
    {
        public readonly EmployeePositionName EmployeePositionName;
        public readonly CareerRequirements CareerRequirements;
        public readonly CraftingRequirements CraftingRequirements;
        public readonly VocationRequirements VocationRequirements;
        
        public EmployeePosition_Requirements(EmployeePositionName employeePositionName,
                                             CareerRequirements careerRequirements,
                                             CraftingRequirements craftingRequirements,
                                             VocationRequirements vocationRequirements)
        {
            EmployeePositionName    = employeePositionName;
            CareerRequirements   = careerRequirements;
            CraftingRequirements = craftingRequirements;
            VocationRequirements = vocationRequirements;
        }
        
        public bool MeetsRequirements(Actor_Data actorData)
        {
            return CareerRequirements.MeetsRequirements(actorData.CareerData)     &&
                   CraftingRequirements.MeetsRequirements(actorData.CraftingData) &&
                   VocationRequirements.MeetsRequirements(actorData.VocationData);
        }
    }
    
    [Serializable]
    public class CareerRequirements
    {
        public readonly CareerName RequiredCareer;

        public CareerRequirements(CareerName requiredCareer)
        {
            RequiredCareer = requiredCareer;
        }
        
        public bool MeetsRequirements(CareerData careerData)
        {
            return careerData.CareerName == RequiredCareer;
        }
    }
    
    [Serializable]
    public class CraftingRequirements
    {
        public readonly List<RecipeName> RequiredRecipes;

        public CraftingRequirements(List<RecipeName> requiredRecipes)
        {
            RequiredRecipes = requiredRecipes;
        }
        
        public bool MeetsRequirements(CraftingData craftingData)
        {
            return RequiredRecipes.TrueForAll(recipeName => craftingData.KnownRecipes.Contains(recipeName));
        }
    }
    
    [Serializable]
    public class VocationRequirements
    {
        public readonly Dictionary<VocationName, float> RequiredVocations;

        public VocationRequirements(Dictionary<VocationName, float> requiredVocations)
        {
            RequiredVocations = requiredVocations;
        }
        
        public bool MeetsRequirements(VocationData vocationData)
        {
            foreach (var requiredVocation in RequiredVocations)
            {
                if (!vocationData.ActorVocations.TryGetValue(requiredVocation.Key, out var vocation))
                {
                    return false;
                }

                if (vocation.VocationExperience < requiredVocation.Value)
                {
                    return false;
                }
            }
            
            return true;
        }
    }

    public enum EmployeePositionName
    {
        Intern,

        Shopkeeper,

        Logger,

        Sawyer,

        Smith,
        
        Miner,

        Hauler,
        
        Owner,
        
        Fisher,
        
        Farmer,
        
        Cook,
        
        Tanner
    }
}