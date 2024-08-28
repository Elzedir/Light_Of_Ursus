using System;
using System.Collections;
using UnityEngine;

public class OperatingAreaComponent : MonoBehaviour
{
    public OperatingAreaData OperatingAreaData;
    public void SetOperatingAreaData(OperatingAreaData operatingAreaData) => OperatingAreaData = operatingAreaData;
    public BoxCollider OperatingArea;

    public string GetName()
    {
        return name;
    }

    public void Awake()
    {
        OperatingArea = GetComponent<BoxCollider>();

        if (!OperatingArea.isTrigger)
        {
            Debug.Log($"Set IsTrigger to true for {name}");
            OperatingArea.isTrigger = true;
        }
    }

    public void Initialise(int stationID)
    {
        
    }

    public float Operate(float baseProgressRate, Recipe recipe)
    {
        if (OperatingAreaData.CurrentOperator == null)
        {
            Debug.Log("No operator assigned.");
            return 0;
        }

        if (OperatingAreaData.IsOperatorMovingToOperatingArea) return 0;

        if (!OperatingArea.bounds.Contains(OperatingAreaData.CurrentOperator.GameObjectProperties.ActorTransform.position))
        {
            StartCoroutine(MoveOperatorToOperatingArea(
                Manager_Actor.GetActor(actorID: OperatingAreaData.CurrentOperator.ActorID, out Actor_Base actor, factionID: OperatingAreaData.CurrentOperator.ActorFactionID), transform.position));
        }

        if (OperatingAreaData.CurrentOperator.GameObjectProperties.ActorTransform.position != transform.position)
        {
            OperatingAreaData.CurrentOperator.GameObjectProperties.ActorTransform.position = transform.position;
        }

        Debug.Log($"Operating {name}");

        float productionRate = baseProgressRate;
        // Then modify production rate by any area modifiers (Land type, events, etc.)

        foreach (var vocation in recipe.RequiredVocations)
        {
            productionRate *= OperatingAreaData.CurrentOperator.VocationData.GetProgress(vocation);
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
