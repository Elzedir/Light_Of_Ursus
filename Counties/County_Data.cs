using System;
using System.Collections.Generic;
using System.Linq;
using Cities;
using Faction;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Counties
{
    [Serializable]
    public class County_Data : Data_Class
    {
        public ulong   ID;
        public string Name;
        public int    FactionID;
        public string Description;

        County_Component        _county;

        public           FactionName     Faction;
        [SerializeField] List<ulong>      _allCityIDs;
        Dictionary<ulong, Barony_Component> _allCitiesInRegion;
        
        public County_Component County => _county ??= County_Manager.GetRegion_Component(ID);
        
        public Dictionary<ulong, Barony_Component> AllCitiesInRegion
        {
            get
            {
                if (_allCitiesInRegion is not null && _allCitiesInRegion.Count != 0) return _allCitiesInRegion;
                
                return County.GetAllCitiesInRegion().ToDictionary(city => city.ID);
            }
        }

        public ulong GetNearestCityInRegion(Vector3 position)
        {
            return AllCitiesInRegion
                   .OrderBy(city => Vector3.Distance(position, city.Value.transform.position))
                   .FirstOrDefault().Key;
        }

        public County_Data(ulong       id,   string name, string description, int factionID,
                           List<ulong> allCityIDs, Barony_ProsperityData baronyProsperityData = null)
        {
            ID          = id;
            Name        = name;
            Description = description;
            FactionID   = factionID;
            _allCityIDs       = allCityIDs;
        }
        
        public void InitialiseRegionData()
        {
            _county = County_Manager.GetRegion_Component(ID);

            if (_county is not null) return;
            
            Debug.LogWarning($"Region with ID {ID} not found in Region_SO.");
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Region ID", $"{ID}" },
                { "Region Name", Name },
                { "Region Faction ID", $"{FactionID}" },
                { "Region Description", Description },
                { "Faction", $"{Faction}" },
                { "All City IDs", string.Join(", ", _allCityIDs) }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Region Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Region Cities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllCitiesInRegion.ToDictionary(
                    city => city.Key.ToString(),
                    city => city.Value.name));

            return DataToDisplay;
        }
    }

    [CustomPropertyDrawer(typeof(County_Data))]
    public class RegionData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var regionName = property.FindPropertyRelative("RegionName");
            label.text = !string.IsNullOrEmpty(regionName.stringValue) ? regionName.stringValue : "Unnamed Region";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }
}