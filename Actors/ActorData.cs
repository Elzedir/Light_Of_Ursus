using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ActorData
{
    public int ActorID;
    public ActorName ActorName;

    public bool OverwriteDataInActor = true;

    public FullIdentification FullIdentification;

    public GameObjectProperties GameObjectProperties;

    public WorldState_Data_SO Worldstate;
    public CareerAndJobs CareerAndJobs;
    public SpeciesAndPersonality SpeciesAndPersonality;
    public StatsAndAbilities StatsAndAbilities;
    public InventoryAndEquipment InventoryAndEquipment;
    public ActorQuests ActorQuests;

    public void InitialiseActorData()
    {
        if (OverwriteDataInActor) Manager_Actor.AddToOrUpdateAllActorList(this);
        else if (Manager_Actor.GetActor(ActorID, out Actor_Base actor) != null)
        {
            actor.SetActorData(Manager_Actor.GetActorDataFromID(ActorID, out ActorData actorData));
        } 
    }

    public ActorData(FullIdentification fullIdentification, GameObjectProperties gameObjectProperties, WorldState_Data_SO worldState, 
        CareerAndJobs careerAndJobs,  SpeciesAndPersonality speciesAndPersonality,InventoryAndEquipment inventoryAndEquipment, StatsAndAbilities statsAndAbilities, 
        ActorQuests actorQuests)
    {
        FullIdentification = fullIdentification;

        ActorID = FullIdentification.ActorID;
        ActorName = FullIdentification.ActorName;
        
        GameObjectProperties = gameObjectProperties;

        Worldstate = worldState;

        CareerAndJobs = careerAndJobs;
        SpeciesAndPersonality = speciesAndPersonality;
        StatsAndAbilities = statsAndAbilities;
        InventoryAndEquipment = inventoryAndEquipment;
        ActorQuests = actorQuests;
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

    public FullIdentification(int actorID, Actor_Base actor, ActorName actorName, FactionName actorFaction)
    {
        ActorID = actorID;
        Actor = actor;
        ActorName = actorName;
        ActorFaction = actorFaction;
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
public class GameObjectProperties
{
    public Vector3 ActorPosition;
    public Quaternion ActorRotation;
    public Vector3 ActorScale = Vector3.one;
    public Mesh ActorMesh;
    public Material ActorMaterial;

    public GameObjectProperties(Vector3 actorPosition, Quaternion actorRotation, Vector3 actorScale, Mesh actorMesh, Material actorMaterial)
    {
        ActorPosition = actorPosition;
        ActorRotation = actorRotation;
        ActorScale = actorScale;
        ActorMesh = actorMesh;
        ActorMaterial = actorMaterial;
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
public class CareerAndJobs
{
    public CareerName ActorCareer;
    public List<Job> ActorJobs;

    public CareerAndJobs(CareerName actorCareer, List<Job> actorJobs)
    {
        ActorCareer = actorCareer;
        ActorJobs = actorJobs;
    }
}

[Serializable]
public class SpeciesAndPersonality
{
    public SpeciesName ActorSpecies;
    public ActorPersonality ActorPersonality;

    public SpeciesAndPersonality(SpeciesName actorSpecies, ActorPersonality actorPersonality)
    {
        ActorSpecies = actorSpecies;
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

    public InventoryAndEquipment(ActorInventory actorInventory, ActorEquipment actorEquipment)
    {
        ActorInventory = actorInventory;
        ActorEquipment = actorEquipment;
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

[CustomPropertyDrawer(typeof(ActorData))]
public class ActorData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty actorNameProp = property.FindPropertyRelative("ActorName");

        if (actorNameProp != null)
        {
            SerializedProperty nameProp = actorNameProp.FindPropertyRelative("Name");
            
            if (nameProp != null)
            {
                SerializedProperty surnameProp = actorNameProp.FindPropertyRelative("Surname");

                if (surnameProp != null)
                {
                    label.text = $"{nameProp.stringValue} {surnameProp.stringValue}";
                }
                else
                {
                    label.text = $"{nameProp.stringValue}";
                }
            }
            else label.text = "No Name";
        }
        else label.text = "No Name";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
