using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actors
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Data_Component_SO<Actor_Data, Actor_Component>
    {
        public Data<Actor_Data>[] Actors                      => Data;
        public Dictionary<ulong, Actor_Component> Actor_Components => _getSceneComponents();
        
        public Data<Actor_Data>   GetActor_Data(ulong actorID) => GetData(actorID);
        
        public Actor_Component GetActor_Component(ulong actorID)
        {
            if (Actor_Components.TryGetValue(actorID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Actor with ID {actorID} not found in Actor_SO.");
            return null;
        }

        public void UpdateActor(ulong actorID, Actor_Data actor_Data) => 
            UpdateData(actorID, actor_Data);
        
        public void UpdateAllActors(Dictionary<ulong, Actor_Data> allActors) => UpdateAllData(allActors);

        protected override Dictionary<ulong, Data<Actor_Data>> _getDefaultData() =>
            _convertDictionaryToData(Actor_List.DefaultActors);

        protected override Dictionary<ulong, Data<Actor_Data>> _getSavedData()
        {
            Dictionary<ulong, Actor_Data> savedData = new();
                
            try
            {
                 savedData = DataPersistence_Manager.CurrentSaveData.SavedActorData.AllActorData
                     .ToDictionary(actor => actor.ActorID, actor => actor);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedActorData == null
                            ? $"LoadData Error: SavedActorData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedActorData.AllActorData == null
                                ? $"LoadData Error: AllActorData is null in SavedActorData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedActorData.AllActorData.Any()
                                    ? $"LoadData Warning: AllActorData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<ulong, Data<Actor_Data>> _getSceneData() =>
            _convertDictionaryToData(Actor_Components.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ActorData));
        
        protected override Data<Actor_Data> _convertToData(Actor_Data data)
        {
            return new Data<Actor_Data>(
                dataID: data.ActorID, 
                data_Object: data,
                dataTitle: $"{data.ActorID}: {data.ActorName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedActorData = new SavedActorData(Actors.Select(actor => actor.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}