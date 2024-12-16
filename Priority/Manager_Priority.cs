using System;
using System.Collections.Generic;
using Actor;
using City;
using JobSite;
using Managers;
using Region;
using Station;
using UnityEngine;

namespace Priority
{
    public class Manager_Priority : MonoBehaviour
    {
    
    }
    
    public enum PriorityParameterName
    {
        None,

        // At some point, figure out how we want to apply maxPriority, maybe per parameter? Like every TotalItems, TotalDistance, etc. has an attached maxPriority.
        ActionOrTask,
        DefaultPriority,
        TotalItems,
        TotalDistance,
        InventoryHauler,
        InventoryTarget,
        CurrentStationType,
        AllStationTypes,
        Jobsite,
    }

    public enum PriorityImportance
    {
        None,

        Critical,
        High,
        Medium,
        Low,
    }
}