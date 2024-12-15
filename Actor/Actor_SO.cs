using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Data_SO<Actor_Data>
    {
        public Data_Object<Actor_Data>[]                        Actors                           => DataObjects;
        public Data_Object<Actor_Data>                          GetActor_Data(uint      actorID) => GetDataObject_Master(actorID);
        public Dictionary<uint, Actor_Component> ActorComponents = new();

        public Actor_Component GetActor_Component(uint actorID)
        {
            if (ActorComponents.TryGetValue(actorID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Actor_Component with ID {actorID} not found in Actor_Components.");
            return null;
        }

        public override uint GetDataObjectID(int id) => Actors[id].DataObject.ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => UpdateDataObject(actorID, actor_Data);
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllDataObjects(allActors);

        public void PopulateSceneActors()
        {
            if (_defaultActors.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
            
            var allActorComponents = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None);
            var allActorData =
                allActorComponents.ToDictionary(actor => actor.ActorID, actor => actor.ActorData);

            foreach (var actor in allActorData)
            {
                if (DataObjectIndexLookup.ContainsKey(actor.Key))
                {
                    return;
                }
                
                Debug.LogWarning($"Actor with ID {actor.Key} not found in Actor_SO.");
            }
        }

        protected override Dictionary<uint, Data_Object<Actor_Data>> _populateDefaultDataObjects()
        {
            var defaultActors = new Dictionary<uint, Actor_Data>();

            foreach (var defaultActor in Actor_List.DefaultActors)
            {
                defaultActors.Add(defaultActor.Key, defaultActor.Value);
            }

            return _convertDictionaryToDataObject(defaultActors);
        }

        static uint _lastUnusedActorID = 1;

        public uint GetUnusedActorID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }
        
        Dictionary<uint, Data_Object<Actor_Data>> _defaultActors => DefaultDataObjects;
        
        protected override Data_Object<Actor_Data> _convertToDataObject(Actor_Data data)
        {
            return new Data_Object<Actor_Data>(
                dataObjectID: data.ActorID, 
                dataObject: data,
                dataObjectTitle: $"{data.ActorID}: {data.ActorName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}