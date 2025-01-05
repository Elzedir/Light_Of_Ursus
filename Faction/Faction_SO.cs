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
        public Data<Faction_Data>[]                      Factions                             => Data;
        public Data<Faction_Data>                        GetFaction_Data(uint      factionID) => GetData(factionID);
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

        public override uint GetDataID(int id) => Factions[id].Data_Object.FactionID;

        public void UpdateFaction(uint factionID, Faction_Data faction_Data) => UpdateData(factionID, faction_Data);
        public void UpdateAllFactions(Dictionary<uint, Faction_Data> allFactions) => UpdateAllData(allFactions);

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
                if (faction?.Data_Object is null) continue;
                
                if (existingFactions.TryGetValue(faction.Data_Object.FactionID, out var existingFaction))
                {
                    Faction_Components[faction.Data_Object.FactionID] = existingFaction;
                    existingFaction.SetFactionData(faction.Data_Object);
                    existingFactions.Remove(faction.Data_Object.FactionID);
                    continue;
                }
                
                Debug.LogWarning($"Faction with ID {faction.Data_Object.FactionID} not found in the scene.");
            }
            
            foreach (var faction in existingFactions)
            {
                if (DataIndexLookup.ContainsKey(faction.Key))
                {
                    Debug.LogWarning($"Faction with ID {faction.Key} wasn't removed from existingFactions.");
                    continue;
                }
                
                Debug.LogWarning($"Faction with ID {faction.Key} does not have DataObject in Faction_SO.");
            }
        }

        protected override Dictionary<uint, Data<Faction_Data>> _getDefaultData(bool initialisation = false)
        {
            if (_defaultData is null || !Application.isPlaying || initialisation)
                return _defaultData ??= _convertDictionaryToData(Faction_List.DefaultFactions);

            if (Factions is null || Factions.Length == 0)
            {
                Debug.LogWarning("No Factions Found in Faction_SO.");
                return _defaultData;
            }

            foreach (var faction in Factions)
            {
                if (faction?.Data_Object is null) continue;
                
                if (!_defaultData.ContainsKey(faction.Data_Object.FactionID))
                {
                    Debug.LogError($"Faction with ID {faction.Data_Object.FactionID} not found in DefaultFactions.");
                    continue;
                }
                
                _defaultData[faction.Data_Object.FactionID] = faction;
            }
            
            return _defaultData;
        }

        static uint _lastUnusedFactionID = 1;

        public uint GetUnusedFactionID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedFactionID))
            {
                _lastUnusedFactionID++;
            }

            return _lastUnusedFactionID;
        }
        
        Dictionary<uint, Data<Faction_Data>> _defaultFactions => DefaultData;

        protected override Data<Faction_Data> _convertToData(Faction_Data data)
        {
            return new Data<Faction_Data>(
                dataID: data.FactionID,
                data_Object: data,
                dataTitle: $"{data.FactionID}: {data.FactionName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Data_SOEditor<Faction_Data>
    {
        public override Data_SO<Faction_Data> SO => _so ??= (Faction_SO)target;
    }
}