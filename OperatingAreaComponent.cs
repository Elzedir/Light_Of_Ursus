using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class OperatingAreaComponent : MonoBehaviour
{
    public int StationID;
    public OperatorData CurrentOperator;
    public BoxCollider OperatingArea;

    public bool IsOperatorMovingToOperatingArea = false;

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
        StationID = stationID;
    }

    public void SetOperator(OperatorData currentOperator)
    {
        CurrentOperator = currentOperator;
        CurrentOperator.SetOperatingArea(this);
    }

    public void RemoveOperator()
    {
        CurrentOperator.RemoveOperatingArea();
        CurrentOperator = null;
        IsOperatorMovingToOperatingArea = false;
    }

    public float Operate(float baseProgressRate, Recipe recipe)
    {
        if (CurrentOperator == null)
        {
            Debug.Log("No operator assigned.");
            return 0;
        }

        if (IsOperatorMovingToOperatingArea) return 0;

        if (!OperatingArea.bounds.Contains(CurrentOperator.GetOperatorPosition()))
        {
            StartCoroutine(MoveOperatorToOperatingArea(
                Manager_Actor.GetActor(actorID: CurrentOperator.ActorData.ActorID, out Actor_Base actor, factionID: CurrentOperator.ActorData.ActorFactionID), transform.position));
        }

        if (CurrentOperator.GetOperatorPosition() != transform.position && CurrentOperator.ActorTransform != null)
        {
            CurrentOperator.ActorTransform.position = transform.position;
        }

        Debug.Log($"Operating {name}");

        float productionRate = baseProgressRate;
        // Then modify production rate by any area modifiers (Land type, events, etc.)

        foreach (var vocation in recipe.RequiredVocations)
        {
            productionRate *= CurrentOperator.ActorData.VocationData.GetProgress(vocation);
        }

        return productionRate;
    }


    protected IEnumerator MoveOperatorToOperatingArea(Actor_Base actor, Vector3 position)
    {
        IsOperatorMovingToOperatingArea = true;

        yield return actor.StartCoroutine(actor.BasicMove(position));

        IsOperatorMovingToOperatingArea = false;
    }
}
