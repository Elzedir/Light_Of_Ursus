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
        public Data_Object<Faction_Data>[]                      Factions                             => DataObjects;
        public Data_Object<Faction_Data>                        GetFaction_Data(uint      factionID) => GetDataObject_Master(factionID);
        public Dictionary<uint, Faction_Component> FactionComponents = new();

        public Faction_Component GetFaction_Component(uint factionID)
        {
            if (FactionComponents.TryGetValue(factionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Faction with ID {factionID} not found in Faction_SO.");
            return null;
        }

        public override uint GetDataObjectID(int id) => Factions[id].DataObject.FactionID;

        public void UpdateFaction(uint factionID, Faction_Data faction_Data) => UpdateDataObject(factionID, faction_Data);
        public void UpdateAllFactions(Dictionary<uint, Faction_Data> allFactions) => UpdateAllDataObjects(allFactions);

        public void PopulateSceneFactions()
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
                    existingFaction.FactionData = faction.DataObject;
                    FactionComponents[faction.DataObject.FactionID] = existingFaction;
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

        protected override Dictionary<uint, Data_Object<Faction_Data>> _populateDefaultDataObjects()
        {
            var defaultFactions = new Dictionary<uint, Faction_Data>();

            foreach (var defaultFaction in Faction_List.DefaultFactions)
            {
                defaultFactions.Add(defaultFaction.Key, new Faction_Data(defaultFaction.Value));
            }

            return _convertDictionaryToDataObject(defaultFactions);
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
        
        Dictionary<uint, Data_Object<Faction_Data>> _defaultFactions => DefaultDataObjects;

        protected override Data_Object<Faction_Data> _convertToDataObject(Faction_Data data)
        {
            return new Data_Object<Faction_Data>(
                dataObjectID: data.FactionID,
                dataObject: data,
                dataObjectTitle: $"{data.FactionID}: {data.FactionName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Data_SOEditor<Faction_Data>
    {
        public override Data_SO<Faction_Data> SO => _so ??= (Faction_SO)target;
    }
}