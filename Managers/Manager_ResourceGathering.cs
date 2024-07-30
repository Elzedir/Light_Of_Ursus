using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResourceStationName
{
    None,
    Iron_Node,
    Tree,
    Fishing_Spot,
    Farming_Plot,
}

public class Manager_ResourceGathering : MonoBehaviour
{
    public static Collider GetTaskArea(Actor_Base actor, string taskObjectName)
    {
        float radius = 100; // Change the distance to depend on the area somehow, later.
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;

        Collider[] colliders = Physics.OverlapSphere(actor.transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.name.Contains(taskObjectName))
            {
                float distance = Vector3.Distance(actor.transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = collider;
                }
            }
        }

        return closestCollider;
    }

    public static IResourceStation GetNearestResource(ResourceStationName resourceStationName, Vector3 currentPosition)
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<IResourceStation>()
            .Where(station => station.GetResourceStationName() == resourceStationName)
            .OrderBy(station => Vector3.Distance(currentPosition, station.GameObject.transform.position))
            .FirstOrDefault();
    }
}

public class GatheringComponent
{
    public Actor_Base Actor;
    public IResourceStation ResourceStation;

    Coroutine _gatheringCoroutine;

    public GatheringComponent(Actor_Base actor)
    {
        Actor = actor;
    }

    public IEnumerator GatherResource(IResourceStation resourceStation)
    {
        ResourceStation = resourceStation;

        // Add in a while loop to gather until a certain condition is fulfilled.

        yield return _gatheringCoroutine = Actor.StartCoroutine(resourceStation.GatherResource(Actor));

        if (!addedIngredientsToActor(resourceStation.GetResourceYield(Actor)))
        {
            // Drop resources on floor
            Debug.Log("Couldn't add to inventory");
        }

        bool addedIngredientsToActor(List<Item> items)
        {
            return Actor.InventoryComponent.AddToInventory(items);
        }
    }
}