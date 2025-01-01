using System.Collections.Generic;
using System.Linq;
using Inventory;
using Tools;
using UnityEngine;

namespace Actor
{
    public class ActorPreset_Data : Data_Class
    {
        public ActorDataPresetName ActorDataPresetName;
        
        public CareerData              CareerData;
        public CraftingData            CraftingData;
        public VocationData            VocationData;
        public StatsAndAbilities       StatsAndAbilities;
        public InventoryData           InventoryData;
        public EquipmentData           EquipmentData;

        public ActorPreset_Data(ActorDataPresetName actorDataPresetName, CareerData    careerData = null,
                                CraftingData        craftingData = null,        VocationData  vocationData = null,
                                StatsAndAbilities   statsAndAbilities = null,   InventoryData inventoryData = null,
                                EquipmentData       equipmentData = null)
        {
            ActorDataPresetName = actorDataPresetName;
            CareerData          = careerData;
            CraftingData        = craftingData;
            VocationData        = vocationData;
            StatsAndAbilities   = statsAndAbilities;
            InventoryData       = inventoryData;
            EquipmentData       = equipmentData;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);

            try
            {
                dataObjects["Stats And Abilities"] = new Data_Display(
                    title: "Stats And Abilities",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Actor Level", $"{StatsAndAbilities.ActorStats.ActorLevelData.ActorLevel}" },
                        { "Actor Experience", $"{StatsAndAbilities.ActorStats.ActorLevelData.TotalExperience}" },
                        { "Actor Special", $"{StatsAndAbilities.ActorStats.ActorSpecial.GetStatsToString()}" }
                    }
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(StatsAndAbilities);
                    Debug.LogWarning(StatsAndAbilities?.ActorStats);
                    Debug.LogWarning(StatsAndAbilities?.ActorStats.ActorSpecial);   
                }
            }

            try
            {
                dataObjects["Career Data"] = new Data_Display(
                    title: "Career Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Career Name", $"{CareerData.CareerName}" },
                        { "Jobs Active", $"{CareerData.JobsActive}" },
                        { "JobSiteID", $"{CareerData.JobSiteID}" },
                        { "Current Job", $"{CareerData.CurrentJob?.JobName}" }
                    }
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CareerData);
                    Debug.LogWarning(CareerData?.CurrentJob);
                }
            }
            
            try
            {
                dataObjects["Known Recipes"] = new Data_Display(
                    title: "Known Recipes",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: CraftingData.KnownRecipes.ToDictionary(recipe => $"{(uint)recipe}", recipe => $"{recipe}")
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CraftingData);
                    Debug.LogWarning(CraftingData?.KnownRecipes);
                }
            }

            try
            {
                dataObjects["Vocations"] = new Data_Display(
                    title: "Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: VocationData.ActorVocations.Values.ToDictionary(vocation => $"{vocation.VocationName}:", vocation => $"{vocation.VocationExperience}")
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(VocationData);
                    Debug.LogWarning(VocationData?.ActorVocations);
                }
            }
            
            try
            {
                dataObjects["All Inventory Items"] = new Data_Display(
                    title: "All Inventory Items",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: InventoryData.AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:", item => $"{item.ItemName} Qty - {item.ItemAmount}")
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(InventoryData);
                    Debug.LogWarning(InventoryData?.AllInventoryItems);
                }
            }

            try
            {
                dataObjects["Equipment Data"] = new Data_Display(
                    title: "Equipment Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Equipment Data", "EquipmentData would be here" }
                    }
                );
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(EquipmentData);
                }
            }

            return dataSO_Object = new Data_Display(
                title: $"{(uint)ActorDataPresetName}: {ActorDataPresetName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
        }
    }
}