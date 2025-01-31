using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using ActorPresets;
using Equipment;
using Faction;
using Inventory;
using Priority;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actors
{
    [Serializable]
    public class Actor_Data : Data_Class
    {
        public bool IsSpawned;
        public ulong ActorID => Identification.ActorID;
        public ulong ActorFactionID => Identification.ActorFactionID;
        public string ActorName => Identification.ActorName.GetName();

        Actor_Component _actor;
        public Actor_Component Actor => _actor ??= Actor_Manager.GetActor_Component(ActorID);

        public ActorDataPresetName ActorDataPresetName;

        public Actor_Data_Identification Identification;
        public Priority_Data_Actor Priority;

        public Actor_Data_SceneObject SceneObject;
        public Actor_Data_Career Career;
        public Actor_Data_Crafting Crafting;
        public Actor_Data_Vocation Vocation;
        public Actor_Data_Species Species;
        public Actor_Data_Personality Personality;
        public Actor_Data_StatsAndAbilities StatsAndAbilities;
        public Actor_Data_StatesAndConditions StatesAndConditions;

        public Actor_Data_Proximity Proximity;

        public InventoryData InventoryData;
        public Equipment_Data EquipmentData;

        public QuestClass ActorQuests;

        public void InitialiseActorData()
        {
            var actor = Actor_Manager.GetActor_Component(ActorID);

            if (actor is null)
            {
                Debug.LogError($"Manager_Actor cannot get actor {ActorID}.");
                return;
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

        public Actor_Data(ActorDataPresetName actorDataPresetName, Actor_Data_Identification identification = null,
            Actor_Data_SceneObject sceneObject = null,
            Actor_Data_Career career = null, Actor_Data_Crafting crafting = null,
            Actor_Data_Vocation vocation = null,
            Actor_Data_Species species = null,
            Actor_Data_Personality personality = null,
            Actor_Data_StatsAndAbilities statsAndAbilities = null,
            Actor_Data_StatesAndConditions statesAndConditions = null, 
            InventoryData inventoryData = null,
            Equipment_Data equipmentData = null, 
            Priority_Data_Actor priority = null,
            Actor_Data_Proximity proximity = null,
            QuestClass actorQuests = null)
        {
            ActorDataPresetName = actorDataPresetName;

            Identification = identification;
            SceneObject = sceneObject;
            Career = career;
            Crafting = crafting;
            Vocation = vocation;
            Species = species;
            Personality = personality;
            StatsAndAbilities = statsAndAbilities;
            StatesAndConditions = statesAndConditions;
            InventoryData = inventoryData;
            EquipmentData = equipmentData;
            Priority = priority;
            Proximity = proximity;
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
                title: "Scene Object Properties",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: SceneObject?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Species",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Species?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Personality?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Career?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Crafting Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Crafting?.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Vocation?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "Stats And Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatsAndAbilities?.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "States And Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: StatesAndConditions?.GetDataToDisplay(toggleMissingDataDebugs));
            
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
            
            _updateDataDisplay(DataToDisplay,
                title: "Proximity Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Proximity?.GetDataToDisplay(toggleMissingDataDebugs));

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
                    Identification?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Scene Object Data",
                    SceneObject?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Species",
                    Species?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Personality",
                    Personality?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Career Data",
                    Career?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Crafting Data",
                    Crafting?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Vocation Data",
                    Vocation?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Stats And Abilities",
                    StatsAndAbilities?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "States And Conditions",
                    StatesAndConditions?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Inventory Data",
                    InventoryData?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Equipment Data",
                    EquipmentData?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Priority Data",
                    Priority?.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Proximity Data",
                    Proximity?.GetDataToDisplay(toggleMissingDataDebugs)
                }
            };
        }

        public HashSet<ActorActionName> GetAllowedActions()
        {
            var allowedActions = new HashSet<ActorActionName>();

            foreach (var action in Identification.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in SceneObject.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Species.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Personality.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in StatsAndAbilities.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in StatesAndConditions.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Career.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Crafting.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Vocation.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in InventoryData.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in EquipmentData.GetAllowedActions()) allowedActions.Add(action);
            foreach (var action in Proximity.GetAllowedActions()) allowedActions.Add(action);

            return allowedActions;
        }

        public float GetActorRelation(Actor_Data otherActor)
        {
            float relation = 0;

            relation += Faction_Manager.GetFaction_Data(ActorFactionID).GetFactionRelationship_Value(otherActor.ActorFactionID);
            relation += Personality.ComparePersonality(otherActor.Personality);

            return relation;
        }

    }

    public enum DataChangedName
    {
        None,

        ChangedName,
        ChangedFaction,
        ChangedCity,
        ChangedBirthdate,
        ChangedFamily,
        ChangedBackground,
        
        ChangedCareer,

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
        ChangedJobSite,
        ChangedStation,
        ChangedOperatingArea,
        ChangedEmployeePosition,
        ChangedSpecies,
        ChangedInventory,
        DroppedItems,
        PriorityCompleted,

        ChangedState,
        ChangedCondition,
    }

    [Serializable]
    public class WorldStateClass : Priority_Class
    {
        public WorldStateClass(ulong actorID) : base(actorID, ComponentType.Actor)
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

        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    }

    [Serializable]
    public class QuestClass : Priority_Class
    {
        public QuestClass(ulong actorID) : base(actorID, ComponentType.Actor)
        {
        }

        public QuestClass(QuestClass questClass) : base(questClass.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorQuests = new List<Quest>(questClass.ActorQuests);
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
        
        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    }
}