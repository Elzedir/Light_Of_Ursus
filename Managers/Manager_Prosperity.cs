using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Prosperity
{
    
}

public class ProsperityComponent : ITickable
{
    public float Prosperity;

    public float ExpectedProsperity;

    public float MaxProsperity;

    public ProsperityComponent(float prosperity)
    {
        Prosperity = prosperity;

        Manager_TickRate.RegisterTickable(this);
    }

    public void ChangeProsperity(float prosperityChange)
    {
        Prosperity += prosperityChange;
    }

    public void SetProsperity(float prosperity)
    {
        Prosperity = prosperity;
    }

    public void OnTick()
    {
        ChangeProsperity(_getProsperityGrowth());
    }

    public float _getProsperityGrowth()
    {
        if (Prosperity > MaxProsperity) return Math.Max(MaxProsperity * 0.05f, 1);
        if (Prosperity == MaxProsperity) return 0;

        return 1;
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneGameDay;
    }
}
