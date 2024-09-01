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
    public WorldStateData WorldstateData;
    public CareerAndJobs CareerAndJobs;
    public CraftingData CraftingData;
    public VocationData VocationData;
    public SpeciesAndPersonality SpeciesAndPersonality;
    public StatsAndAbilities StatsAndAbilities;
    public InventoryAndEquipment InventoryAndEquipment;
    public QuestData ActorQuests;

    public void PrepareForInitialisation()
    {
        Manager_Initialisation.OnInitialiseActorData += InitialiseActorData;
    }

    public void InitialiseActorData()
    {
        if (Manager_Actor.GetActor(ActorID, true) != null)
        {
            Manager_City.GetCityData(FullIdentification.ActorCityID).Population.AddCitizen(ActorID);
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
        FullIdentification.SetData(this);

        GameObjectProperties = new GameObjectProperties(this);
        WorldstateData = new WorldStateData(this);
        CareerAndJobs = new CareerAndJobs(this);
        CraftingData = new CraftingData(this);
        VocationData = new VocationData(this);
        SpeciesAndPersonality = new SpeciesAndPersonality(this);
        StatsAndAbilities = new StatsAndAbilities(this);
        InventoryAndEquipment = new InventoryAndEquipment(this);
        ActorQuests = new QuestData(this);

        ActorID = FullIdentification.ActorID;
        ActorFactionID = FullIdentification.ActorFactionID;
        ActorName = FullIdentification.ActorName;
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
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

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
}

[Serializable]
public class Background
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public string Birthplace;
    public Date Birthdate;
    public Family ActorFamily;
    public Dynasty ActorDynasty;
    public string Religion;
}

[Serializable]
public class GameObjectProperties
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;
    [NonSerialized] private Transform _actorTransform;
    public Transform ActorTransform { get { return _actorTransform ??= Manager_Actor.GetActor(_actorData.ActorID)?.transform; } }
    public Vector3 LastSavedActorPosition;
    public void SetActorPosition(Vector3 actorPosition) => LastSavedActorPosition = actorPosition;
    public Quaternion LastSavedActorRotation;
    public void SetActorRotation(Quaternion actorRotation) => LastSavedActorRotation = actorRotation;
    public Vector3 LastSavedActorScale;
    public void SetActorScale(Vector3 actorScale) => LastSavedActorScale = actorScale;
    public Mesh ActorMesh;
    public void SetActorMesh(Mesh actorMesh) => ActorMesh = actorMesh;
    public Material ActorMaterial;
    public void SetActorMaterial(Material actorMaterial) => ActorMaterial = actorMaterial;

    public GameObjectProperties(ActorData actorData)
    {
        SetData(actorData);

        ActorMesh = ActorTransform.GetComponent<MeshFilter>().sharedMesh;
        ActorMaterial = ActorTransform.GetComponent<MeshRenderer>().sharedMaterial;

        // Temporary

        ActorMesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx"); // Later will come from species
        ActorMaterial ??= Resources.Load<Material>("Materials/Material_Red"); // Later will come from species
    }
}

[Serializable]
public class Relationships
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public List<Relation> AllRelationships;
}

[Serializable]
public class CareerAndJobs
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public bool JobsActive;
    public void ToggleDoJobs(bool jobsActive) => JobsActive = jobsActive;

    public int JobsiteID;
    public void SetJobsiteID(int jobsiteID) => JobsiteID = jobsiteID;

    public int StationID;
    public void SetStationID(int stationID) => StationID = stationID;
    
    public int OperatingAreaID;
    public void SetOperatingAreaID(int operatingAreaID) => OperatingAreaID = operatingAreaID;

    public EmployeePosition EmployeePosition;
    public void SetEmployeePosition(EmployeePosition employeePosition) => EmployeePosition = employeePosition;

    public CareerAndJobs(ActorData actorData)
    {
        SetData(actorData);
    }
}

[Serializable]
public class SpeciesAndPersonality
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public SpeciesName ActorSpecies;
    public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
    public ActorPersonality ActorPersonality;
    public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

    public SpeciesAndPersonality(ActorData actorData)
    {
        SetData(actorData);
    }
}

[Serializable]
public class StatsAndAbilities
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public ActorStats ActorStats;
    public void SetActorStats(ActorStats actorStats) => ActorStats = actorStats;
    public Actor_States_And_Conditions ActorStates;
    public void SetActorStates(Actor_States_And_Conditions actorStates) => ActorStates = actorStates;
    public ActorAspects ActorAspects;
    public void SetActorAspects(ActorAspects actorAspects) => ActorAspects = actorAspects;
    public ActorAbilities ActorAbilities;
    public void SetActorAbilities(ActorAbilities actorAbilities) => ActorAbilities = actorAbilities;

    public StatsAndAbilities(ActorData actorData)
    {
        _actorData = actorData;
    }
}

[Serializable]
public class InventoryAndEquipment
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;
    
    public InventoryData InventoryData;
    public void SetInventoryData(InventoryData inventoryData) => InventoryData = inventoryData;
    public EquipmentData EquipmentData;
    public void SetEquipmentData(EquipmentData equipmentData) => EquipmentData = equipmentData;

    public InventoryAndEquipment(ActorData actorData)
    {
        SetData(actorData);
    }
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
public class ActorStats
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public Date ActorBirthDate;
    public float ActorAge;

    public ActorLevelData ActorLevelData;
    public int ActorGold;
    public SPECIAL ActorSpecial;

    public CombatStats CombatStats;
}

[Serializable]
public class ActorAspects
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public ClassName ActorTitle;
    public void SetActorTitle(ClassName actorTitle) => ActorTitle = actorTitle;
    public List<AspectName> ActorAspectList;
    public void SetActorAspectList(List<AspectName> actorAspectList) => ActorAspectList = actorAspectList;
    public void AddAspect(AspectName aspect)
    {
        var index = ActorAspectList.FindIndex(a => a == AspectName.None);

        if (index == -1)
        {
            Debug.Log("No empty aspect slots available.");
        }

        ActorAspectList[index] = aspect;
        SetActorTitle(Manager_Aspect.GetCharacterTitle(ActorAspectList));
    }
    public void ChangeAspect(AspectName aspect, int index)
    {
        if (index < 0 || index >= ActorAspectList.Count)
        {
            Debug.Log("Index out of range.");
            return;
        }

        ActorAspectList[index] = aspect;
        SetActorTitle(Manager_Aspect.GetCharacterTitle(ActorAspectList));
    }
}

[Serializable]
public class ActorLevelData
{
    Actor_Base _actor;
    public int Level;
    public int TotalExperience;
    public int TotalSkillPoints;
    public int UsedSkillPoints;
    public int TotalSPECIALPoints;
    public int UsedSPECIALPoints;
    public bool CanAddNewSkillSet;

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

[Serializable]
public class CraftingData
{
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public List<RecipeName> KnownRecipes;

    public CraftingData(ActorData actorData)
    {
        SetData(actorData);
        KnownRecipes = new();
    }

    public bool AddRecipe(RecipeName recipeName)
    {
        if (!KnownRecipes.Contains(recipeName)) return false;

        KnownRecipes.Add(recipeName);

        return true;
    }

    public IEnumerator CraftItemAll(RecipeName recipeName)
    {
        var recipe = Manager_Recipe.GetRecipe(recipeName);

        var actorData = Manager_Actor.GetActorData(_actorData.ActorID);

        while (inventoryContainsAllIngredients(recipe.RequiredIngredients))
        {
            yield return CraftItem(recipeName);
        }

        bool inventoryContainsAllIngredients(List<Item> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                var inventoryItem = actorData.InventoryAndEquipment.InventoryData.GetItemFromInventory(ingredient.ItemID);

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

        var actorData = Manager_Actor.GetActorData(_actorData.ActorID);

        if (actorData.InventoryAndEquipment.InventoryData.RemoveFromInventory(recipe.RequiredIngredients))
        {
            if (actorData.InventoryAndEquipment.InventoryData.AddToInventory(recipe.RequiredIngredients))
            {
                yield break;
            }

            actorData.InventoryAndEquipment.InventoryData.AddToInventory(recipe.RequiredIngredients);
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
    [NonSerialized] ActorData _actorData;
    public void SetData(ActorData actorData) => _actorData = actorData;

    public List<Quest> ActorQuests;
    public void SetStage(int QuestID, int stageID, int stageProgress)
    {
        ActorQuests.FirstOrDefault(q => q.QuestID == QuestID).SetQuestStage(stageID, stageProgress);
    }

    public QuestData(ActorData actorData)
    {
        SetData(actorData);
        ActorQuests = new();
    }
}