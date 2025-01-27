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

        Dictionary<ulong, GameObject> _getRelevantProximityGameObjects()
        {
            var relevantProximityGameObjects = new Dictionary<ulong, GameObject>();

            foreach (var actor in Proximity_Manager.S_Proximity_Actors)
            {
                
            }
            
            return relevantProximityGameObjects;
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