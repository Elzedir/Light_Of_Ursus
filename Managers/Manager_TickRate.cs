using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public enum TickRate 
    { 
        OneTenthSecond, OneSecond, TenSeconds, OneHundredSeconds,
        OneGameHour, OneGameDay, OneGameMonth, OneGameYear 
    }

    public class Manager_TickRate : MonoBehaviour
    {
        static Dictionary<TickRate, float>        _nextTickTimes;
        static Dictionary<TickRate, List<Action>> _tickableGroups;

        public void OnSceneLoaded()
        {
            _nextTickTimes = new Dictionary<TickRate, float>
            {
                { TickRate.OneTenthSecond, UnityEngine.Time.time    + 0.1f },
                { TickRate.OneSecond, UnityEngine.Time.time         + 1f },
                { TickRate.TenSeconds, UnityEngine.Time.time        + 10f },
                { TickRate.OneHundredSeconds, UnityEngine.Time.time + 100f },
                { TickRate.OneGameHour, UnityEngine.Time.time       + 120f },
                { TickRate.OneGameDay, UnityEngine.Time.time        + 2880f },
                { TickRate.OneGameMonth, UnityEngine.Time.time      + 43200f },
                { TickRate.OneGameYear, UnityEngine.Time.time       + 172800f }
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
            var currentTime = UnityEngine.Time.time;

            var keys = new List<TickRate>(_nextTickTimes.Keys);

            foreach (var tickRate in keys.Where(tickRate => currentTime >= _nextTickTimes[tickRate]))
            {
                _tick(tickRate);
                _nextTickTimes[tickRate] = currentTime + _getTickInterval(tickRate);
            }
        }

        float _getTickInterval(TickRate tickRate)
        {
            return tickRate switch
            {
                TickRate.OneTenthSecond    => 0.1f,
                TickRate.OneSecond         => 1f,
                TickRate.TenSeconds        => 10f,
                TickRate.OneHundredSeconds => 100f,
                TickRate.OneGameHour       => 120f,
                TickRate.OneGameDay        => 2880f,
                TickRate.OneGameMonth      => 43200f,
                TickRate.OneGameYear       => 172800f,
                _                          => throw new ArgumentOutOfRangeException()
            };
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

    public abstract class Manager_DeferredActions
    {
        static bool                      _initialised;
        static Dictionary<Action, float> _deferredActions;

        static void _initialise()
        {
            Manager_TickRate.RegisterTickable(_onTickStatic, TickRate.OneTenthSecond);
            _initialised = true;
        }

        static void _onTickStatic()
        {
            if (_deferredActions == null || _deferredActions.Count <= 0) return;

            _tickDeferredActions();
        }
    
        public static void AddDeferredAction(Action function, float timeDeferment)
        {
            if (!_initialised) _initialise();

            _deferredActions ??= new Dictionary<Action, float>();

            _deferredActions.Add(function, timeDeferment);
        }

        static void _executeDeferredAction(Action function)
        {
            if (_deferredActions == null) return;

            if (!_deferredActions.ContainsKey(function))
            {
                Debug.LogError($"Function: {function} does not exist in DeferredActions.");
                return;
            }

            function();
            _removeDeferredAction(function);
        }

        static void _removeDeferredAction(Action function)
        {
            if (_deferredActions == null) return;

            if (!_deferredActions.ContainsKey(function))
            {
                Debug.LogError($"Function: {function} does not exist in DeferredActions.");
                return;
            }

            _deferredActions.Remove(function);
        }

        static void _tickDeferredActions()
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

            foreach (var t in actionsToExecute)
            {
                _executeDeferredAction(t);
            }
        }
    }
}