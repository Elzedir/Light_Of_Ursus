using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Settlements
{
    [CreateAssetMenu(fileName = "Settlement_SO", menuName = "SOList/Settlement_SO")]
    [Serializable]
    public class Settlement_SO : Data_Component_SO<Settlement_Data, Settlement_Component>
    {
        public Data<Settlement_Data>[]                         Cities                         => Data;
        public Data<Settlement_Data>                           GetSettlement_Data(ulong      settlementID) => GetData(settlementID);
        public Dictionary<ulong, Settlement_Component> Settlement_Components => _getSceneComponents();

        public Settlement_Component GetSettlement_Component(ulong settlementID)
        {
            if (settlementID == 0)
            {
                Debug.LogError("SettlementID cannot be 0.");
                return null;
            }
            
            if (Settlement_Components.TryGetValue(settlementID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Settlement with ID {settlementID} not found in Settlement_SO.");
            return null;
        }
        
        public void UpdateSettlement(ulong settlementID, Settlement_Data settlement_Data) => UpdateData(settlementID, settlement_Data);
        public void UpdateAllCities(Dictionary<ulong, Settlement_Data> allCities) => UpdateAllData(allCities);

        protected override Dictionary<ulong, Data<Settlement_Data>> _getDefaultData() 
            => _convertDictionaryToData(Settlement_List.S_PreExistingSettlements);
            

        protected override Dictionary<ulong, Data<Settlement_Data>> _getSavedData()
        {
            Dictionary<ulong, Settlement_Data> savedData = new();
                
            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedSettlementData.AllSettlementData
                    .ToDictionary(settlement => settlement.ID, settlement => settlement);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedSettlementData == null
                            ? $"LoadData Error: SavedSettlementData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedSettlementData.AllSettlementData == null
                                ? $"LoadData Error: AllSettlementData is null in SavedSettlementData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedSettlementData.AllSettlementData.Any()
                                    ? $"LoadData Warning: AllSettlementData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }
            
            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<ulong, Data<Settlement_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Settlement_Data));
        
        protected override Data<Settlement_Data> _convertToData(Settlement_Data data)
        {
            return new Data<Settlement_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Type}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        public override void SaveData(Save_Data saveData) =>
            saveData.SavedSettlementData = new SavedSettlementData(Cities.Select(settlement => settlement.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Settlement_SO))]
    public class Settlement_SOEditor : Data_SOEditor<Settlement_Data>
    {
        public override Data_SO<Settlement_Data> SO => _so ??= (Settlement_SO)target;
    }
}