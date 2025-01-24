using System.Collections.Generic;
using UnityEngine;

namespace Proximity
{
    public abstract class Proximity_Manager
    {
        static Dictionary<uint, GameObject> _proximity_GameObjects;
        public static Dictionary<uint, GameObject> Proximity_GameObjects => _proximity_GameObjects ??= PopulateProximity_GameObjects();  
        
        //* We should then make all IDs consistent across the game so we can do this more easily. We can do a check where each gameObjectType is assigned an
        //* ID range, so we can check the ID range and then assign the gameObject type. Assign per 100 000 or something like that and we can scale up the IDs
        //* as we scale up the game. For example, just add 1 000 000, 
        //* So, items: 0 - 99 999, characters 100 000 - 199 999. Then if they fill, then items becomes 1 099 999, characters 1 100 000 - 1 199 999.
        
        BoxCollider _priority_Collider;
        public BoxCollider Priority_Collider => _priority_Collider ??= _getPriority_Collider();
        
        //* Instead of this, why not create a BoxCollider that goes over the player. It enables, collects all GameObjects in it, and then disables.
        //* Every one in the scene that is enabled then iterates through that list and assigns variables to it, like closestEnemy, closestAlly, lowestHealthEnemy,
        //* lowestHealthAlly, etc. Then we populate priority with those variables. Check the efficieny of that.
        
        //* Eventually upgrade the boxCollider to a grid priorityArea, with each subsequent collider updating less and less frequently. So within 1 metre of the player, 5 metres, 10 metres, etc.
        BoxCollider _getPriority_Collider()
        {
            //* Put a BoxCollider in the scene, that when populating game Objects, it centres on the player's location and then gets
            //* all GameObjects within the collider. 
            return null;
        }
        
        public static Dictionary<uint, GameObject> PopulateProximity_GameObjects(bool refresh = false)
        {
            if (_proximity_GameObjects is null || refresh)
            {
                _proximity_GameObjects = new Dictionary<uint, GameObject>();
            }
            
            //Physics.OverlapSphere(ClosestAlly.transform.position, 10f); Maybe this, or something else.
            
            //* Populate the dictionary with all GameObjects in the scene.
            
            return _proximity_GameObjects; //* Will there be an issue with returning itself to assign itself?
        }
    }
}