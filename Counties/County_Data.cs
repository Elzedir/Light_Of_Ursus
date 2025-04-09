using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Baronies;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Counties
{
    [Serializable]
    public class County_Data : Data_Class
    {
        public ulong ID;
        public ulong RulerID;
        public ulong DuchyID;
        public ulong Capital_BaronyID;

        public float Gold;

        public float TaxRate = 0.1f; 

        public string Name;
        public string Description;
        
        County_Component _county;

        Actor_Data _ruler;

        SerializableDictionary<ulong, Barony_Data> _allBaronies;
        
        public County_Component County => _county ??= County_Manager.GetCounty_Component(ID);
        public Actor_Data Ruler => _ruler ??= Actor_Manager.GetActor_Data(RulerID);
        public SerializableDictionary<ulong, Barony_Data> AllBaronies
        {
            get
            {
                if (_allBaronies is not null && _allBaronies.Count != 0) return _allBaronies;

                return _allBaronies = County.GetAllBaroniesInCounty();
            }
        }
        
        public County_Data(ulong id, ulong rulerID, string name, string description,
            SerializableDictionary<ulong, Barony_Data> allBaronies)
        {
            ID = id;
            RulerID = rulerID;
            Name = name;
            Description = description;
            _allBaronies = allBaronies;
        }

        public void InitialiseCountyData()
        {
            if (County?.ID is not null and not 0) return;

            Debug.LogWarning($"County with ID {ID} not found in County_SO.");
        }

        public void OnProgressDay()
        {
            foreach (var barony in AllBaronies.Values)
            {
                barony.OnProgressDay();
            }

            _generateIncome();
        }

        void _generateIncome()
        {
            foreach(var barony in AllBaronies.Values)
            {
                Gold += barony.GenerateIncome(TaxRate);
            }
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "County ID", $"{ID}" },
                { "County Name", Name },
                { "County Description", Description },
                { "All Barony IDs", string.Join(", ", AllBaronies.Keys) }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base County Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "County Baronies",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllBaronies.ToDictionary(
                    barony => barony.Key.ToString(),
                    barony => barony.Value.Name));

            return DataToDisplay;
        }
    }

    [CustomPropertyDrawer(typeof(County_Data))]
    public class CountyData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var countyName = property.FindPropertyRelative("CountyName");
            label.text = !string.IsNullOrEmpty(countyName.stringValue) ? countyName.stringValue : "Unnamed County";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }
}