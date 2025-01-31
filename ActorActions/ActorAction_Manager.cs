using UnityEngine;

namespace ActorActions
{
    public abstract class ActorAction_Manager : MonoBehaviour
    {
        public static ActorAction_Data GetActorAction_Data(ActorActionName actorActionName)
        {
            if (ActorAction_List.S_AllActorAction_Data.TryGetValue(actorActionName, out var actorAction))
            {
                return actorAction;
            }

            Debug.LogError($"ActorAction_Data not found for: {actorActionName}.");
            return null;
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
        
        //* Change the system so that only these three, or minimal ones, are used, instead of having a different one for every station.
        //* Have them determined by task and station together. So sawmill process is logs, sawmill craft is arrows and poles.

        Perform_Station_Task,
        
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
        Haul_Deliver,
        Haul_Fetch,

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