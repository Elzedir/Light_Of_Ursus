using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PriorityData
{
    public ComponentReference Reference { get; private set; }

    public PriorityData (uint componentID, ComponentType componentType)
    {
        switch(componentType)
        {
            case ComponentType.Actor:
                Reference = new ComponentReference_Actor(componentID);
                break;
            case ComponentType.Station:
                Reference = new ComponentReference_Station(componentID);
                break;
            default:
                Debug.LogError($"ComponentType: {componentType} not found.");
                break;
        }
    }
    
    protected PriorityComponent _priorityComponent;
    public abstract PriorityComponent PriorityComponent { get; }
    
    public Action<DataChanged, Dictionary<PriorityParameter, object>> OnDataChange { get; set; }
    
    protected void _priorityChangeCheck(DataChanged dataChanged, bool forceChange = false)
    {
        if (!_priorityChangeNeeded(dataChanged) && !forceChange) return;

        if (OnDataChange == null) _setOnDataChange();

        if (OnDataChange == null) 
        {
            Debug.LogError("OnDataChange is still null after resetting data change notifications.");
            return;
        }

        OnDataChange(dataChanged, _getActionsToChange(dataChanged));
    }
    
    protected abstract bool _priorityChangeNeeded(object dataChanged);
    protected void _setOnDataChange() => OnDataChange = (DataChanged, changedParameters)
    => PriorityComponent.OnDataChanged(DataChanged, changedParameters);
    protected Dictionary<PriorityParameter, object> _getActionsToChange(DataChanged dataChanged)
    {
        if (_priorityParameterList.Count == 0)
        {
            Debug.LogError("ActionsAndParameters is empty.");
            return null;
        }

        if (!_priorityParameterList.TryGetValue(dataChanged, out var priorityParameters))
        {
            Debug.LogError($"DataChanged: {dataChanged} is not in ActionsAndParameters list");
            return null;
        }

        return priorityParameters;
    }

    protected abstract Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList { get; set; }
}
