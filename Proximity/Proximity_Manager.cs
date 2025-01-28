using System;
using System.Collections.Generic;
using Actor;
using City;
using IDs;
using JobSite;
using Managers;
using Station;
using UnityEngine;

namespace Proximity
{
    public abstract class Proximity_Manager
    {
        static Dictionary<ulong, GameObject> s_proximity_GameObjects;
        public static Dictionary<ulong, GameObject> S_Proximity_GameObjects => s_proximity_GameObjects ??= PopulateProximity_GameObjects();
        
        static Dictionary<ulong, Actor_Component> s_proximity_Actors;
        public static Dictionary<ulong, Actor_Component> S_Proximity_Actors => s_proximity_Actors ??= _populateProximity_Actors();
        
        static Dictionary<ulong, Station_Component> s_proximity_Stations;
        public static Dictionary<ulong, Station_Component> S_Proximity_Stations => s_proximity_Stations ??= _populateProximity_Stations();
        static Dictionary<ulong, JobSite_Component> s_proximity_JobSites;
        public static Dictionary<ulong, JobSite_Component> S_Proximity_JobSites => s_proximity_JobSites ??= _populateProximity_JobSites();
        
        static SphereCollider s_proximity_Collider_5M;
        static SphereCollider S_Proximity_Collider_5M => s_proximity_Collider_5M ??= _getPriority_Collider();
        
        static Vector3 _playerPosition => Manager_Game.S_Instance.Player.transform.position;
        
        //* Eventually upgrade the boxCollider to a grid priorityArea, with each subsequent collider updating less and less frequently.
        //* So within 5 metres of the player, 10 metres, 20, etc.
        
        static SphereCollider _getPriority_Collider()
        {
            var collider_5M = Manager_Game
                .FindTransformRecursively(GameObject.Find("GameObjects").transform, "Proximity_Collider_5m")?.gameObject
                .AddComponent<SphereCollider>();

            if (collider_5M is null)
            {
                throw new NullReferenceException("Proximity_Collider_5m not found.");
            }
            
            collider_5M.radius = 5;
            collider_5M.isTrigger = true;
            
            return collider_5M;
        }

        static Dictionary<ulong, Actor_Component> _populateProximity_Actors()
        {
            PopulateProximity_GameObjects();

            return s_proximity_Actors;
        }
        
        static Dictionary<ulong, Station_Component> _populateProximity_Stations()
        {
            PopulateProximity_GameObjects();

            return s_proximity_Stations;
        }
        
        static Dictionary<ulong, JobSite_Component> _populateProximity_JobSites()
        {
            PopulateProximity_GameObjects();

            return s_proximity_JobSites;
        }
        
        public static Dictionary<ulong, GameObject> PopulateProximity_GameObjects(bool refresh = false)
        {
            s_proximity_GameObjects ??= new Dictionary<ulong, GameObject>();
            s_proximity_Actors ??= new Dictionary<ulong, Actor_Component>();
            s_proximity_Stations ??= new Dictionary<ulong, Station_Component>();
            s_proximity_JobSites ??= new Dictionary<ulong, JobSite_Component>();

            if (refresh)
            {
                s_proximity_GameObjects.Clear();
                s_proximity_Actors.Clear();
                s_proximity_Stations.Clear();
                s_proximity_JobSites.Clear();
            }

            S_Proximity_Collider_5M.transform.position = _playerPosition;
            
            var hitColliders = new Collider[100000];
            
            var numColliders = Physics.OverlapSphereNonAlloc(S_Proximity_Collider_5M.transform.position, S_Proximity_Collider_5M.radius, hitColliders);
            
            for (var i = 0; i < numColliders; i++)
            {
                var collider = hitColliders[i];
                var key = ID_Manager.GetGameObjectID(collider.gameObject);
        
                s_proximity_GameObjects[key] = collider.gameObject;
                
                switch (ID_Manager.GetIDType(ID_Manager.GetGameObjectID(collider.gameObject)))
                {
                    case IDType.Actor:
                        s_proximity_Actors[key] = collider.gameObject.GetComponent<Actor_Component>();
                        break;
                    case IDType.Station:
                        s_proximity_Stations[key] = collider.gameObject.GetComponent<Station_Component>();
                        break;
                    case IDType.JobSite:
                        s_proximity_JobSites[key] = collider.gameObject.GetComponent<JobSite_Component>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"IDType not found for GameObject: {collider.gameObject.name}");
                }
            }

            return s_proximity_GameObjects;
        }
    }
}