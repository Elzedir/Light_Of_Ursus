using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Careers;
using Recipes;
using Tools;
using UnityEngine;

namespace EmployeePosition
{
    [Serializable]
    public class EmployeePosition_Data : Data_Class
    {
        public readonly EmployeePositionName EmployeePositionName;
        public readonly ActorDataPresetName  EmployeeDataPreset;
        
        public readonly CareerName                      RequiredCareer;
        public readonly Dictionary<VocationName, float> RequiredVocations;
        public readonly List<RecipeName>                RequiredRecipes;

        public EmployeePosition_Data(EmployeePositionName            employeePositionName,
                                     ActorDataPresetName             employeeDataPreset, CareerName requiredCareer,
                                     Dictionary<VocationName, float> requiredVocations,
                                     List<RecipeName>                requiredRecipes)
        {
            EmployeePositionName = employeePositionName;
            EmployeeDataPreset   = employeeDataPreset;
            RequiredCareer       = requiredCareer;
            RequiredVocations    = requiredVocations;
            RequiredRecipes      = requiredRecipes;
        }

        public bool MeetsRequirements(Actor_Data actorData)
        {
            return MeetsRequirements(actorData.CareerData)     &&
                   MeetsRequirements(actorData.CraftingData) &&
                   MeetsRequirements(actorData.VocationData);
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
        
        public bool MeetsRequirements(CraftingData craftingData)
        {
            return RequiredRecipes.TrueForAll(recipeName => craftingData.KnownRecipes.Contains(recipeName));
        }
        
        public bool MeetsRequirements(CareerData careerData)
        {
            return careerData.CareerName == RequiredCareer;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Employee Position Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Employee Position Name: {EmployeePositionName}",
                        $"Employee Data Preset: {EmployeeDataPreset}",
                        $"Required Career: {RequiredCareer}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Employee Position Data");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Required Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: RequiredVocations.Select(vocation => $"{vocation.Key}: {vocation.Value}").ToList()
                    ));
            }
            catch
            {
                Debug.LogError("Error in Employee Position Data");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Required Recipes",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: RequiredRecipes.Select(recipe => recipe.ToString()).ToList()
                    ));
            }
            catch
            {
                Debug.LogError("Error in Employee Position Data");
            }

            return new Data_Display(
                title: "Employee Position Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
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
        
        Fisher,
        
        Farmer,
        
        Cook,
        
        Tanner
    }
}