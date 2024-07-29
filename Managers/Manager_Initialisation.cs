using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Initialisation
{
    public static event Action OnInitialiseActors;

    public static void InitialiseActors()
    {
        OnInitialiseActors?.Invoke();
    }
}
