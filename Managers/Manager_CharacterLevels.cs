using System;
using System.Collections.Generic;

namespace Managers
{
    public enum LevelUpBonusType { None, Health, Mana, Stamina, SkillSet, Ultimate }

    public abstract class Manager_CharacterLevels
    {
        public static CharacterLevelData GetLevelUpData(ulong level) => _allLevelUpData[level];

        static readonly Dictionary<ulong, CharacterLevelData> _allLevelUpData = new()
        {
            {
                1, new CharacterLevelData(1, 0, LevelUpBonusType.Health, 10, 1, 10)
            },
            {
                2, new CharacterLevelData(2, 250, LevelUpBonusType.Mana, 10, 1, 2)
            },
            {
                3, new CharacterLevelData(3, 750, LevelUpBonusType.Stamina, 10, 1, 2)
            },
            {
                4, new CharacterLevelData(4, 1750, LevelUpBonusType.SkillSet, 1, 2, 4)
            },
            {
                5, new CharacterLevelData(5, 3000, LevelUpBonusType.Health, 20, 1, 3)
            },
            {
                6, new CharacterLevelData(6, 4500, LevelUpBonusType.Mana, 20, 1, 3)
            },
            {
                7, new CharacterLevelData(7, 6250, LevelUpBonusType.Stamina, 20, 1, 3)
            },
            {
                8, new CharacterLevelData(8, 8250, LevelUpBonusType.SkillSet, 1, 2, 5)
            },
            {
                9, new CharacterLevelData(9, 10500, LevelUpBonusType.Health, 30, 1, 4)
            },
            {
                10, new CharacterLevelData(10, 13000, LevelUpBonusType.Mana, 30, 1, 4)
            },
            {
                11, new CharacterLevelData(11, 15750, LevelUpBonusType.Stamina, 30, 1, 4)
            },
            {
                12, new CharacterLevelData(12, 18750, LevelUpBonusType.Ultimate, 1, 2, 6)
            },
            {
                13, new CharacterLevelData(13, 22000, LevelUpBonusType.Health, 40, 1, 5)
            },
            {
                14, new CharacterLevelData(14, 25500, LevelUpBonusType.Mana, 40, 1, 5)
            },
            {
                15, new CharacterLevelData(15, 29250, LevelUpBonusType.Stamina, 40, 1, 5)
            },
            {
                16, new CharacterLevelData(16, 33250, LevelUpBonusType.Ultimate, 1, 2, 7)
            },
            {
                17, new CharacterLevelData(17, 37500, LevelUpBonusType.Health, 50, 1, 6)
            },
            {
                18, new CharacterLevelData(18, 42000, LevelUpBonusType.Mana, 50, 1, 6)
            },
            {
                19, new CharacterLevelData(19, 46750, LevelUpBonusType.Stamina, 50, 1, 6)
            },
            {
                20, new CharacterLevelData(20, 51750, LevelUpBonusType.Ultimate, 1, 2, 10)
            }
        };
        
        public static ulong GetLevelFromExperience(ulong totalExperience)
        {
            ulong level = 1;

            while (true)
            {
                if (_allLevelUpData[level].TotalExperienceRequired > totalExperience) break;
                level++;
            }

            return level;
        }
        
        public static ulong GetTotalSkillPointsFromExperience(ulong totalExperience)
        {
            ulong level              = 1;
            ulong totalSkillPoints   = 0;
            
            while (true)
            {
                if (_allLevelUpData[level].TotalExperienceRequired > totalExperience) break;
                totalSkillPoints += _allLevelUpData[level].SkillPoints;
                level++;
            }

            return totalSkillPoints;
        }
        
        public static ulong GetTotalSpecialPointsFromExperience(ulong totalExperience)
        {
            ulong level              = 1;
            ulong totalSpecialPoints = 0;
            
            while (true)
            {
                if (_allLevelUpData[level].TotalExperienceRequired > totalExperience) break;
                totalSpecialPoints += _allLevelUpData[level].SpecialPoints;
                level++;
            }

            return totalSpecialPoints;
        }
    }

    [Serializable]
    public class CharacterLevelData
    {
        public ulong             Level;
        public ulong             TotalExperienceRequired;
        public LevelUpBonusType BonusType;
        public ulong             BonusStatPoints;
        public ulong             SkillPoints;
        public ulong             SpecialPoints;

        public CharacterLevelData(
            ulong             level,
            ulong             totalExperienceRequired,
            LevelUpBonusType bonusType,
            ulong             bonusStatPoints,
            ulong             skillPoints,
            ulong             specialPoints
        )
        {
            Level                   = level;
            TotalExperienceRequired = totalExperienceRequired;
            BonusType               = bonusType;
            BonusStatPoints         = bonusStatPoints;
            SkillPoints             = skillPoints;
            SpecialPoints           = specialPoints;
        }
    }
}