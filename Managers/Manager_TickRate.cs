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
    static Dictionary<TickRate, float> _nextTickTimes;
    static Dictionary<TickRate, List<Action>> _tickableGroups;

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

        _tickableGroups = new Dictionary<TickRate, List<Action>>
            {
                { TickRate.OneTenthSecond, new List<Action>() },
                { TickRate.OneSecond, new List<Action>() },
                { TickRate.TenSeconds, new List<Action>() },
                { TickRate.OneHundredSeconds, new List<Action>() },
                { TickRate.OneGameHour, new List<Action>() },
                { TickRate.OneGameDay, new List<Action>() },
                { TickRate.OneGameMonth, new List<Action>() },
                { TickRate.OneGameYear, new List<Action>() }
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

    public static void RegisterTickable(Action tickable, TickRate tickRate)
    {
        if (_tickableGroups.ContainsKey(tickRate) && !_tickableGroups[tickRate].Contains(tickable))
        {
            _tickableGroups[tickRate].Add(tickable);
        }
    }

    public static void UnregisterTickable(Action tickable, TickRate tickRate)
    {
        if (_tickableGroups.ContainsKey(tickRate) && _tickableGroups[tickRate].Contains(tickable))
        {
            _tickableGroups[tickRate].Remove(tickable);
        }
    }

    void _tick(TickRate tickRate)
    {
        foreach (var tickable in _tickableGroups[tickRate])
        {
            tickable();
        }
    }
}

public class Manager_DeferredActions
{
    static bool _initialised;
    static Dictionary<Action, float> _deferredActions;

    static void _initialise()
    {
        Manager_TickRate.RegisterTickable(OnTickStatic, TickRate.OneTenthSecond);
        _initialised = true;
    }

    public static void OnTickStatic()
    {
        if (_deferredActions == null || _deferredActions.Count <= 0) return;

        TickDeferredActions();
    }
    
    public static void AddDeferredAction(Action function, float timeDeferment)
    {
        if (!_initialised) _initialise();

        if (_deferredActions == null) _deferredActions = new Dictionary<Action, float>();

        _deferredActions.Add(function, timeDeferment);
    }

    public static void ExecuteDeferredAction(Action function)
    {
        if (_deferredActions == null) return;

        if (!_deferredActions.ContainsKey(function))
        {
            Debug.LogError($"Function: {function} does not exist in DeferredActions.");
            return;
        }

        function();
        RemoveDeferredAction(function);
    }

    public static void RemoveDeferredAction(Action function)
    {
        if (_deferredActions == null) return;

        if (!_deferredActions.ContainsKey(function))
        {
            Debug.LogError($"Function: {function} does not exist in DeferredActions.");
            return;
        }

        _deferredActions.Remove(function);
    }

    public static void TickDeferredActions()
    {
        var actionsToExecute = new List<Action>();

        foreach (var deferredAction in _deferredActions)
        {
            if (deferredAction.Value <= 0)
            {
                actionsToExecute.Add(deferredAction.Key);
            }
            else
            {
                _deferredActions[deferredAction.Key] -= UnityEngine.Time.deltaTime;
            }
        }

        for (int i = 0; i < actionsToExecute.Count; i++)
        {
            ExecuteDeferredAction(actionsToExecute[i]);
        }
    }
}