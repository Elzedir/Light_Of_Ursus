using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Base_SO_Test<Actor_Data>
    {
        public Base_Object<Actor_Data>[]                        Actors                           => BaseObjects;
        public Base_Object<Actor_Data>                          GetActor_Data(uint      actorID) => GetBaseObject_Master(actorID);
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

        public override uint GetBaseObjectID(int id) => Actors[id].DataObject.ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => UpdateBaseObject(actorID, actor_Data);
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllBaseObjects(allActors);

        public void PopulateSceneActors()
        {
            // var allActorComponents = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None);
            // var allActorData =
            //     allActorComponents.ToDictionary(actor => actor.ActorID, actor => actor.ActorData);
            //
            // // Make sure default actors can't be overridden by rewriting save file, or put in a check if it has happened.
            //
            // UpdateAllActors(allActorData);
            
            if (_defaultActors.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }

        protected override Dictionary<uint, Base_Object<Actor_Data>> _populateDefaultBaseObjects()
        {
            var defaultFactions = new Dictionary<uint, Actor_Data>();

            foreach (var defaultActor in Actor_List.DefaultActors)
            {
                defaultFactions.Add(defaultActor.Key, new Actor_Data(defaultActor.Value));
            }

            return _convertDictionaryToBaseObject(defaultFactions);
        }
        
        protected override Dictionary<uint, Base_Object<Actor_Data>> _convertDictionaryToBaseObject(
            Dictionary<uint, Actor_Data> actor_Datas)
        {
            return actor_Datas.ToDictionary(actor_Data => actor_Data.Key,
                actor_Data => new Base_Object<Actor_Data>(actor_Data.Key,
                    GetDataToDisplay(actor_Data.Value), actor_Data.Value,
                    $"{actor_Data.Key}: {actor_Data.Value.ActorName}"));
        }

        protected override Base_Object<Actor_Data> _convertToBaseObject(Actor_Data actor_Data)
        {
            return new Base_Object<Actor_Data>(actor_Data.ActorID, GetDataToDisplay(actor_Data),
                actor_Data,
                $"{actor_Data.ActorName}{actor_Data.ActorName}");
        }

        static uint _lastUnusedActorID = 1;

        public uint GetUnusedActorID()
        {
            while (BaseObjectIndexLookup.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }
        
        Dictionary<uint, Base_Object<Actor_Data>> _defaultActors => DefaultBaseObjects;

        enum ActorDataCategories
        {
            FullIdentification,
            GameObjectProperties,
            SpeciesAndPersonality,
            StatsAndAbilities,
            CareerAndJobs,
            Inventory,
            Equipment
        }

        public override Dictionary<uint, DataToDisplay> GetDataToDisplay(Actor_Data actor_Data)
        {
            try
            {
                return new Dictionary<uint, DataToDisplay>
                {
                    {
                        (uint)ActorDataCategories.FullIdentification, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Actor ID: {actor_Data.ActorID}",
                                $"Actor Name: {actor_Data.ActorName.GetName()}",
                                $"ActorFaction: {actor_Data.ActorFactionID}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {
                        (uint)ActorDataCategories.GameObjectProperties, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Actor Last Saved Position: {actor_Data.GameObjectData.LastSavedActorPosition}",
                                $"Actor Last Saved Scale: {actor_Data.GameObjectData.LastSavedActorScale}",
                                $"Actor Last Saved Rotation: {actor_Data.GameObjectData.LastSavedActorRotation.eulerAngles}",
                                $"Actor Mesh: {actor_Data.GameObjectData.ActorMesh}",
                                $"Actor Material: {actor_Data.GameObjectData.ActorMaterial}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {

                        (uint)ActorDataCategories.SpeciesAndPersonality, new DataToDisplay(
                            data: new List<string>
                            {
                                $"ActorSpecies: {actor_Data.SpeciesAndPersonality.ActorSpecies}",
                                $"ActorPersonality: {actor_Data.SpeciesAndPersonality.ActorPersonality?.PersonalityTitle}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {
                        (uint)ActorDataCategories.StatsAndAbilities, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Actor ID: {actor_Data.StatsAndAbilities.ActorStats.ActorSpecial.GetStatsToString()}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {
                        (uint)ActorDataCategories.CareerAndJobs, new DataToDisplay(
                            data: new List<string>
                            {
                                $"JobsActive: {actor_Data.CareerData.JobsActive}",
                                $"JobSiteID: {actor_Data.CareerData.JobsiteID}",
                                $"JobName: {actor_Data.CareerData.CurrentJob?.JobName}",
                                $"Employee Position: {actor_Data.CareerData.EmployeePositionName}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {
                        (uint)ActorDataCategories.Inventory, new DataToDisplay(
                            data: actor_Data.InventoryData.AllInventoryItems.Select(item =>
                                $"{item.Value.ItemName}: {item.Value.ItemAmount}").ToList(),
                            dataDisplayType: DataDisplayType.SelectableList)
                    },
                    {
                        (uint)ActorDataCategories.Equipment, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Has Equipment Component: {actor_Data.EquipmentData != null}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Debug.LogWarning(actor_Data);
                
                Debug.LogWarning(actor_Data.FullIdentification);
                Debug.LogWarning(actor_Data.FullIdentification.ActorName);
                
                Debug.LogWarning(actor_Data.GameObjectData);
                Debug.LogWarning(actor_Data.GameObjectData.ActorMesh);
                Debug.LogWarning(actor_Data.GameObjectData.ActorMaterial);
                
                Debug.LogWarning(actor_Data.SpeciesAndPersonality);
                Debug.LogWarning(actor_Data.SpeciesAndPersonality.ActorSpecies);
                Debug.LogWarning(actor_Data.SpeciesAndPersonality.ActorPersonality);
                
                Debug.LogWarning(actor_Data.StatsAndAbilities);
                Debug.LogWarning(actor_Data.StatsAndAbilities.ActorStats);
                Debug.LogWarning(actor_Data.StatsAndAbilities.ActorStats.ActorSpecial);
                
                Debug.LogWarning(actor_Data.CareerData);
                Debug.LogWarning(actor_Data.CareerData.CurrentJob);
                
                Debug.LogWarning(actor_Data.InventoryData);
                Debug.LogWarning(actor_Data.InventoryData.AllInventoryItems);
                
                Debug.LogWarning(actor_Data.EquipmentData);
                
                throw;
            }
        }
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Base_SOEditor<Actor_Data>
    {
        public override Base_SO_Test<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}