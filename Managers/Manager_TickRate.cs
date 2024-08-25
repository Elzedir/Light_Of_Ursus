using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TickRate 
{ 
    OneTenthSecond, OneSecond, TenSeconds, OneHundredSeconds,
    OneGameHour, OneGameDay, OneGameMonth, OneGameYear 
}

public class Manager_TickRate : MonoBehaviour
{
    public static Manager_TickRate Instance { get; private set; }

    static Dictionary<TickRate, float> _nextTickTimes;
    static Dictionary<TickRate, List<ITickable>> _tickableGroups;

    void Awake()
    {
        Instance = this;
    }

    public void OnSceneLoaded()
    {
        _nextTickTimes = new Dictionary<TickRate, float>
            {
                { TickRate.OneTenthSecond, UnityEngine.Time.time + 0.1f },
                { TickRate.OneSecond, UnityEngine.Time.time + 1f },
                { TickRate.TenSeconds, UnityEngine.Time.time + 10f },
                { TickRate.OneHundredSeconds, UnityEngine.Time.time + 100f },
                { TickRate.OneGameHour, UnityEngine.Time.time + 120f },
                { TickRate.OneGameDay, UnityEngine.Time.time + 2880f },
                { TickRate.OneGameMonth, UnityEngine.Time.time + 43200f },
                { TickRate.OneGameYear, UnityEngine.Time.time + 172800f }
            };

        _tickableGroups = new Dictionary<TickRate, List<ITickable>>
            {
                { TickRate.OneTenthSecond, new List<ITickable>() },
                { TickRate.OneSecond, new List<ITickable>() },
                { TickRate.TenSeconds, new List<ITickable>() },
                { TickRate.OneHundredSeconds, new List<ITickable>() },
                { TickRate.OneGameHour, new List<ITickable>() },
                { TickRate.OneGameDay, new List<ITickable>() },
                { TickRate.OneGameMonth, new List<ITickable>() },
                { TickRate.OneGameYear, new List<ITickable>() }
            };
    }

    void Update()
    {
        float currentTime = UnityEngine.Time.time;

        var keys = new List<TickRate>(_nextTickTimes.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            TickRate tickRate = keys[i];

            if (currentTime >= _nextTickTimes[tickRate])
            {
                _tick(tickRate);
                _nextTickTimes[tickRate] = currentTime + GetTickInterval(tickRate);
            }
        }
    }

    float GetTickInterval(TickRate tickRate)
    {
        switch (tickRate)
        {
            case TickRate.OneTenthSecond:
                return 0.1f;
            case TickRate.OneSecond:
                return 1f;
            case TickRate.TenSeconds:
                return 10f;
            case TickRate.OneHundredSeconds:
                return 100f;
            case TickRate.OneGameHour:
                return 120f;
            case TickRate.OneGameDay:
                return 2880f;
            case TickRate.OneGameMonth:
                return 43200f;
            case TickRate.OneGameYear:
                return 172800f;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void RegisterTickable(ITickable tickable)
    {
        TickRate tickRate = tickable.GetTickRate();

        if (_tickableGroups.ContainsKey(tickRate) && !_tickableGroups[tickRate].Contains(tickable))
        {
            _tickableGroups[tickRate].Add(tickable);
        }
    }

    public static void UnregisterTickable(ITickable tickable)
    {
        TickRate tickRate = tickable.GetTickRate();

        if (_tickableGroups.ContainsKey(tickRate) && _tickableGroups[tickRate].Contains(tickable))
        {
            _tickableGroups[tickRate].Remove(tickable);
        }
    }

    void _tick(TickRate tickRate)
    {
        foreach (var tickable in _tickableGroups[tickRate])
        {
            tickable.OnTick();
        }
    }
}

public interface ITickable
{
    void OnTick();
    TickRate GetTickRate();
}
