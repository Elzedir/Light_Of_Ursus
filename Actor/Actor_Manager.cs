using System.Collections.Generic;
using System.Linq;
using Ability;
using Career;
using DateAndTime;
using Faction;
using Initialisation;
using Inventory;
using Items;
using Jobs;
using Managers;
using Personality;
using Recipe;
using Tools;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Actor
{
    public class Actor_Manager : MonoBehaviour, IDataPersistence
    {
        const string _actor_SOPath = "ScriptableObjects/Actor_SO";
        
        static Actor_SO _allActors;
        static Actor_SO AllActors => _allActors ??= _getActor_SO();

        public void SaveData(SaveData saveData) =>
            saveData.SavedActorData = new SavedActorData(AllActors.Actors.Select(actor => actor.DataObject).ToArray());

        public void LoadData(SaveData saveData)
        {
            // NB: Apply this to all other LoadData functions.
            try
            {
                AllActors.LoadSO(saveData.SavedActorData.AllActorData);
            }
            catch
            {
                if (saveData is null)
                {
                    Debug.LogWarning("No SaveData found in LoadData.");
                    return;
                }

                if (saveData.SavedActorData == null)
                {
                    Debug.LogWarning("No SavedActorData found in SaveData.");
                    return;
                }

                if (saveData.SavedActorData.AllActorData == null)
                {
                    Debug.LogWarning("No AllActorData found in SavedActorData.");
                    return;
                }
                
                Debug.LogWarning("AllActorData count is 0.");
            }
        }

        public static void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerActor += _initialise;
        }

        static void _initialise()
        {
            AllActors.PopulateSceneActors();
        }
        
        public static Actor_Data GetActor_Data(uint actorID)
        {
            return AllActors.GetActor_Data(actorID).DataObject;
        }
        
        public static Actor_Component GetActor_Component(uint actorID)
        {
            return AllActors.GetActor_Component(actorID);
        }
        
        static Actor_SO _getActor_SO()
        {
            var actor_SO = Resources.Load<Actor_SO>(_actor_SOPath);
            
            if (actor_SO is not null) return actor_SO;
            
            Debug.LogError("Actor_SO not found. Creating temporary Actor_SO.");
            actor_SO = ScriptableObject.CreateInstance<Actor_SO>();
            
            return actor_SO;
        }

        public static Actor_Component GetNearestActor(Vector3 position)
        {
            Actor_Component nearestActor = null;

            var nearestDistance = float.MaxValue;

            foreach (var actor in AllActors.ActorComponents.Values)
            {
                var distance = Vector3.Distance(position, actor.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestActor  = actor;
                nearestDistance = distance;
            }

            return nearestActor;
        }

        public static uint GetUnusedActorID()
        {
            return AllActors.GetUnusedActorID();
        }

        public static Actor_Component SpawnNewActor(Vector3             spawnPoint,
                                                    ActorDataPresetName actorDataPreset)
        {
            var actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(_generateNewActorData(actor, actorDataPreset));

            AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            actor.Initialise();

            Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

            return actor;
        }

        // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
        static Actor_Component _spawnExistingActor(Vector3 spawnPoint, uint actorID, bool despawnActorIfExists = false)
        {
            if (despawnActorIfExists) _despawnActor(actorID);

            var actor = AllActors.GetActor_Component(actorID);
            
            if (actor != null && actor.IsSpawned) return actor;

            actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(GetActor_Data(actorID));
            actor.Initialise();

            Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);
            
            actor.IsSpawned = true;

            return actor;
        }

        static void _despawnActor(uint actorID)
        {
            var actor = GetActor_Component(actorID);

            if (actor is null)
            {
                Debug.LogError($"Actor: {actorID} does not exist.");
                return;
            }

            actor.IsSpawned = false;
            Destroy(actor.gameObject);
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
                actorID: actorDataPreset?.FullIdentification.ActorID               ?? _getUnusedActorID(),
                actorName: actorDataPreset?.FullIdentification.ActorName           ?? _getRandomActorName(),
                actorFactionID: actorDataPreset?.ActorFactionID ?? _getRandomFaction(),
                actorBirthDate: actorDataPreset?.FullIdentification.ActorBirthDate ??
                                new Date(Manager_DateAndTime.GetCurrentTotalDays()),
                actorCityID: actorDataPreset?.FullIdentification.ActorCityID ?? 0
                // Change this so that it generates correct CityID
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
                actorStats: actorDataPreset?.StatsAndAbilities.ActorStats ?? _getNewActorStats(fullIdentification.ActorID),
                actorAspects: actorDataPreset?.StatsAndAbilities.ActorAspects ?? _getNewActorAspects(fullIdentification.ActorID),
                actorAbilities: actorDataPreset?.StatsAndAbilities.ActorAbilities ?? _getNewActorAbilities(fullIdentification.ActorID)
                );

            var statesAndConditions = new StatesAndConditionsData(
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
                actorDataPresetName,
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

            AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            return actor.ActorData;
        }

        static uint _getUnusedActorID()
        {
            return AllActors.GetUnusedActorID();
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
            return new ActorPersonality(Personality_Manager.GetRandomPersonalityTraits(null, 3));
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
                abilityList: new Dictionary<AbilityName, float>()
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