using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Data_SO<Actor_Data>
    {
        public Object_Data<Actor_Data>[]                        Actors                           => Objects_Data;
        public Object_Data<Actor_Data>                          GetActor_Data(uint      actorID) => GetObject_Data(actorID);
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

        public void UpdateActor(uint actorID, Actor_Data actor_Data, Actor_Component actor_Component)
        {
            UpdateDataObject(actorID, actor_Data);
            ActorComponents[actorID] = actor_Component;
        }
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
            
            var existingActors = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None)
                                 .Where(actor => Regex.IsMatch(actor.name, @"\d+"))
                                 .ToDictionary(
                                     actor_Component => uint.Parse(new string(actor_Component.name.Where(char.IsDigit).ToArray())),
                                     actor_Component => actor_Component
                                 );
            
            foreach (var actor in Actors)
            {
                if (actor?.DataObject is null) continue;
                
                if (existingActors.TryGetValue(actor.DataObject.ActorID, out var existingActor))
                {
                    ActorComponents[actor.DataObject.ActorID] = existingActor;
                    existingActor.SetActorData(actor.DataObject);
                    existingActors.Remove(actor.DataObject.ActorID);
                    continue;
                }
                
                Debug.LogWarning($"Actor with ID {actor.DataObject.ActorID} not found in the scene.");
            }
            
            foreach (var actor in existingActors)
            {
                if (DataObjectIndexLookup.ContainsKey(actor.Key))
                {
                    Debug.LogWarning($"Actor with ID {actor.Key} wasn't removed from existingActors.");
                    continue;
                }
                
                Debug.LogWarning($"Actor with ID {actor.Key} does not have DataObject in Actor_SO.");
            }
        }

        protected override Dictionary<uint, Object_Data<Actor_Data>> _populateDefaultDataObjects()
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
        
        Dictionary<uint, Object_Data<Actor_Data>> _defaultActors => DefaultDataObjects;
        
        protected override Object_Data<Actor_Data> _convertToDataObject(Actor_Data data)
        {
            return new Object_Data<Actor_Data>(
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