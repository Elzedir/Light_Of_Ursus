using System;
using System.Collections.Generic;
using Actor;
using IDs;
using Managers;
using UnityEngine;

namespace Proximity
{
    public abstract class Proximity_Manager
    {
        static Dictionary<ulong, Actor_Component> s_proximity_GameObjects;
        public static Dictionary<ulong, Actor_Component> S_Proximity_GameObjects => s_proximity_GameObjects ??= PopulateProximity_GameObjects();
        
        static Dictionary<ulong, Actor_Component> s_proximity_Actors;
        public static Dictionary<ulong, Actor_Component> S_Proximity_Actors => s_proximity_Actors ??= PopulateProximity_GameObjects(IDType.Actor);
        
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
        
        public static Dictionary<ulong, GameObject> PopulateProximity_GameObjects(bool refresh = false)
        {
            s_proximity_Actors ??= new Dictionary<ulong, GameObject>();
            
            if (refresh) s_proximity_Actors.Clear();

            S_Proximity_Collider_5M.transform.position = _playerPosition;
            
            var hitColliders = new Collider[100000];
            
            var numColliders = Physics.OverlapSphereNonAlloc(S_Proximity_Collider_5M.transform.position, S_Proximity_Collider_5M.radius, hitColliders);
            
            for (var i = 0; i < numColliders; i++)
            {
                var collider = hitColliders[i];
                var key = ID_Manager.GetGameObjectID(collider.gameObject);
        
                s_proximity_Actors[key] = collider.gameObject;
            }

            return s_proximity_Actors;
        }
    }
}