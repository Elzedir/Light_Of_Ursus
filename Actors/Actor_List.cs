using System.Collections.Generic;
using Abilities;
using Actor;
using ActorPresets;
using Careers;
using DateAndTime;
using Equipment;
using Inventory;
using Items;
using Managers;
using Personality;
using Priorities;
using Priority;
using Recipes;
using StateAndCondition;
using Tools;

namespace Actors
{
    public abstract class Actor_List
    {
        static Dictionary<ulong, Actor_Data> s_defaultActors;
        public static Dictionary<ulong, Actor_Data> DefaultActors => s_defaultActors ??= _initialiseDefaultActors();

        static Dictionary<ulong, Actor_Data> _initialiseDefaultActors()
        {
            return new Dictionary<ulong, Actor_Data>
            {
                {
                    1, new Actor_Data(
                        actorDataPresetName: ActorDataPresetName.No_Preset,
                        identification: new Actor_Data_Identification(
                            actorID: 1,
                            actorName: new ActorName
                            (
                                name: "Test",
                                surname: "One"
                            ),
                            actorFactionID: 1,
                            actorCityID: 1,
                            actorBirthDate: new Date
                            (
                                year: 1,
                                month: 1,
                                day: 1
                            )),
                        sceneObject: new Actor_Data_SceneObject
                        (
                            actorID: 1
                        ),
                        career: new Actor_Data_Career
                        (
                            actorID: 1,
                            careerName: CareerName.Lumberjack
                        ),
                        priority: new Priority_Data_Actor
                        (
                            actorID: 1
                        ),
                        crafting: new Actor_Data_Crafting
                        (
                            actorID: 1,
                            knownRecipes: new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank,
                                RecipeName.Iron_Ingot
                            }),
                        vocation: new Actor_Data_Vocation(
                            actorID: 1,
                            actorVocations: new Dictionary<VocationName, ActorVocation>
                            {
                                {
                                    VocationName.Logging, new ActorVocation
                                    (
                                        vocationName: VocationName.Logging,
                                        vocationExperience: 20000)
                                },
                                {
                                    VocationName.Mining, new ActorVocation
                                    (
                                        vocationName: VocationName.Mining,
                                        vocationExperience: 20000)
                                }
                            }),
                        species: new Actor_Data_Species(
                            actorID: 1,
                            actorSpecies: SpeciesName.Human
                        ),
                        personality: new Actor_Data_Personality(
                            actorID: 1,
                            actorPersonality: new List<PersonalityTraitName>
                            {
                                PersonalityTraitName.Brave,
                                PersonalityTraitName.Humble
                            },
                            actorSpecies: SpeciesName.Human
                            ),
                        statsAndAbilities: new Actor_Data_StatsAndAbilities(
                            actorID: 1,
                            actorStats: new Actor_Stats(
                                actorID: 1,
                                actorLevelData: new ActorLevelData(
                                    totalExperience: 5000
                                ),
                                actorSpecial: new Special(
                                    agility: 5,
                                    charisma: 5,
                                    endurance: 5,
                                    intelligence: 5,
                                    luck: 5,
                                    perception: 5,
                                    strength: 5
                                ),
                                actorCombatStats:
                                new CombatStats(
                                    baseMaxHealth: 100,
                                    baseMaxMana: 100,
                                    baseMaxStamina: 100,

                                    baseAttackDamage: 1,
                                    baseAttackSpeed: 1,
                                    baseAttackSwingTime: 1,
                                    baseAttackRange: 1,
                                    baseAttackPushForce: 1,
                                    baseAttackCooldown: 1,

                                    basePhysicalDefence: 1,
                                    baseMagicalDefence: 1,

                                    baseMoveSpeed: 1,
                                    baseDodgeCooldownReduction: 1
                                )
                            ),
                            actorAspects: new Actor_Aspects(
                                actorID: 1,
                                new List<AspectName>
                                {
                                    AspectName.Defiance,
                                    AspectName.Glory,
                                    AspectName.Grace
                                }),
                            actorAbilities: new Actor_Abilities(
                                actorID: 1,
                                abilityList: new SerializableDictionary<AbilityName, float>
                                {
                                    { AbilityName.Eagle_Stomp, 0 },
                                    { AbilityName.Charge, 0 }
                                })
                        ),
                        statesAndConditions: new Actor_Data_StatesAndConditions(
                            actorID: 1,
                            states: new Actor_Data_States(
                                actorID: 1,
                                initialisedStates: new ObservableDictionary<StateName, bool>
                                {
                                    { StateName.IsAlive, true },
                                    { StateName.CanIdle, true },
                                    { StateName.CanCombat, true },
                                    { StateName.CanMove, true },
                                    { StateName.CanTalk, true }
                                }
                            ),
                            conditions: new Actor_Data_Conditions(
                                actorID: 1,
                                currentConditions: new ObservableDictionary<ConditionName, float>()
                            )
                        ),
                        inventoryData: new InventoryData_Actor(
                            actorID: 1,
                            allInventoryItems: new ObservableDictionary<ulong, Item>
                            {
                                {
                                    1, new Item(1, 1)
                                },
                                {
                                    2, new Item(2, 1)
                                }
                            }
                        ),
                        equipmentData: new Equipment_Data(
                            actorID: 1
                        ),
                        actorQuests: new QuestClass(
                            actorID: 1
                        )
                    )
                }
            };
        }
    }
}