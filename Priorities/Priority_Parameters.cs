using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Actors;
using Buildings;
using Inventory;
using Items;
using Station;
using UnityEngine;
using UnityEngine.Serialization;

namespace Priorities
{
    [Serializable]
    public class Priority_Parameters
    {
        public float DefaultPriorityValue = 0;

        public ulong ActorID_Source, ActorID_Target;
        public ulong BuildingID_Source;
        public ulong BuildingID_Target;
        
        public Station_Component HighestPriorityStation_Source, HighestPriorityStation_Target;
        
        public List<Station_Component> AllStation_Sources, AllStation_Targets;
        
        public List<Item> Items;
        
        public Vector3 Position_Source, Position_Destination;
        
        public float DefaultMaxPriority, TotalDistance;
        public long TotalItems; 
        
        public Actor_Component Actor_Component_Source => ActorID_Source != 0
        ? Actor_Manager.GetActor_Component(ActorID_Source)
        : null;
        
        public Actor_Component Actor_Component_Target => ActorID_Target != 0
            ? Actor_Manager.GetActor_Component(ActorID_Target)
            : null;
        
        public Building_Component Building_Component_Source =>  BuildingID_Source != 0
            ? Building_Manager.GetBuilding_Component(BuildingID_Source)
            : null;
        
        public Building_Component Building_Component_Target =>  BuildingID_Target != 0
            ? Building_Manager.GetBuilding_Component(BuildingID_Target)
            : null;
        
        public InventoryData Inventory_Hauler => ActorID_Source != 0 
            ? Actor_Component_Source.ActorData.InventoryData 
            : null;
    }
}