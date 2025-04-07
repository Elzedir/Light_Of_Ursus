using Actors;
using Buildings;
using Priorities;
using Station;
using UnityEngine;

namespace Tools
{
    public abstract class ComponentReference
    {
        public ulong ComponentID { get; }
        public ComponentReference(ulong componentID) => ComponentID = componentID;
        protected abstract object               _component { get; }
        public abstract    GameObject           GameObject { get; }
        public abstract    Priority_Data GetPriorityComponent();
    }
    public class ComponentReference_Actor : ComponentReference
    {
        public ulong ActorID => ComponentID;
        public ComponentReference_Actor(ulong actorID) : base(actorID) { }
        Actor_Component                    _actor;
        protected override object         _component => _actor ??= Actor_Manager.GetActor_Component(ComponentID);
        public             Actor_Component Actor_Component      => _component as Actor_Component;
        public Actor_Data ActorData => Actor_Component.ActorData;
        public override GameObject GameObject => Actor_Component.gameObject;
        public override Priority_Data GetPriorityComponent() => Actor_Component.ActorData.Priority;
    }
    public class ComponentReference_Station : ComponentReference
    {
        public ulong StationID => ComponentID;
        public ComponentReference_Station(ulong stationID) : base(stationID) {  }
        Station_Component                    _station;
        protected override object            _component => _station ??= Station_Manager.GetStation_Component(StationID);
        public             Station_Component Station    => _component as Station_Component;
        public Station_Data StationData => Station.Station_Data;
        public override GameObject           GameObject                => Station.gameObject;
        public override Priority_Data GetPriorityComponent() => Station.Building.Building_Data.PriorityData;
    }
    public class ComponentReference_Building : ComponentReference
    {
        public ulong BuildingID => ComponentID;
        public ComponentReference_Building(ulong buildingID) : base(buildingID) { }
        Building_Component                    _building;
        protected override object           _component => _building ??= Building_Manager.GetBuilding_Component(BuildingID);
        public             Building_Component Building    => _component as Building_Component;
        public Building_Data BuildingData => Building.Building_Data;
        public override GameObject           GameObject                => Building.gameObject;
        
        public override Priority_Data GetPriorityComponent() => Building.Building_Data.PriorityData;
    }
}