using System.Collections.Generic;
using Actor;
using Actors;
using Equipment;
using Inventory;
using Tools;

namespace ActorPreset
{
    public class ActorPreset_Data : Data_Class
    {
        public readonly ActorDataPresetName ActorDataPresetName;
        
        public readonly Actor_Data_Career       ActorDataCareer;
        public readonly Actor_Data_Crafting     ActorDataCrafting;
        public readonly Actor_Data_Vocation     ActorDataVocation;
        public readonly Actor_Data_StatsAndAbilities ActorDataStatsAndAbilities;
        public readonly InventoryData    InventoryData;
        public readonly Equipment_Data    EquipmentData;

        public ActorPreset_Data(ActorDataPresetName actorDataPresetName, Actor_Data_Career    actorDataCareer = null,
                                Actor_Data_Crafting        actorDataCrafting = null,        Actor_Data_Vocation  actorDataVocation = null,
                                Actor_Data_StatsAndAbilities   actorDataStatsAndAbilities = null,   InventoryData inventoryData = null,
                                Equipment_Data       equipmentData = null)
        {
            ActorDataPresetName = actorDataPresetName;
            ActorDataCareer          = actorDataCareer;
            ActorDataCrafting        = actorDataCrafting;
            ActorDataVocation        = actorDataVocation;
            ActorDataStatsAndAbilities   = actorDataStatsAndAbilities;
            InventoryData       = inventoryData;
            EquipmentData       = equipmentData;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Actor Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ActorDataStatsAndAbilities?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ActorDataCareer?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Crafting Recipes",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ActorDataCrafting?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ActorDataVocation?.GetDataToDisplay(toggleMissingDataDebugs));

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
                { "Stats and Abilities", ActorDataStatsAndAbilities.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Career Data", ActorDataCareer.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Crafting Recipes", ActorDataCrafting.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Vocation Data", ActorDataVocation.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Inventory Data", InventoryData.GetDataToDisplay(toggleMissingDataDebugs) },
                { "Equipment Data", EquipmentData.GetDataToDisplay(toggleMissingDataDebugs) }
            };
        }
    }
}