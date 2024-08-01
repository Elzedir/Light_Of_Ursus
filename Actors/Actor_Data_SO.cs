using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actor", menuName = "Actor/Actor Data")]
public class Actor_Data_SO : ScriptableObject
{
    public BasicIdentification BasicIdentification;
    public FullIdentification FullIdentification;
    public WorldState_Data_SO Worldstate;

    public AttributesCareerAndPersonality AttributesCareerAndPersonality;
    public StatsAndAbilities StatsAndAbilities;
    public InventoryAndEquipment InventoryAndEquipment;
    public ActorQuests ActorQuests;

    // Split them into the categories from chatgpt

    public void Initialise(Actor_Base actor)
    {
        //ActorAspects.InitialiseAspects(actor);
    }

    public void InitialiseNewData(FullIdentification fullIdentification, ActorQuests actorQuests, AttributesCareerAndPersonality attributesCareerAndPersonality,
        InventoryAndEquipment inventoryAndEquipment, StatsAndAbilities statsAndAbilities, WorldState_Data_SO worldState)
    {
        FullIdentification = fullIdentification;
        BasicIdentification = fullIdentification.BasicIdentification;

        Worldstate = worldState;

        AttributesCareerAndPersonality = attributesCareerAndPersonality;
        StatsAndAbilities = statsAndAbilities;
        InventoryAndEquipment = inventoryAndEquipment;
        ActorQuests = actorQuests;
    }
}

[Serializable]
public class BasicIdentification
{
    public int ActorID;
    public Actor_Base Actor;
    public ActorName ActorName;

    public BasicIdentification(int actorID, Actor_Base actor, ActorName actorName)
    {
        ActorID = actorID;
        Actor = actor;
        ActorName = actorName;
    }
}

[Serializable]

public class FullIdentification
{
    public int ActorID;
    public Actor_Base Actor;
    public ActorName ActorName;
    public Family ActorFamily;
    public FactionName ActorFaction;
    public Background Background;
    public BasicIdentification BasicIdentification;

    public FullIdentification(int actorID, Actor_Base actor, ActorName actorName, FactionName actorFaction)
    {
        ActorID = actorID;
        Actor = actor;
        ActorName = actorName;
        ActorFaction = actorFaction;

        BasicIdentification = new BasicIdentification(actorID, actor, actorName);
    }
}

[Serializable]
public class Background
{
    public string Birthplace;
    public Date Birthdate;
    public Family ActorFamily;
    public Dynasty ActorDynasty;
    public string Religion;

    public Background()
    {

    }
}

[Serializable]
public class Relationships
{
    public List<Relation> AllRelationships;

    public Relationships()
    {

    }
}

[Serializable]
public class AttributesCareerAndPersonality
{
    public SpeciesName ActorSpecies;
    public CareerName ActorCareer;
    public ActorPersonality ActorPersonality;

    public AttributesCareerAndPersonality(SpeciesName actorSpecies, CareerName actorCareer, ActorPersonality actorPersonality)
    {
        ActorSpecies = actorSpecies;
        ActorCareer = actorCareer;
        ActorPersonality = actorPersonality;
    }
}

[Serializable]
public class StatsAndAbilities
{
    public ActorStats ActorStats;
    public Actor_States_And_Conditions ActorStates;
    public ActorAspects ActorAspects;
    public ActorAbilities ActorAbilities;

    public StatsAndAbilities()
    {

    }
}

[Serializable]
public class InventoryAndEquipment
{
    public ActorInventory ActorInventory;
    public ActorEquipment ActorEquipment;

    public InventoryAndEquipment()
    {

    }
}

[Serializable]
public class ActorName
{
    public string Name;
    public string Surname;
    public Title CurrentTitle;
    public List<Title> AvailableTitles;

    public ActorName(string name, string surname)
    {
        Name = name;
        Surname = surname;
    }

    public string GetName()
    {
        // Somehow integrate the title with the names
        
        return $"{Name} {Surname}";
    }

    public void SetTitleAsCurrentTitle(TitleName titleName)
    {
        Manager_Title.GetTitle(titleName, out Title title);
        if (AvailableTitles.Contains(title)) throw new ArgumentException($"Title: {title.TitleName} does not exist in AllTitles.");

        CurrentTitle = title;
    }
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

    public ActorLevelData ActorLevelData;
    public int ActorGold;
    public SPECIAL ActorSpecial;

    [SerializeField] CombatStats _combatStats; 
    public CombatStats CombatStats { get { return _combatStats; } set { _combatStats = value; } }

    public Dictionary<Vocation, float> VocationStats;
}

[Serializable]
public class ActorAspects
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
                _actor.ActorData.StatsAndAbilities.ActorStats.CombatStats.MaxHealth += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Mana:
                _actor.ActorData.StatsAndAbilities.ActorStats.CombatStats.MaxMana += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Stamina:
                _actor.ActorData.StatsAndAbilities.ActorStats.CombatStats.MaxStamina += levelData.BonusStatPoints;
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
