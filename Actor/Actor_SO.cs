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
        Dictionary<uint, Actor_Component>       _actor_Components;
        public Dictionary<uint, Actor_Component> Actor_Components => _actor_Components ??= _getExistingActor_Components();

        Dictionary<uint, Actor_Component> _getExistingActor_Components()
        {
            return FindObjectsByType<Actor_Component>(FindObjectsSortMode.None)
                                 .Where(actor => Regex.IsMatch(actor.name, @"\d+"))
                                 .ToDictionary(
                                     actor_Component => uint.Parse(new string(actor_Component.name.Where(char.IsDigit).ToArray())),
                                     actor_Component => actor_Component
                                 );
        }
        
        public Actor_Component GetActor_Component(uint actorID)
        {
            if (actorID == 0)
            {
                Debug.LogError("ActorID cannot be 0.");
                return null;
            }
            
            if (Actor_Components.TryGetValue(actorID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Actor_Component with ID {actorID} not found in Actor_Components.");
            return null;
        }

        public override uint GetDataObjectID(int id) => Actors[id].DataObject.ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => 
            UpdateDataObject(actorID, actor_Data);
        
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllDataObjects(allActors);

        public override void PopulateSceneData()
        {
            var physicalActors = _getExistingActor_Components();
            
            foreach (var actor in Actors)
            {
                if (actor?.DataObject is null || actor.DataObjectID == 0) continue;

                Debug.LogError(actor.DataObjectID);
                
                if (physicalActors.TryGetValue(actor.DataObject.ActorID, out var physicalActor))
                {
                    physicalActor.SetActorData(actor.DataObject);
                    Actor_Components[actor.DataObject.ActorID] = physicalActor;
                    physicalActors.Remove(actor.DataObject.ActorID);
                    continue;
                }
                
                Debug.LogWarning($"Actor with ID {actor.DataObject.ActorID} not found in the scene.");
            }

            foreach (var actor in physicalActors)
            {
                UpdateActor(actor.Key, actor.Value.ActorData);
            }
        }

        protected override Dictionary<uint, Object_Data<Actor_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            if (_defaultDataObjects is null || !Application.isPlaying || initialisation)
                return _defaultDataObjects ??= _convertDictionaryToDataObject(Actor_List.DefaultActors);
            
            if (Actors is null || Actors.Length == 0)
            {
                Debug.LogWarning("Actors is null or empty.");
                return _defaultDataObjects;
            }
            
            foreach (var actor in Actors)
            {
                if (actor?.DataObject is null || actor.DataObjectID == 0) continue;
                
                if (!_defaultDataObjects.ContainsKey(actor.DataObject.ActorID))
                {
                    Debug.LogWarning($"Actor with ID {actor.DataObject.ActorID} not found in DefaultActors.");
                    continue;
                }
                
                _defaultDataObjects[actor.DataObject.ActorID] = actor;
            }

            return _defaultDataObjects;
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
        
        protected override Object_Data<Actor_Data> _convertToDataObject(Actor_Data dataObject)
        {
            return new Object_Data<Actor_Data>(
                dataObjectID: dataObject.ActorID, 
                dataObject: dataObject,
                dataObjectTitle: $"{dataObject.ActorID}: {dataObject.ActorName}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}