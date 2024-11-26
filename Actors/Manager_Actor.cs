using System;
using System.Collections.Generic;
using System.Linq;
using DateAndTime;
using Items;
using Managers;
using Recipes;
using ScriptableObjects;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Actors
{
    public class Manager_Actor : MonoBehaviour, IDataPersistence
    {
        static          AllActors_SO                     _allActors;
        static          Dictionary<uint, ActorData>      _allActorData       = new();
        static          uint                             _lastUnusedActorID  = 1;
        static readonly Dictionary<uint, ActorComponent> _allActorComponents = new();

        public void SaveData(SaveData data)
        {
            _allActorData.Values.ToList().ForEach(actorData => actorData.UpdateActorData());

            data.SavedActorData    = new SavedActorData(_allActorData.Values.ToList());
            _allActors.AllActorData = _allActorData.Values.ToList();
        }

        public void LoadData(SaveData data)
        {
            try
            {
                _allActorData = data.SavedActorData.AllActorData.ToDictionary(x => x.ActorID);
            }
            catch
            {
                _allActorData = new();
                //Debug.Log("No Actor Data found in SaveData.");
            }

            _allActors.AllActorData = _allActorData.Values.ToList();
        }

        public void OnSceneLoaded()
        {
            _allActors = Resources.Load<AllActors_SO>("ScriptableObjects/AllActors_SO");

            Manager_Initialisation.OnInitialiseManagerActor += _initialise;
        }

        static void _initialise()
        {
            foreach (var actor in _findAllActorComponents())
            {
                if (actor.ActorData == null) { Debug.Log($"Actor: {actor.name} does not have ActorData."); continue; }

                if (!_allActorComponents.TryAdd(actor.ActorData.ActorID, actor))
                {
                    if (_allActorComponents[actor.ActorData.ActorID].gameObject == actor.gameObject) continue;
                    Debug.LogError($"ActorID {actor.ActorData.ActorID} and name {actor.name} already exists for actor {_allActorComponents[actor.ActorData.ActorID].name}");
                    actor.ActorData.ActorID = _getRandomActorID();
                }

                if (!_allActorData.ContainsKey(actor.ActorData.ActorID)) Debug.Log($"Actor: {actor.ActorData.ActorID} {actor.name} does not exist in AllActorData.");
            }

            foreach (var actor in _allActorData)
            {
                actor.Value.PrepareForInitialisation();
            }
        }

        static List<ActorComponent> _findAllActorComponents()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                   .OfType<ActorComponent>()
                   .ToList();
        }

        static void _addToAllActorData(ActorData actorData)
        {
            if (_allActorData.TryAdd(actorData.ActorID, actorData)) return;
        
            Debug.Log($"ActorData: {actorData.ActorID} already exists in AllActorData.");
        }

        public static void UpdateAllActorData(ActorData actorData)
        {
            if (!_allActorData.ContainsKey(actorData.ActorID))
            {
                Debug.Log($"ActorData: {actorData.ActorID} does not exist in AllActorData.");
                return;
            }

            _allActorData[actorData.ActorID] = actorData;
        }
        public static void RemoveFromAllActorData(uint actorID)
        {
            if (!_allActorData.ContainsKey(actorID))
            {
                Debug.Log($"ActorData: {actorID} does not exist in AllActorData.");
                return;
            }

            _allActorData.Remove(actorID);
        }

        public static ActorData GetActorData(uint actorID)
        {
            if (_allActorData.TryGetValue(actorID, out var data)) return data;
        
            Debug.Log($"ActorData: {actorID} does not exist in AllActorData.");
            return null;

        }

        public static ActorComponent GetActor(uint actorID, bool generateActorIfNotFound = false)
        {
            if (_allActorComponents.TryGetValue(actorID, out var actor))
            {
                return actor;
            }

            if (generateActorIfNotFound)
            {
                return _allActorComponents[actorID] = _spawnActor(GetActorData(actorID).GameObjectProperties.LastSavedActorPosition, actorID);
            }

            return null;
        }

        public static ActorComponent SpawnNewActor(Vector3 spawnPoint, ActorGenerationParameters actorGenerationParameters)
        {
            ActorComponent actor = _createNewActorGO(spawnPoint).AddComponent<ActorComponent>();
        
            actor.SetActorData(_generateNewActorData(actor, actorGenerationParameters));

            _allActorComponents[actor.ActorData.ActorID] = actor;
        
            actor.Initialise();

            Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

            return actor;
        }

        // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
        static ActorComponent _spawnActor(Vector3 spawnPoint, uint actorID, bool despawnActorIfExists = false)
        {
            if (despawnActorIfExists) _despawnActor(actorID);
            else if (_allActorComponents.TryGetValue(actorID, out var existingActor)) return existingActor;

            var actor = _createNewActorGO(spawnPoint).AddComponent<ActorComponent>();

            actor.SetActorData(GetActorData(actorID));
            actor.Initialise();

            Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

            return actor;
        }

        static void _despawnActor(uint actorID)
        {
            if (!_allActorComponents.TryGetValue(actorID, out var component)) return;
        
            Destroy(component.gameObject);
            _allActorComponents.Remove(actorID);
        }

        static GameObject _createNewActorGO(Vector3 spawnPoint)
        {
            var actorBody = new GameObject
            {
                transform =
                {
                    position = spawnPoint
                }
            };
            var actorRb = actorBody.AddComponent<Rigidbody>();
            actorRb.useGravity = false;

            var actorGO = new GameObject
            {
                transform =
                {
                    parent        = actorBody.transform,
                    localPosition = Vector3.zero
                }
            };

            actorGO.AddComponent<BoxCollider>();
            actorGO.AddComponent<Animator>();
            actorGO.AddComponent<Animation>();
            actorGO.AddComponent<MeshRenderer>();
            actorGO.AddComponent<MeshFilter>();

            return actorGO;
        }

        static ActorData _generateNewActorData(ActorComponent actor, ActorGenerationParameters actorGenerationParameters)
        {
            var fullIdentification = new FullIdentification(
                actorID: actorGenerationParameters.ActorID != 0 ? actorGenerationParameters.ActorID : _getRandomActorID(),
                actorName: actorGenerationParameters.ActorName ?? _getRandomActorName(),
                actorFactionID: actorGenerationParameters.FactionID != 0 ? actorGenerationParameters.FactionID : _getRandomFaction(),
                actorCityID: actorGenerationParameters.CityID,
                actorBirthDate: new Date(Manager_DateAndTime.GetCurrentTotalDays())
            );

            actor.SetActorData(new ActorData(fullIdentification));

            foreach (var recipe in actorGenerationParameters.InitialRecipes)
            {
                //Debug.Log($"Adding recipe: {recipe}");
                actor.ActorData.CraftingData.AddRecipe(recipe);
            }

            // Add initial vocations
            foreach (var vocation in actorGenerationParameters.InitialVocations)
            {
                //Debug.Log($"Adding vocation: {vocation.VocationName} with experience: {vocation.VocationExperience}");
                actor.ActorData.VocationData.AddVocation(vocation.VocationName, vocation.VocationExperience);
            }

            actor.ActorData.CraftingData.AddRecipe(RecipeName.Log);
            actor.ActorData.CraftingData.AddRecipe(RecipeName.Plank);

            //Find a better way to put into groups.
            actor.ActorData.InventoryData.SetInventory(new ObservableDictionary<uint, Item>(), true);
            actor.ActorData.EquipmentData.SetEquipment(null, null, null, null, null, null, null, null, null);

            actor.ActorData.SpeciesAndPersonality.SetSpecies(_getRandomSpecies());
            actor.ActorData.SpeciesAndPersonality.SetPersonality(_getRandomPersonality());
            actor.ActorData.GameObjectProperties.SetGameObjectProperties(actor.transform);
            
            // Set ActorStatesAndConditions based on Race, Religion, etc.

            _addToAllActorData(actor.ActorData);

            return actor.ActorData;
        }

        static uint _getRandomActorID()
        {
            while (_allActorData.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }

        static ActorName _getRandomActorName()
        {
            // Get name based on culture, religion, species, etc.

            return new ActorName($"Test_{Random.Range(0, 50000)}", "of Tester");
        }

        private static uint _getRandomFaction()
        {
            // Take race and things into account for faction

            return 0;
        }

        static SpeciesName _getRandomSpecies()
        {
            return SpeciesName.Human;
        }

        static ActorPersonality _getRandomPersonality()
        {
            return new ActorPersonality(Manager_Personality.GetRandomPersonalityTraits(null, 3));
        }
    }

    [Serializable]
    public class ActorGenerationParameters
    {
        public uint                ActorID             { get; private set; }
        public ActorName           ActorName           { get; private set; }
        public uint                FactionID           { get; private set; }
        public uint                CityID              { get; private set; }
        public List<RecipeName>    InitialRecipes      { get; private set; } = new();
        public List<ActorVocation> InitialVocations    { get; private set; } = new();
        public StatesAndConditions StatesAndConditions { get; private set; }

        public void SetActorID(uint                         actorID)        => ActorID = actorID;
        public void SetActorName(ActorName                  actorName)      => ActorName = actorName;
        public void SetFactionID(uint                       factionID)      => FactionID = factionID;
        public void SetCityID(uint                          cityID)         => CityID = cityID;
        public void SetInitialRecipes(List<RecipeName>      initialRecipes) => InitialRecipes = initialRecipes;
        public void AddInitialRecipe(RecipeName             recipeName)     => InitialRecipes.Add(recipeName);
        public void SetInitialVocations(List<ActorVocation> actorVocations) => InitialVocations = actorVocations;
        public void AddInitialVocation(ActorVocation        actorVocation)  => InitialVocations.Add(actorVocation);
        public void SetStatesAndConditions(StatesAndConditions statesAndConditions) => StatesAndConditions = statesAndConditions;
    }
}