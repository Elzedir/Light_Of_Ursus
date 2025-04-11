using System;
using System.Collections.Generic;
using Buildings;
using Tools;
using UnityEngine;

namespace Settlements
{
    [Serializable]
    public class Settlement_Buildings
    {
        public Settlement_Data Settlement_Data;
        
        public int MaxLevel;
        public Dictionary<int, int> BuildingSlotsPerLevel;
        
        Dictionary<ulong, Building_Data> _allBuildings;
        
        public Dictionary<ulong, Building_Data> AllBuildings
        {
            get
            {
                if (_allBuildings is not null && _allBuildings.Count != 0) return _allBuildings;

                return _allBuildings = Settlement_Data.Settlement.GetAllBuildingsInSettlement();
            }
        }

        public Settlement_Buildings(Settlement_Buildings data, Settlement_Data settlement_Data)
        {
            Settlement_Data = settlement_Data;
            MaxLevel = data.MaxLevel;
        }

        public Settlement_Buildings()
        {
            
        }

        public void InitialiseBuildings()
        {
            foreach (var building in AllBuildings.Values)
            {
                _spawnBuilding(building);
            }
        }

        void _spawnBuilding(Building_Data building_Data)
        {
            var buildingGO = new GameObject($"{building_Data.BuildingType}_{building_Data.ID}");
            buildingGO.transform.SetParent(Settlement_Data.Settlement.transform);
            
            var building = buildingGO.AddComponent<Building_Component>();
            building.Initialise();
        }

        public void OnProgressDay()
        {
            foreach (var building in AllBuildings.Values)
            {
                building.OnProgressDay();
            }
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            var income = 0f;
            
            foreach (var building in AllBuildings.Values)
            {
                income += building.GenerateIncome(liegeTaxRate);
            }
            
            return income;
        }
    }
}