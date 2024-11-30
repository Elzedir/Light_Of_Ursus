using System;
using System.Collections.Generic;
using Actor;
using Inventory;
using Managers;
using UnityEngine;

namespace Priority
{
    public abstract class PriorityData
    {
        public ComponentReference Reference { get; }

        protected PriorityData (uint componentID, ComponentType componentType)
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

        PriorityComponent _priorityComponent;
        public PriorityComponent PriorityComponent => _priorityComponent ??= Reference.GetPriorityComponent();

        Action<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _onDataChange { get; set; }
    
        protected void _priorityChangeCheck(PriorityUpdateTrigger priorityUpdateTrigger, bool forceChange = false)
        {
            if (!_priorityChangeNeeded(priorityUpdateTrigger) && !forceChange) return;

            if (_onDataChange == null) _setOnDataChange();

            if (_onDataChange == null) 
            {
                Debug.LogError("OnDataChange is still null after resetting data change notifications.");
                return;
            }
            
            var newPriorityParameters = _getNewPriorityParameters(priorityUpdateTrigger);
            
            if (newPriorityParameters == null) return;

            _onDataChange(priorityUpdateTrigger, newPriorityParameters);
        }
    
        protected abstract bool _priorityChangeNeeded(object dataChanged);

        void _setOnDataChange() => _onDataChange = (dataChanged, changedParameters)
            => PriorityComponent.CriticalDataChanged(dataChanged, changedParameters);

        Dictionary<PriorityParameterName, object> _getNewPriorityParameters(PriorityUpdateTrigger priorityUpdateTrigger)
        {
            if (_priorityParameterList.Count == 0)
            {
                Debug.LogError("ActionsAndParameters is empty.");
                return null;
            }

            if (_priorityParameterList.TryGetValue(priorityUpdateTrigger, out var priorityParameters)) return priorityParameters;
            
            Debug.LogError($"DataChanged: {priorityUpdateTrigger} is not in ActionsAndParameters list");
            return null;

        }

        protected abstract Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList { get; set; }
    }
}
