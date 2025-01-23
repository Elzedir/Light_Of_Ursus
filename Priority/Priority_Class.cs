using System.Collections.Generic;
using ActorActions;
using Inventory;
using Tools;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Updater : Data_Class
    {
        public ComponentReference Reference { get; }

        protected Priority_Updater (uint componentID, ComponentType componentType)
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

        Priority_Data _priorityData;
        public Priority_Data PriorityData => _priorityData ??= Reference.GetPriorityComponent();
        public abstract List<ActorActionName> GetAllowedActions();
    }
}
