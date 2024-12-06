using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Career;
using DateAndTime;
using EmployeePosition;
using Initialisation;
using Inventory;
using Items;
using Jobs;
using JobSite;
using Managers;
using Priority;
using Recipes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actor
{
    [Serializable]
    public class Actor_Data
    {
        public void UpdateActorData()
        {
            GameObjectData.UpdateActorGOProperties();
        }

        public uint      ActorID;
        public uint      ActorFactionID;
        public ActorName ActorName;
        
        public ActorDataPresetName ActorDataPresetName;

        public FullIdentification FullIdentification;

        public GameObjectData          GameObjectData;
        public CareerData              CareerData;
        public CraftingData            CraftingData;
        public VocationData            VocationData;
        public SpeciesAndPersonality   SpeciesAndPersonality;
        public StatsAndAbilities       StatsAndAbilities;
        public StatesAndConditionsData StatesAndConditionsData;
        public InventoryData           InventoryData;
        public EquipmentData           EquipmentData;

        public QuestData ActorQuests;

        //public OrderData OrderData;
        public Order_Base CurrentOrder;

        public void PrepareForInitialisation()
        {
            Manager_Initialisation.OnInitialiseActorData += InitialiseActorData;
        }

        public void InitialiseActorData()
        {
            var actor = Actor_Manager.GetActor(ActorID, true);

            if (actor is null)
            {
                Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
            }

            var actorFaction = Manager_Faction.GetFaction(ActorFactionID);

            if (actorFaction is null)
            {
                Debug.LogError($"Actor {ActorID} cannot find faction {ActorFactionID}.");
                return;
            }

            var factionGO = GameObject.Find($"{actorFaction.FactionID}: {actorFaction.FactionName}");

            if (factionGO is null)
            {
                Debug.LogError(
                    $"Actor {ActorID} cannot find faction GameObject {actorFaction.FactionID}: {actorFaction.FactionName}.");
                return;
            }

            actor.transform.parent.SetParent(factionGO.transform);
        }

        // Make an ability to make a deep copy of every class here and every class that needs to be saved.

        public Actor_Data(ActorDataPresetName actorDataPresetName, FullIdentification fullIdentification = null, GameObjectData gameObjectData = null,
                         CareerData careerData = null, CraftingData craftingData = null,
                         VocationData vocationData = null,
                         SpeciesAndPersonality speciesAndPersonality = null, StatsAndAbilities statsAndAbilities = null,
                         StatesAndConditionsData statesAndConditionsData = null, InventoryData inventoryData = null,
                         EquipmentData equipmentData = null, QuestData actorQuests = null,
                         Order_Base currentOrder = null)
        {
            ActorDataPresetName = actorDataPresetName;
            
            FullIdentification = fullIdentification;

            if (FullIdentification != null)
            {
                ActorID        = FullIdentification.ActorID;
                ActorFactionID = FullIdentification.ActorFactionID;
                ActorName      = FullIdentification.ActorName;    
            }

            GameObjectData          = gameObjectData;
            CareerData              = careerData;
            CraftingData            = craftingData;
            VocationData            = vocationData;
            SpeciesAndPersonality   = speciesAndPersonality;
            StatsAndAbilities       = statsAndAbilities;
            StatesAndConditionsData = statesAndConditionsData;
            InventoryData           = inventoryData;
            EquipmentData           = equipmentData;
            ActorQuests             = actorQuests;
            CurrentOrder            = currentOrder;
        }
    }

    [CustomPropertyDrawer(typeof(Actor_Data))]
    public class ActorData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var actorIDProp   = property.FindPropertyRelative("ActorID");
            var actorNameProp = property.FindPropertyRelative("ActorName");
            var nameProp      = actorNameProp.FindPropertyRelative("Name");
            var surnameProp   = actorNameProp.FindPropertyRelative("Surname");

            var name = surnameProp != null
                ? $"{nameProp.stringValue} {surnameProp.stringValue}"
                : $"{nameProp.stringValue}";

            label.text = $"{actorIDProp.intValue}: {name}";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    public enum PriorityUpdateTrigger
    {
        None,

        #region Full Identification

        ChangedName,
        ChangedFaction,
        ChangedCity,
        ChangedBirthdate,
        ChangedFamily,
        ChangedBackground,

        #endregion

        ChangedPersonality,
        ChangedBirthplace,
        ChangedDynasty,
        ChangedReligion,
        MovedActor,
        RotatedActor,
        ScaledActor,
        ChangedMesh,
        ChangedMaterial,
        ToggledDoJobs,
        ChangedJobsite,
        ChangedStation,
        ChangedOperatingArea,
        ChangedEmployeePosition,
        ChangedSpecies,
        ChangedInventory,
        DroppedItems,
        PriorityCompleted,

        #region States And Conditions

        ChangedPrimaryState,
        ChangedSubState,
        ChangedCondition,

        #endregion
    }

    [Serializable]
    public class FullIdentification : PriorityData
    {
        public FullIdentification(uint actorID, ActorName actorName, uint actorFactionID, uint actorCityID,
                                  Date actorBirthDate) : base(actorID, ComponentType.Actor)
        {
            ActorName      = actorName;
            ActorFactionID = actorFactionID;
            ActorCityID    = actorCityID;
            ActorBirthDate = actorBirthDate;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public uint                ActorID;
        public ActorName           ActorName;
        public uint                ActorFactionID;
        public uint                ActorCityID;
        public Date                ActorBirthDate;
        public float               ActorAge => ActorBirthDate.GetAge();
        public Family              ActorFamily;
        public Background          Background;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Background : PriorityData
    {
        public Background(uint actorID, string birthplace, Date birthdate, Family actorFamily, Dynasty actorDynasty,
                          string religion) : base(actorID, ComponentType.Actor)
        {
            Birthplace  = birthplace;
            Birthdate   = birthdate;
            ActorFamily = actorFamily;
            ActorDynasty = actorDynasty;
            Religion    = religion;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public string  Birthplace;
        public Date    Birthdate;
        public Family  ActorFamily;
        public Dynasty ActorDynasty;
        public string  Religion;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class GameObjectData : PriorityData
    {
        public GameObjectData(uint actorID, Transform actorTransform, Mesh actorMesh, Material actorMaterial) : base(actorID, ComponentType.Actor)
        {
            _actorTransform = actorTransform;
            ActorMesh       = actorMesh;
            ActorMaterial   = actorMaterial;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public void UpdateActorGOProperties()
        {
            SetActorTransformProperties();
        }

        [NonSerialized] Transform _actorTransform;

        public Transform ActorTransform
        {
            get { return _actorTransform ??= Actor_Manager.GetActor(ActorReference.ActorID)?.transform; }
        }

        public void SetActorTransformProperties()
        {
            if (ActorTransform is null)
            {
                Debug.Log($"ActorTransform for actor {ActorReference.ActorID} is null.");
                return;
            }

            _setActorPosition(ActorTransform.position);
            _setActorRotation(ActorTransform.rotation);
            _setActorScale(ActorTransform.localScale);
        }

        public Vector3    LastSavedActorPosition;
        void              _setActorPosition(Vector3 actorPosition) => LastSavedActorPosition = actorPosition;
        public Quaternion LastSavedActorRotation;
        void              _setActorRotation(Quaternion actorRotation) => LastSavedActorRotation = actorRotation;
        public Vector3    LastSavedActorScale;
        void              _setActorScale(Vector3 actorScale) => LastSavedActorScale = actorScale;

        public Mesh     ActorMesh;
        public void     SetActorMesh(Mesh actorMesh) => ActorMesh = actorMesh;
        public Material ActorMaterial;
        public void     SetActorMaterial(Material actorMaterial) => ActorMaterial = actorMaterial;

        public void SetGameObjectProperties(Transform actorTransform)
        {
            _actorTransform = actorTransform;

            ActorMesh     = ActorTransform.GetComponent<MeshFilter>().sharedMesh;
            ActorMaterial = ActorTransform.GetComponent<MeshRenderer>().sharedMaterial;

            // Temporary

            ActorMesh     ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx");     // Later will come from species
            ActorMaterial ??= Resources.Load<Material>("Materials/Material_Red"); // Later will come from species
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class WorldStateData : PriorityData
    {
        public WorldStateData(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Relationships : PriorityData
    {
        public Relationships(uint actorID, List<Relation> allRelationships) : base(actorID, ComponentType.Actor)
        {
            AllRelationships = allRelationships;
        }
        
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        public List<Relation>           AllRelationships;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class CareerData : PriorityData
    {
        public CareerData(uint actorID, CareerName careerName, HashSet<JobName> jobsNotFromCareer) : base(actorID,
            ComponentType.Actor)
        {
            CareerName = careerName;
            AllJobs    = jobsNotFromCareer;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public CareerName CareerName;

        public void SetCareer(CareerName careerName, bool changeAllCareerJobs = true)
        {
            CareerName = careerName;

            if (!changeAllCareerJobs) return;

            AllJobs.Clear();

            var careerJobs = Career_Manager.GetCareer_Master(careerName).CareerJobs;

            foreach (var job in careerJobs)
            {
                AddJob(job);
            }
        }

        public HashSet<JobName> AllJobs = new();

        public void AddJob(JobName jobName)
        {
            // Find a way to use actorData to exclude jobs that are not allowed due to status and conditions like paralyzed, or
            // personalities.

            if (AllJobs.Add(jobName)) return;

            Debug.LogWarning($"Job {jobName} already exists in AllActorJobs.");
        }

        public void RemoveJob(JobName jobName)
        {
            if (AllJobs.Remove(jobName)) return;

            Debug.LogWarning($"Job {jobName} does not exist in AllActorJobs.");
        }

        public Job  CurrentJob             { get; private set; }
        public void SetCurrentJob(Job job) => CurrentJob = job;

        public bool HasCurrentJob() => CurrentJob != null;

        public void StopCurrentJob() => CurrentJob = null;

        public bool GetNewCurrentJob(uint stationID = 0)
        {
            return CareerName != CareerName.Wanderer && JobSite.GetNewCurrentJob(ActorReference.Actor, stationID);
        }

        public bool JobsActive = true;
        public void ToggleDoJobs(bool jobsActive) => JobsActive = jobsActive;

        public uint             JobsiteID;
        JobSite_Component        _jobSite;
        public JobSite_Component JobSite                      => _jobSite ??= Jobsite_Manager.GetJobSite_Component(JobsiteID);
        public void             SetJobsiteID(uint jobsiteID) => JobsiteID = jobsiteID;
        
        public EmployeePositionName EmployeePositionName;

        public void SetEmployeePosition(EmployeePositionName employeePositionName) =>
            EmployeePositionName = employeePositionName;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class SpeciesAndPersonality : PriorityData
    {
        public SpeciesAndPersonality(uint actorID, SpeciesName actorSpecies, ActorPersonality actorPersonality) : base(
            actorID, ComponentType.Actor)
        {
            ActorSpecies     = actorSpecies;
            ActorPersonality = actorPersonality;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public SpeciesName ActorSpecies;
        public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
        public ActorPersonality ActorPersonality;
        public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class StatsAndAbilities
    {
        public StatsAndAbilities(uint actorID, Actor_Stats actorStats, Actor_Aspects actorAspects,
                                 Actor_Abilities actorAbilities)
        {
            ActorStats     = actorStats;
            ActorAspects   = actorAspects;
            ActorAbilities = actorAbilities;
        }

        [FormerlySerializedAs("Actor_Stats")] public Actor_Stats ActorStats;
        public  void        SetActorStats(Actor_Stats actorStats) => ActorStats = actorStats;

        [FormerlySerializedAs("Actor_Aspects")] public Actor_Aspects ActorAspects;
        public  void          SetActorAspects(Actor_Aspects actor_Aspects) => ActorAspects = actor_Aspects;

        [FormerlySerializedAs("Actor_Abilities")] public Actor_Abilities ActorAbilities;
        public  void            SetActorAbilities(Actor_Abilities actor_Abilities) => ActorAbilities = actor_Abilities;
    }

    [Serializable]
    public class ActorName
    {
        public string          Name;
        public string          Surname;
        public string          GetName() => $"{Name} {Surname}";
        public TitleName       CurrentTitle;
        public List<TitleName> AvailableTitles;

        public void SetTitleAsCurrentTitle(TitleName titleName)
        {
            if (AvailableTitles.Contains(titleName)) CurrentTitle = titleName;
        }

        public ActorName(string name, string surname)
        {
            Name    = name;
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
    public class Actor_Stats : PriorityData
    {
        public Actor_Stats(uint actorID, ActorLevelData actorLevelData, Special actorSpecial, CombatStats actorCombatStats) :
            base(actorID, ComponentType.Actor)
        {
            ActorLevelData   = actorLevelData;
            ActorSpecial     = actorSpecial;
            ActorCombatStats = actorCombatStats;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ActorLevelData ActorLevelData;
        public Special        ActorSpecial;
        public CombatStats    ActorCombatStats;

        public float TotalCarryWeight =>
            100; // For now. Eventually. ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.

        public float AvailableCarryWeight => TotalCarryWeight -
                                             Item.GetItemListTotal_Weight(ActorReference.Actor.ActorData.InventoryData
                                                 .GetAllInventoryItemsClone().Values.ToList());

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Actor_Aspects : PriorityData
    {
        public Actor_Aspects(uint actorID, List<AspectName> actorAspectList) : base(actorID, ComponentType.Actor)
        {
            ActorAspectList = actorAspectList;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ClassTitle ActorClassTitle => Manager_Aspect.GetCharacterTitle(ActorAspectList);

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

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class ActorLevelData : PriorityData
    {
        public ActorLevelData(uint actorID, uint level = 1, uint totalExperience = 0, uint totalSkillPoints = 0,
                              uint totalSpecialPoints = 0, bool canAddNewSkillSet = false) : base(actorID,
            ComponentType.Actor)
        {
            ActorLevel         = level;
            TotalExperience    = totalExperience;
            TotalSkillPoints   = totalSkillPoints;
            TotalSpecialPoints = totalSpecialPoints;
            CanAddNewSkillSet  = canAddNewSkillSet;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public uint ActorLevel;
        public uint TotalExperience;
        public uint TotalSkillPoints;
        public uint UsedSkillPoints;
        public uint TotalSpecialPoints;
        public uint UsedSpecialPoints;
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
            var actorData = Actor_Manager.GetActorData(ActorReference.ActorID);

            ActorLevel         =  levelData.Level;
            TotalSkillPoints   += levelData.SkillPoints;
            TotalSpecialPoints += levelData.SPECIALPoints;

            switch (levelData.BonusType)
            {
                case LevelUpBonusType.Health:
                    actorData.StatsAndAbilities.ActorStats.ActorCombatStats.MaxHealth += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Mana:
                    actorData.StatsAndAbilities.ActorStats.ActorCombatStats.MaxMana += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Stamina:
                    actorData.StatsAndAbilities.ActorStats.ActorCombatStats.MaxStamina += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Skillset:
                    CanAddNewSkillSet = true;
                    break;
                default:
                    Debug.Log("LevelData Bonus Type was none.");
                    break;
            }
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class CraftingData : PriorityData
    {
        public CraftingData(uint actorID, List<RecipeName> knownRecipes) : base(actorID, ComponentType.Actor)
        {
            KnownRecipes = knownRecipes;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public List<RecipeName> KnownRecipes = new();

        public bool AddRecipe(RecipeName recipeName)
        {
            if (KnownRecipes.Contains(recipeName)) return false;

            KnownRecipes.Add(recipeName);

            return true;
        }

        public IEnumerator CraftItemAll(RecipeName recipeName)
        {
            var recipe = Manager_Recipe.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActorData(ActorReference.ActorID);

            while (_inventoryContainsAllIngredients(actorData, recipe.RequiredIngredients))
            {
                yield return CraftItem(recipeName);
            }
        }

        bool _inventoryContainsAllIngredients(Actor_Data actorData, List<Item> ingredients)
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

        public IEnumerator CraftItem(RecipeName recipeName)
        {
            if (!KnownRecipes.Contains(recipeName))
            {
                Debug.Log($"KnownRecipes does not contain RecipeName: {recipeName}");
                yield break;
            }

            Recipe_Master recipeMaster = Manager_Recipe.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActorData(ActorReference.ActorID);

            if (!_inventoryContainsAllIngredients(actorData, recipeMaster.RequiredIngredients))
            {
                Debug.Log("Inventory does not contain all ingredients.");
                yield break;
            }

            if (!actorData.InventoryData.HasSpaceForItems(recipeMaster.RecipeProducts))
            {
                Debug.Log("Inventory does not have space for produced items.");
                yield break;
            }

            actorData.InventoryData.RemoveFromInventory(recipeMaster.RequiredIngredients);
            actorData.InventoryData.AddToInventory(recipeMaster.RecipeProducts);
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class QuestData : PriorityData
    {
        public QuestData(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public List<Quest> ActorQuests = new();

        public void SetStage(int questID, int stageID, int stageProgress)
        {
            ActorQuests.FirstOrDefault(q => q.QuestID == questID)?.SetQuestStage(stageID, stageProgress);
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class VocationData : PriorityData
    {
        public VocationData(uint actorID, Dictionary<VocationName, ActorVocation> actorVocations) : base(actorID,
            ComponentType.Actor)
        {
            ActorVocations = actorVocations;
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Dictionary<VocationName, ActorVocation> ActorVocations;
        public void                SetVocations(Dictionary<VocationName, ActorVocation> vocations) => ActorVocations = vocations;

        public void AddVocation(VocationName vocationName, float vocationExperience)
        {
            if (ActorVocations.ContainsKey(vocationName))
            {
                Debug.Log($"Vocation: {vocationName} already exists in Vocations.");
                return;
            }

            ActorVocations.Add(vocationName, new ActorVocation(vocationName, vocationExperience));
        }

        public void RemoveVocation(VocationName vocationName)
        {
            if (!ActorVocations.ContainsKey(vocationName))
            {
                Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
                return;
            }
            
            ActorVocations.Remove(vocationName);
        }

        public void ChangeVocationExperience(VocationName vocationName, float experienceChange)
        {
            if (!ActorVocations.TryGetValue(vocationName, out var vocation))
            {
                Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
                return;
            }

            vocation.VocationExperience += experienceChange;
        }

        public float GetVocationExperience(VocationName vocationName)
        {
            if (ActorVocations.TryGetValue(vocationName, out var vocation)) return vocation.VocationExperience;
            
            Debug.LogError($"Vocation: {vocationName} does not exist in Vocations.");
            return -1;

        }

        public float GetProgress(VocationRequirement vocationRequirement)
        {
            var currentExperience = GetVocationExperience(vocationRequirement.VocationName);

            if (currentExperience < vocationRequirement.MinimumVocationExperience)
            {
                return 0;
            }

            var progress = ((currentExperience - vocationRequirement.ExpectedVocationExperience) /
                            Math.Max(currentExperience, 1));

            if (progress < 0) return 1 / Math.Abs(progress);

            return progress;
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class ActorVocation
    {
        public VocationName  VocationName;
        public VocationTitle VocationTitle;
        public float         VocationExperience;

        public ActorVocation(VocationName vocationName, float vocationExperience)
        {
            VocationName       = vocationName;
            VocationExperience = vocationExperience;

            // Implement later
            //VocationTitle = Manager_Vocation.GetVocation(vocationName).GetVocationTitle(vocationExperience);
        }
    }
}