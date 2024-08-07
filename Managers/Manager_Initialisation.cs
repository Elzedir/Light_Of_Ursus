using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseAllActorSO;
    public static event Action OnInitialiseActors;
    public static event Action OnInitialiseCities;
    public static event Action OnInitialiseJobsites;

    public static void InitialiseAllActorSO()
    {
        OnInitialiseAllActorSO?.Invoke();
    }

    public static void InitialiseActors()
    {
        OnInitialiseActors?.Invoke();
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
