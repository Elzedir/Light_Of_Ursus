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
        
        public CareerUpdater              CareerUpdater;
        public CraftingUpdater            CraftingUpdater;
        public VocationUpdater            VocationUpdater;
        public StatsAndAbilities       StatsAndAbilities;
        public InventoryUpdater           InventoryUpdater;
        public EquipmentData           EquipmentData;

        public ActorPreset_Data(ActorDataPresetName actorDataPresetName, CareerUpdater    careerUpdater = null,
                                CraftingUpdater        craftingUpdater = null,        VocationUpdater  vocationUpdater = null,
                                StatsAndAbilities   statsAndAbilities = null,   InventoryUpdater inventoryUpdater = null,
                                EquipmentData       equipmentData = null)
        {
            ActorDataPresetName = actorDataPresetName;
            CareerUpdater          = careerUpdater;
            CraftingUpdater        = craftingUpdater;
            VocationUpdater        = vocationUpdater;
            StatsAndAbilities   = statsAndAbilities;
            InventoryUpdater       = inventoryUpdater;
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
                        $"Career Name: {CareerUpdater.CareerName}",
                        $"Jobs Active: {CareerUpdater.JobsActive}",
                        $"JobSiteID: {CareerUpdater.JobSiteID}",
                        $"Employee Position: {CareerUpdater.EmployeePositionName}",
                        $"Current Job: {CareerUpdater.CurrentJob?.JobName}"
                    }
                ));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CareerUpdater);
                    Debug.LogWarning(CareerUpdater?.CurrentJob);
                }
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Known Recipes",
                    dataDisplayType: DataDisplayType.Item,
                    data: CraftingUpdater.KnownRecipes.Select(recipe => $"{recipe}").ToList()
                ));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(CraftingUpdater);
                    Debug.LogWarning(CraftingUpdater?.KnownRecipes);
                }
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Vocations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: VocationUpdater.ActorVocations.Values.Select(vocation => $"{vocation.VocationName}: {vocation.VocationExperience}").ToList()
                ));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(VocationUpdater);
                    Debug.LogWarning(VocationUpdater?.ActorVocations);
                }
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "All Inventory Items",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: InventoryUpdater.AllInventoryItems.Values.Select(item => $"{item.ItemID}: {item.ItemName} Qty - {item.ItemAmount}").ToList()
                ));
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.LogWarning(InventoryUpdater);
                    Debug.LogWarning(InventoryUpdater?.AllInventoryItems);
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
                subData: new List<Data_Display>(dataObjects));
        }
    }
}