using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ActorData
{
    public void UpdateActorData()
    {
        GameObjectProperties.UpdateActorData();
    }

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
    public OrderData OrderData;

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
        InventoryAndEquipment = new InventoryAndEquipment(ActorID);
        ActorQuests = new QuestData(ActorID);
        OrderData = new OrderData(ActorID);
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
    public int ActorID;
    public ActorName ActorName;
    public int ActorFactionID;
    public int ActorCityID;
    public Date ActorBirthDate;
    public float ActorAge => ActorBirthDate.GetAge();
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
    public int ActorID;

    public string Birthplace;
    public Date Birthdate;
    public Family ActorFamily;
    public Dynasty ActorDynasty;
    public string Religion;

    public Background(int actorID) => ActorID = actorID;
}

[Serializable]
public class GameObjectProperties
{
    public void UpdateActorData()
    {
        SetActorTransformProperties();
    }

    public int ActorID;
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

    public GameObjectProperties(int actorID) => ActorID = actorID;

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
    public int ActorID;
    public WorldStateData(int actorID) => ActorID = actorID;
}

[Serializable]
public class Relationships
{
    public int ActorID;

    public List<Relation> AllRelationships;

    public Relationships(int actorID) => ActorID = actorID;
}

[Serializable]
public class CareerAndJobs
{
    public int ActorID;

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

    public CareerAndJobs(int actorID) => ActorID = actorID;
}

[Serializable]
public class SpeciesAndPersonality
{
    public int ActorID;

    public SpeciesName ActorSpecies;
    public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
    public ActorPersonality ActorPersonality;
    public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

    public SpeciesAndPersonality(int actorID) => ActorID = actorID;
}

[Serializable]
public class StatsAndAbilities
{
    public int ActorID;

    public ActorStats ActorStats;
    public void SetActorStats(ActorStats actorStats) => ActorStats = actorStats;
    public Actor_States_And_Conditions ActorStates;
    public void SetActorStates(Actor_States_And_Conditions actorStates) => ActorStates = actorStates;
    public ActorAspects ActorAspects;
    public void SetActorAspects(ActorAspects actorAspects) => ActorAspects = actorAspects;
    public ActorAbilities ActorAbilities;
    public void SetActorAbilities(ActorAbilities actorAbilities) => ActorAbilities = actorAbilities;

    public StatsAndAbilities(int actorID) => ActorID = actorID;
}

[Serializable]
public class InventoryAndEquipment
{
    public int ActorID;
    
    public InventoryData InventoryData;
    public void SetInventoryData(InventoryData inventoryData) => InventoryData = inventoryData;
    public EquipmentData EquipmentData;
    public void SetEquipmentData(EquipmentData equipmentData) => EquipmentData = equipmentData;

    public InventoryAndEquipment(int actorID) => ActorID = actorID;
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
    public int ActorID;

    public ActorLevelData ActorLevelData;
    public SPECIAL ActorSpecial;
    public CombatStats CombatStats;

    public ActorStats(int actorID) => ActorID = actorID;

    public float CarryWeight => ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.
}

[Serializable]
public class ActorAspects
{
    public int ActorID;

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
            return;
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

    public ActorAspects(int actorID) => ActorID = actorID;
}

[Serializable]
public class ActorLevelData
{
    public int ActorID;
    public int ActorLevel;
    public int TotalExperience;
    public int TotalSkillPoints;
    public int UsedSkillPoints;
    public int TotalSPECIALPoints;
    public int UsedSPECIALPoints;
    public bool CanAddNewSkillSet;

    public ActorLevelData(int actorID, int level = 1, int totalExperience = 0, int totalSkillPoints = 0, int totalSPECIALPoints = 0, bool canAddNewSkillSet = false)
    {
        ActorID = actorID;
        ActorLevel = level;
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
        var levelData = Manager_CharacterLevels.AllLevelUpData[ActorLevel];

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
                actorData.StatsAndAbilities.ActorStats.CombatStats.MaxHealth += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Mana:
                actorData.StatsAndAbilities.ActorStats.CombatStats.MaxMana += levelData.BonusStatPoints;
                break;
            case LevelUpBonusType.Stamina:
                actorData.StatsAndAbilities.ActorStats.CombatStats.MaxStamina += levelData.BonusStatPoints;
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
    public int ActorID;

    public List<RecipeName> KnownRecipes = new();

    public CraftingData(int actorID) => ActorID = actorID;

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

        var actorData = Manager_Actor.GetActorData(ActorID);

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
    public int ActorID;

    public List<Quest> ActorQuests = new();
    public void SetStage(int QuestID, int stageID, int stageProgress)
    {
        ActorQuests.FirstOrDefault(q => q.QuestID == QuestID).SetQuestStage(stageID, stageProgress);
    }

    public QuestData(int actorID) => ActorID = actorID;
}


[Serializable]
public class OrderData
{
    public int ActorID;
    public List<int> AllOrderIDs;
    public bool HasOrder() => AllOrderIDs.Any(o => Manager_Order.GetOrder(o).OrderStatus != OrderStatus.Complete);

    public OrderData(int actorID) => ActorID = actorID;

    public void AddOrder(int orderID) => AllOrderIDs.Add(orderID);
    public void RemoveOrder(int orderID) => AllOrderIDs.Remove(orderID);
    public void RemoveCompletedOrders() => AllOrderIDs.RemoveAll(o => Manager_Order.GetOrder(o).OrderStatus == OrderStatus.Complete);
    public void RemoveAllOrders() => AllOrderIDs.Clear();
}

[Serializable]
public class VocationData
{
    public int ActorID;

    public List<ActorVocation> ActorVocations = new();
    public void SetVocations(List<ActorVocation> vocations) => ActorVocations = vocations;

    public VocationData(int actorID) => ActorID = actorID;

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