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
        GameObjectProperties.UpdateActorGOProperties();
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

        if (actor == null)
        {
            Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
        }
        
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

        _setDataChangeEvents(actor.PriorityComponent);
    }

    public event Action OnFullIdentificationChange;

    void _setDataChangeEvents(PriorityComponent priorityComponent)
    {
        FullIdentification.OnDataChange = priorityComponent.OnFullIdentificationChange;
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

public abstract class DataSubClass
{
    public uint ActorID;
    public DataSubClass(uint actorID) => ActorID = actorID;

    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }
    public Action OnDataChange;
    protected void _priorityChangeCheck()
    {
        if (!_priorityChangeNeeded()) return;
        OnDataChange?.Invoke();
    }

    protected abstract bool _priorityChangeNeeded();
}

[Serializable]
public class FullIdentification : DataSubClass
{
    public FullIdentification(uint actorID, ActorName actorName, uint actorFactionID, uint actorCityID) : base(actorID)
    {
        ActorName = actorName;
        ActorFactionID = actorFactionID;
        ActorCityID = actorCityID;
    }

    public ActorName ActorName;
    public uint ActorFactionID;
    public uint ActorCityID;
    public Date ActorBirthDate;
    public float ActorAge => ActorBirthDate.GetAge();
    public Family ActorFamily;
    public Background Background;

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class Background : DataSubClass
{
    public Background(uint actorID) : base(actorID) { }

    public string Birthplace;
    public Date Birthdate;
    public Family ActorFamily;
    public Dynasty ActorDynasty;
    public string Religion;

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class GameObjectProperties : DataSubClass
{
    public GameObjectProperties(uint actorID) : base(actorID) { }

    public void UpdateActorGOProperties()
    {
        SetActorTransformProperties();
    }

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

    public void SetGameObjectProperties(Transform actorTransform)
    {
        _actorTransform = actorTransform;

        ActorMesh = ActorTransform.GetComponent<MeshFilter>().sharedMesh;
        ActorMaterial = ActorTransform.GetComponent<MeshRenderer>().sharedMaterial;

        // Temporary

        ActorMesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx"); // Later will come from species
        ActorMaterial ??= Resources.Load<Material>("Materials/Material_Red"); // Later will come from species
    }

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class WorldStateData : DataSubClass
{
    public WorldStateData(uint actorID) : base(actorID) { }

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class Relationships : DataSubClass
{
    public Relationships(uint actorID) : base(actorID) { }
    public List<Relation> AllRelationships;

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class CareerAndJobs : DataSubClass
{
    public CareerAndJobs(uint actorID) : base(actorID) { }

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

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class SpeciesAndPersonality : DataSubClass
{
    public SpeciesAndPersonality(uint actorID) : base(actorID) { }

    public SpeciesName ActorSpecies;
    public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
    public ActorPersonality ActorPersonality;
    public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class StatsAndAbilities : DataSubClass
{
    public StatsAndAbilities(uint actorID) : base(actorID) { }

    public Actor_Stats Actor_Stats;
    public void SetActorStats(Actor_Stats actorStats) => Actor_Stats = actorStats;

    public Actor_StatesAndConditions Actor_StatesAndConditions;
    public void SetActorStates(Actor_StatesAndConditions actorStates) => Actor_StatesAndConditions = actorStates;

    public Actor_Aspects Actor_Aspects;
    public void SetActorAspects(Actor_Aspects actor_Aspects) => Actor_Aspects = actor_Aspects;

    public Actor_Abilities Actor_Abilities;
    public void SetActorAbilities(Actor_Abilities actor_Abilities) => Actor_Abilities = actor_Abilities;

    public override event Action OnDataChange;

    protected override bool _priorityChangeNeeded()
    {
        return false;
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
public class Actor_Stats : DataSubClass
{
    public Actor_Stats(uint actorID) : base(actorID) { }

    public ActorLevelData ActorLevelData;
    public SPECIAL ActorSpecial;
    public CombatStats CombatStats;

    public float TotalCarryWeight => ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.
    public float AvailableCarryWeight => TotalCarryWeight - Manager_Actor.GetActorData(ActorID).InventoryData.GetTotalInventoryWeight();

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class Actor_Aspects : DataSubClass
{
    public Actor_Aspects(uint actorID) : base(actorID) { }

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

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class ActorLevelData : DataSubClass
{
    public ActorLevelData(uint actorID, uint level = 1, uint totalExperience = 0, uint totalSkillPoints = 0, uint totalSPECIALPoints = 0, bool canAddNewSkillSet = false) : base(actorID)
    {
        ActorLevel = level;
        TotalExperience = totalExperience;
        TotalSkillPoints = totalSkillPoints;
        TotalSPECIALPoints = totalSPECIALPoints;
        CanAddNewSkillSet = canAddNewSkillSet;
    }

    public uint ActorLevel;
    public uint TotalExperience;
    public uint TotalSkillPoints;
    public uint UsedSkillPoints;
    public uint TotalSPECIALPoints;
    public uint UsedSPECIALPoints;
    public bool CanAddNewSkillSet;

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

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class CraftingData : DataSubClass
{
    public CraftingData(uint actorID) : base(actorID) { }

    public List<RecipeName> KnownRecipes = new();

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

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class QuestData : DataSubClass
{
    public QuestData(uint actorID) : base(actorID) { }

    public List<Quest> ActorQuests = new();
    public void SetStage(int QuestID, int stageID, int stageProgress)
    {
        ActorQuests.FirstOrDefault(q => q.QuestID == QuestID).SetQuestStage(stageID, stageProgress);
    }

    protected override bool _priorityChangeNeeded()
    {
        return false;
    }
}

[Serializable]
public class VocationData : DataSubClass
{
    public VocationData(uint actorID) : base(actorID) { }

    public List<ActorVocation> ActorVocations = new();
    public void SetVocations(List<ActorVocation> vocations) => ActorVocations = vocations;

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

    protected override bool _priorityChangeNeeded()
    {
        return false;
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

public abstract class PriorityGenerator
{
    protected static float DefaultMaxPriority => 10;

    protected static float _stayAboveTarget(float current, float target, float maxPriority) 
    => Math.Clamp(current - target, 0, maxPriority);
    protected static float _stayBelowTarget(float current, float target, float maxPriority) 
    => Math.Clamp(target - current, 0, maxPriority);
    protected static float _stayAtTarget(float current, float target, float maxPriority) 
    => Math.Clamp(current == target ? 0 : maxPriority, 0, maxPriority);

    protected static float _stayWithinRange(float current, float min, float max, float maxPriority) 
    => Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority);
    protected static float _stayOutsideRange(float current, float min, float max, float maxPriority) 
    => Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority);

    protected static float _stayAbovePercentage(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp(Math.Abs(targetPercentage / 100 - (current / total)) * 100, 0, maxPriority);
    protected static float _stayBelowPercentage(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp(Math.Abs((current / total) - targetPercentage / 100) * 100, 0, maxPriority);
    protected static float _stayAtPercentage(float current, float total, float targetPercentage, float maxPriority) 
    => Math.Clamp(Math.Abs((current / total) - targetPercentage / 100) * 100, 0, maxPriority);

    protected static float _stayWithinPercentageRange(float current, float total, float minPercentage, float maxPercentage, float maxPriority)
    => Math.Clamp(Math.Min(Math.Abs((current / total) - minPercentage / 100), Math.Abs((current / total) - maxPercentage / 100)) * 100, 0, maxPriority);
    protected static float _stayOutsidePercentageRange(float current, float total, float minPercentage, float maxPercentage, float maxPriority)
    => Math.Clamp(Math.Max(Math.Abs((current / total) - minPercentage / 100), Math.Abs((current / total) - maxPercentage / 100)) * 100, 0, maxPriority);
}

public class PriorityGenerator_Fetch : PriorityGenerator
{
    public static List<float> GeneratePriority(List<Item> items, Vector3Int targetPosition, float maxPriority = 0)
    {
        maxPriority = Math.Max(DefaultMaxPriority, maxPriority);

        return new List<float>
        {
            _stayAboveTarget(Item.GetItemListCount_AllItems(items), 0, maxPriority)
            + _stayAboveTarget(Vector3Int.Distance(Vector3Int.zero, targetPosition), 0, maxPriority),


        };
    }
}

public class PriorityGenerator_Condition : PriorityGenerator
{
    public static List<float> GeneratePriority(List<Item> items, Vector3Int targetPosition, float maxPriority = 0)
    {
        return new List<float>
        {
            (
                0
            ), 
        };
    }
}