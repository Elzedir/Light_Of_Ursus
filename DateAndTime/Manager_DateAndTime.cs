using System;
using System.Collections;
using System.Collections.Generic;
using Counties;
using TickRates;
using TMPro;
using UnityEngine;

namespace DateAndTime
{
    public abstract class Manager_DateAndTime
    {
        const string          c_dateAndTime_SOPath = "ScriptableObjects/DateAndTime_SO";
        
        static DateAndTime_SO        s_dateAndTime;
        public static DateAndTime_SO S_DateAndTime => s_dateAndTime ??= _getDateAndTime_SO();
        
        static TextMeshProUGUI s_dateText;
        public static TextMeshProUGUI DateText => s_dateText ??= GameObject.Find("Date").GetComponent<TextMeshProUGUI>();
        static TextMeshProUGUI s_timeText;
        public static TextMeshProUGUI TimeText => s_timeText ??= GameObject.Find("Time").GetComponent<TextMeshProUGUI>();
        static TextMeshProUGUI s_timeScaleText;
        public static TextMeshProUGUI TimeScaleText => s_timeScaleText ??= GameObject.Find("TimeScale").GetComponent<TextMeshProUGUI>();

        static float s_currentTimeScale = 1f;

        public static event Action OnProgressDay;

        public static void Initialise()
        {
            var totalMinutes = S_DateAndTime.GetTime();
            CurrentTime.Initialise(totalMinutes);
        }
        
        static DateAndTime_SO _getDateAndTime_SO()
        {
            var dateAndTime_SO = Resources.Load<DateAndTime_SO>(c_dateAndTime_SOPath);
            
            if (dateAndTime_SO is not null) return dateAndTime_SO;
            
            Debug.LogError("DateAndTime_SO not found. Creating temporary DateAndTime_SO.");
            dateAndTime_SO = ScriptableObject.CreateInstance<DateAndTime_SO>();
            
            return dateAndTime_SO;
        }
        
        public static uint GetCurrentTotalDays()
        {
            return S_DateAndTime.CurrentTotalDays;
        }
        
        static (uint day, uint month, uint year) _convertFromTotalDays(uint totalDays)
        {
            var year          = totalDays / 60;
            var remainingDays = totalDays % 60;
            var month         = (remainingDays / 15) + 1;
            var day           = (remainingDays % 15) + 1;

            return (day, month, year);
        }
        
        public static string GetCurrentDateAsString()
        {
            var (day, month, year) = _convertFromTotalDays(Manager_DateAndTime.GetCurrentTotalDays());
            return $"{day:D2}/{month:D2}/{year}";
        }

        public static void ProgressDay()
        {
            S_DateAndTime.CurrentTotalDays++;
            DateText.text = GetCurrentDateAsString();
            S_DateAndTime.SetDate();
            
            OnProgressDay?.Invoke();
        }

        public static void ProgressTime()
        {
            TimeText.text = CurrentTime.GetCurrentTimeAsString();
            S_DateAndTime.SetDate();
        }

        static void _setCurrentTimeScale(string timeScale)
        {
            TimeScaleText.text = timeScale;
        }

        public float GetTimeScale()
        {
            return s_currentTimeScale;
        }

        static void _setTimeScale(float timeScale)
        {
            if (timeScale < 0) return;
            
            s_currentTimeScale               = timeScale;
            UnityEngine.Time.timeScale      = s_currentTimeScale;
            UnityEngine.Time.fixedDeltaTime = 0.02f * UnityEngine.Time.timeScale;
            
            _setCurrentTimeScale($"Time Scale: {timeScale}x");
        }

        public static IEnumerator SetTimeScaleGradual(float targetTimeScale, float duration)
        {
            var start   = s_currentTimeScale;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                _setTimeScale(Mathf.Lerp(start, targetTimeScale, elapsed / duration));
                yield return null;
            }

            _setTimeScale(targetTimeScale);
        }
        
        const float c_maxTimeScale = 10;
        
        public static void DecreaseTimeScale()
        {
            if (s_currentTimeScale <= 0.1f) return;
            
            _setTimeScale(s_currentTimeScale - 0.1f);
        }

        public static void IncreaseTimeScale()
        {
            if (s_currentTimeScale >= c_maxTimeScale) return;
            
            _setTimeScale(s_currentTimeScale + 0.1f);
        }

        public static void ToggleTimeScale()
        {
            _setTimeScale(s_currentTimeScale == 0 ? 1f : 0f);
        }
    }

    public class EditorTime
    {
        // private static float editorDeltaTime = 0f;
        // private static float lastEditorTime  = 0f;
        // private static float timer           = 0f;
        // private static float duration        = 5f; // Timer duration in seconds
        // private static bool  isTimerRunning  = false;
        
        // [MenuItem("Tools/Start Editor Timer")]
        // public static void StartTimer()
        // {
        //     if (!isTimerRunning)
        //     {
        //         isTimerRunning           =  true;
        //         timer                    =  0f;
        //         lastEditorTime           =  (float)EditorApplication.timeSinceStartup;
        //         EditorApplication.update += UpdateTimer;
        //     }
        // }
        //
        // private static void UpdateTimer()
        // {
        //     float currentTime = (float)EditorApplication.timeSinceStartup;
        //     editorDeltaTime = currentTime - lastEditorTime;
        //     lastEditorTime  = currentTime;
        //
        //     timer += editorDeltaTime;
        //
        //     Debug.Log($"Timer: {timer:F2}, DeltaTime: {editorDeltaTime:F4}");
        //
        //     if (timer >= duration)
        //     {
        //         Debug.Log("Timer Complete!");
        //         StopTimer();
        //     }
        // }
        //
        // public static void StopTimer()
        // {
        //     isTimerRunning           =  false;
        //     EditorApplication.update -= UpdateTimer;
        // }
    }

    public enum DayName { None, Mon, Tumon, Tu, Wetu, Wed, Thured, Thur, Frith, Fri, Satri, Satu, Sunsa, Sun, Monsun }
    public enum MonthName { None, Janbruach, Aprayne, Julaugber, Octevmadec }

    public class Date
    {
        readonly uint _day;
        readonly uint _month;
        readonly uint _year;

        uint _totalDays => _year * 60 + (_month - 1) * 15 + (_day - 1);

        public Date(uint totalDays)
        {
            _year = totalDays / 60;
            var remainingDays = totalDays % 60;
            _month = (remainingDays        / 15) + 1;
            _day   = (remainingDays        % 15) + 1;
        }

        public Date(uint day, uint month, uint year)
        {
            _year = year;
            _month = month;
            _day = day;
        }

        public Date(Date date)
        {
            _year = date._year;
            _month = date._month;
            _day = date._day;
        }

        public float GetAge() => Manager_DateAndTime.GetCurrentTotalDays() - _totalDays;
    }

    public class Time
    {
        readonly uint _minute;
        readonly uint _hour;
        
        uint _totalMinutes => _hour * 60 + _minute;
        
        public Time(uint minute, uint hour)
        {
            _minute = minute;
            _hour   = hour;
        }
        
        public Time(uint totalMinutes)
        {
            _hour   = totalMinutes / 60;
            _minute = totalMinutes % 60;
        }
    }
    
    public abstract class CurrentTime
    {
        static bool s_halfTime;
        static uint s_currentMinute;
        static uint s_currentHour;
        
        public static void Initialise(uint totalMinutes)
        {
            s_currentHour   = totalMinutes / 60;
            s_currentMinute = totalMinutes % 60;
            
            Manager_TickRate.RegisterTicker(TickerTypeName.DateAndTime, TickRateName.OneSecond, 1, _onTick);
        }

        static void _onTick()
        {
            if (s_halfTime)
            {
                s_currentMinute++;

                if (s_currentMinute >= 60)
                {
                    s_currentMinute = 0;

                    s_currentHour++;

                    if (s_currentHour >= 24)
                    {
                        s_currentHour = 0;
                        Manager_DateAndTime.ProgressDay();
                    }
                }

                Manager_DateAndTime.ProgressTime();

                s_halfTime = false;
            }
            else
            {
                s_halfTime = true;
            }
        }
        
        public static string GetCurrentTimeAsString()
        {
            return $"{s_currentHour:00}:{s_currentMinute:00}";   
        }
    }
}