using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ActorData
{
    public int ActorID;
    public int ActorFactionID;
    public ActorName ActorName;

    public FullIdentification FullIdentification;

    public GameObjectProperties GameObjectProperties;

    public WorldState_Data_SO Worldstate;
    public CareerAndJobs CareerAndJobs;
    public SpeciesAndPersonality SpeciesAndPersonality;
    public StatsAndAbilities StatsAndAbilities;
    public InventoryAndEquipment InventoryAndEquipment;
    public ActorQuests ActorQuests;

    public void PrepareForInitialisation()
    {
        Manager_Initialisation.OnInitialiseActorData += InitialiseActorData;
    }

    public void InitialiseActorData()
    {
        if (Manager_Actor.GetActor(ActorFactionID, ActorID, out Actor_Base actor) != null)
        {
            actor.SetActorData(Manager_Actor.GetActorData(ActorFactionID, ActorID, out ActorData actorData));
            FullIdentification.Initialise();
            CareerAndJobs.Initialise();
        } 
        else
        {
            Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
        }
    }

    public ActorData(FullIdentification fullIdentification, GameObjectProperties gameObjectProperties, WorldState_Data_SO worldState, 
        CareerAndJobs careerAndJobs,  SpeciesAndPersonality speciesAndPersonality,InventoryAndEquipment inventoryAndEquipment, StatsAndAbilities statsAndAbilities, 
        ActorQuests actorQuests)
    {
        FullIdentification = fullIdentification;

        ActorID = FullIdentification.ActorID;
        ActorFactionID = FullIdentification.ActorFactionID;
        ActorName = FullIdentification.ActorName;
        
        GameObjectProperties = gameObjectProperties;

        Worldstate = worldState;

        CareerAndJobs = careerAndJobs;
        CareerAndJobs.SetActorAndFactionID(ActorID, ActorFactionID);
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
    public ActorName ActorName;
    public int ActorFactionID;
    public int ActorCityID;
    public Family ActorFamily;
    public Background Background;

    public FullIdentification(int actorID, ActorName actorName, int actorFactionID, int actorCityID)
    {
        ActorID = actorID;
        ActorName = actorName;
        ActorFactionID = actorFactionID;
        ActorCityID = actorCityID;
    }

    public void Initialise()
    {
        Manager_City.GetCityData(cityID: ActorCityID);
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
public class CareerAndJobs : ITickable
{
    public int ActorID;
    public int FactionID;
    public CareerName ActorCareer;
    public CraftingComponent Crafting;

    public List<JobData> ActorJobs;

    public bool JobsActive;

    Coroutine _jobCoroutine;

    public CareerAndJobs(CareerName actorCareer, List<JobData> actorJobs, bool jobsActive = false)
    {
        ActorCareer = actorCareer;
        JobsActive = jobsActive;
        ActorJobs = actorJobs;

        Initialise();
    }

    public void SetActorAndFactionID(int actorID, int factionID)
    {
        ActorID = actorID;
        FactionID = factionID;
    }

    public void Initialise()
    {
        Manager_TickRate.RegisterTickable(this);
    }

    public void AddJob(JobName jobName, int jobsiteID)
    {
        if (ActorJobs.Any(j => j.JobName == jobName && j.JobsiteID == jobsiteID)) return;

        ActorJobs.Add(new JobData(jobName, jobsiteID));
    }

    public void RemoveJob(JobName jobName, int jobsiteID)
    {
        if (!ActorJobs.Any(j => j.JobName == jobName && j.JobsiteID == jobsiteID)) return;

        ActorJobs.Remove(ActorJobs.FirstOrDefault(j => j.JobName == jobName && j.JobsiteID == jobsiteID));
    }

    public void ReorganiseJobs(JobName jobName, int jobsiteID, int index)
    {
        if (index < 0 || index > ActorJobs.Count) { Debug.Log($"Index: {index} is less than 0 or greater than ActorJobs length: {ActorJobs.Count}."); return; }

        for (int i = ActorJobs.Count - 1; i > index; i--)
        {
            ActorJobs[i] = ActorJobs[i - 1];
        }

        ActorJobs[index] = new JobData(jobName, jobsiteID);
    }

    public void OnTick()
    {
        Manager_Game.Instance.StartCoroutine(PerformJobs());
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneSecond;
    }

    public void ToggleDoJobs(bool jobsActive)
    {
        JobsActive = jobsActive;
    }

    public IEnumerator PerformJobs()
    {
        if (_jobCoroutine != null) yield break;

        foreach (var jobData in ActorJobs)
        {
            yield return _jobCoroutine = 
                Manager_Game.Instance.StartCoroutine(Manager_Job.GetJob(jobData.JobName, jobData.JobsiteID)
                .PerformJob(Manager_Actor.GetActor(FactionID, ActorID, out Actor_Base actor)));
        }

        _jobCoroutine = null;
    }
}

[Serializable]
public class JobData
{
    public JobName JobName;
    public int JobsiteID;

    public JobData(JobName jobName, int jobsiteID)
    {
        JobName = jobName;
        JobsiteID = jobsiteID;
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
    public InventoryData Inventory;
    public ActorEquipment Equipment;

    public InventoryAndEquipment(InventoryData inventory, ActorEquipment equipment)
    {
        Inventory = inventory;
        Equipment = equipment;
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
