using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Faction
{
    [CreateAssetMenu(fileName = "Faction_SO", menuName = "SOList/Faction_SO")]
    [Serializable]
    public class Faction_SO : Data_Component_SO<Faction_Data, Faction_Component>
    {
        public Data<Faction_Data>[]                      Factions                             => Data;
        public Data<Faction_Data>                        GetFaction_Data(uint      factionID) => GetData(factionID);
        Dictionary<uint, Faction_Component>     _faction_Components;
        public Dictionary<uint, Faction_Component> Faction_Components => _faction_Components ??= _getExistingFaction_Components();
        
        Dictionary<uint, Faction_Component> _getExistingFaction_Components()
        {
            return FindObjectsByType<Faction_Component>(FindObjectsSortMode.None)
                .Where(faction => Regex.IsMatch(faction.name, @"\d+"))
                .ToDictionary(
                    faction => uint.Parse(new string(faction.name.Where(char.IsDigit).ToArray())),
                    faction => faction
                );
        }

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

        protected override Dictionary<uint, Data<Faction_Data>> _getDefaultData() => 
            _convertDictionaryToData(Faction_List.DefaultFactions);

        protected override Dictionary<uint, Data<Faction_Data>> _getSavedData()
        {
            Dictionary<uint, Faction_Data> savedData = new();
            
            try
            {
                savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedFactionData.AllFactionData
                    .ToDictionary(faction => faction.FactionID, faction => faction);
            }
            catch (Exception ex)
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;
                
                if (saveData == null)
                {
                    Debug.LogWarning("LoadData Error: CurrentSaveData is null.");
                }
                else if (saveData.SavedFactionData == null)
                {
                    Debug.LogWarning($"LoadData Error: SavedFactionData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (saveData.SavedFactionData.AllFactionData == null)
                {
                    Debug.LogWarning($"LoadData Error: AllFactionData is null in SavedFactionData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (!saveData.SavedFactionData.AllFactionData.Any())
                {
                    Debug.LogWarning($"LoadData Warning: AllFactionData is empty (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                
                Debug.LogError($"LoadData Exception: {ex.Message}\n{ex.StackTrace}");
            }

            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<uint, Data<Faction_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FactionData));

        static uint _lastUnusedFactionID = 1;

        public uint GetUnusedFactionID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedFactionID))
            {
                _lastUnusedFactionID++;
            }

            return _lastUnusedFactionID;
        }

        protected override Data<Faction_Data> _convertToData(Faction_Data data)
        {
            return new Data<Faction_Data>(
                dataID: data.FactionID,
                data_Object: data,
                dataTitle: $"{data.FactionID}: {data.FactionName}",
                getData_Display: data.GetData_Display);
        }
        
        public override void SaveData(SaveData saveData) =>
            saveData.SavedFactionData = new SavedFactionData(Factions.Select(x => x.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Data_SOEditor<Faction_Data>
    {
        public override Data_SO<Faction_Data> SO => _so ??= (Faction_SO)target;
    }
}