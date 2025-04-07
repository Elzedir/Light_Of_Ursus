using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Buildings
{
    [CreateAssetMenu(fileName = "Building_SO", menuName = "SOList/Building_SO")]
    [Serializable]
    public class Building_SO : Data_Component_SO<Building_Data, Building_Component>
    {
        public Data<Building_Data>[] Buildings => Data;
        public Data<Building_Data>        GetBuilding_Data(ulong      building) => GetData(building);
        public Dictionary<ulong, Building_Component> Building_Components => _getSceneComponents();

        public Building_Component GetBuilding_Component(ulong buildingID)
        {
            if (buildingID == 0)
            {
                Debug.LogError("BuildingID cannot be 0.");
                return null;
            }
            
            if (Building_Components.TryGetValue(buildingID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Building with ID {buildingID} not found in Building_SO.");
            return null;
        }

        public void UpdateBuilding(ulong buildingID, Building_Data building_Component) => UpdateData(buildingID, building_Component);
        public void UpdateAllBuildings(Dictionary<ulong, Building_Data> allBuildings) => UpdateAllData(allBuildings);

        protected override Dictionary<ulong, Data<Building_Data>> _getDefaultData() =>
            _convertDictionaryToData(Building_PreExisting.S_DefaultBuildings);

        protected override Dictionary<ulong, Data<Building_Data>> _getSavedData()
        {
            Dictionary<ulong, Building_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedBuildingData.AllBuildingData
                    .ToDictionary(building => building.ID, building => building);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedBuildingData == null
                            ? $"LoadData Error: SavedBuildingData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedBuildingData.AllBuildingData == null
                                ? $"LoadData Error: AllBuildingData is null in SavedBuildingData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedBuildingData.AllBuildingData.Any()
                                    ? $"LoadData Warning: AllBuildingData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<ulong, Data<Building_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Building_Data));
        
        protected override Data<Building_Data> _convertToData(Building_Data data)
        {
            return new Data<Building_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Name}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedBuildingData = new SavedBuildingData(Buildings.Select(building => building.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Building_SO))]
    public class JobSites_SOEditor : Data_SOEditor<Building_Data>
    {
        public override Data_SO<Building_Data> SO => _so ??= (Building_SO)target;
    }
}