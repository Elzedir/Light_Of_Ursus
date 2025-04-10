using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Cities;
using Counties;
using Settlements;
using Tools;
using UnityEditor;
using UnityEngine;
using SettlementType = Cities.SettlementType;

namespace Baronies
{
    [Serializable]
    public class Barony_Data : Data_Class
    {
        public ulong ID;
        public ulong RulerID;
        public ulong CountyID;

        public float Gold;
        public float TaxRate = 0.1f;

        public string Name;
        public string Description;

        Barony_Component _barony;
        County_Component _county;

        Actor_Data _ruler;

        Dictionary<ulong, Settlement_Data> _allSettlements;

        const int c_maxBaronyLevel = 5;
        const int c_maxBaronyBuildings = 10;

        public Barony_Component Barony => _barony ??= Barony_Manager.GetBarony_Component(ID);
        public County_Component County => _county ??= County_Manager.GetCounty_Component(CountyID);
        public Actor_Data Ruler => _ruler ??= Actor_Manager.GetActor_Data(RulerID);
        
        public Dictionary<ulong, Settlement_Data> AllSettlements
        {
            get
            {
                if (_allSettlements is not null && _allSettlements.Count != 0) return _allSettlements;

                return _allSettlements = Barony.GetAllSettlementsInBarony();
            }
        }

        public Barony_Data(ulong id, string name, string description, ulong countyID,
            Dictionary<ulong, Settlement_Data> allSettlements)
        {
            ID = id;
            Name = name;
            Description = description;
            CountyID = countyID;
            
            _allSettlements = allSettlements;
        }

        public void InitialiseBaronyData()
        {
            if (Barony?.ID is not null and not 0) return;

            Debug.LogWarning($"Barony with ID {ID} not found in Barony_SO.");
        }

        public void OnProgressDay() 
        {
            foreach (var settlement in AllSettlements.Values)
            {
                settlement.OnProgressDay();
            }
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            float income = 0;
            
            foreach (var settlement in AllSettlements.Values)
            {
                income += settlement.GenerateIncome(TaxRate);
            }
            
            var tax = income * liegeTaxRate;
            
            Gold += income - tax;
            
            return tax;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Barony ID", $"{ID}" },
                { "Barony Name", Name },
                { "Region ID", $"{CountyID}" },
                { "Barony Description", Description }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Barony Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Barony Settlements",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllSettlements.ToDictionary(
                    barony => barony.Key.ToString(),
                    barony => barony.Value.Name));

            return DataToDisplay;
        }
    }


    [CustomPropertyDrawer(typeof(Barony_Data))]
    public class BaronyData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var baronyName = property.FindPropertyRelative("BaronyName");
            label.text = !string.IsNullOrEmpty(baronyName?.stringValue) ? baronyName.stringValue : "Unnamed Barony";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}