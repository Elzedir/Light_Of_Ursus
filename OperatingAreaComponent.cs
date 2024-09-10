using System;
using System.Collections;
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

        var actorData = Manager_Actor.GetActorData(OperatingAreaData.CurrentOperatorID);

        var actorTransform = actorData.GameObjectProperties.ActorTransform;

        if (actorTransform.position != null && !OperatingArea.bounds.Contains(actorTransform.position))
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
                productionRate *= actorData.VocationData.GetProgress(vocation);
            }

            return productionRate;
        }
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
