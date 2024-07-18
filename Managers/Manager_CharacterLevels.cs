using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelUpBonusType { Health, Mana, Stamina, Skillset, Ultimate }

public class Manager_CharacterLevels
{
    public static List<LevelData> AllLevelUpData = new();

    public static void InitialiseLevels()
    {
        var levelInfos = new[]
        {
            new LevelData( 1,     0, LevelUpBonusType.Health,  10, 1, 10),
            new LevelData( 2,   250, LevelUpBonusType.Mana,    10, 1,  2),
            new LevelData( 3,   750, LevelUpBonusType.Stamina, 10, 1,  2),
            new LevelData( 4,  1750, LevelUpBonusType.Skillset, 1, 2,  4),
            new LevelData( 5,  3000, LevelUpBonusType.Health,  20, 1,  3),
            new LevelData( 6,  4500, LevelUpBonusType.Mana,    20, 1,  3),
            new LevelData( 7,  6250, LevelUpBonusType.Stamina, 20, 1,  3),
            new LevelData( 8,  8250, LevelUpBonusType.Skillset, 1, 2,  5),
            new LevelData( 9, 10500, LevelUpBonusType.Health,  30, 1,  4),
            new LevelData(10, 13000, LevelUpBonusType.Mana,    30, 1,  4),
            new LevelData(11, 15750, LevelUpBonusType.Stamina, 30, 1,  4),
            new LevelData(12, 18750, LevelUpBonusType.Ultimate, 1, 2,  6),
            new LevelData(13, 22000, LevelUpBonusType.Health,  40, 1,  5),
            new LevelData(14, 25500, LevelUpBonusType.Mana,    40, 1,  5),
            new LevelData(15, 29250, LevelUpBonusType.Stamina, 40, 1,  5),
            new LevelData(16, 33250, LevelUpBonusType.Ultimate, 1, 2,  7),
            new LevelData(17, 37500, LevelUpBonusType.Health,  50, 1,  6),
            new LevelData(18, 42000, LevelUpBonusType.Mana,    50, 1,  6),
            new LevelData(19, 46750, LevelUpBonusType.Stamina, 50, 1,  6),
            new LevelData(20, 51750, LevelUpBonusType.Ultimate, 1, 2, 10)
        };

        AllLevelUpData.AddRange(levelInfos);

        for (int i = 1; i < AllLevelUpData.Count; i++)
        {
            if (AllLevelUpData[i].TotalExperienceRequired < AllLevelUpData[i - 1].TotalExperienceRequired)
            {
                Debug.LogWarning($"Total experience required for level {AllLevelUpData[i].Level} is less than its predecessor");
            }
        }
    }
}

[Serializable]
public class LevelData
{
    public int Level;
    public int TotalExperienceRequired;
    public LevelUpBonusType BonusType;
    public int BonusStatPoints;
    public int SkillPoints;
    public int SPECIALPoints;

    public LevelData(
        int level,
        int totalExperienceRequired,
        LevelUpBonusType bonusType,
        int bonusStatPoints,
        int skillPoints,
        int specialPoints
        )
    {
        Level = level;
        TotalExperienceRequired = totalExperienceRequired;
        BonusType = bonusType;
        BonusStatPoints = bonusStatPoints;
        SkillPoints = skillPoints;
        SPECIALPoints = specialPoints;
    }
}