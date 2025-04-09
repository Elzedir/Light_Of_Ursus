using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Tools;

namespace Buildings
{
    [Serializable]
    public class Building_Production : Data_Class
    {
        public float Temp_TotalProduction;
        
        public List<Item> AllProducedItems;
        public HashSet<Item> EstimatedProductionRatePerHour;
        public ulong BuildingID;

        Building_Component _building;
        public Building_Component Building => _building ??= Building_Manager.GetBuilding_Component(BuildingID);
        
        public Building_Production(ulong buildingID)
        {
            BuildingID = buildingID;
            AllProducedItems = new List<Item>();
        }

        public Building_Production(Building_Production buildingProduction)
        {
            BuildingID = buildingProduction.BuildingID;
            AllProducedItems = buildingProduction.AllProducedItems;
        }

        public float GenerateIncome()
        {
            return Temp_TotalProduction;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var productionData = new Dictionary<string, string>
            {
                { "Station ID", $"{BuildingID}" }
            };

            var allProducedItems = AllProducedItems?.ToDictionary(item => item.ItemID.ToString(),
                item => item.ItemName.ToString()) ?? new Dictionary<string, string>();
            var estimatedProductionRatePerHour = EstimatedProductionRatePerHour?.ToDictionary(
                item => item.ItemID.ToString(),
                item => item.ItemName.ToString()) ?? new Dictionary<string, string>();

            return productionData.Concat(allProducedItems).Concat(estimatedProductionRatePerHour)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Production Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public HashSet<Item> GetEstimatedProductionRatePerHour()
        {
            return EstimatedProductionRatePerHour = Building.Building_Data.GetEstimatedProductionRatePerHour();
        }
    }
}