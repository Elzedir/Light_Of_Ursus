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
        public Data<Faction_Data>                        GetFaction_Data(ulong      factionID) => GetData(factionID);
        public Dictionary<ulong, Faction_Component> Faction_Components => _getSceneComponents();

        public Faction_Component GetFaction_Component(ulong factionID)
        {
            if (Faction_Components.TryGetValue(factionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Faction with ID {factionID} not found in Faction_SO.");
            return null;
        }

        public void UpdateFaction(ulong factionID, Faction_Data faction_Data) => UpdateData(factionID, faction_Data);
        public void UpdateAllFactions(Dictionary<ulong, Faction_Data> allFactions) => UpdateAllData(allFactions);

        protected override Dictionary<ulong, Data<Faction_Data>> _getDefaultData() => 
            _convertDictionaryToData(Faction_List.DefaultFactions);

        protected override Dictionary<ulong, Data<Faction_Data>> _getSavedData()
        {
            Dictionary<ulong, Faction_Data> savedData = new();
            
            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedFactionData.AllFactionData
                    .ToDictionary(faction => faction.FactionID, faction => faction);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedFactionData == null
                            ? $"LoadData Error: SavedFactionData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedFactionData.AllFactionData == null
                                ? $"LoadData Error: AllFactionData is null in SavedFactionData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedFactionData.AllFactionData.Any()
                                    ? $"LoadData Warning: AllFactionData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<ulong, Data<Faction_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FactionData));

        protected override Data<Faction_Data> _convertToData(Faction_Data data)
        {
            return new Data<Faction_Data>(
                dataID: data.FactionID,
                data_Object: data,
                dataTitle: $"{data.FactionID}: {data.FactionName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedFactionData = new SavedFactionData(Factions.Select(x => x.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Data_SOEditor<Faction_Data>
    {
        public override Data_SO<Faction_Data> SO => _so ??= (Faction_SO)target;
    }
}