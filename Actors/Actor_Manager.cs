using System.Collections.Generic;
using Abilities;
using Actor;
using ActorPresets;
using Careers;
using DateAndTime;
using Equipment;
using Faction;
using IDs;
using Inventory;
using Items;
using Managers;
using Personality;
using Priorities;
using Recipes;
using Species;
using StateAndCondition;
using Tools;
using UnityEngine;

namespace Actors
{
    public abstract class Actor_Manager
    {
        const string _actor_SOPath = "ScriptableObjects/Actor_SO";
        
        static Actor_SO s_allActors;
        static Actor_SO S_AllActors => s_allActors ??= _getActor_SO();

        // public static void OnSceneLoaded()
        // {
        //     Manager_Initialisation.OnInitialiseManagerActor += _initialise;
        // }
        //
        // static void _initialise()
        // {
        //     AllActors.PopulateSceneData();
        // }
        
        public static Actor_Data GetActor_Data(ulong actorID)
        {
            return S_AllActors.GetActor_Data(actorID).Data_Object;
        }
        
        public static Actor_Data GetActor_DataFromComponent(Actor_Component actor_Component)
        {
            return S_AllActors.GetDataFromName(actor_Component.name)?.Data_Object;
        }
        
        public static Actor_Component GetActor_Component(ulong actorID)
        {
            return S_AllActors.GetActor_Component(actorID);
        }
        
        public static List<ulong> GetAllActorIDs()
        {
            return S_AllActors.GetAllDataIDs();
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

            //var nearestDistance = float.PositiveInfinity;
            
            //* Flash a collider at increasing distances until it hits something.

            return nearestActor;
        }

        public static Actor_Component SpawnNewActor(Vector3 spawnPoint, ActorDataPresetName actorDataPreset)
        {
            var actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(_generateNewActorData(actor, actorDataPreset));

            S_AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            actor.Initialise();

            Faction_Manager.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

            return actor;
        }

        // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
        static Actor_Component _spawnExistingActor(Vector3 spawnPoint, ulong actorID, bool despawnActorIfExists = false)
        {
            if (despawnActorIfExists) _despawnActor(actorID);

            var actor = S_AllActors.GetActor_Component(actorID);
            
            if (actor != null && actor.IsSpawned) return actor;

            actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Component>();

            actor.SetActorData(GetActor_Data(actorID));
            actor.Initialise();

            Faction_Manager.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);
            
            actor.IsSpawned = true;

            return actor;
        }

        static void _despawnActor(ulong actorID)
        {
            var actor = GetActor_Component(actorID);

            if (actor is null)
            {
                Debug.LogError($"Actor: {actorID} does not exist.");
                return;
            }

            actor.IsSpawned = false;
            Object.Destroy(actor.gameObject);
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

            var gameObjectData = new Actor_Data_SceneObject(
                actorID: fullIdentification.ActorID,
                actorTransform: actor.transform,
                actorMesh: null,
                actorMaterial: null
            );

            var careerData = new Actor_Data_Career(
                actorID: fullIdentification.ActorID,
                careerName: actorDataPreset?.ActorDataCareer?.CareerName ?? CareerName.Wanderer,
                job: actorDataPreset?.ActorDataCareer?.Job
            );
            
            var craftingData = new Actor_Data_Crafting(
                actorID: fullIdentification.ActorID,
                knownRecipes: actorDataPreset?.ActorDataCrafting?.KnownRecipes ?? new List<RecipeName>()
            );
            
            var priorityData = new Priority_Data_Actor(
                actorID: fullIdentification.ActorID
            );

            var vocationData = new Actor_Data_Vocation(
                actorID: fullIdentification.ActorID,
                actorVocations: actorDataPreset?.ActorDataVocation?.ActorVocations ?? new Dictionary<VocationName, ActorVocation>()
                );
            
            var species = new Actor_Data_Species(
                actorID: fullIdentification.ActorID,
                actorSpecies: _getRandomSpecies()
            );
            
            var personality = new Actor_Data_Personality(
                actorID: fullIdentification.ActorID,
                actorPersonality: _getRandomPersonality(),
                actorSpecies: species.ActorSpecies
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
                new ObservableDictionary<ulong, Item>()
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
            
            var proximityData = new Actor_Data_Proximity(
                actorID: fullIdentification.ActorID
            );

            actor.SetActorData(new Actor_Data(
                actorDataPresetName,
                fullIdentification,
                gameObjectData,
                careerData,
                craftingData,
                vocationData,
                species,
                personality,
                statsAndAbilities,
                statesAndConditions,
                inventoryData,
                equipmentData,
                priorityData,
                proximityData
                ));

            S_AllActors.UpdateActor(actor.ActorID, actor.ActorData);

            return actor.ActorData;
        }

        static ulong _getUnusedActorID()
        {
            //* Maybe can put in rules later with ID numbers being in certain ranges again, like per species or anything.
            return ID_Manager.GetNewID(IDType.Actor);
        }

        static ActorName _getRandomActorName(ulong actorID)
        {
            // Get name based on culture, religion, species, etc.

            return new ActorName($"Test_{actorID}", "of Tester");
        }

        static ulong _getRandomFaction()
        {
            // Take race and things into account for faction

            return 1;
        }

        static SpeciesName _getRandomSpecies()
        {
            return SpeciesName.Human;
        }

        static List<PersonalityTraitName> _getRandomPersonality()
        {
            return Personality_Manager.GetRandomPersonalityTraits(null, 3);
        }

        static Actor_Stats _getNewActorStats(ulong actorID)
        {
            return new Actor_Stats(
                actorID: actorID,
                actorLevelData: new ActorLevelData(actorID),
                actorSpecial: new Special(),
                actorCombatStats: new CombatStats()
                );
        }
        
        static Actor_Aspects _getNewActorAspects(ulong actorID)
        {
            return new Actor_Aspects(
                actorID: actorID,
                actorAspectList: new List<AspectName>()
                );
        }
        
        static Actor_Abilities _getNewActorAbilities(ulong actorID)
        {
            return new Actor_Abilities(
                actorID: actorID,
                abilityList: new SerializableDictionary<AbilityName, float>()
                );
        }
        
        static Actor_Data_States _getNewActorStates(ulong actorID)
        {
            return new Actor_Data_States(
                actorID: actorID,
                initialisedStates: new ObservableDictionary<StateName, bool>()
                );
        }

        static Actor_Data_Conditions _getNewActorConditions(ulong actorID)
        {
            return new Actor_Data_Conditions(
                actorID: actorID,
                currentConditions: new ObservableDictionary<ConditionName, float>()
                );
        }
        
        public static void ClearSOData()
        {
            S_AllActors.ClearSOData();
        }
    }
}