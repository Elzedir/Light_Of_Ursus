using System;
using System.Collections.Generic;
using UnityEngine;

public enum VocationType
{
    Farmer,
    LumberJack,
    Miner
}

public class Manager_Vocation
{
    public static List<Vocation> AllVocationsList = new();

    private void InitialiseVocations()
    {
        // Add vocations
    }
}

public class Vocation
{
    public string Name {  get; private set; }
    public VocationType VocationType { get; private set; }
    public string Description { get; private set; }
}