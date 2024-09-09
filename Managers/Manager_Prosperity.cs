using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Prosperity
{
    
}

[Serializable]
public class ProsperityData : ITickable
{
    public GameObject GameObject;
    public float CurrentProsperity;
    public float MaxProsperity;
    public float BaseProsperityGrowthPerDay;

    public ProsperityData(GameObject gameObject)
    {
        CurrentProsperity = 50;
        MaxProsperity = 100;
        BaseProsperityGrowthPerDay = 10;

        GameObject = gameObject;
        // Eventually initialise the data based on region, city, jobsite, etc.
        Manager_TickRate.RegisterTickable(this);
    }

    public void ChangeProsperity(float prosperityChange)
    {
        CurrentProsperity += Math.Min(prosperityChange, MaxProsperity - CurrentProsperity);
    }

    public void SetProsperity(float prosperity)
    {
        CurrentProsperity = prosperity >= 0 ? prosperity : 0;
    }

    public float GetProsperityPercentage()
    {
        // Eventually prosperity percent will take into account all levels, region, city, jobsite, etc.
        return Math.Min((CurrentProsperity / Math.Max(MaxProsperity, 1)) + 0.1f, 1);
    }

    public void OnTick()
    {
        ChangeProsperity(_getProsperityGrowth());
    }

    public float _getProsperityGrowth()
    {
        if (CurrentProsperity > MaxProsperity) return Math.Max(MaxProsperity * 0.05f, 1);
        if (CurrentProsperity == MaxProsperity) return 0;

        return BaseProsperityGrowthPerDay; // Add modifiers afterwards.
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneGameDay;
    }
}
