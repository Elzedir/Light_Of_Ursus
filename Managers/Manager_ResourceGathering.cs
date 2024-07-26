using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceName
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

    public static Interactable_Resource GetNearestCraftingStation(ResourceName resourceName, Vector3 currentPosition)
    {
        float radius = 100; // Change the distance to depend on the area somehow, later.
        Interactable_Resource closestResource = null;
        float closestDistance = float.MaxValue;

        Collider[] colliders = Physics.OverlapSphere(currentPosition, radius);

        foreach (Collider collider in colliders)
        {
            Interactable_Resource craftingStation = collider.GetComponent<Interactable_Resource>();

            if (craftingStation.ResourceName == resourceName)
            {
                float distance = Vector3.Distance(currentPosition, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestResource = craftingStation;
                }
            }
        }

        return closestResource;
    }
}
