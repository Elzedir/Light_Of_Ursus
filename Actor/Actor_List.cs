using System.Collections.Generic;
using Ability;
using Careers;
using DateAndTime;
using Inventory;
using Items;
using Managers;
using Personality;
using Recipes;
using Tools;

namespace Actor
{
    public abstract class Actor_List
    {
        static        Dictionary<uint, Actor_Data> _defaultActors;
        public static Dictionary<uint, Actor_Data> DefaultActors => _defaultActors ??= _initialiseDefaultActors();

        static Dictionary<uint, Actor_Data> _initialiseDefaultActors()
        {
            return new Dictionary<uint, Actor_Data>
            {
                {
                    1, new Actor_Data(
                        actorDataPresetName: ActorDataPresetName.No_Preset,
                        fullIdentification: new FullIdentification(
                            actorID: 1,
                            actorName: new ActorName
                            (
                                name: "Test",
                                surname: "One"
                            ),
                            actorFactionID: 0,
                            actorCityID: 1,
                            actorBirthDate: new Date
                            (
                                year: 1,
                                month: 1,
                                day: 1
                            )),
                        gameObjectData: new GameObjectData
                        (
                            actorID: 1
                        ),
                        careerDataPreset: new Career_Data_Preset
                        (
                            actorID: 1,
                            careerName: CareerName.Lumberjack
                        ),
                        craftingDataPreset: new Crafting_Data_Preset
                        (
                            actorID: 1,
                            knownRecipes: new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank,
                                RecipeName.Iron_Ingot
                            }),
                        vocationDataPreset: new Vocation_Data_Preset(
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
                        speciesAndPersonality: new SpeciesAndPersonality(
                            actorID: 1,
                            actorSpecies: SpeciesName.Human,
                            actorPersonality: new ActorPersonality(
                                new List<PersonalityTraitName>
                                {
                                    PersonalityTraitName.Brave
                                })
                        ),
                        statsAndAbilitiesPreset: new StatsAndAbilities_Preset(
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
                                abilityList: new Dictionary<AbilityName, float>
                                {
                                    { AbilityName.Eagle_Stomp, 0 },
                                    { AbilityName.Charge, 0 }
                                })
                        ),
                        statesAndConditionsData: new StatesAndConditionsData(
                            actorStates: new Actor_States(
                                actorID: 1,
                                initialisedStates: new ObservableDictionary<PrimaryStateName, bool>
                                {
                                    { PrimaryStateName.IsAlive, true },
                                    { PrimaryStateName.CanIdle, true },
                                    { PrimaryStateName.CanCombat, true },
                                    { PrimaryStateName.CanMove, true },
                                    { PrimaryStateName.CanTalk, true }
                                }
                            ),
                            actorConditions: new Actor_Conditions(
                                actorID: 1,
                                currentConditions: new ObservableDictionary<ConditionName, float>()
                            )
                        ),
                        inventoryDataPreset: new InventoryDataPreset_Actor(
                            actorID: 1,
                            allInventoryItems: new ObservableDictionary<uint, Item>
                            {
                                {
                                    1, new Item(1, 1)
                                },
                                {
                                    2, new Item(2, 1)
                                }
                            }
                        ),
                        equipmentDataPreset: new Equipment_Data_Preset(
                            actorID: 1
                        ),
                        actorQuests: new QuestUpdater(
                            actorID: 1
                        )
                    )
                }
            };
        }
    }
}