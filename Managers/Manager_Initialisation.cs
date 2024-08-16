using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseManagerFaction;
    public static event Action OnInitialiseAllFactionSO;

    public static event Action OnInitialiseManagerRegion;
    public static event Action OnInitialiseManagerCity;
    public static event Action OnInitialiseManagerJobsite;
    public static event Action OnInitialiseManagerStation;
    public static event Action OnInitialiseAllRegionSO;

    public static event Action OnInitialiseManagerActor;
    public static event Action OnInitialiseActorData;
    public static event Action OnInitialiseActors;

    public static event Action OnInitialiseJobsites;

    public static void InitialiseFactions()
    {
        OnInitialiseManagerFaction?.Invoke();
        OnInitialiseAllFactionSO?.Invoke();
    }

    public static void InitialiseRegions() 
    {
        OnInitialiseManagerRegion?.Invoke();
        OnInitialiseManagerCity?.Invoke();
        OnInitialiseManagerJobsite?.Invoke();
        OnInitialiseManagerStation?.Invoke();
        OnInitialiseAllRegionSO?.Invoke();
    }

    public static void InitialiseActors()
    {
        OnInitialiseManagerActor?.Invoke();
        OnInitialiseActorData?.Invoke();
        OnInitialiseActors?.Invoke();
    }

    public static void InitialiseJobsites()
    {
        OnInitialiseJobsites?.Invoke();
    }
}
