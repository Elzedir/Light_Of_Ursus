using System.Collections.Generic;
using System.Linq;
using Inventory;
using Tools;
using UnityEngine;

namespace Actor
{
    public class ActorPreset_Data : Data_Class
    {
        public readonly ActorDataPresetName ActorDataPresetName;
        
        public readonly Career_Data_Preset       CareerDataPreset;
        public readonly Crafting_Data_Preset     CraftingDataPreset;
        public readonly Vocation_Data_Preset     VocationDataPreset;
        public readonly StatsAndAbilities_Preset StatsAndAbilitiesPreset;
        public readonly Inventory_Data_Preset    InventoryDataPreset;
        public readonly Equipment_Data_Preset    EquipmentDataPreset;

        public ActorPreset_Data(ActorDataPresetName actorDataPresetName, Career_Data_Preset    careerDataPreset = null,
                                Crafting_Data_Preset        craftingDataPreset = null,        Vocation_Data_Preset  vocationDataPreset = null,
                                StatsAndAbilities_Preset   statsAndAbilitiesPreset = null,   Inventory_Data_Preset inventoryDataPreset = null,
                                Equipment_Data_Preset       equipmentDataPreset = null)
        {
            ActorDataPresetName = actorDataPresetName;
            CareerDataPreset          = careerDataPreset;
            CraftingDataPreset        = craftingDataPreset;
            VocationDataPreset        = vocationDataPreset;
            StatsAndAbilitiesPreset   = statsAndAbilitiesPreset;
            InventoryDataPreset       = inventoryDataPreset;
            EquipmentDataPreset       = equipmentDataPreset;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Actor Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    data: new Dictionary<string, string>(),
                    firstData: true);

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Actor Data", out var actorData))
                {
                    dataSO_Object.SubData["Actor Data"] = new Data_Display(
                        title: "Actor Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (actorData is not null)
                {
                    actorData.Data = new Dictionary<string, string>
                    {
                        { "Actor Level", $"{StatsAndAbilitiesPreset.ActorStats.ActorLevelData.ActorLevel}" },
                        { "Actor Experience", $"{StatsAndAbilitiesPreset.ActorStats.ActorLevelData.TotalExperience}" },
                        { "Actor Special", $"{StatsAndAbilitiesPreset.ActorStats.ActorSpecial.GetStatsToString()}" }
                    };
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(StatsAndAbilitiesPreset);
                    Debug.LogWarning(StatsAndAbilitiesPreset?.ActorStats);
                    Debug.LogWarning(StatsAndAbilitiesPreset?.ActorStats.ActorSpecial);   
                }
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Career Data", out var careerData))
                {
                    dataSO_Object.SubData["Career Data"] = new Data_Display(
                        title: "Career Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (careerData is not null)
                {
                    careerData.Data = new Dictionary<string, string>
                    {
                        { "Career Name", $"{CareerDataPreset.CareerName}" },
                        { "Jobs Active", $"{CareerDataPreset.JobsActive}" },
                        { "JobSiteID", $"{CareerDataPreset.JobSiteID}" },
                        { "Current Job", $"{CareerDataPreset.CurrentJob?.JobName}" }
                    };
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CareerDataPreset);
                    Debug.LogWarning(CareerDataPreset?.CurrentJob);
                }
            }
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Known Recipes", out var knownRecipes))
                {
                    dataSO_Object.SubData["Known Recipes"] = new Data_Display(
                        title: "Known Recipes",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: CraftingDataPreset.KnownRecipes.ToDictionary(recipe => $"{(uint)recipe}", recipe => $"{recipe}")
                    );
                }
                
                if (knownRecipes is not null)
                {
                    knownRecipes.Data = CraftingDataPreset.KnownRecipes.ToDictionary(recipe => $"{(uint)recipe}", recipe => $"{recipe}");
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CraftingDataPreset);
                    Debug.LogWarning(CraftingDataPreset?.KnownRecipes);
                }
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Vocations", out var vocations))
                {
                    dataSO_Object.SubData["Vocations"] = new Data_Display(
                        title: "Vocations",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: VocationDataPreset.ActorVocations.Values.ToDictionary(vocation => $"{vocation.VocationName}:", vocation => $"{vocation.VocationExperience}")
                    );
                }
                
                if (vocations is not null)
                {
                    vocations.Data = VocationDataPreset.ActorVocations.Values.ToDictionary(vocation => $"{vocation.VocationName}:", vocation => $"{vocation.VocationExperience}");
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(VocationDataPreset);
                    Debug.LogWarning(VocationDataPreset?.ActorVocations);
                }
            }
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Stats and Abilities", out var statsAndAbilities))
                {
                    dataSO_Object.SubData["Stats and Abilities"] = new Data_Display(
                        title: "Stats and Abilities",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (statsAndAbilities is not null)
                {
                    statsAndAbilities.Data = InventoryDataPreset.AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:",
                        item => $"{item.ItemName} Qty - {item.ItemAmount}");
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(InventoryDataPreset);
                    Debug.LogWarning(InventoryDataPreset?.AllInventoryItems);
                }
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Inventory Data", out var inventoryData))
                {
                    dataSO_Object.SubData["Inventory Data"] = new Data_Display(
                        title: "Inventory Data",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (inventoryData is not null)
                {
                    inventoryData.Data = new Dictionary<string, string>
                    {
                        { "Equipment Data", "EquipmentData would be here" }
                    };
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(EquipmentDataPreset);
                }
            }

            return dataSO_Object;
        }
    }
}