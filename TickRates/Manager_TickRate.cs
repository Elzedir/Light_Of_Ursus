using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TickRates
{
    public enum TickRateName 
    { 
        OneTenthSecond, OneSecond, TenSeconds, OneHundredSeconds,
        OneGameHour, OneGameDay, OneGameMonth, OneGameYear 
    }

    public enum TickerTypeName
    {
        DateAndTime,
        DeferredTicker,
        Manager,
        Actor,
        Actor_StateAndCondition,
        Station,
        Jobsite,
    }

    public class Manager_TickRate : MonoBehaviour
    {
        //Maybe save next tick times since last save and load them on scene load.
        
        static          Dictionary<TickRateName, float>                                            _nextTickTimes = new();
        static readonly Dictionary<TickerTypeName, Dictionary<TickRateName, Dictionary<uint, Action>>> _allTickerTypes    = new();
        
        //TickableSpreader _tickableSpreader;

        public void OnSceneLoaded()
        {
            _nextTickTimes = new Dictionary<TickRateName, float>
            {
                { TickRateName.OneTenthSecond, Time.time    + 0.1f },
                { TickRateName.OneSecond, Time.time         + 1f },
                { TickRateName.TenSeconds, Time.time        + 10f },
                { TickRateName.OneHundredSeconds, Time.time + 100f },
                { TickRateName.OneGameHour, Time.time       + 120f },
                { TickRateName.OneGameDay, Time.time        + 2880f },
                { TickRateName.OneGameMonth, Time.time      + 43200f },
                { TickRateName.OneGameYear, Time.time       + 172800f }
            };
            
            //_tickableSpreader = new TickableSpreader(new Dictionary<float, SpreadTickables>());
        }

        void Update()
        {
            var currentTime = Time.time;

            var keys = new List<TickRateName>(_nextTickTimes.Keys);

            foreach (var tickRate in keys.Where(tickRate => currentTime >= _nextTickTimes[tickRate]))
            {
                _tick(tickRate);
                _nextTickTimes[tickRate] = currentTime + _getTickInterval(tickRate);
            }
            
            //_tickableSpreader.Update();
        }

        static float _getTickInterval(TickRateName tickRateName)
        {
            return tickRateName switch
            {
                TickRateName.OneTenthSecond    => 0.1f,
                TickRateName.OneSecond         => 1f,
                TickRateName.TenSeconds        => 10f,
                TickRateName.OneHundredSeconds => 100f,
                TickRateName.OneGameHour       => 120f,
                TickRateName.OneGameDay        => 2880f,
                TickRateName.OneGameMonth      => 43200f,
                TickRateName.OneGameYear       => 172800f,
                _                          => throw new ArgumentOutOfRangeException()
            };
        }

        public static void RegisterTicker(TickerTypeName tickerTypeName, TickRateName tickRateName, uint tickerID, Action tickerAction, bool unregisterExisting = false)
        {
            if (unregisterExisting) UnregisterTicker(tickerTypeName, tickRateName, tickerID);
            
            var tickerGroup = _tickerCheck(tickerTypeName, tickRateName);
            
            if (!tickerGroup.TryGetValue(tickerID, out _))
            {
                tickerGroup.Add(tickerID, tickerAction);
                return;
            }
            
            Debug.LogWarning($"TickerID: {tickerID} already exists in TickerGroups, so replacing ticker action.");
            tickerGroup[tickerID] = tickerAction;
        }

        public static void UnregisterTicker(TickerTypeName tickerTypeName, TickRateName tickRateName, uint tickerID)
        {
            var tickerGroup = _tickerCheck(tickerTypeName, tickRateName);

            if (!tickerGroup.ContainsKey(tickerID))
            {
                Debug.LogError($"TickerID: {tickerID} does not exist in TickerGroups.");
                return;
            }

            tickerGroup.Remove(tickerID);
        }

        static Dictionary<uint, Action> _tickerCheck(TickerTypeName tickerTypeName, TickRateName tickRateName)
        {
            if (!_allTickerTypes.TryGetValue(tickerTypeName, out var tickerRate))
            {
                tickerRate = new Dictionary<TickRateName, Dictionary<uint, Action>>();
                _allTickerTypes.Add(tickerTypeName, tickerRate);
            }
            
            if (!tickerRate.TryGetValue(tickRateName, out var tickerGroup))
            {
                tickerGroup = new Dictionary<uint, Action>();
                tickerRate.Add(tickRateName, tickerGroup);
            }
            
            return tickerGroup;
        }

        static void _tick(TickRateName tickRateName)
        {
            foreach (var tickerTypeName in _allTickerTypes.Keys)
            {
                if (!_allTickerTypes.TryGetValue(tickerTypeName, out var tickerRate))
                {
                    Debug.LogError($"TickerType: {tickerTypeName} does not exist in TickerGroups.");
                    continue;
                }

                if (!tickerRate.TryGetValue(tickRateName, out var tickerGroup))
                {
                    //Debug.LogError($"TickRate: {tickRate} does not exist in TickerGroups.");
                    continue;
                }

                foreach (var tickerAction in tickerGroup.Values)
                {
                    tickerAction();
                }
            }
        }
    }

    public class TickerType
    {
        public          TickerTypeName                     TickerTypeName;
        public readonly Dictionary<TickRateName, TickRate> TickerGroups;
        
        public TickerType(TickerTypeName tickerTypeName)
        {
            TickerTypeName = tickerTypeName;
            TickerGroups     = new Dictionary<TickRateName, TickRate>();
        }
    }

    public class TickRate
    {
        public TickRateName TickRateName;
        public Dictionary<TickerType, TickGroup> Tickers;
        
        public TickRate(TickRateName tickRateName)
        {
            TickRateName = tickRateName;
            Tickers     = new Dictionary<TickerType, TickGroup>();
        }
    }

    public class TickGroup
    {
        public TickRateName TickerType;
        public Dictionary<uint, Action> Tickers;
        
        public TickGroup(TickRateName tickRateName)
        {
            TickerType = tickRateName;
            Tickers   = new Dictionary<uint, Action>();
        }
    }

    public abstract class Manager_DeferredActions
    {
        static bool                      _initialised;
        static Dictionary<Action, float> _deferredActions;

        static void _initialise()
        {
            Manager_TickRate.RegisterTicker(TickerTypeName.DeferredTicker, TickRateName.OneTenthSecond, 1, _onTickStatic);
            _initialised = true;
        }

        static void _onTickStatic()
        {
            if (_deferredActions is not { Count: > 0 }) return;

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
                    _deferredActions[deferredAction.Key] -= Time.deltaTime;
                }
            }

            foreach (var t in actionsToExecute)
            {
                _executeDeferredAction(t);
            }
        }
    }
}

public abstract class SpreadTickers
{
    public readonly Queue<Action> Tickers;
    public readonly int           MaxExecutionsPerTick;
    public          float         NextTickTime;
    
    public SpreadTickers(float tickInterval, int maxExecutionsPerTick, Queue<Action> tickers)
    {
        Tickers            = tickers;
        MaxExecutionsPerTick = Mathf.Max(maxExecutionsPerTick, 10000);
        NextTickTime         = Time.time + tickInterval;
    }
}

public class TickableSpreader
{
    readonly Dictionary<float, SpreadTickers> _spreadTickables;
    
    public TickableSpreader(Dictionary<float, SpreadTickers> spreadTickables)
    {
        _spreadTickables = spreadTickables;
    }

    public void RegisterTickable(float tickRate, SpreadTickers ticker)
    {
        if (!_spreadTickables.TryAdd(tickRate, ticker))
        {
            Debug.LogError($"TickRate: {tickRate} already exists in SpreadTickables.");
        }
    }

    public void UnregisterTickable(float tickRate, SpreadTickers ticker)
    {
        if (!_spreadTickables.Remove(tickRate, out _))
        {
            Debug.LogError($"TickRate: {tickRate} does not exist in SpreadTickables.");
        }
    }

    public void Update()
    {
        foreach (var spreadTickable in _spreadTickables
                     .Where(spreadTickable =>
                         !(Time.time < spreadTickable.Value.NextTickTime)))
        {
            _executeTickables(spreadTickable.Value);
            spreadTickable.Value.NextTickTime =
                Time.time + (spreadTickable.Key / spreadTickable.Value.MaxExecutionsPerTick);
        }
    }

    static void _executeTickables(SpreadTickers spreadTickers)
    {
        var tickableQueue = spreadTickers.Tickers;
        var maxExecutions = spreadTickers.MaxExecutionsPerTick;

        for (var i = 0; i < maxExecutions; i++)
        {
            if (tickableQueue.Count <= 0) break;

            Debug.Log("Dequeued and invoked");
            
            tickableQueue.Dequeue().Invoke();
        }
    }
}