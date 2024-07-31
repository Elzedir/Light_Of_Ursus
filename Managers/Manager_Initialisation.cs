using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action<Actor_Data_SO> OnInitialiseActors;
    public static event Action OnInitialiseCities;
    public static event Action OnInitialiseJobsites;

    public static void InitialiseActors()
    {
        OnInitialiseActors?.Invoke(null);
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
