using System.Collections.Generic;
using System.Linq;
using Career;
using DateAndTime;
using Inventory;
using Items;
using Jobs;
using Managers;
using Recipes;
using ScriptableObjects;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Actor
{
    public class Actor_Manager : MonoBehaviour, IDataPersistence
    {
        static          AllActors_SO                      _allActors;
        static          Dictionary<uint, Actor_Data>      _allActorData       = new();
        static          uint                              _lastUnusedActorID  = 1;
        static readonly Dictionary<uint, Actor_Component> _allActorComponents = new();

        public void SaveData(SaveData data)
        {
            _allActorData.Values.ToList().ForEach(actorData => actorData.UpdateActorData());

            data.SavedActorData     = new SavedActorData(_allActorData.Values.ToList());
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
                if (actor.ActorData == null)
                {
                    Debug.Log($"Actor: {actor.name} does not have ActorData.");
                    continue;
                }

                if (!_allActorComponents.TryAdd(actor.ActorData.ActorID, actor))
                {
                    if (_allActorComponents[actor.ActorData.ActorID].gameObject == actor.gameObject) continue;
                    Debug.LogError(
                        $"ActorID {actor.ActorData.ActorID} and name {actor.name} already exists for actor {_allActorComponents[actor.ActorData.ActorID].name}");
                    actor.ActorData.ActorID = _getRandomActorID();
                }

                if (!_allActorData.ContainsKey(actor.ActorData.ActorID))
                    Debug.Log($"Actor: {actor.ActorData.ActorID} {actor.name} does not exist in AllActorData.");
            }

            foreach (var actor in _allActorData)
            {
                actor.Value.PrepareForInitialisation();
            }
        }

        static List<Actor_Component> _findAllActorComponents()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                   .OfType<Actor_Component>()
                   .ToList();
        }

        static void _addToAllActorData(Actor_Data actorData)
        {
            if (_allActorData.TryAdd(actorData.ActorID, actorData)) return;

            Debug.Log($"ActorData: {actorData.ActorID} already exists in AllActorData.");
        }

        public static void UpdateAllActorData(Actor_Data actorData)
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

        public static Actor_Data GetActorData(uint actorID)
        {
            if (_allActorData.TryGetValue(actorID, out var data)) return data;

            Debug.Log($"ActorData: {actorID} does not exist in AllActorData.");
            return null;

        }

        public static Actor_Component GetActor(uint actorID, bool generateActorIfNotFound = false)
        {
            if (_allActorComponents.TryGetValue(actorID, out var actor))
            {
                return actor;
            }

            if (generateActorIfNotFound)
            {
                return _allActorComponents[actorID] =
                    _spawnActor(GetActorData(actorID).GameObjectData.LastSavedActorPosition, actorID);
            }

            return null;
        }

        public static Actor_Component SpawnNewActor(Vector3             spawnPoint,
                                                    ActorDataPresetName actorDataPreset)
        {
            Actor_Component actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(_generateNewActorData(actor, actorDataPreset));

            _allActorComponents[actor.ActorData.ActorID] = actor;

            actor.Initialise();

            Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

            return actor;
        }

        // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
        static Actor_Component _spawnActor(Vector3 spawnPoint, uint actorID, bool despawnActorIfExists = false)
        {
            if (despawnActorIfExists) _despawnActor(actorID);
            else if (_allActorComponents.TryGetValue(actorID, out var existingActor)) return existingActor;

            var actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

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

        static Actor_Data _generateNewActorData(Actor_Component     actor,
                                                ActorDataPresetName actorDataPresetName)
        {
            var actorDataPreset = ActorDataPreset_Manager.GetActorDataPreset(actorDataPresetName);

            var fullIdentification = new FullIdentification(
                actorID: actorDataPreset?.ActorID               ?? _getRandomActorID(),
                actorName: actorDataPreset?.ActorName           ?? _getRandomActorName(),
                actorFactionID: actorDataPreset?.ActorFactionID ?? _getRandomFaction(),
                actorCityID: actorDataPreset?.FullIdentification.ActorCityID ??
                             1, // Change this so that it generates cityID
                actorBirthDate: actorDataPreset?.FullIdentification.ActorBirthDate ??
                                new Date(Manager_DateAndTime.GetCurrentTotalDays()),
                actorDataPresetName: actorDataPreset?.FullIdentification.ActorDataPresetName ??
                                     ActorDataPresetName.No_Preset
            );

            var gameObjectData = new GameObjectData(
                actorID: fullIdentification.ActorID,
                actorTransform: actor.transform,
                actorMesh: null,
                actorMaterial: null
            );

            var careerData = new CareerData(
                actorID: fullIdentification.ActorID,
                careerName: actorDataPreset?.CareerData.CareerName     ?? CareerName.Wanderer,
                jobsNotFromCareer: actorDataPreset?.CareerData.AllJobs ?? new HashSet<JobName>()
            );
            
            var craftingData = new CraftingData(
                actorID: fullIdentification.ActorID,
                knownRecipes: actorDataPreset?.CraftingData.KnownRecipes ?? new List<RecipeName>()
            );

            var vocationData = new VocationData(
                actorID: fullIdentification.ActorID,
                actorVocations: actorDataPreset?.VocationData.ActorVocations ?? new Dictionary<VocationName, ActorVocation>()
                );
            
            var speciesAndPersonality = new SpeciesAndPersonality(
                actorID: fullIdentification.ActorID,
                actorSpecies: actorDataPreset?.SpeciesAndPersonality.ActorSpecies         ?? _getRandomSpecies(),
                actorPersonality: actorDataPreset?.SpeciesAndPersonality.ActorPersonality ?? _getRandomPersonality()
            );

            var statsAndAbilities = new StatsAndAbilities(
                actorID: fullIdentification.ActorID,
                actorStats: actorDataPreset?.StatsAndAbilities.ActorStats ?? _getNewActorStats(fullIdentification.ActorID),
                actorAspects: actorDataPreset?.StatsAndAbilities.ActorAspects ?? _getNewActorAspects(fullIdentification.ActorID),
                actorAbilities: actorDataPreset?.StatsAndAbilities.ActorAbilities ?? _getNewActorAbilities(fullIdentification.ActorID)
                );

            var statesAndConditions = new StatesAndConditionsData(
                actorID: fullIdentification.ActorID,
                actorStates: actorDataPreset?.StatesAndConditionsData.Actor_States ?? _getNewActorStates(fullIdentification.ActorID),
                actorConditions: actorDataPreset?.StatesAndConditionsData.Actor_Conditions ?? _getNewActorConditions(fullIdentification.ActorID)
                );

            var inventoryData = new InventoryData_Actor(
                actorID: fullIdentification.ActorID,
                new ObservableDictionary<uint, Item>()
            );

            var equipmentData = new EquipmentData(
                actorID: fullIdentification.ActorID,
                head: null,
                neck: null,
                chest: null,
                leftHand: null,
                rightHand: null,
                rings: new Item[2],
                waist: null,
                legs: null,
                feet: null
            );

            actor.SetActorData(new Actor_Data(
                fullIdentification,
                gameObjectData,
                careerData,
                craftingData,
                vocationData,
                speciesAndPersonality,
                statsAndAbilities,
                statesAndConditions,
                inventoryData,
                equipmentData
                ));

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

        static uint _getRandomFaction()
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

        static Actor_Stats _getNewActorStats(uint actorID)
        {
            return new Actor_Stats(
                actorID: actorID,
                actorLevelData: new ActorLevelData(actorID),
                actorSpecial: new Special(),
                actorCombatStats: new CombatStats()
                );
        }
        
        static Actor_Aspects _getNewActorAspects(uint actorID)
        {
            return new Actor_Aspects(
                actorID: actorID,
                actorAspectList: new List<AspectName>()
                );
        }
        
        static Actor_Abilities _getNewActorAbilities(uint actorID)
        {
            return new Actor_Abilities(
                actorID: actorID,
                abilityList: new Dictionary<Ability, float>()
                );
        }
        
        static Actor_States _getNewActorStates(uint actorID)
        {
            return new Actor_States(
                actorID: actorID,
                initialisedStates: new ObservableDictionary<PrimaryStateName, bool>()
                );
        }

        static Actor_Conditions _getNewActorConditions(uint actorID)
        {
            return new Actor_Conditions(
                actorID: actorID,
                currentConditions: new ObservableDictionary<ConditionName, float>()
                );
        }
    }
}