using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TickRate { OneTenth, One, Ten }

public class Manager_TickRate : MonoBehaviour
{
    public static Manager_TickRate Instance { get; private set; }

    Dictionary<TickRate, float> _nextTickTimes;
    Dictionary<TickRate, List<ITickable>> _tickableGroups;

    void Awake()
    {
        Instance = this;
    }

    public void OnSceneLoaded()
    {
        _nextTickTimes = new Dictionary<TickRate, float>
            {
                { TickRate.OneTenth, UnityEngine.Time.time + 0.1f },
                { TickRate.One, UnityEngine.Time.time + 1f },
                { TickRate.Ten, UnityEngine.Time.time + 10f }
            };

        _tickableGroups = new Dictionary<TickRate, List<ITickable>>
            {
                { TickRate.OneTenth, new List<ITickable>() },
                { TickRate.One, new List<ITickable>() },
                { TickRate.Ten, new List<ITickable>() }
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
            case TickRate.OneTenth:
                return 0.1f;
            case TickRate.One:
                return 1f;
            case TickRate.Ten:
                return 10f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RegisterTickable(ITickable tickable)
    {
        TickRate tickRate = tickable.GetTickRate();

        if (_tickableGroups.ContainsKey(tickRate) && !_tickableGroups[tickRate].Contains(tickable))
        {
            _tickableGroups[tickRate].Add(tickable);
        }
    }

    public void UnregisterTickable(ITickable tickable)
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
