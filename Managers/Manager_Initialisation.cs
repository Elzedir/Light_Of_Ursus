using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseManagerRegion;
    public static event Action OnInitialiseManagerCity;
    public static event Action OnInitialiseManagerJobsite;
    public static event Action OnInitialiseManagerStation;
    public static event Action OnInitialiseAllRegionSO;
    public static event Action OnInitialiseManagerActor;
    public static event Action OnInitialiseAllActorSO;
    public static event Action OnInitialiseActors;
    public static event Action OnInitialiseJobsites;

    public static void InitialiseAllRegionSO() 
    {
        OnInitialiseManagerRegion?.Invoke();
        OnInitialiseManagerCity?.Invoke();
        OnInitialiseManagerJobsite?.Invoke();
        OnInitialiseManagerStation?.Invoke();
        OnInitialiseAllRegionSO?.Invoke();
    }

    public static void InitialiseActors()
    {
        OnInitialiseManagerActor.Invoke();
        OnInitialiseAllActorSO?.Invoke();
        OnInitialiseActors?.Invoke();
    }

    public static void InitialiseJobs()
    {
        OnInitialiseJobsites?.Invoke();
    }
}
