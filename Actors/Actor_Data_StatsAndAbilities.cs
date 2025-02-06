using System;
using System.Collections.Generic;
using System.Linq;
using Abilities;
using Actor;
using ActorActions;
using Inventory;
using Items;
using Managers;
using Priorities;
using Priority;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actors
{
    [Serializable]
    public class Actor_Data_StatsAndAbilities : Priority_Class
    {
        public Actor_Data_StatsAndAbilities(ulong actorID, Actor_Stats actorStats, Actor_Aspects actorAspects, Actor_Abilities actorAbilities) : base(actorID, ComponentType.Actor)
        {
            Stats = actorStats ?? new Actor_Stats(0, new ActorLevelData(), new Special(), new CombatStats());
            Aspects = actorAspects ?? new Actor_Aspects(0);
            Abilities = actorAbilities ?? new Actor_Abilities(0);
        }

        public Actor_Data_StatsAndAbilities(Actor_Data_StatsAndAbilities actorDataStatsAndAbilities) : base(actorDataStatsAndAbilities.ActorReference.ActorID, ComponentType.Actor)
        {
            Stats = new Actor_Stats(actorDataStatsAndAbilities.Stats);
            Aspects = new Actor_Aspects(actorDataStatsAndAbilities.Aspects);
            Abilities = new Actor_Abilities(actorDataStatsAndAbilities.Abilities);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Return actions based on abilities.
            return new List<ActorActionName>();
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Stats", $"{Stats}" },
                { "Actor Aspects", $"{Aspects}" },
                { "Actor Abilities", $"{Abilities}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Stats.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Aspects",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Aspects.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Abilities.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }
        
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Actor_Stats Stats;
        public void SetActorStats(Actor_Stats actorStats) => Stats = actorStats;

        public Actor_Aspects Aspects;
        public void SetActorAspects(Actor_Aspects actor_Aspects) => Aspects = actor_Aspects;

        public Actor_Abilities Abilities;
        public void SetActorAbilities(Actor_Abilities actor_Abilities) => Abilities = actor_Abilities;
    }
    
    [Serializable]
    public class Actor_Stats : Priority_Class
    {
        public Actor_Stats(ulong actorID, ActorLevelData actorLevelData, Special actorSpecial,
            CombatStats actorCombatStats) :
            base(actorID, ComponentType.Actor)
        {
            ActorLevelData = actorLevelData;
            ActorSpecial = actorSpecial;
            ActorCombatStats = actorCombatStats;
        }

        public Actor_Stats(Actor_Stats actorStats) : base(actorStats.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorLevelData = new ActorLevelData(actorStats.ActorLevelData);
            ActorSpecial = new Special(actorStats.ActorSpecial);
            ActorCombatStats = new CombatStats(actorStats.ActorCombatStats);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Can't think of anything to do with stats at the moment, maybe some cosmetic stuff.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Actor Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var actorLevelData = ActorLevelData.GetStringData();
            var actorSpecial = ActorSpecial.GetStringData();
            var actorCombatStats = ActorCombatStats.GetStringData();

            return actorLevelData.Concat(actorSpecial).Concat(actorCombatStats)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ActorLevelData ActorLevelData;
        public Special ActorSpecial;
        public CombatStats ActorCombatStats;

        public float TotalCarryWeight =>
            100; // For now. Eventually. ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.

        public float AvailableCarryWeight => TotalCarryWeight -
                                             Item.GetItemListTotal_Weight(ActorReference.Actor_Component.ActorData
                                                 .InventoryData
                                                 .GetAllInventoryItemsClone().Values.ToList());

        public void AddExperience(ulong experience)
        {
            ActorLevelData.TotalExperience += experience;

            LevelUpCheck();
        }

        public void LevelUpCheck()
        {
            var levelData = Manager_CharacterLevels.GetLevelUpData(ActorLevelData.ActorLevel);

            if (ActorLevelData.TotalExperience < levelData.TotalExperienceRequired) return;

            _levelUp(levelData);
        }

        void _levelUp(CharacterLevelData levelData)
        {
            switch (levelData.BonusType)
            {
                case LevelUpBonusType.Health:
                    ActorCombatStats.BaseMaxHealth += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Mana:
                    ActorCombatStats.BaseMaxMana += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Stamina:
                    ActorCombatStats.BaseMaxStamina += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.SkillSet:
                    //ActorLevelData.CanAddNewSkillSet = true;
                    // Change it so that instead of a bool it just checks against the level to return the bool.
                    break;
                case LevelUpBonusType.Ultimate:
                default:
                    Debug.Log("LevelData Bonus Type was none.");
                    break;
            }
        }
    }

    [Serializable]
    public class Actor_Aspects : Priority_Class
    {
        public Actor_Aspects(ulong actorID, List<AspectName> actorAspectList = null) : base(actorID, ComponentType.Actor)
        {
            ActorAspectList = actorAspectList ?? new List<AspectName>
                { AspectName.None, AspectName.None, AspectName.None };
        }

        public Actor_Aspects(Actor_Aspects actorAspects) : base(actorAspects.ActorReference.ActorID,
            ComponentType.Actor)
        {
            ActorAspectList = new List<AspectName>(actorAspects.ActorAspectList);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Again, maybe some random cosmetic things that they can do depending on their class.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Actor Aspects",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Class Title", $"{ActorClassTitle}" },
                { "Actor Aspect List", $"{ActorAspectList[0]}, {ActorAspectList[1]}, {ActorAspectList[2]}" }
            };
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ClassTitle ActorClassTitle => Actor_Aspect_List.GetCharacterTitle(ActorAspectList);

        public List<AspectName> ActorAspectList;
        public void SetActorAspectList(List<AspectName> actorAspectList) => ActorAspectList = actorAspectList;

        public bool CanAddAspect(AspectName aspectName)
        {
            return ActorAspectList.Contains(AspectName.None) || !ActorAspectList.Contains(aspectName);
        }

        public bool AddAspect(AspectName aspect)
        {
            if (!CanAddAspect(aspect))
            {
                Debug.Log("Cannot add aspect.");
                return false;
            }

            var index = ActorAspectList.FindIndex(a => a == AspectName.None);

            if (index == -1)
            {
                Debug.LogError("No empty aspect slots available even after check.");
                return false;
            }

            ActorAspectList[index] = aspect;
            return true;
        }

        public void ChangeAspect(AspectName aspect, int index)
        {
            if (index < 0 || index >= ActorAspectList.Count)
            {
                Debug.Log("Index out of range.");
                return;
            }

            ActorAspectList[index] = aspect;
        }
    }

    [Serializable]
    public class ActorLevelData
    {
        public ActorLevelData(ulong totalExperience = 0)
        {
            TotalExperience = totalExperience;
        }

        public ActorLevelData(ActorLevelData actorLevelData)
        {
            TotalExperience = actorLevelData.TotalExperience;
            UsedSkillPoints = actorLevelData.UsedSkillPoints;
            UsedSpecialPoints = actorLevelData.UsedSpecialPoints;
        }

        public Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Level", $"{ActorLevel}" },
                { "Total Experience", $"{TotalExperience}" },
                { "Total Skill Points", $"{TotalSkillPoints}" },
                { "Used Skill Points", $"{UsedSkillPoints}" },
                { "Total Special Points", $"{TotalSpecialPoints}" },
                { "Used Special Points", $"{UsedSpecialPoints}" }
            };
        }

        public ulong ActorLevel => Manager_CharacterLevels.GetLevelFromExperience(TotalExperience);
        public ulong TotalExperience;
        public ulong TotalSkillPoints => Manager_CharacterLevels.GetTotalSkillPointsFromExperience(TotalExperience);
        public ulong UsedSkillPoints; // Can change this to be calculated by used skill points from the other class.
        public ulong TotalSpecialPoints => Manager_CharacterLevels.GetTotalSpecialPointsFromExperience(TotalExperience);
        public ulong UsedSpecialPoints; // Can change this to be calculated by used skill points from the other class.
    }
}