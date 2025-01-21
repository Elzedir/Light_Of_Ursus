using System.Collections.Generic;
using System.Linq;
using Actor;
using Items;
using JobSite;
using Priority;
using Station;
using UnityEngine;

namespace ActorAction
{
    public abstract class ActorAction_Manager : MonoBehaviour
    {
        public static ActorAction_Data GetActorAction_Data(ActorActionName actorActionName)
        {
            if (ActorAction_List.AllActorAction_Data.TryGetValue(actorActionName, out var actorAction_Data))
            {
                return actorAction_Data;
            }

            Debug.LogError($"ActorAction_Data not found for: {actorActionName}.");
            return null;
        }

        public static Dictionary<PriorityParameterName, object> PopulateActionParameters(
            ActorActionName actorActionName, Dictionary<PriorityParameterName, object> parameters)
        {
            JobSite_Component jobSite_Component = null;
            Actor_Component actor_Component = null;

            var requiredParameters = GetActorAction_Data(actorActionName).RequiredParameters;

            foreach (var requiredParameter in requiredParameters)
            {
                if (parameters.TryGetValue(requiredParameter, out var parameter))
                {
                    if (parameter is null)
                    {
                        Debug.LogError($"Parameter: {requiredParameter} is null.");
                        return null;
                    }

                    switch (requiredParameter)
                    {
                        case PriorityParameterName.Jobsite_Component:
                            jobSite_Component = parameter as JobSite_Component;
                            break;
                        case PriorityParameterName.Worker_Component:
                            actor_Component = parameter as Actor_Component;
                            break;
                    }

                    continue;
                }

                Debug.LogError($"Required Parameter: {requiredParameter} is null in parameters.");
                return null;
            }

            return _populateJobSite(actorActionName, jobSite_Component);
        }

        static Dictionary<PriorityParameterName, object> _populateJobSite(ActorActionName actorAction, JobSite_Component jobSite_Component)
        {
            var taskParameters = new Dictionary<PriorityParameterName, object>
            {
                { PriorityParameterName.Jobsite_Component, jobSite_Component },
                { PriorityParameterName.Total_Items, 0 }
            };

            var allRelevantStations = jobSite_Component.GetRelevantStations(actorAction);

            if (allRelevantStations.Count is 0)
            {
                Debug.LogWarning("No stations to fetch from.");
                return null;
            }

            taskParameters[PriorityParameterName.Total_Items] = allRelevantStations.Sum(station =>
                Item.GetItemListTotal_CountAllItems(station.GetInventoryItems(actorAction)));

            return _setParameters(taskParameters, allRelevantStations, actorAction);
        }

        static Dictionary<PriorityParameterName, object> _setParameters(
            Dictionary<PriorityParameterName, object> taskParameters, List<Station_Component> allRelevantStations,
            ActorActionName actorActionName)
        {
            float highestPriority = 0;
            var currentStationParameters = new Dictionary<PriorityParameterName, object>(taskParameters);

            foreach (var station in allRelevantStations)
            {
                currentStationParameters[PriorityParameterName.Target_Component] =
                    station.Station_Data.InventoryData;
                var stationPriority =
                    Priority_Generator.GeneratePriority((uint)actorActionName, currentStationParameters);

                if (stationPriority is 0 || stationPriority < highestPriority) continue;

                highestPriority = stationPriority;
                taskParameters[PriorityParameterName.Target_Component] = station.Station_Data.InventoryData;
            }

            foreach (var parameter in taskParameters)
            {
                if (parameter.Value is not null && parameter.Value is not 0) continue;

                Debug.LogError($"Parameter: {parameter.Key} is null or 0.");
                return null;
            }

            return taskParameters;
        }
    }

    // ActorAction:
    // A spontaneous or situational action unrelated to structured jobs,
    // often driven by immediate needs, combat, exploration, or player commands.
    // These actions typically occur in reaction to dynamic game states.
    public enum ActorActionName
    {
        // PriorityState None (Can do in all situations)
        Idle,
        All,

        Wander,

        // PriorityState Combat

        Attack,
        Defend,
        Cast_Spell,
        Parry_Attack,

        //Deliver,
        //Fetch,
        Scavenge,

        // PriorityState_All

        Drink_Health_Potion,
        Flee,
        Heal_Ally,
        Explore_Area,
        Loot_Chest,
        Interact_With_Object,
        Equip_Armor,
        Inspect_Tool,
        Open_Door,
        Climb_Wall,
        Eat_Fruit,
        Gather_Herbs,
        Drop_Item,
        Inspect_Inventory,

        // JobTasks:
        // An action performed as part of a structured job or process,
        // typically repetitive and contributing to resource production, crafting, or other systematic gameplay mechanics.
        // Tasks often require specific tools, conditions, or environments.
        
        a,
        //* Change the system so that only these three, or minimal ones, are used, instead of having a different one for every station.
        //* Have them determined by task and station together. So sawmill process is logs, sawmill craft is arrows and poles.

        Haul,
        Craft,
        Process,
        
        // Smith
        Beat_Metal,
        Forge_Armor,
        Forge_Weapon,
        Sharpen_Sword,
        Repair_Armor,
        Repair_Tool,

        // Lumberjack
        Chop_Wood,
        Process_Logs,

        // Vendor
        Stand_At_Counter,
        Restock_Shelves,
        Sort_Items,

        // Guard
        Defend_Ally,
        Defend_Neutral,

        // Medic
        HealSelf,
        SplintSelf,
        HealAllies,
        SplintAllies,
        HealNeutral,
        SplintNeutral,
        HealEnemies,
        SplintEnemies,

        // Forager
        Gather_Food,

        // Miner
        Mine_Ore,
        Refine_Ore,

        // Farmer
        Sow_Crops,
        Water_Crops,
        Harvest_Crops,

        // Hauler
        Deliver_Items,
        Fetch_Items,

        // Cook
        Cook_Food,

        // Other
        Tinker_Item,
        Spin_Thread,
        Weave_Cloth,

        // Scouts
        Map_Area,
    }
}