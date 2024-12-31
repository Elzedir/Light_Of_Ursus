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
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Stats And Abilities",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Actor Level: {StatsAndAbilities.ActorStats.ActorLevelData.ActorLevel}",
                        $"Actor Experience: {StatsAndAbilities.ActorStats.ActorLevelData.TotalExperience}",
                        $"Actor Special: {StatsAndAbilities.ActorStats.ActorSpecial.GetStatsToString()}"
                    }
                ));
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
                dataObjects.Add(new Data_Display(
                    title: "Career Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Career Name: {CareerData.CareerName}",
                        $"Jobs Active: {CareerData.JobsActive}",
                        $"JobSiteID: {CareerData.JobSiteID}",
                        $"Current Job: {CareerData.CurrentJob?.JobName}"
                    }
                ));
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
                dataObjects.Add(new Data_Display(
                    title: "Known Recipes",
                    dataDisplayType: DataDisplayType.Item,
                    data: CraftingData.KnownRecipes.Select(recipe => $"{recipe}").ToList()
                ));
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
                dataObjects.Add(new Data_Display(
                    title: "Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: VocationData.ActorVocations.Values.Select(vocation => $"{vocation.VocationName}: {vocation.VocationExperience}").ToList()
                ));
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
                dataObjects.Add(new Data_Display(
                    title: "All Inventory Items",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: InventoryData.AllInventoryItems.Values.Select(item => $"{item.ItemID}: {item.ItemName} Qty - {item.ItemAmount}").ToList()
                ));
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
                dataObjects.Add(new Data_Display(
                    title: "Equipment Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        "Equipment Data"
                    }
                ));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(EquipmentData);
                }
            }

            return new Data_Display(
                title: $"{(uint)ActorDataPresetName}: {ActorDataPresetName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects),
                selectedIndex: dataSO_Object?.SelectedIndex ?? -1,
                showData: dataSO_Object?.ShowData           ?? false);
        }
    }
}