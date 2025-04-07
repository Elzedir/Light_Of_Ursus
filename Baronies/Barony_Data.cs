using System;
using System.Collections.Generic;
using System.Linq;
using Baronies;
using Buildings;
using Managers;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cities
{
    [Serializable]
    public class Barony_Data : Data_Class
    {
        public BaronyName BaronyName;
        public bool IsCapital;
        public Dictionary<int, int> BuildingSlotsPerLevel;
        
        public ulong ID;
        public ulong FactionID;
        public ulong RegionID;
        
        public string Name;
        public string Description;

        public int Size;

        [FormerlySerializedAs("_allJobSiteIDs")] [SerializeField] List<ulong> _allBuildingIDs;

        Barony_Component _barony;
        public Barony_BuildingData Buildings;
        public Barony_PopulationData Population;
        public Barony_ProsperityData Prosperity;

        const int c_maxBaronySize = 5;
        
        public Barony_Component Barony => _barony ??= Barony_Manager.GetBarony_Component(ID);

        Dictionary<ulong, Building_Component> _allJobSitesInBarony;

        public Dictionary<ulong, Building_Component> AllJobSitesInBarony
        {
            get
            {
                if (_allJobSitesInBarony is not null && _allJobSitesInBarony.Count != 0) return _allJobSitesInBarony;

                return Barony.GetAllBuildingsInBarony();
            }
        }

        public Barony_Data(ulong id, string name, string description, ulong factionID, ulong regionID,
            List<ulong> allBuildingIDs, Barony_PopulationData population, Barony_ProsperityData baronyProsperityData = null)
        {
            ID = id;
            Name = name;
            Description = description;
            FactionID = factionID;
            RegionID = regionID;
            _allBuildingIDs = allBuildingIDs;
            Population = population;

            Prosperity = new Barony_ProsperityData(baronyProsperityData);
        }

        public void InitialiseBaronyData()
        {
            _barony = Barony_Manager.GetBarony_Component(ID);

            if (_barony is not null) return;

            Debug.LogWarning($"Barony with ID {ID} not found in Barony_SO.");
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
                allStringData: AllJobSitesInBarony.ToDictionary(
                    building => building.Key.ToString(),
                    building => building.Value.name));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Barony ID", $"{ID}" },
                { "Barony Name", Name },
                { "Barony Faction ID", $"{FactionID}" },
                { "Region ID", $"{RegionID}" },
                { "Barony Description", Description }
            };
        }
    }


    [CustomPropertyDrawer(typeof(Barony_Data))]
    public class BaronyData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var BaronyName = property.FindPropertyRelative("BaronyName");
            label.text = !string.IsNullOrEmpty(BaronyName?.stringValue) ? BaronyName?.stringValue : "Unnamed Barony";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}