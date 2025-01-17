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
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Actor Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatsAndAbilities?.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CareerData?.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Crafting Recipes",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CraftingData?.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: VocationData?.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData?.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Equipment Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: EquipmentData?.GetSubData(toggleMissingDataDebugs));
            
            return _dataToDisplay;
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
                { "Stats and Abilities", StatsAndAbilities.GetSubData(toggleMissingDataDebugs) },
                { "Career Data", CareerData.GetSubData(toggleMissingDataDebugs) },
                { "Crafting Recipes", CraftingData.GetSubData(toggleMissingDataDebugs) },
                { "Vocation Data", VocationData.GetSubData(toggleMissingDataDebugs) },
                { "Inventory Data", InventoryData.GetSubData(toggleMissingDataDebugs) },
                { "Equipment Data", EquipmentData.GetSubData(toggleMissingDataDebugs) }
            };
        }
    }
}