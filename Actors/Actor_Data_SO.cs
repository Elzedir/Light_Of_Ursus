using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayableRace
{
    Demon,
    Human,
    Orc
}

public enum NonPlayableType
{
    Crate,
    Box,
    Tree
}

[CreateAssetMenu(fileName = "New Character", menuName = "Character/Character Data")]
public class Actor_Data_SO : ScriptableObject
{
    [SerializeField] private int _actorID;
    public int ActorID { get { return _actorID; } private set { _actorID = value; } }

    public ActorName ActorName;

    public bool Playable;
    public string CharacterName;
    public FactionName Faction;
    private PlayableRace _playableRace;
    private NonPlayableType _nonPlayableType;

    public Worldstate Worldstate;
    public PlayableRace PlayableRace
    {
        get { return _playableRace; }
        set { _playableRace = value; }
    }
    public NonPlayableType NonPlayableType
    {
        get { return _nonPlayableType; }
        set { _nonPlayableType = value; }
    }

    Actor_States _actorStates;
    public ActorStats ActorStats;
    public Aspects ActorAspects;
    //public Inventory ActorInventory;
    //public Equipment ActorEquipment;
    public Abilities ActorAbilities;
    public ActorQuests ActorQuests;

    public void Initialise(Actor_Base actor)
    {
        if (Manager_Game.ActorIDs.Contains(_actorID))
        {
            throw new ArgumentException($"ActorID: {_actorID} has already been used");
        }

        if (Playable)
        {
            ActorAspects.InitialiseAspects(actor);
        }
    }
}

[Serializable]
public class ActorName
{
    public string Name;
    public string Surname;
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

[System.Serializable]
public class CombatStats
{
    [SerializeField] private bool _initialised; public bool Initialised { get { return _initialised; } }

    public float CurrentHealth;
    public float CurrentMana;
    public float CurrentStamina;
    public float MaxHealth;
    public float MaxMana;
    public float MaxStamina;
    public float PushRecovery;

    public float AttackDamage;
    public float AttackSpeed;
    public float AttackSwingTime;
    public float AttackRange;
    public float AttackPushForce;
    public float AttackCooldown;

    public float PhysicalDefence;
    public float MagicalDefence;

    public float MoveSpeed;
    public float DodgeCooldownReduction;

    public CombatStats(
        bool initialised = true,
        float currentHealth = 0,
        float currentMana = 0,
        float currentStamina = 0,
        float maxHealth = 1,
        float maxMana = 1,
        float maxStamina = 1,
        float pushRecovery = 1,
        float attackDamage = 1,
        float attackSpeed = 1,
        float attackSwingTime = 1,
        float attackRange = 1,
        float attackPushForce = 1,
        float attackCooldown = 1,
        float physicalDefence = 0,
        float magicalDefence = 0,
        float moveSpeed = 1,
        float dodgeCooldown = 1)
    {
        _initialised = initialised;
        CurrentHealth = currentHealth;
        CurrentMana = currentMana;
        CurrentStamina = currentStamina;
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        MaxStamina = maxStamina;
        PushRecovery = pushRecovery;

        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        AttackSwingTime = attackSwingTime;
        AttackRange = attackRange;
        AttackPushForce = attackPushForce;
        AttackCooldown = attackCooldown;

        PhysicalDefence = physicalDefence;
        MagicalDefence = magicalDefence;

        MoveSpeed = moveSpeed;
        DodgeCooldownReduction = dodgeCooldown;
    }

    public void Initialise(CombatStats combatStats)
    {
        combatStats = new CombatStats();
    }

    public CombatStats(CombatStats original)
    {
        _initialised = original._initialised;

        CurrentHealth = original.CurrentHealth;
        CurrentMana = original.CurrentMana;
        CurrentStamina = original.CurrentStamina;
        MaxHealth = original.MaxHealth;
        MaxMana = original.MaxMana;
        MaxStamina = original.MaxStamina;
        PushRecovery = original.PushRecovery;

        AttackDamage = original.AttackDamage;
        AttackSpeed = original.AttackSpeed;
        AttackSwingTime = original.AttackSwingTime;
        AttackRange = original.AttackRange;
        AttackPushForce = original.AttackPushForce;
        AttackCooldown = original.AttackCooldown;

        PhysicalDefence = original.PhysicalDefence;
        MagicalDefence = original.MagicalDefence;

        MoveSpeed = original.MoveSpeed;
        DodgeCooldownReduction = original.DodgeCooldownReduction;
    }

    //public static CombatStats operator +(CombatStats a, CombatStats b)
    //{
    //    return new CombatStats
    //    {
    //        MaxHealth = a.MaxHealth + b.MaxHealth,
    //        MaxMana = a.MaxMana + b.MaxMana,
    //        MaxStamina = a.MaxStamina + b.MaxStamina,
    //        PushRecovery = a.PushRecovery + b.PushRecovery,
    //        AttackDamage = a.AttackDamage + b.AttackDamage,
    //        AttackSpeed = a.AttackSpeed + b.AttackSpeed,
    //        AttackSwingTime = a.AttackSwingTime + b.AttackSwingTime,
    //        AttackRange = a.AttackRange + b.AttackRange,
    //        AttackPushForce = a.AttackPushForce + b.AttackPushForce,
    //        AttackCooldown = a.AttackCooldown + b.AttackCooldown,
    //        PhysicalDefence = a.PhysicalDefence + b.PhysicalDefence,
    //        MagicalDefence = a.MagicalDefence + b.MagicalDefence,
    //        DodgeCooldownReduction = a.DodgeCooldownReduction + b.DodgeCooldownReduction
    //    };
    //}

    //public static CombatStats operator +(CombatStats a, FixedModifiers b)
    //{
    //    return new CombatStats
    //    {
    //        MaxHealth = a.MaxHealth + b.MaxHealth,
    //        MaxMana = a.MaxMana + b.MaxMana,
    //        MaxStamina = a.MaxStamina + b.MaxStamina,
    //        PushRecovery = a.PushRecovery + b.PushRecovery,
    //        AttackDamage = a.AttackDamage + b.AttackDamage,
    //        AttackSpeed = a.AttackSpeed + b.AttackSpeed,
    //        AttackSwingTime = a.AttackSwingTime + b.AttackSwingTime,
    //        AttackRange = a.AttackRange + b.AttackRange,
    //        AttackPushForce = a.AttackPushForce + b.AttackPushForce,
    //        AttackCooldown = a.AttackCooldown + b.AttackCooldown,
    //        PhysicalDefence = a.PhysicalDefence + b.PhysicalDefence,
    //        MagicalDefence = a.MagicalDefence + b.MagicalDefence,
    //        DodgeCooldownReduction = a.DodgeCooldownReduction + b.DodgeCooldownReduction
    //    };
    //}

    //public static CombatStats operator *(CombatStats a, PercentageModifiers b)
    //{
    //    return new CombatStats
    //    {
    //        MaxHealth = b.MaxHealth != 0 ? a.MaxHealth * b.MaxHealth : a.MaxHealth,
    //        MaxMana = b.MaxMana != 0 ? a.MaxMana * b.MaxMana : a.MaxMana,
    //        MaxStamina = b.MaxStamina != 0 ? a.MaxStamina * b.MaxStamina : a.MaxStamina,
    //        PushRecovery = b.PushRecovery != 0 ? a.PushRecovery * b.PushRecovery : a.PushRecovery,
    //        AttackDamage = b.AttackDamage != 0 ? a.AttackDamage * b.AttackDamage : a.AttackDamage,
    //        AttackSpeed = b.AttackSpeed != 0 ? a.AttackSpeed * b.AttackSpeed : a.AttackSpeed,
    //        AttackSwingTime = b.AttackSwingTime != 0 ? a.AttackSwingTime * b.AttackSwingTime : a.AttackSwingTime,
    //        AttackRange = b.AttackRange != 0 ? a.AttackRange * b.AttackRange : a.AttackRange,
    //        AttackPushForce = b.AttackPushForce != 0 ? a.AttackPushForce * b.AttackPushForce : a.AttackPushForce,
    //        AttackCooldown = b.AttackCooldown != 0 ? a.AttackCooldown * b.AttackCooldown : a.AttackCooldown,
    //        PhysicalDefence = b.PhysicalDefence != 0 ? a.PhysicalDefence * b.PhysicalDefence : a.PhysicalDefence,
    //        MagicalDefence = b.MagicalDefence != 0 ? a.MagicalDefence * b.MagicalDefence : a.MagicalDefence,
    //        DodgeCooldownReduction = b.DodgeCooldownReduction != 0 ? a.DodgeCooldownReduction * b.DodgeCooldownReduction : a.DodgeCooldownReduction
    //    };
    //}
}

[Serializable]
public class SPECIAL
{
    public int Agility; // Dexterity
    public int Charisma;
    public int Endurance; // Constitution
    public int Intelligence;
    public int Luck;
    public int Perception; // Wisdom
    public int Strength;
}

[Serializable]
public struct Aspects
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
public class Abilities
{
    public Dictionary<Ability, float> AbilityList = new();
}

[Serializable]
public class ActorQuests
{
    public List<Quest> MainQuestLine;

    public List<Quest> QuestList;
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

    void _levelUp(LevelData levelData)
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
