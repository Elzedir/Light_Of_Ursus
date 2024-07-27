using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actor", menuName = "Actor/Actor Data")]
public class Actor_Data_SO : ScriptableObject
{
    public int ActorID;
    public Actor_Base Actor;
    public ActorName ActorName;

    public SpeciesName ActorSpecies;
    public FactionName Faction;
    public WorldState_Data_SO Worldstate;

    public CareerName ActorCareer;
    public Actor_States ActorStates;
    public ActorStats ActorStats;
    public ActorAspects ActorAspects;
    public DisplayInventory ActorInventory;
    public ActorEquipment ActorEquipment;
    public ActorAbilities ActorAbilities;
    public ActorQuests ActorQuests;

    public void Initialise(Actor_Base actor)
    {
        if (Manager_Game.ActorIDs.Contains(ActorID))
        {
            throw new ArgumentException($"ActorID: {ActorID} has already been used");
        }

        //ActorAspects.InitialiseAspects(actor);
    }
}

[Serializable]
public class ActorName
{
    public string Name;
    public string Surname;
}

public enum SpeciesName
{
    Default,
    Demon,
    Human,
    Orc
}

[System.Serializable]
public class ActorStats
{
    public Date ActorBirthDate;
    public float ActorAge;

    public Personality ActorPersonality;

    public ActorLevelData ActorLevelData;
    public int ActorGold;
    public SPECIAL ActorSpecial;

    [SerializeField] CombatStats _combatStats; 
    public CombatStats CombatStats { get { return _combatStats; } set { _combatStats = value; } }

    public Dictionary<Vocation, float> VocationStats;
}

[Serializable]
public struct ActorAspects
{
    public ClassTitle ActorTitle;
    public List<Aspect> _actorAspectList;
    public List<Aspect> ActorAspectList
    {
        get { return _actorAspectList; }
        set
        {
            _actorAspectList = value;
            InitialiseAspects(aspectList: _actorAspectList);
        }
    }

    public void InitialiseAspects(Actor_Base actor = null, List<Aspect> aspectList = null)
    {
        ActorTitle = Manager_Aspect.GetCharacterTitle(actor: actor, aspectList: aspectList);
    }
}

[Serializable]
public class ActorLevelData
{
    Actor_Base _actor;
    public int Level { get; private set; }
    public int TotalExperience { get; private set; }
    public int TotalSkillPoints { get; private set; }
    public int UsedSkillPoints { get; private set; }
    public int TotalSPECIALPoints { get; private set; }
    public int UsedSPECIALPoints { get; private set; }
    public bool CanAddNewSkillSet { get; private set; }

    public ActorLevelData(Actor_Base actor, int level = 1, int totalExperience = 0, int totalSkillPoints = 0, int totalSPECIALPoints = 0, bool canAddNewSkillSet = false)
    {
        _actor = actor;
        Level = level;
        TotalExperience = totalExperience;
        TotalSkillPoints = totalSkillPoints;
        TotalSPECIALPoints = totalSPECIALPoints;
        CanAddNewSkillSet = canAddNewSkillSet;
    }

    public void AddExperience(int experience)
    {
        TotalExperience += experience;

        LevelUpCheck();
    }

    public void LevelUpCheck()
    {
        var levelData = Manager_CharacterLevels.AllLevelUpData[Level];

        if (TotalExperience >= levelData.TotalExperienceRequired)
        {
            _levelUp(levelData);
        }
    }

    void _levelUp(CharacterLevelData levelData)
    {
        Level = levelData.Level;
        TotalSkillPoints += levelData.SkillPoints;
        TotalSPECIALPoints += levelData.SPECIALPoints;

        switch (levelData.BonusType)
        {
            case LevelUpBonusType.Health:
                _actor.ActorData.ActorStats.CombatStats.MaxHealth += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Mana:
                _actor.ActorData.ActorStats.CombatStats.MaxMana += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Stamina:
                _actor.ActorData.ActorStats.CombatStats.MaxStamina += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Skillset:
                CanAddNewSkillSet = true;
                break;
            default:
                Debug.Log("LevelData Bonus Type was none.");
                break;
        }
    }
}
