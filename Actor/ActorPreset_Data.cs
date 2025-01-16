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
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs, DataToDisplay dataToDisplay)
        {
            _updateDataDisplay(ref dataToDisplay,
                title: "Actor Data",
                subData: StatsAndAbilities.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);
            
            _updateDataDisplay(ref dataToDisplay,
                title: "Career Data",
                subData: CareerData.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);
            
            _updateDataDisplay(ref dataToDisplay,
                title: "Crafting Recipes",
                subData: CraftingData.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);
            
            _updateDataDisplay(ref dataToDisplay,
                title: "Vocation Data",
                subData: VocationData.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);

            _updateDataDisplay(ref dataToDisplay,
                title: "Inventory Data",
                subData: InventoryData.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);

            _updateDataDisplay(ref dataToDisplay,
                title: "Equipment Data",
                subData: EquipmentData.GetSubData(toggleMissingDataDebugs, dataToDisplay).SubData);
            
            return dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Data Preset Name", ActorDataPresetName.ToString() }
            };
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDebugs, DataToDisplay dataToDisplay)
        {
            return new Dictionary<string, DataToDisplay>
            {
                { "Stats and Abilities", StatsAndAbilities.GetData_Display(toggleMissingDebugs) },
                { "Career Data", CareerData.GetData_Display(toggleMissingDebugs) },
                { "Crafting Recipes", CraftingData.GetData_Display(toggleMissingDebugs) },
                { "Vocation Data", VocationData.GetData_Display(toggleMissingDebugs) },
                { "Inventory Data", InventoryData.GetData_Display(toggleMissingDebugs) },
                { "Equipment Data", EquipmentData.GetData_Display(toggleMissingDebugs) }
            };
        }
    }
}