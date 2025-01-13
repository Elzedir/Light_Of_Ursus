using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Data_Component_SO<Actor_Data, Actor_Component>
    {
        public Data<Actor_Data>[] Actors                      => Data;
        public Data<Actor_Data>   GetActor_Data(uint actorID) => GetData(actorID);
        public Actor_Component GetActor_Component(uint actorID) => Actor_Components[actorID];
        public Dictionary<uint, Actor_Component> Actor_Components => _getSceneComponents();

        public override uint GetDataID(int id) => Actors[id].Data_Object.ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => 
            UpdateData(actorID, actor_Data);
        
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllData(allActors);

        protected override Dictionary<uint, Data<Actor_Data>> _getDefaultData() =>
            _convertDictionaryToData(Actor_List.DefaultActors);

        protected override Dictionary<uint, Data<Actor_Data>> _getSavedData()
        {
            Dictionary<uint, Actor_Data> savedData = new();
                
            try
            {
                 savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedActorData.AllActorData
                     .ToDictionary(actor => actor.ActorID, actor => actor);
            }
            catch (Exception ex)
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;

                if (saveData == null)
                {
                    Debug.LogWarning("LoadData Error: CurrentSaveData is null.");
                }
                else if (saveData.SavedActorData == null)
                {
                    Debug.LogWarning($"LoadData Error: SavedActorData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (saveData.SavedActorData.AllActorData == null)
                {
                    Debug.LogWarning($"LoadData Error: AllActorData is null in SavedActorData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (!saveData.SavedActorData.AllActorData.Any())
                {
                    Debug.LogWarning($"LoadData Warning: AllActorData is empty (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }

                Debug.LogError($"LoadData Exception: {ex.Message}\n{ex.StackTrace}");
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<uint, Data<Actor_Data>> _getSceneData() =>
            _convertDictionaryToData(Actor_Components.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ActorData));
        

        static uint _lastUnusedActorID = 1;

        public uint GetUnusedActorID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }
        
        protected override Data<Actor_Data> _convertToData(Actor_Data data)
        {
            return new Data<Actor_Data>(
                dataID: data.ActorID, 
                data_Object: data,
                dataTitle: $"{data.ActorID}: {data.ActorName}",
                getData_Display: data.GetData_Display);
        }
        
        public override void SaveData(SaveData saveData) =>
            saveData.SavedActorData = new SavedActorData(Actors.Select(actor => actor.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}