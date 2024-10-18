using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ActorData
{
    public void UpdateActorData()
    {
        GameObjectProperties.UpdateActorData();
    }

    public uint ActorID;
    public uint ActorFactionID;
    public ActorName ActorName;

    public FullIdentification FullIdentification;

    public GameObjectProperties GameObjectProperties;
    public WorldStateData WorldstateData;
    public CareerAndJobs CareerAndJobs;
    public CraftingData CraftingData;
    public VocationData VocationData;
    public SpeciesAndPersonality SpeciesAndPersonality;
    public StatsAndAbilities StatsAndAbilities;
    public InventoryData InventoryData;
    public EquipmentData EquipmentData;
    public QuestData ActorQuests;
    //public OrderData OrderData;
    public Order_Base CurrentOrder;

    public void PrepareForInitialisation()
    {
        Manager_Initialisation.OnInitialiseActorData += InitialiseActorData;
    }

    public void InitialiseActorData()
    {
        var actor = Manager_Actor.GetActor(ActorID, true);

        if ( actor != null)
        {
            var actorFaction = Manager_Faction.GetFaction(ActorFactionID);

            if (actorFaction == null)
            {
                Debug.LogError($"Actor {ActorID} cannot find faction {ActorFactionID}.");
                return;
            }

            var factionGO = GameObject.Find($"{actorFaction.FactionID}: {actorFaction.FactionName}");

            if (factionGO == null)
            {
                Debug.LogError($"Actor {ActorID} cannot find faction GameObject {actorFaction.FactionID}: {actorFaction.FactionName}.");
                return;
            }

            actor.transform.parent.SetParent(factionGO.transform);
        } 
        else
        {
            Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
        }
    }

    // Make an ability to make a deep copy of every class here and every class that needs to be saved.

    public ActorData(FullIdentification fullIdentification)
    {
        FullIdentification = fullIdentification;

        ActorID = FullIdentification.ActorID;
        ActorFactionID = FullIdentification.ActorFactionID;
        ActorName = FullIdentification.ActorName;

        GameObjectProperties = new GameObjectProperties(ActorID);
        WorldstateData = new WorldStateData(ActorID);
        CareerAndJobs = new CareerAndJobs(ActorID);
        CraftingData = new CraftingData(ActorID);
        VocationData = new VocationData(ActorID);
        SpeciesAndPersonality = new SpeciesAndPersonality(ActorID);
        StatsAndAbilities = new StatsAndAbilities(ActorID);

        InventoryData = new InventoryData(ActorID);
        EquipmentData = new EquipmentData(ActorID);
        ActorQuests = new QuestData(ActorID);
        //OrderData = new OrderData(ActorID);
        CurrentOrder = null;
    }
}

[CustomPropertyDrawer(typeof(ActorData))]
public class ActorData_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var actorIDProp = property.FindPropertyRelative("ActorID");
        var actorNameProp = property.FindPropertyRelative("ActorName");
        var nameProp = actorNameProp.FindPropertyRelative("Name");
        var surnameProp = actorNameProp.FindPropertyRelative("Surname");
        var name = "Nameless";

        if (surnameProp != null)
        {
            name = $"{nameProp.stringValue} {surnameProp.stringValue}";
        }
        else
        {
            name = $"{nameProp.stringValue}";
        }

        label.text = $"{actorIDProp.intValue}: {name}";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

[Serializable]

public class FullIdentification
{
    public uint ActorID;
    public ActorName ActorName;
    public uint ActorFactionID;
    public uint ActorCityID;
    public Date ActorBirthDate;
    public float ActorAge => ActorBirthDate.GetAge();
    public Family ActorFamily;
    public Background Background;

    public FullIdentification(uint actorID, ActorName actorName, uint actorFactionID, uint actorCityID)
    {
        ActorID = actorID;
        ActorName = actorName;
        ActorFactionID = actorFactionID;
        ActorCityID = actorCityID;
    }
}

[Serializable]
public class Background
{
    public uint ActorID;

    public string Birthplace;
    public Date Birthdate;
    public Family ActorFamily;
    public Dynasty ActorDynasty;
    public string Religion;

    public Background(uint actorID) => ActorID = actorID;
}

[Serializable]
public class GameObjectProperties
{
    public void UpdateActorData()
    {
        SetActorTransformProperties();
    }

    public uint ActorID;
    [NonSerialized] Transform _actorTransform;
    public Transform ActorTransform { get { return _actorTransform ??= Manager_Actor.GetActor(ActorID)?.transform; } }
    public void SetActorTransformProperties()
    {
        if (ActorTransform == null)
        {
            Debug.Log($"ActorTransform for actor {ActorID} is null.");
            return;
        }

        _setActorPosition(ActorTransform.position);
        _setActorRotation(ActorTransform.rotation);
        _setActorScale(ActorTransform.localScale);
    }
    public Vector3 LastSavedActorPosition;
    void _setActorPosition(Vector3 actorPosition) => LastSavedActorPosition = actorPosition;
    public Quaternion LastSavedActorRotation;
    void _setActorRotation(Quaternion actorRotation) => LastSavedActorRotation = actorRotation;
    public Vector3 LastSavedActorScale;
    void _setActorScale(Vector3 actorScale) => LastSavedActorScale = actorScale;
    public Mesh ActorMesh;
    public void SetActorMesh(Mesh actorMesh) => ActorMesh = actorMesh;
    public Material ActorMaterial;
    public void SetActorMaterial(Material actorMaterial) => ActorMaterial = actorMaterial;

    public GameObjectProperties(uint actorID) => ActorID = actorID;

    public void SetGameObjectProperties(Transform actorTransform)
    {
        _actorTransform = actorTransform;

        ActorMesh = ActorTransform.GetComponent<MeshFilter>().sharedMesh;
        ActorMaterial = ActorTransform.GetComponent<MeshRenderer>().sharedMaterial;

        // Temporary

        ActorMesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx"); // Later will come from species
        ActorMaterial ??= Resources.Load<Material>("Materials/Material_Red"); // Later will come from species
    }
}

[Serializable]
public class WorldStateData
{
    public uint ActorID;
    public WorldStateData(uint actorID) => ActorID = actorID;
}

[Serializable]
public class Relationships
{
    public uint ActorID;

    public List<Relation> AllRelationships;

    public Relationships(uint actorID) => ActorID = actorID;
}

[Serializable]
public class CareerAndJobs
{
    public uint ActorID;

    public bool JobsActive;
    public void ToggleDoJobs(bool jobsActive) => JobsActive = jobsActive;

    public uint JobsiteID;
    public void SetJobsiteID(uint jobsiteID) => JobsiteID = jobsiteID;

    public uint StationID;
    public void SetStationID(uint stationID) => StationID = stationID;
    
    public uint OperatingAreaID;
    public void SetOperatingAreaID(uint operatingAreaID) => OperatingAreaID = operatingAreaID;

    public EmployeePosition EmployeePosition;
    public void SetEmployeePosition(EmployeePosition employeePosition) => EmployeePosition = employeePosition;

    public CareerAndJobs(uint actorID) => ActorID = actorID;
}

[Serializable]
public class SpeciesAndPersonality
{
    public uint ActorID;

    public SpeciesName ActorSpecies;
    public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
    public ActorPersonality ActorPersonality;
    public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

    public SpeciesAndPersonality(uint actorID) => ActorID = actorID;
}

[Serializable]
public class StatsAndAbilities
{
    public uint ActorID;
    public StatsAndAbilities(uint actorID) => ActorID = actorID;

    public Actor_Stats Actor_Stats;
    public void SetActorStats(Actor_Stats actorStats) => Actor_Stats = actorStats;

    public Actor_StatesAndConditions Actor_StatesAndConditions;
    public void SetActorStates(Actor_StatesAndConditions actorStates) => Actor_StatesAndConditions = actorStates;

    public Actor_Aspects Actor_Aspects;
    public void SetActorAspects(Actor_Aspects actor_Aspects) => Actor_Aspects = actor_Aspects;

    public Actor_Abilities Actor_Abilities;
    public void SetActorAbilities(Actor_Abilities actor_Abilities) => Actor_Abilities = actor_Abilities;
}

[Serializable]
public class ActorName
{
    public string Name;
    public string Surname;
    public string GetName() => $"{Name} {Surname}";
    public TitleName CurrentTitle;
    public List<TitleName> AvailableTitles;
    public void SetTitleAsCurrentTitle(TitleName titleName) { if (AvailableTitles.Contains(titleName)) CurrentTitle = titleName; }

    public ActorName(string name, string surname)
    {
        Name = name;
        Surname = surname;
    }
}

public enum SpeciesName
{
    Default,
    Demon,
    Human,
    Orc
}

[Serializable]
public class Actor_Stats
{
    public uint ActorID;
    public Actor_Stats(uint actorID) => ActorID = actorID;

    public ActorLevelData ActorLevelData;
    public SPECIAL ActorSpecial;
    public CombatStats CombatStats;


    public float TotalCarryWeight => ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.
    public float AvailableCarryWeight => TotalCarryWeight - Manager_Actor.GetActorData(ActorID).InventoryData.GetTotalInventoryWeight();
}

[Serializable]
public class Actor_Aspects
{
    public uint ActorID;
    public Actor_Aspects(uint actorID) => ActorID = actorID;

    public ClassTitle ActorClassTitle { get { return Manager_Aspect.GetCharacterTitle(ActorAspectList); } }

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
    public uint ActorID;
    public uint ActorLevel;
    public uint TotalExperience;
    public uint TotalSkillPoints;
    public uint UsedSkillPoints;
    public uint TotalSPECIALPoints;
    public uint UsedSPECIALPoints;
    public bool CanAddNewSkillSet;

    public ActorLevelData(uint actorID, uint level = 1, uint totalExperience = 0, uint totalSkillPoints = 0, uint totalSPECIALPoints = 0, bool canAddNewSkillSet = false)
    {
        ActorID = actorID;
        ActorLevel = level;
        TotalExperience = totalExperience;
        TotalSkillPoints = totalSkillPoints;
        TotalSPECIALPoints = totalSPECIALPoints;
        CanAddNewSkillSet = canAddNewSkillSet;
    }

    public void AddExperience(uint experience)
    {
        TotalExperience += experience;

        LevelUpCheck();
    }

    public void LevelUpCheck()
    {
        var levelData = Manager_CharacterLevels.AllLevelUpData[(int)ActorLevel];

        if (TotalExperience >= levelData.TotalExperienceRequired)
        {
            _levelUp(levelData);
        }
    }

    void _levelUp(CharacterLevelData levelData)
    {
        var actorData = Manager_Actor.GetActorData(ActorID);

        ActorLevel = levelData.Level;
        TotalSkillPoints += levelData.SkillPoints;
        TotalSPECIALPoints += levelData.SPECIALPoints;

        switch (levelData.BonusType)
        {
            case LevelUpBonusType.Health:
                actorData.StatsAndAbilities.Actor_Stats.CombatStats.MaxHealth += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Mana:
                actorData.StatsAndAbilities.Actor_Stats.CombatStats.MaxMana += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Stamina:
                actorData.StatsAndAbilities.Actor_Stats.CombatStats.MaxStamina += levelData.BonusStatPoints;
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

[Serializable]
public class CraftingData
{
    public uint ActorID;

    public List<RecipeName> KnownRecipes = new();

    public CraftingData(uint actorID) => ActorID = actorID;

    public bool AddRecipe(RecipeName recipeName)
    {
        if (KnownRecipes.Contains(recipeName)) return false;

        KnownRecipes.Add(recipeName);

        return true;
    }

    public IEnumerator CraftItemAll(RecipeName recipeName)
    {
        var recipe = Manager_Recipe.GetRecipe(recipeName);

        var actorData = Manager_Actor.GetActorData(ActorID);

        while (inventoryContainsAllIngredients(recipe.RequiredIngredients))
        {
            yield return CraftItem(recipeName);
        }

        bool inventoryContainsAllIngredients(List<Item> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                var inventoryItem = actorData.InventoryData.GetItemFromInventory(ingredient.ItemID);

                if (inventoryItem == null || inventoryItem.ItemAmount < ingredient.ItemAmount)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public IEnumerator CraftItem(RecipeName recipeName)
    {
        if (!KnownRecipes.Contains(recipeName)) { Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}"); yield break; }

        Recipe recipe = Manager_Recipe.GetRecipe(recipeName);

        var actorData = Manager_Actor.GetActorData(ActorID);

        if (actorData.InventoryData.RemoveFromInventory(recipe.RequiredIngredients))
        {
            if (actorData.InventoryData.AddToInventory(recipe.RequiredIngredients))
            {
                yield break;
            }

            actorData.InventoryData.AddToInventory(recipe.RequiredIngredients);
            Debug.Log($"Cannot add products into inventory"); 
            yield break;
        }
        else
        {
            Debug.Log($"Inventory does not have all required ingredients");
            yield break;
        }
    }
}



[Serializable]
public class QuestData
{
    public uint ActorID;

    public List<Quest> ActorQuests = new();
    public void SetStage(int QuestID, int stageID, int stageProgress)
    {
        ActorQuests.FirstOrDefault(q => q.QuestID == QuestID).SetQuestStage(stageID, stageProgress);
    }

    public QuestData(uint actorID) => ActorID = actorID;
}

[Serializable]
public class VocationData
{
    public uint ActorID;

    public List<ActorVocation> ActorVocations = new();
    public void SetVocations(List<ActorVocation> vocations) => ActorVocations = vocations;

    public VocationData(uint actorID) => ActorID = actorID;

    public void AddVocation(VocationName vocationName, float vocationExperience)
    {
        if (ActorVocations.Any(v => v.VocationName == vocationName)) 
        {
            Debug.Log($"Vocation: {vocationName} already exists in Vocations.");
            return;
        }

        ActorVocations.Add(new ActorVocation(vocationName, vocationExperience));
    }

    public void RemoveVocation(VocationName vocationName)
    {
        if (!ActorVocations.Any(v => v.VocationName == vocationName)) 
        {
            Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
            return;
        }

        ActorVocations.Remove(ActorVocations.First(v => v.VocationName == vocationName));
    }

    public void ChangeVocationExperience(VocationName vocationName, float experienceChange)
    {
        if (!ActorVocations.Any(v => v.VocationName == vocationName)) 
        {
            Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
            return;
        }

        ActorVocations.First(v => v.VocationName == vocationName).VocationExperience += experienceChange;
    }

    public float GetVocationExperience(VocationName vocationName)
    {
        if (!ActorVocations.Any(v => v.VocationName == vocationName)) 
        {
            Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
            return 0;
        }

        return ActorVocations.First(v => v.VocationName == vocationName).VocationExperience;
    }

    public float GetProgress(VocationRequirement vocationRequirement)
    {
        var currentExperience = GetVocationExperience(vocationRequirement.VocationName);

        if (currentExperience < vocationRequirement.MinimumVocationExperience)
        {
            return 0;
        }

        var progress = ((currentExperience - vocationRequirement.ExpectedVocationExperience) / Math.Max(currentExperience, 1));

        if (progress < 0) return 1 / Math.Abs(progress);

        return progress;
    }
}

[Serializable]
public class ActorVocation
{
    public VocationName VocationName;
    public VocationTitle VocationTitle;
    public float VocationExperience;
    public ActorVocation(VocationName vocationName, float vocationExperience)
    {
        VocationName = vocationName;
        VocationExperience = vocationExperience;

        // Impement later
        //VocationTitle = Manager_Vocation.GetVocation(vocationName).GetVocationTitle(vocationExperience);
    }
}