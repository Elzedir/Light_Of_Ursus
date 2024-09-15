using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatingAreaComponent : MonoBehaviour
{
    public OperatingAreaData OperatingAreaData;
    public void SetOperatingAreaData(OperatingAreaData operatingAreaData) => OperatingAreaData = operatingAreaData;
    public BoxCollider OperatingArea;

    public string GetName() => name;

    public void Initialise(OperatingAreaData operatingAreaData, BoxCollider operatingArea)
    {
        OperatingAreaData = operatingAreaData;

        OperatingArea = operatingArea;

        if (!OperatingArea.isTrigger)
        {
            Debug.Log($"Set IsTrigger to true for {name}");
            OperatingArea.isTrigger = true;
        }
    }

    public float Operate(float baseProgressRate, Recipe recipe)
    {
        if (OperatingAreaData.CurrentOperatorID == 0 || OperatingAreaData.IsOperatorMovingToOperatingArea) return 0;

        if (OperatingAreaData.CurrentOperator.transform.position != null && !OperatingArea.bounds.Contains(OperatingAreaData.CurrentOperator.transform.position))
        {
            StartCoroutine(MoveOperatorToOperatingArea(Manager_Actor.GetActor(actorID: OperatingAreaData.CurrentOperatorID), transform.position));

            return 0;
        }

        return produce();

        float produce()
        {
            float productionRate = baseProgressRate;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var vocation in recipe.RequiredVocations)
            {
                productionRate *= OperatingAreaData.CurrentOperator.ActorData.VocationData.GetProgress(vocation);
            }

            return productionRate;
        }
    }

    public bool CanHaul()
    {
        if (OperatingAreaData.CurrentOperatorID == 0 ) return false;

        // Other things that could prevent the operator from hauling like full inventory weight, or other conditions.
        // Not if HasOrder() since I want to be able to queue orders.

        return true;
    }

    protected IEnumerator MoveOperatorToOperatingArea(Actor_Base actor, Vector3 position)
    {
        if (OperatingAreaData.IsOperatorMovingToOperatingArea) yield break;

        OperatingAreaData.IsOperatorMovingToOperatingArea = true;

        yield return actor.StartCoroutine(actor.BasicMove(position));

        if (actor.ActorData.GameObjectProperties.ActorTransform.position != position)
        {
            actor.ActorData.GameObjectProperties.ActorTransform.position = position;
        }

        OperatingAreaData.IsOperatorMovingToOperatingArea = false;
    }
}
