using System;
using System.Collections;
using UnityEngine;

public class OperatingAreaComponent : MonoBehaviour
{
    public OperatingAreaData OperatingAreaData;
    public void SetOperatingAreaData(OperatingAreaData operatingAreaData) => OperatingAreaData = operatingAreaData;
    public void SetStationID(int stationID) => OperatingAreaData.StationID = stationID;
    public BoxCollider OperatingArea;

    public string GetName() => name;

    public void Awake()
    {
        OperatingArea = GetComponent<BoxCollider>();

        if (!OperatingArea.isTrigger)
        {
            Debug.Log($"Set IsTrigger to true for {name}");
            OperatingArea.isTrigger = true;
        }
    }

    public void Initialise()
    {
        
    }

    public float Operate(float baseProgressRate, Recipe recipe)
    {
        if (OperatingAreaData.CurrentOperatorID == -1)
        {
            Debug.Log("No operator assigned.");
            return 0;
        }

        if (OperatingAreaData.IsOperatorMovingToOperatingArea) return 0;

        var actorData = Manager_Actor.GetActorData(OperatingAreaData.CurrentOperatorID);

        if (!OperatingArea.bounds.Contains(actorData.GameObjectProperties.ActorTransform.position))
        {
            StartCoroutine(MoveOperatorToOperatingArea(Manager_Actor.GetActor(actorID: OperatingAreaData.CurrentOperatorID), transform.position));
        }

        if (actorData.GameObjectProperties.ActorTransform.position != transform.position)
        {
            actorData.GameObjectProperties.ActorTransform.position = transform.position;
        }

        Debug.Log($"Operating {name}");

        float productionRate = baseProgressRate;
        // Then modify production rate by any area modifiers (Land type, events, etc.)

        foreach (var vocation in recipe.RequiredVocations)
        {
            productionRate *= actorData.VocationData.GetProgress(vocation);
        }

        return productionRate;
    }

    protected IEnumerator MoveOperatorToOperatingArea(Actor_Base actor, Vector3 position)
    {
        OperatingAreaData.IsOperatorMovingToOperatingArea = true;

        yield return actor.StartCoroutine(actor.BasicMove(position));

        OperatingAreaData.IsOperatorMovingToOperatingArea = false;
    }
}
