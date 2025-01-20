using System;
using System.Collections.Generic;
using System.Linq;
using ActorPreset;
using Equipment;
using Faction;
using Inventory;
using Managers;
using Priority;
using Relationships;
using StateAndCondition;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actor
{
    [Serializable]
    public class Actor_Data : Data_Class
    {
        public bool IsSpawned;
        public uint ActorID => Identification.ActorID;
        public uint ActorFactionID => Identification.ActorFactionID;
        public string ActorName => Identification.ActorName.GetName();

        Actor_Component _actor;
        public Actor_Component Actor => _actor ??= Actor_Manager.GetActor_Component(ActorID);

        public ActorDataPresetName ActorDataPresetName;

        public Actor_Data_Identification Identification;
        public Priority_Data_Actor Priority;
        
        public Actor_Data_GameObject SceneObject;
        public Actor_Data_Career Career;
        public Actor_Data_Crafting Crafting;
        public Actor_Data_Vocation Vocation;
        public Actor_Data_SpeciesAndPersonality SpeciesAndPersonality;

        public Actor_Data_StatsAndAbilities StatsAndAbilities;

        public Actor_Data_StatesAndConditions StatesAndConditions;
        
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

            var factionGO = UnityEngine.GameObject.Find($"{actorFaction.FactionID}: {actorFaction.FactionName}");

            if (factionGO is null)
            {
                Debug.LogError(
                    $"Actor {ActorID} cannot find faction GameObject {actorFaction.FactionID}: {actorFaction.FactionName}.");
                return;
            }

            actor.transform.parent.SetParent(factionGO.transform);
        }

        // Make an ability to make a deep copy of every class here and every class that needs to be saved.

        public Actor_Data(ActorDataPresetName actorDataPresetName, Actor_Data_Identification identification = null,
            Actor_Data_GameObject sceneObject = null,
            Actor_Data_Career career = null, Actor_Data_Crafting crafting = null,
            Actor_Data_Vocation vocation = null,
            Actor_Data_SpeciesAndPersonality speciesAndPersonality = null,
            Actor_Data_StatsAndAbilities statsAndAbilities = null,
            Actor_Data_StatesAndConditions statesAndConditions = null, Inventory_Data inventoryData = null,
            Equipment_Data equipmentData = null, Priority_Data_Actor priority = null, QuestUpdater actorQuests = null)
        {
            ActorDataPresetName = actorDataPresetName;

            Identification = identification;
            SceneObject = sceneObject;
            Career = career;
            Crafting = crafting;
            Vocation = vocation;
            SpeciesAndPersonality = speciesAndPersonality;
            StatsAndAbilities = statsAndAbilities;
            StatesAndConditions = statesAndConditions;
            InventoryData = inventoryData;
            EquipmentData = equipmentData;
            Priority = priority;
            ActorQuests = actorQuests;
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Actor Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Full Identification",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Identification?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Game Object Properties",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: SceneObject?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Species And Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: SpeciesAndPersonality?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatsAndAbilities?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatesAndConditions?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Career?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Inventory Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: InventoryData?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Equipment Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: EquipmentData?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Priority Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Priority?.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
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
                    Identification.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Game Object Data",
                    SceneObject.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Species And Personality",
                    SpeciesAndPersonality.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Stats And Abilities",
                    StatsAndAbilities.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Career Data",
                    Career.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Inventory Data",
                    InventoryData.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Equipment Data",
                    EquipmentData.GetDataToDisplay(toggleMissingDataDebugs)
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

        ChangedState,
        ChangedCondition,

        #endregion
    }

    [Serializable]
    public class WorldStateUpdater : Priority_Updater
    {
        public WorldStateUpdater(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "World State",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
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
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Relationships",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
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
    public class QuestUpdater : Priority_Updater
    {
        public QuestUpdater(uint actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public QuestUpdater(QuestUpdater questUpdater) : base(questUpdater.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorQuests = new List<Quest>(questUpdater.ActorQuests);
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Quests",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
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
}