using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ability;
using Careers;
using DateAndTime;
using Equipment;
using Faction;
using Inventory;
using Items;
using Jobs;
using JobSite;
using Managers;
using Personality;
using Priority;
using Recipes;
using Relationships;
using Tools;
using UnityEngine;

namespace Actor
{
    [Serializable]
    public class Actor_Data : Data_Class
    {
        public bool IsSpawned;
        public uint ActorID => FullIdentification.ActorID;
        public uint ActorFactionID => FullIdentification.ActorFactionID;
        public string ActorName => FullIdentification.ActorName.GetName();

        Actor_Component _actor;
        public Actor_Component Actor => _actor ??= Actor_Manager.GetActor_Component(ActorID);

        public ActorDataPresetName ActorDataPresetName;

        public FullIdentification FullIdentification;

        public GameObjectData GameObjectData;
        public Career_Data CareerData;
        public Crafting_Data CraftingData;
        public Vocation_Data VocationData;
        public SpeciesAndPersonality SpeciesAndPersonality;

        public StatsAndAbilities StatsAndAbilities;

        public StatesAndConditionsData StatesAndConditionsData;
        public Inventory_Data InventoryData;

        public Equipment_Data EquipmentData;

        public QuestUpdater ActorQuests;

        public void InitialiseActorData()
        {
            var actor = Actor_Manager.GetActor_Component(ActorID);

            if (actor is null)
            {
                Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
            }

            var actorFaction = Faction_Manager.GetFaction_Data(ActorFactionID);

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

        public Actor_Data(ActorDataPresetName actorDataPresetName, FullIdentification fullIdentification = null,
            GameObjectData gameObjectData = null,
            Career_Data careerData = null, Crafting_Data craftingData = null,
            Vocation_Data vocationData = null,
            SpeciesAndPersonality speciesAndPersonality = null,
            StatsAndAbilities statsAndAbilities = null,
            StatesAndConditionsData statesAndConditionsData = null, Inventory_Data inventoryData = null,
            Equipment_Data equipmentData = null, QuestUpdater actorQuests = null)
        {
            ActorDataPresetName = actorDataPresetName;

            FullIdentification = fullIdentification;
            GameObjectData = gameObjectData;
            CareerData = careerData;
            CraftingData = craftingData;
            VocationData = vocationData;
            SpeciesAndPersonality = speciesAndPersonality;
            StatsAndAbilities = statsAndAbilities;
            StatesAndConditionsData = statesAndConditionsData;
            InventoryData = inventoryData;
            EquipmentData = equipmentData;
            ActorQuests = actorQuests;
        }

        // Check if we need a clone function later

        // public Actor_Data(Actor_Data actor_Data)
        // {
        //     FullIdentification      = new FullIdentification(actor_Data.FullIdentification);
        //     GameObjectData          = new GameObjectData(actor_Data.GameObjectData);
        //     CareerData              = new CareerData(actor_Data.CareerData);
        //     CraftingData            = new CraftingData(actor_Data.CraftingData);
        //     VocationData            = new VocationData(actor_Data.VocationData);
        //     SpeciesAndPersonality   = new SpeciesAndPersonality(actor_Data.SpeciesAndPersonality);
        //     StatsAndAbilities       = new StatsAndAbilities(actor_Data.StatsAndAbilities);
        //     StatesAndConditionsData = new StatesAndConditionsData(actor_Data.StatesAndConditionsData);
        //     InventoryData           = new InventoryData_Actor(actor_Data.InventoryData);
        //     EquipmentData           = new EquipmentData(actor_Data.EquipmentData);
        //     ActorQuests             = new QuestData(actor_Data.ActorQuests);
        // }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Base Actor Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Full Identification",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: FullIdentification.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Game Object Properties",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: GameObjectData.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Species And Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: SpeciesAndPersonality.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatsAndAbilities.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CareerData.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData.GetSubData(toggleMissingDataDebugs));

            _updateDataDisplay(ref _dataToDisplay,
                title: "Equipment Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: EquipmentData.GetSubData(toggleMissingDataDebugs));

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor ID", $"{ActorID}" },
                { "Actor Name", $"{ActorName}" },
                { "Actor Faction ID", $"{ActorFactionID}" }
            };
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                {
                    "Full Identification",
                    FullIdentification.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Game Object Data",
                    GameObjectData.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Species And Personality",
                    SpeciesAndPersonality.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Stats And Abilities",
                    StatsAndAbilities.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Career Data",
                    CareerData.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Inventory Data",
                    InventoryData.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Equipment Data",
                    EquipmentData.GetSubData(toggleMissingDataDebugs)
                }
            };
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
    public class FullIdentification : Priority_Updater
    {
        public FullIdentification(uint actorID, ActorName actorName, uint actorFactionID, uint actorCityID,
            Date actorBirthDate = null) : base(actorID, ComponentType.Actor)
        {
            ActorID = actorID;
            ActorName = actorName;
            ActorFactionID = actorFactionID;
            ActorCityID = actorCityID;
            ActorBirthDate = actorBirthDate ?? new Date(Manager_DateAndTime.GetCurrentTotalDays());
        }

        public FullIdentification(FullIdentification fullIdentification) : base(fullIdentification.ActorID,
            ComponentType.Actor)
        {
            ActorID = fullIdentification.ActorID;
            ActorName = new ActorName(fullIdentification.ActorName.Name, fullIdentification.ActorName.Surname);
            ActorFactionID = fullIdentification.ActorFactionID;
            ActorCityID = fullIdentification.ActorCityID;
            ActorBirthDate = new Date(fullIdentification.ActorBirthDate);
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Full Identification",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData() => new()
        {
            { "Actor ID", $"{ActorID}" },
            { "Actor Name", $"{ActorName.GetName()}" },
            { "ActorFaction", $"{ActorFactionID}" },
            { "Actor City ID", $"{ActorCityID}" }
        };

        public uint ActorID;
        public ActorName ActorName;
        public uint ActorFactionID;
        public uint ActorCityID;
        public Date ActorBirthDate;
        public float ActorAge => ActorBirthDate.GetAge();
        public Family ActorFamily;
        public Background Background;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Background : Priority_Updater
    {
        public Background(uint actorID, string birthplace, Date birthdate, Family actorFamily, Dynasty actorDynasty,
            string religion) : base(actorID, ComponentType.Actor)
        {
            Birthplace = birthplace;
            Birthdate = birthdate;
            ActorFamily = actorFamily;
            ActorDynasty = actorDynasty;
            Religion = religion;
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Background",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Birthplace", $"{Birthplace}" },
                { "Birthdate", $"{Birthdate}" },
                { "Family", $"{ActorFamily}" },
                { "Dynasty", $"{ActorDynasty}" },
                { "Religion", $"{Religion}" }
            };
        }

        public string Birthplace;
        public Date Birthdate;
        public Family ActorFamily;
        public Dynasty ActorDynasty;
        public string Religion;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class GameObjectData : Priority_Updater
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public GameObjectData(uint actorID, Transform actorTransform = null, Mesh actorMesh = null,
            Material actorMaterial = null) : base(actorID, ComponentType.Actor)
        {
            _actorTransform = actorTransform;
            ActorMesh = actorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            ActorMaterial = actorMaterial ?? Resources.Load<Material>("Materials/Material_Red");
        }

        public GameObjectData(GameObjectData gameObjectData) : base(gameObjectData.ActorReference.ActorID,
            ComponentType.Actor)
        {
            _actorTransform = gameObjectData.ActorTransform;
            ActorMesh = gameObjectData.ActorMesh;
            ActorMaterial = gameObjectData.ActorMaterial;
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Game Object Properties",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData() => new()
        {
            { "Actor Last Saved Position", $"{LastSavedActorPosition}" },
            { "Actor Last Saved Scale", $"{LastSavedActorScale}" },
            { "Actor Last Saved Rotation", $"{LastSavedActorRotation.eulerAngles}" },
            { "Actor Mesh", $"{ActorMesh}" },
            { "Actor Material", $"{ActorMaterial}" }
        };

        public void UpdateActorGOProperties()
        {
            SetActorTransformProperties();
        }

        [NonSerialized] Transform _actorTransform;

        public Transform ActorTransform
        {
            get { return _actorTransform ??= Actor_Manager.GetActor_Component(ActorReference.ActorID)?.transform; }
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

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class WorldStateUpdater : Priority_Updater
    {
        public WorldStateUpdater(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "World State",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                {"No data yet... ", "Yet"}
            };
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Relationships : Priority_Updater
    {
        public Relationships(uint actorID, List<Relation> allRelationships) : base(actorID, ComponentType.Actor)
        {
            AllRelationships = allRelationships;
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Relationships",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                {"No data yet... ", "Yet"}
            };
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        public List<Relation> AllRelationships;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class Career_Data : Priority_Updater
    {
        public Career_Data(uint actorID, CareerName careerName, HashSet<JobName> jobsNotFromCareer = null) : base(
            actorID,
            ComponentType.Actor)
        {
            CareerName = careerName;
            AllJobs = jobsNotFromCareer ?? new HashSet<JobName>();

            foreach (var job in Career_Manager.GetCareer_Master(careerName).CareerBaseJobs)
            {
                AddJob(job);
            }
        }

        public Career_Data(Career_Data careerData) : base(careerData.ActorReference.ActorID, ComponentType.Actor)
        {
            CareerName = careerData.CareerName;
            AllJobs = new HashSet<JobName>(careerData.AllJobs);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Career Name", $"{CareerName}" },
                { "Jobs Active", $"{JobsActive}" },
                { "JobSiteID", $"{JobSiteID}" },
                { "Current Job", $"{CurrentJob?.JobName}" }
            };
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public CareerName CareerName;

        public void SetCareer(CareerName careerName, bool changeAllCareerJobs = true)
        {
            CareerName = careerName;

            if (!changeAllCareerJobs) return;

            AllJobs.Clear();

            var careerJobs = Career_Manager.GetCareer_Master(careerName).CareerBaseJobs;

            foreach (var job in careerJobs)
            {
                AddJob(job);
            }
        }

        List<JobName> _allJobs; // For saving purposes
        public HashSet<JobName> AllJobs;

        public HashSet<JobTaskName> AllJobTasks =>
            AllJobs.SelectMany(jobName => Job_Manager.GetJob_Master(jobName).JobTasks).ToHashSet();

        public void AddJob(JobName jobName)
        {
            // Find a way to use actorData to exclude jobs that are not allowed due to status and conditions like paralyzed, or
            // personalities.

            if (AllJobs.Add(jobName)) return;

            //Debug.LogWarning($"Job {jobName} already exists in AllActorJobs.");
        }

        public void RemoveJob(JobName jobName)
        {
            if (AllJobs.Remove(jobName)) return;

            //Debug.LogWarning($"Job {jobName} does not exist in AllActorJobs.");
        }

        [SerializeField] Job _currentJob;
        public Job CurrentJob => _currentJob ??= new Job(JobName.Idle, 0, 0);

        public HashSet<JobTaskName> CurrentJobTasks =>
            Job_Manager.GetJob_Master(CurrentJob.JobName).JobTasks.ToHashSet();

        public void SetCurrentJob(Job job)
        {
            _currentJob = job;
        }

        public bool HasCurrentJob() => CurrentJob != null && CurrentJob.JobName != JobName.Idle;

        public void StopCurrentJob() => _currentJob = null;

        public bool GetNewCurrentJob(uint stationID = 0)
        {
            return CareerName != CareerName.Wanderer &&
                   JobSite.GetNewCurrentJob(ActorReference.Actor_Component, stationID);
        }

        public bool JobsActive = true;
        public void ToggleDoJobs(bool jobsActive) => JobsActive = jobsActive;

        public uint JobSiteID;
        JobSite_Component _jobSite;
        public JobSite_Component JobSite => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        public void SetJobSiteID(uint jobSiteID) => JobSiteID = jobSiteID;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class SpeciesAndPersonality : Priority_Updater
    {
        public SpeciesAndPersonality(uint actorID, SpeciesName actorSpecies, ActorPersonality actorPersonality) : base(
            actorID, ComponentType.Actor)
        {
            ActorSpecies = actorSpecies;
            ActorPersonality = actorPersonality ??
                               new ActorPersonality(
                                   Personality_Manager.GetRandomPersonalityTraits(null, 3, ActorSpecies));
        }

        public SpeciesAndPersonality(SpeciesAndPersonality speciesAndPersonality) : base(
            speciesAndPersonality.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorSpecies = speciesAndPersonality.ActorSpecies;
            ActorPersonality = new ActorPersonality(speciesAndPersonality.ActorPersonality);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Species And Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var speciesData = new Dictionary<string, string>
            {
                { "Actor Species", $"{ActorSpecies}" }
            };

            var personalityData = ActorPersonality.SubData;

            return speciesData.Concat(personalityData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
    public class StatsAndAbilities : Priority_Updater
    {
        public StatsAndAbilities(uint actorID, Actor_Stats actorStats, Actor_Aspects actorAspects, Actor_Abilities actorAbilities) : base(actorID, ComponentType.Actor)
        {
            Actor_Stats = actorStats ?? new Actor_Stats(0, new ActorLevelData(), new Special(), new CombatStats());
            Actor_Aspects = actorAspects ?? new Actor_Aspects(0);
            Actor_Abilities = actorAbilities ?? new Actor_Abilities(0);
        }

        public StatsAndAbilities(StatsAndAbilities statsAndAbilities) : base(statsAndAbilities.ActorReference.ActorID, ComponentType.Actor)
        {
            Actor_Stats = new Actor_Stats(statsAndAbilities.Actor_Stats);
            Actor_Aspects = new Actor_Aspects(statsAndAbilities.Actor_Aspects);
            Actor_Abilities = new Actor_Abilities(statsAndAbilities.Actor_Abilities);
        }

        public override Dictionary<string, string> GetStringData()
        {
            var statsData = Actor_Stats.GetStringData();
            var aspectsData = Actor_Aspects.GetStringData();
            var abilitiesData = Actor_Abilities.GetStringData();

            return statsData.Concat(aspectsData).Concat(abilitiesData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Actor_Stats Actor_Stats;
        public void SetActorStats(Actor_Stats actorStats) => Actor_Stats = actorStats;

        public Actor_Aspects Actor_Aspects;
        public void SetActorAspects(Actor_Aspects actor_Aspects) => Actor_Aspects = actor_Aspects;

        public Actor_Abilities Actor_Abilities;
        public void SetActorAbilities(Actor_Abilities actor_Abilities) => Actor_Abilities = actor_Abilities;
        
        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class ActorName
    {
        public string Name;
        public string Surname;
        public string GetName() => $"{Name} {Surname}";
        public TitleName CurrentTitle;
        public List<TitleName> AvailableTitles;

        public void SetTitleAsCurrentTitle(TitleName titleName)
        {
            if (AvailableTitles.Contains(titleName)) CurrentTitle = titleName;
        }

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
    public class Actor_Stats : Priority_Updater
    {
        public Actor_Stats(uint actorID, ActorLevelData actorLevelData, Special actorSpecial,
            CombatStats actorCombatStats) :
            base(actorID, ComponentType.Actor)
        {
            ActorLevelData = actorLevelData;
            ActorSpecial = actorSpecial;
            ActorCombatStats = actorCombatStats;
        }

        public Actor_Stats(Actor_Stats actorStats) : base(actorStats.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorLevelData = new ActorLevelData(actorStats.ActorLevelData);
            ActorSpecial = new Special(actorStats.ActorSpecial);
            ActorCombatStats = new CombatStats(actorStats.ActorCombatStats);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Actor Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var actorLevelData = ActorLevelData.GetStringData();
            var actorSpecial = ActorSpecial.GetStringData();
            var actorCombatStats = ActorCombatStats.GetStringData();

            return actorLevelData.Concat(actorSpecial).Concat(actorCombatStats)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ActorLevelData ActorLevelData;
        public Special ActorSpecial;
        public CombatStats ActorCombatStats;

        public float TotalCarryWeight =>
            100; // For now. Eventually. ActorSpecial.Strength * 10; // Later add any effects from perks, equipment, etc.

        public float AvailableCarryWeight => TotalCarryWeight -
                                             Item.GetItemListTotal_Weight(ActorReference.Actor_Component.ActorData
                                                 .InventoryData
                                                 .GetAllInventoryItemsClone().Values.ToList());

        public void AddExperience(uint experience)
        {
            ActorLevelData.TotalExperience += experience;

            LevelUpCheck();
        }

        public void LevelUpCheck()
        {
            var levelData = Manager_CharacterLevels.GetLevelUpData(ActorLevelData.ActorLevel);

            if (ActorLevelData.TotalExperience < levelData.TotalExperienceRequired) return;

            _levelUp(levelData);
        }

        void _levelUp(CharacterLevelData levelData)
        {
            switch (levelData.BonusType)
            {
                case LevelUpBonusType.Health:
                    ActorCombatStats.BaseMaxHealth += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Mana:
                    ActorCombatStats.BaseMaxMana += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.Stamina:
                    ActorCombatStats.BaseMaxStamina += levelData.BonusStatPoints;
                    break;
                case LevelUpBonusType.SkillSet:
                    //ActorLevelData.CanAddNewSkillSet = true;
                    // Change it so that instead of a bool it just checks against the level to return the bool.
                    break;
                case LevelUpBonusType.Ultimate:
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
    public class Actor_Aspects : Priority_Updater
    {
        public Actor_Aspects(uint actorID, List<AspectName> actorAspectList = null) : base(actorID, ComponentType.Actor)
        {
            ActorAspectList = actorAspectList ?? new List<AspectName>
                { AspectName.None, AspectName.None, AspectName.None };
        }

        public Actor_Aspects(Actor_Aspects actorAspects) : base(actorAspects.ActorReference.ActorID,
            ComponentType.Actor)
        {
            ActorAspectList = new List<AspectName>(actorAspects.ActorAspectList);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Actor Aspects",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Class Title", $"{ActorClassTitle}" },
                { "Actor Aspect List", $"{ActorAspectList[0]}, {ActorAspectList[1]}, {ActorAspectList[2]}" }
            };
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public ClassTitle ActorClassTitle => Actor_Aspect_List.GetCharacterTitle(ActorAspectList);

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
    public class ActorLevelData
    {
        public ActorLevelData(uint totalExperience = 0)
        {
            TotalExperience = totalExperience;
        }

        public ActorLevelData(ActorLevelData actorLevelData)
        {
            TotalExperience = actorLevelData.TotalExperience;
            UsedSkillPoints = actorLevelData.UsedSkillPoints;
            UsedSpecialPoints = actorLevelData.UsedSpecialPoints;
        }

        public Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Level", $"{ActorLevel}" },
                { "Total Experience", $"{TotalExperience}" },
                { "Total Skill Points", $"{TotalSkillPoints}" },
                { "Used Skill Points", $"{UsedSkillPoints}" },
                { "Total Special Points", $"{TotalSpecialPoints}" },
                { "Used Special Points", $"{UsedSpecialPoints}" }
            };
        }

        public uint ActorLevel => Manager_CharacterLevels.GetLevelFromExperience(TotalExperience);
        public uint TotalExperience;
        public uint TotalSkillPoints => Manager_CharacterLevels.GetTotalSkillPointsFromExperience(TotalExperience);
        public uint UsedSkillPoints; // Can change this to be calculated by used skill points from the other class.
        public uint TotalSpecialPoints => Manager_CharacterLevels.GetTotalSpecialPointsFromExperience(TotalExperience);
        public uint UsedSpecialPoints; // Can change this to be calculated by used skill points from the other class.
    }

    [Serializable]
    public class Crafting_Data : Priority_Updater
    {
        public Crafting_Data(uint actorID, List<RecipeName> knownRecipes = null) : base(actorID, ComponentType.Actor)
        {
            KnownRecipes = knownRecipes ?? new List<RecipeName>();
        }

        public Crafting_Data(Crafting_Data craftingData) : base(craftingData.ActorReference.ActorID,
            ComponentType.Actor)
        {
            KnownRecipes = new List<RecipeName>(craftingData.KnownRecipes);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Crafting Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return KnownRecipes.ToDictionary(recipe => $"{(uint)recipe}", recipe => $"{recipe}");
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public List<RecipeName> KnownRecipes;

        public bool AddRecipe(RecipeName recipeName)
        {
            if (KnownRecipes.Contains(recipeName)) return false;

            KnownRecipes.Add(recipeName);

            return true;
        }

        public IEnumerator CraftItemAll(RecipeName recipeName)
        {
            var recipe = Recipe_Manager.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActor_Data(ActorReference.ActorID);

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

            Recipe_Data recipeData = Recipe_Manager.GetRecipe_Master(recipeName);

            var actorData = Actor_Manager.GetActor_Data(ActorReference.ActorID);

            if (!_inventoryContainsAllIngredients(actorData, recipeData.RequiredIngredients))
            {
                Debug.Log("Inventory does not contain all ingredients.");
                yield break;
            }

            if (!actorData.InventoryData.HasSpaceForItems(recipeData.RecipeProducts))
            {
                Debug.Log("Inventory does not have space for produced items.");
                yield break;
            }

            actorData.InventoryData.RemoveFromInventory(recipeData.RequiredIngredients);
            actorData.InventoryData.AddToInventory(recipeData.RecipeProducts);
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }

    [Serializable]
    public class QuestUpdater : Priority_Updater
    {
        public QuestUpdater(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public QuestUpdater(QuestUpdater questUpdater) : base(questUpdater.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorQuests = new List<Quest>(questUpdater.ActorQuests);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Quests",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return ActorQuests.ToDictionary(quest => $"{quest.QuestID}", quest => $"{quest.QuestName}");
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
    public class Vocation_Data : Priority_Updater
    {
        public Vocation_Data(uint actorID, Dictionary<VocationName, ActorVocation> actorVocations = null) : base(
            actorID,
            ComponentType.Actor)
        {
            ActorVocations = actorVocations ?? new Dictionary<VocationName, ActorVocation>();
        }

        public Vocation_Data(Vocation_Data vocationData) : base(vocationData.ActorReference.ActorID,
            ComponentType.Actor)
        {
            ActorVocations = new Dictionary<VocationName, ActorVocation>(vocationData.ActorVocations);
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return ActorVocations.Values.ToDictionary(vocation => $"{vocation.VocationName}:",
                vocation => $"{vocation.VocationExperience}");
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Dictionary<VocationName, ActorVocation> ActorVocations;
        public void SetVocations(Dictionary<VocationName, ActorVocation> vocations) => ActorVocations = vocations;

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
        public VocationName VocationName;
        public VocationTitle VocationTitle;
        public float VocationExperience;

        public ActorVocation(VocationName vocationName, float vocationExperience)
        {
            VocationName = vocationName;
            VocationExperience = vocationExperience;

            // Implement later
            //VocationTitle = Manager_Vocation.GetVocation(vocationName).GetVocationTitle(vocationExperience);
        }
    }
}