using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class Manager_DateAndTime : MonoBehaviour
    {
        static TextMeshProUGUI _dateText;
        public static TextMeshProUGUI DateText => _dateText ??= GameObject.Find("Date").GetComponent<TextMeshProUGUI>();
        static TextMeshProUGUI _timeText;
        public static TextMeshProUGUI TimeText => _timeText ??= GameObject.Find("Time").GetComponent<TextMeshProUGUI>();
        static TextMeshProUGUI _timeScaleText;
        public static TextMeshProUGUI TimeScaleText => _timeScaleText ??= GameObject.Find("TimeScale").GetComponent<TextMeshProUGUI>();

        static float _currentTimeScale = 1f;

        public static void Initialise()
        {
            CurrentDate.Initialise();
            Time.Initialise();
        }

        public static void SetCurrentDate(string date)
        {
            DateText.text = date;
        }

        public static void SetCurrentTime(string time)
        {
            TimeText.text = time;
        }
        
        public static void SetCurrentTimeScale(string timeScale)
        {
            TimeScaleText.text = timeScale;
        }

        public float GetTimeScale()
        {
            return _currentTimeScale;
        }

        static void _setTimeScale(float timeScale)
        {
            if (timeScale < 0) return;
            
            _currentTimeScale               = timeScale;
            UnityEngine.Time.timeScale      = _currentTimeScale;
            UnityEngine.Time.fixedDeltaTime = 0.02f * UnityEngine.Time.timeScale;
            
            SetCurrentTimeScale($"Time Scale: {timeScale}x");
        }

        public static IEnumerator SetTimeScaleGradual(float targetTimeScale, float duration)
        {
            var start   = _currentTimeScale;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                _setTimeScale(Mathf.Lerp(start, targetTimeScale, elapsed / duration));
                yield return null;
            }

            _setTimeScale(targetTimeScale);
        }
        
        const float _maxTimeScale = 10;
        
        public static void DecreaseTimeScale()
        {
            if (_currentTimeScale <= 0.1f) return;
            
            _setTimeScale(_currentTimeScale - 0.1f);
        }

        public static void IncreaseTimeScale()
        {
            if (_currentTimeScale >= _maxTimeScale) return;
            
            _setTimeScale(_currentTimeScale + 0.1f);
        }

        public static void ToggleTimeScale()
        {
            _setTimeScale(_currentTimeScale == 0 ? 1f : 0f);
        }
    }

    public enum DayName { Mon, Tumon, Tu, Wetu, Wed, Thured, Thur, Frith, Fri, Satri, Satu, Sunsa, Sun, Monsun }
    public enum MonthName { Janbruach, Aprayne, Julaugber, Octevmadec }

    public class Date
    {
        readonly int _totalDays;

        public Date(int  totalDays) => _totalDays = totalDays;
        public Date (int day, int month, int year) => _totalDays = ConvertToTotalDays(day, month, year);

        public float GetAge() => CurrentDate.CurrentTotalDays - _totalDays;

        public static (int day, int month, int year) ConvertFromTotalDays(int totalDays)
        {
            var year          = totalDays     / 60;
            var remainingDays = totalDays     % 60;
            var month         = remainingDays / 14;
            var day           = remainingDays % 14;

            return (day + 1, month + 1, year);
        }

        public static int ConvertToTotalDays(int day, int month, int year)
        {
            return (year * 60) + ((month - 1) * 14) + (day - 1);
        }
    }

    public abstract class CurrentDate
    {
        public static int     CurrentTotalDays { get; private set; }
        public static DayName Day;
        public static Action  NewDay;

        public static void Initialise(int totalDays = 6000)
        {
            CurrentTotalDays = totalDays;
        }

        public static void ProgressCurrentDate(int days = 1)
        {
            CurrentTotalDays += days;

            Manager_DateAndTime.SetCurrentDate(GetCurrentDateAsString());

            if (days != 0) NewDay?.Invoke();
        }

        public static string GetCurrentDateAsString()
        {
            var (day, month, year) = Date.ConvertFromTotalDays(CurrentTotalDays);
            return $"{day:D2}/{month:D2}/{year}";
        }

        public static (int day, int month, int year) GetCurrentDateAsInt()
        {
            return Date.ConvertFromTotalDays(CurrentTotalDays);
        }
    }

    public class Time
    {
        public int Minute;
        public int Hour;

        public Time(int minute, int hour)
        {
            Minute = minute;
            Hour   = hour;
        }

        public static void Initialise(int minute = 59, int hour = 7)
        {
            CurrentTime.CurrentMinute = minute;
            CurrentTime.CurrentHour   = hour;
            Manager_TickRate.RegisterTickable(new CurrentTime().OnTick, TickRate.OneSecond);
        }
    }

    public class CurrentTime
    {
        bool              _halfTime;
        public static int CurrentMinute;
        public static int CurrentHour;

        public void OnTick()
        {
            if (_halfTime)
            {
                CurrentMinute++;

                if (CurrentMinute >= 60)
                {
                    CurrentMinute = 0;

                    CurrentHour++;

                    if (CurrentHour >= 24)
                    {
                        CurrentHour = 0;
                        CurrentDate.ProgressCurrentDate();
                    }
                }

                Manager_DateAndTime.SetCurrentTime($"{CurrentHour:00}:{CurrentMinute:00}");

                _halfTime = false;
            }
            else
            {
                _halfTime = true;
            }
        }
    }
}