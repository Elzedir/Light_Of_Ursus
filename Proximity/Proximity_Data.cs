using System.Collections.Generic;
using Priority;
using UnityEngine;

namespace Proximity
{
    public class Proximity_Data : Priority_Class //* Just for the references right now, change later.
    {
        public GameObject ClosestAlly;
        
        public GameObject ClosestEnemy;

        public void PopulateProximityData()
        {
            _assignAllies();
            _assignNeutrals();
            _assignEnemies();
        }

        Dictionary<uint, GameObject> _getRelevantProximityGameObjects()
        {
            var proximityGameObjects = new Dictionary<uint, GameObject>();

            foreach (var proximityGameObject in Proximity_Manager.Proximity_GameObjects)
            {
                
            }
            
            return proximityGameObjects;
        }
        
        void _assignAllies()
        {
            _assignClosestAlly();
        }
        
        void _assignClosestAlly()
        {
            
        }
        
        void _assignNeutrals()
        {
            
        }

        void _assignEnemies()
        {
            _assignClosestEnemy();
        }

        void _assignClosestEnemy()
        {
            
        }
    }
}