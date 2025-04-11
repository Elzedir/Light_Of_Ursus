using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Settlements
{
    [Serializable]
    public class Settlement_Data : Data_Class
    {
        public ulong ID;
        public ulong RulerID;
        public ulong BaronyID;

        public float Gold;
        public float TaxRate = 0.1f;

        public SettlementType Type;

        public string Name;
        public string Description;

        Settlement_Component _settlement;

        Actor_Data _ruler;

        public Settlement_Buildings Buildings;
        public Settlement_Population Population;
        public Settlement_Prosperity Prosperity;

        const int c_maxSettlementLevel = 5;
        const int c_maxSettlementBuildings = 10;

        public Settlement_Component Settlement => _settlement ??= Settlement_Manager.GetSettlement_Component(ID);
        public Actor_Data Ruler => _ruler ??= Actor_Manager.GetActor_Data(RulerID);

        public Settlement_Data(ulong id, SettlementType type, string name, string description, ulong baronyID,
            Settlement_Population population, Settlement_Buildings buildings,
            Settlement_Prosperity settlementProsperity = null)
        {
            ID = id;
            Name = name;
            Type = type;
            Description = description;
            BaronyID = baronyID;

            Population = new Settlement_Population(population);
            Buildings = new Settlement_Buildings(buildings, this);
            Prosperity = new Settlement_Prosperity(settlementProsperity);
        }

        public void InitialiseSettlementData(ulong settlementID)
        {
            if (ID != settlementID) throw new Exception($"Settlement ID {settlementID} does not match " +
                $"Settlement_Data ID {ID}.");

            Buildings.InitialiseBuildings();
        }

        public void OnProgressDay() 
        {
            Buildings.OnProgressDay();
            Population.OnProgressDay();
            Prosperity.OnProgressDay();
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            var income = Buildings.GenerateIncome(TaxRate);

            var tax = income * liegeTaxRate;

            Gold += income - tax;
            
            return tax;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Settlement ID", $"{ID}" },
                { "Settlement Name", Name },
                { "Settlement Type", $"{Type}" },
                { "Region ID", $"{BaronyID}" },
                { "Settlement Description", Description }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Settlement Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Population Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Population.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Settlement JobSites",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: Buildings.AllBuildings.ToDictionary(
                    building => building.Key.ToString(),
                    building => building.Value.Name));

            return DataToDisplay;
        }
    }


    [CustomPropertyDrawer(typeof(Settlement_Data))]
    public class SettlementData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var settlementName = property.FindPropertyRelative("SettlementName");
            label.text = !string.IsNullOrEmpty(settlementName?.stringValue) ? settlementName.stringValue : "Unnamed Settlement";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}