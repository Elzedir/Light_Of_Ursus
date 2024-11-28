using System;
using System.Collections.Generic;
using Actors;
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

        Action<DataChanged, Dictionary<PriorityParameterName, object>> _onDataChange { get; set; }
    
        protected void _priorityChangeCheck(DataChanged dataChanged, bool forceChange = false)
        {
            if (!_priorityChangeNeeded(dataChanged) && !forceChange) return;

            if (_onDataChange == null) _setOnDataChange();

            if (_onDataChange == null) 
            {
                Debug.LogError("OnDataChange is still null after resetting data change notifications.");
                return;
            }
            
            var newPriorityParameters = _getNewPriorityParameters(dataChanged);
            
            if (newPriorityParameters == null) return;

            _onDataChange(dataChanged, newPriorityParameters);
        }
    
        protected abstract bool _priorityChangeNeeded(object dataChanged);

        void _setOnDataChange() => _onDataChange = (dataChanged, changedParameters)
            => PriorityComponent.OnDataChanged(dataChanged, changedParameters);

        Dictionary<PriorityParameterName, object> _getNewPriorityParameters(DataChanged dataChanged)
        {
            if (_priorityParameterList.Count == 0)
            {
                Debug.LogError("ActionsAndParameters is empty.");
                return null;
            }

            if (_priorityParameterList.TryGetValue(dataChanged, out var priorityParameters)) return priorityParameters;
            
            Debug.LogError($"DataChanged: {dataChanged} is not in ActionsAndParameters list");
            return null;

        }

        protected abstract Dictionary<DataChanged, Dictionary<PriorityParameterName, object>> _priorityParameterList { get; set; }
    }
}
