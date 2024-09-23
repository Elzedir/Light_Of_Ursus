using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseManagerFaction;

    public static event Action OnInitialiseManagerRegion;
    public static event Action OnInitialiseManagerCity;
    public static event Action OnInitialiseManagerJobsite;
    public static event Action OnInitialiseManagerStation;
    public static event Action OnInitialiseManagerOperatingArea;
    public static event Action OnInitialiseManagerOrder;

    public static event Action OnInitialiseManagerActor;
    public static event Action OnInitialiseActorData;
    public static event Action OnInitialiseActors;

    public static event Action OnInitialiseJobsiteDatas;
    public static event Action OnInitialiseStationDatas;

    public static void InitialiseFactions()
    {
        OnInitialiseManagerFaction?.Invoke();
    }

    public static void InitialiseRegions() 
    {
        OnInitialiseManagerRegion?.Invoke();
        OnInitialiseManagerCity?.Invoke();
        OnInitialiseManagerJobsite?.Invoke();
        OnInitialiseManagerStation?.Invoke();
        OnInitialiseManagerOperatingArea?.Invoke();
        OnInitialiseManagerOrder?.Invoke();
    }

    public static void InitialiseActors()
    {
        OnInitialiseManagerActor?.Invoke();
        OnInitialiseActorData?.Invoke();
        OnInitialiseActors?.Invoke();
    }

    public static void InitialiseJobsites()
    {
        OnInitialiseJobsiteDatas?.Invoke();
        OnInitialiseStationDatas?.Invoke();
    }
}
