using System.Collections.Generic;
using System.Linq;
using Equipment;
using Inventory;
using Tools;
using UnityEngine;

namespace Actor
{
    public class ActorPreset_Data : Data_Class
    {
        public readonly ActorDataPresetName ActorDataPresetName;
        
        public readonly Career_Data       CareerData;
        public readonly Crafting_Data     CraftingData;
        public readonly Vocation_Data     VocationData;
        public readonly StatsAndAbilities StatsAndAbilities;
        public readonly Inventory_Data    InventoryData;
        public readonly Equipment_Data    EquipmentData;

        public ActorPreset_Data(ActorDataPresetName actorDataPresetName, Career_Data    careerData = null,
                                Crafting_Data        craftingData = null,        Vocation_Data  vocationData = null,
                                StatsAndAbilities   statsAndAbilities = null,   Inventory_Data inventoryData = null,
                                Equipment_Data       equipmentData = null)
        {
            ActorDataPresetName = actorDataPresetName;
            CareerData          = careerData;
            CraftingData        = craftingData;
            VocationData        = vocationData;
            StatsAndAbilities   = statsAndAbilities;
            InventoryData       = inventoryData;
            EquipmentData       = equipmentData;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Actor Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatsAndAbilities?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CareerData?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Crafting Recipes",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CraftingData?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: VocationData?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Equipment Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: EquipmentData?.GetDataToDisplay(toggleMissingDataDebugs));
            
            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Data Preset Name", ActorDataPresetName.ToString() }
            };
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                { "Stats and Abilities", StatsAndAbilities.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Career Data", CareerData.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Crafting Recipes", CraftingData.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Vocation Data", VocationData.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Inventory Data", InventoryData.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Equipment Data", EquipmentData.GetDataToDisplay(toggleMissingDataDebugs) }
            };
        }
    }
}