using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Buildings;
using Cities;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Baronies
{
    [Serializable]
    public class Barony_Data : Data_Class
    {
        public ulong ID;
        public ulong RulerID;
        public ulong CountyID;

        public BaronyType Type;

        public string Name;
        public string Description;

        Barony_Component _barony;

        Actor_Data _ruler;

        public Barony_BuildingData Buildings;
        public Barony_PopulationData Population;
        public Barony_ProsperityData Prosperity;
        
        SerializableDictionary<ulong, Building_Data> _allBuildings;

        const int c_maxBaronyLevel = 5;
        const int c_maxBaronyBuildings = 10;

        public Barony_Component Barony => _barony ??= Barony_Manager.GetBarony_Component(ID);
        public Actor_Data Ruler => _ruler ??= Actor_Manager.GetActor_Data(RulerID);
        public SerializableDictionary<ulong, Building_Data> AllBuildings
        {
            get
            {
                if (_allBuildings is not null && _allBuildings.Count != 0) return _allBuildings;

                return _allBuildings = Barony.GetAllBuildingsInBarony();
            }
        }

        public Barony_Data(ulong id, BaronyType type, string name, string description, ulong countyID,
            Barony_PopulationData population, Barony_BuildingData buildings,
            Barony_ProsperityData baronyProsperityData = null)
        {
            ID = id;
            Name = name;
            Type = type;
            Description = description;
            CountyID = countyID;

            Population = new Barony_PopulationData(population);
            Buildings = new Barony_BuildingData(buildings);
            Prosperity = new Barony_ProsperityData(baronyProsperityData);
        }

        public void InitialiseBaronyData()
        {
            if (Barony?.ID is not null and not 0) return;

            Debug.LogWarning($"Barony with ID {ID} not found in Barony_SO.");
        }

        public void OnProgressDay() 
        {
            Buildings.OnProgressDay();
            Population.OnProgressDay();
            Prosperity.OnProgressDay();
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Barony ID", $"{ID}" },
                { "Barony Name", Name },
                { "Barony Type", $"{Type}" },
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
                title: "Population Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Population.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Barony JobSites",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllBuildings.ToDictionary(
                    building => building.Key.ToString(),
                    building => building.Value.Name));

            return DataToDisplay;
        }
    }


    [CustomPropertyDrawer(typeof(Barony_Data))]
    public class BaronyData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var baronyName = property.FindPropertyRelative("BaronyName");
            label.text = !string.IsNullOrEmpty(baronyName?.stringValue) ? baronyName?.stringValue : "Unnamed Barony";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}