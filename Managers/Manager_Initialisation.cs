using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseAllRegionSO;
    public static event Action OnInitialiseManagerRegion;
    public static event Action OnInitialiseAllActorSO;
    public static event Action OnInitialiseActors;
    public static event Action OnInitialiseManagerActor;
    public static event Action OnInitialiseCities;
    public static event Action OnInitialiseJobsites;

    public static void InitialiseAllRegionSO() 
    {
        OnInitialiseAllRegionSO?.Invoke();
        OnInitialiseManagerRegion?.Invoke();
    }

    public static void InitialiseActors()
    {
        OnInitialiseAllActorSO?.Invoke();
        OnInitialiseActors?.Invoke();
        OnInitialiseManagerActor.Invoke();
    }

    public static void InitialiseCities()
    {
        OnInitialiseCities?.Invoke();
    }

    public static void InitialiseJobsites()
    {
        OnInitialiseJobsites?.Invoke();
    }
}
