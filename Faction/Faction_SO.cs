using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Faction
{
    [CreateAssetMenu(fileName = "Faction_SO", menuName = "SOList/Faction_SO")]
    [Serializable]
    public class Faction_SO : Data_SO<Faction_Data>
    {
        public Object_Data<Faction_Data>[]                      Factions                             => Objects_Data;
        public Object_Data<Faction_Data>                        GetFaction_Data(uint      factionID) => GetObject_Data(factionID);
        public Dictionary<uint, Faction_Component> Faction_Components = new();

        public Faction_Component GetFaction_Component(uint factionID)
        {
            if (Faction_Components.TryGetValue(factionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Faction with ID {factionID} not found in Faction_SO.");
            return null;
        }

        public override uint GetDataObjectID(int id) => Factions[id].DataObject.FactionID;

        public void UpdateFaction(uint factionID, Faction_Data faction_Data) => UpdateDataObject(factionID, faction_Data);
        public void UpdateAllFactions(Dictionary<uint, Faction_Data> allFactions) => UpdateAllDataObjects(allFactions);

        public override void PopulateSceneData()
        {
            if (_defaultFactions.Count == 0)
            {
                Debug.Log("No Default Factions Found");
            }
            
            var existingFactions = FindObjectsByType<Faction_Component>(FindObjectsSortMode.None)
                                     .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                     .ToDictionary(
                                         station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                         station => station
                                     );
            
            foreach (var faction in Factions)
            {
                if (faction?.DataObject is null) continue;
                
                if (existingFactions.TryGetValue(faction.DataObject.FactionID, out var existingFaction))
                {
                    Faction_Components[faction.DataObject.FactionID] = existingFaction;
                    existingFaction.SetFactionData(faction.DataObject);
                    existingFactions.Remove(faction.DataObject.FactionID);
                    continue;
                }
                
                Debug.LogWarning($"Faction with ID {faction.DataObject.FactionID} not found in the scene.");
            }
            
            foreach (var faction in existingFactions)
            {
                if (DataObjectIndexLookup.ContainsKey(faction.Key))
                {
                    Debug.LogWarning($"Faction with ID {faction.Key} wasn't removed from existingFactions.");
                    continue;
                }
                
                Debug.LogWarning($"Faction with ID {faction.Key} does not have DataObject in Faction_SO.");
            }
        }

        protected override Dictionary<uint, Object_Data<Faction_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            if (_defaultDataObjects is null || !Application.isPlaying || initialisation)
                return _defaultDataObjects ??= _convertDictionaryToDataObject(Faction_List.DefaultFactions);

            if (Factions is null || Factions.Length == 0)
            {
                Debug.LogWarning("No Factions Found in Faction_SO.");
                return _defaultDataObjects;
            }

            foreach (var faction in Factions)
            {
                if (faction?.DataObject is null) continue;
                
                if (!_defaultDataObjects.ContainsKey(faction.DataObject.FactionID))
                {
                    Debug.LogError($"Faction with ID {faction.DataObject.FactionID} not found in DefaultFactions.");
                    continue;
                }
                
                _defaultDataObjects[faction.DataObject.FactionID] = faction;
            }
            
            return _defaultDataObjects;
        }

        static uint _lastUnusedFactionID = 1;

        public uint GetUnusedFactionID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedFactionID))
            {
                _lastUnusedFactionID++;
            }

            return _lastUnusedFactionID;
        }
        
        Dictionary<uint, Object_Data<Faction_Data>> _defaultFactions => DefaultDataObjects;

        protected override Object_Data<Faction_Data> _convertToDataObject(Faction_Data dataObject)
        {
            return new Object_Data<Faction_Data>(
                dataObjectID: dataObject.FactionID,
                dataObject: dataObject,
                dataObjectTitle: $"{dataObject.FactionID}: {dataObject.FactionName}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Data_SOEditor<Faction_Data>
    {
        public override Data_SO<Faction_Data> SO => _so ??= (Faction_SO)target;
    }
}