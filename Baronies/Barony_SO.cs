using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Baronies
{
    [CreateAssetMenu(fileName = "Barony_SO", menuName = "SOList/Barony_SO")]
    [Serializable]
    public class Barony_SO : Data_Component_SO<Barony_Data, Barony_Component>
    {
        public Data<Barony_Data>[]                         Cities                         => Data;
        public Data<Barony_Data>                           GetBarony_Data(ulong      baronyID) => GetData(baronyID);
        public Dictionary<ulong, Barony_Component> Barony_Components => _getSceneComponents();

        public Barony_Component GetBarony_Component(ulong baronyID)
        {
            if (baronyID == 0)
            {
                Debug.LogError("BaronyID cannot be 0.");
                return null;
            }
            
            if (Barony_Components.TryGetValue(baronyID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Barony with ID {baronyID} not found in Barony_SO.");
            return null;
        }
        
        public void UpdateBarony(ulong baronyID, Barony_Data barony_Data) => UpdateData(baronyID, barony_Data);
        public void UpdateAllCities(Dictionary<ulong, Barony_Data> allCities) => UpdateAllData(allCities);

        protected override Dictionary<ulong, Data<Barony_Data>> _getDefaultData() => 
            _convertDictionaryToData(Barony_List.S_PreExistingBaronies);

        protected override Dictionary<ulong, Data<Barony_Data>> _getSavedData()
        {
            Dictionary<ulong, Barony_Data> savedData = new();
                
            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedBaronyData.AllBaronyData
                    .ToDictionary(barony => barony.ID, barony => barony);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedBaronyData == null
                            ? $"LoadData Error: SavedBaronyData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedBaronyData.AllBaronyData == null
                                ? $"LoadData Error: AllBaronyData is null in SavedBaronyData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedBaronyData.AllBaronyData.Any()
                                    ? $"LoadData Warning: AllBaronyData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }
            
            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<ulong, Data<Barony_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Barony_Data));
        
        protected override Data<Barony_Data> _convertToData(Barony_Data data)
        {
            return new Data<Barony_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Type}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        public override void SaveData(Save_Data saveData) =>
            saveData.SavedBaronyData = new SavedBaronyData(Cities.Select(barony => barony.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Barony_SO))]
    public class Barony_SOEditor : Data_SOEditor<Barony_Data>
    {
        public override Data_SO<Barony_Data> SO => _so ??= (Barony_SO)target;
    }
}