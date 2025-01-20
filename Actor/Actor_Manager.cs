using System.Collections.Generic;
using Ability;
using ActorPreset;
using Careers;
using DateAndTime;
using Equipment;
using Faction;
using Inventory;
using Items;
using Jobs;
using Managers;
using Personality;
using Priority;
using Recipes;
using StateAndCondition;
using Tools;
using UnityEngine;

namespace Actor
{
    public class Actor_Manager : MonoBehaviour
    {
        const string _actor_SOPath = "ScriptableObjects/Actor_SO";
        
        static Actor_SO _allActors;
        static Actor_SO AllActors => _allActors ??= _getActor_SO();

        // public static void OnSceneLoaded()
        // {
        //     Manager_Initialisation.OnInitialiseManagerActor += _initialise;
        // }
        //
        // static void _initialise()
        // {
        //     AllActors.PopulateSceneData();
        // }
        
        public static Actor_Data GetActor_Data(uint actorID)
        {
            return AllActors.GetActor_Data(actorID).Data_Object;
        }
        
        public static Actor_Data GetActor_DataFromComponent(Actor_Component actor_Component)
        {
            return AllActors.GetDataFromName(actor_Component.name)?.Data_Object;
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

            //var nearestDistance = float.MaxValue;
            
            //* Flash a collider at increasing distances until it hits something.

            return nearestActor;
        }

        public static uint GetUnusedActorID()
        {
            return AllActors.GetUnusedActorID();
        }

        public static Actor_Component SpawnNewActor(Vector3 spawnPoint, ActorDataPresetName actorDataPreset)
        {
            var actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(_generateNewActorData(actor, actorDataPreset));

            AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            actor.Initialise();

            Faction_Manager.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

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

            Faction_Manager.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);
            
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
            var actorDataPreset = ActorPreset_Manager.GetActorDataPreset(actorDataPresetName);
            
            if (actorDataPreset is null && actorDataPresetName != ActorDataPresetName.No_Preset)
            {
                Debug.LogError($"ActorDataPreset: {actorDataPresetName} does not exist when it should.");
            }

            var actorID = _getUnusedActorID();
            
            var fullIdentification = new Actor_Data_Identification(
                actorID: actorID,
                actorName: _getRandomActorName(actorID),
                actorFactionID: _getRandomFaction(),
                actorBirthDate: new Date(Manager_DateAndTime.GetCurrentTotalDays()),
                actorCityID: 0
                // Change this so that it generates correct CityID
            );

            var gameObjectData = new Actor_Data_GameObject(
                actorID: fullIdentification.ActorID,
                actorTransform: actor.transform,
                actorMesh: null,
                actorMaterial: null
            );

            var careerData = new Actor_Data_Career(
                actorID: fullIdentification.ActorID,
                careerName: actorDataPreset?.ActorDataCareer.CareerName     ?? CareerName.Wanderer,
                jobsNotFromCareer: actorDataPreset?.ActorDataCareer.AllJobs ?? new HashSet<JobName>()
            );
            
            var craftingData = new Actor_Data_Crafting(
                actorID: fullIdentification.ActorID,
                knownRecipes: actorDataPreset?.ActorDataCrafting.KnownRecipes ?? new List<RecipeName>()
            );
            
            var priorityData = new Priority_Data_Actor(
                actorID: fullIdentification.ActorID
            );

            var vocationData = new Actor_Data_Vocation(
                actorID: fullIdentification.ActorID,
                actorVocations: actorDataPreset?.ActorDataVocation.ActorVocations ?? new Dictionary<VocationName, ActorVocation>()
                );
            
            var speciesAndPersonality = new Actor_Data_SpeciesAndPersonality(
                actorID: fullIdentification.ActorID,
                actorSpecies: _getRandomSpecies(),
                actorPersonality: _getRandomPersonality()
            );
            
            var statsAndAbilities = new Actor_Data_StatsAndAbilities(
                actorID: fullIdentification.ActorID,
                actorStats: actorDataPreset?.ActorDataStatsAndAbilities?.Stats ?? _getNewActorStats(fullIdentification.ActorID),
                actorAspects: actorDataPreset?.ActorDataStatsAndAbilities?.Aspects ?? _getNewActorAspects(fullIdentification.ActorID),
                actorAbilities: actorDataPreset?.ActorDataStatsAndAbilities?.Abilities ?? _getNewActorAbilities(fullIdentification.ActorID)
                );

            var statesAndConditions = new Actor_Data_StatesAndConditions(
                actorID: fullIdentification.ActorID,
                states: _getNewActorStates(fullIdentification.ActorID),
                conditions: _getNewActorConditions(fullIdentification.ActorID)
                );

            var inventoryData = new InventoryData_Actor(
                actorID: fullIdentification.ActorID,
                new ObservableDictionary<uint, Item>()
            );

            var equipmentData = new Equipment_Data(
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
                equipmentData,
                priorityData
                ));

            AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            return actor.ActorData;
        }

        static uint _getUnusedActorID()
        {
            return AllActors.GetUnusedActorID();
        }

        static ActorName _getRandomActorName(uint actorID)
        {
            // Get name based on culture, religion, species, etc.

            return new ActorName($"Test_{actorID}", "of Tester");
        }

        static uint _getRandomFaction()
        {
            // Take race and things into account for faction

            return 1;
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
        
        static Actor_Data_States _getNewActorStates(uint actorID)
        {
            return new Actor_Data_States(
                actorID: actorID,
                initialisedStates: new ObservableDictionary<StateName, bool>()
                );
        }

        static Actor_Data_Conditions _getNewActorConditions(uint actorID)
        {
            return new Actor_Data_Conditions(
                actorID: actorID,
                currentConditions: new ObservableDictionary<ConditionName, float>()
                );
        }
        
        public static void ClearSOData()
        {
            AllActors.ClearSOData();
        }
    }
}