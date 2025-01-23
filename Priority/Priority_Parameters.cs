using System;
using System.Collections.Generic;
using Actor;
using Items;
using JobSite;
using Station;
using UnityEngine;
using WorkPosts;

namespace ActorActions
{
    [Serializable]
    public class Priority_Parameters
    {
        public uint ActorID_Source;
        public Actor_Component Actor_Component_Source => Actor_Manager.GetActor_Component(ActorID_Source);
        
        public uint ActorID_Target;
        public Actor_Component Actor_Component_Target => Actor_Manager.GetActor_Component(ActorID_Target);
        
        public uint JobSiteID_Source;
        public JobSite_Component JobSite_Component_Source => JobSite_Manager.GetJobSite_Component(JobSiteID_Source);
        
        public uint JobSiteID_Target;
        public JobSite_Component JobSite_Component_Target => JobSite_Manager.GetJobSite_Component(JobSiteID_Target);
        
        public uint StationID_Source;
        public Station_Component Station_Component_Source => Station_Manager.GetStation_Component(StationID_Source);
        
        public uint StationID_Destination;
        public Station_Component Station_Component_Destination => Station_Manager.GetStation_Component(StationID_Destination);
        
        public uint WorkPostID_Source;
        public WorkPost_Component WorkPost_Component_Source => Station_Component_Source.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Source, null);

        public uint WorkPostID_Destination;
        public WorkPost_Component WorkPost_Component_Destination => Station_Component_Destination.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Destination, null); 
        
        public List<Item> Items;
        
        public Vector3 Position_Source;
        public Vector3 Position_Destination;
    }
}