using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class Manager_DateAndTime : MonoBehaviour
    {
        static TextMeshProUGUI _dateText;
        static TextMeshProUGUI _timeText;

        static float _currentTimeScale = 1f;

        public static void Initialise()
        {
            _dateText = GameObject.Find("Date").GetComponent<TextMeshProUGUI>();
            _timeText = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();

            CurrentDate.Initialise();
            Time.Initialise();
        }

        public static void SetCurrentDate(string date)
        {
            _dateText.text = date;
        }

        public static void SetCurrentTime(string time)
        {
            _timeText.text = time;
        }

        public float GetTimeScale()
        {
            return _currentTimeScale;
        }

        public static void SetTimeScale(float timeScale)
        {
            if (timeScale < 0) { Debug.LogError($"Timescale: {timeScale} cannot be less than 0."); return; }

            _currentTimeScale               = timeScale;
            UnityEngine.Time.timeScale      = _currentTimeScale;
            UnityEngine.Time.fixedDeltaTime = 0.02f * UnityEngine.Time.timeScale;
        }

        public static IEnumerator SetTimeScaleGradual(float targetTimeScale, float duration)
        {
            float start   = _currentTimeScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                SetTimeScale(Mathf.Lerp(start, targetTimeScale, elapsed / duration));
                yield return null;
            }

            SetTimeScale(targetTimeScale);
        }

        public static void DecreaseTimeScale()
        {
            if (_currentTimeScale - 0.1f > 0) SetTimeScale(_currentTimeScale - 0.1f);
        }

        public static void IncreaseTimeScale()
        {
            SetTimeScale(_currentTimeScale + 0.1f);
        }

        public static void ResetTimeScale()
        {
            SetTimeScale(1f);
        }
    }

    public enum DayName { Mon, Tumon, Tu, Wetu, Wed, Thured, Thur, Frith, Fri, Satri, Satu, Sunsa, Sun, Monsun }
    public enum MonthName { Janbruach, Aprayne, Julaugber, Octevmadec }

    public class Date
    {
        public int TotalDays;

        public Date(int  totalDays) => TotalDays = totalDays;
        public Date (int day, int month, int year) => TotalDays = ConvertToTotalDays(day, month, year);

        public float GetAge() => CurrentDate.CurrentTotalDays - TotalDays;

        public static (int day, int month, int year) ConvertFromTotalDays(int totalDays)
        {
            int year          = totalDays     / 60;
            int remainingDays = totalDays     % 60;
            int month         = remainingDays / 14;
            int day           = remainingDays % 14;

            return (day + 1, month + 1, year);
        }

        public static int ConvertToTotalDays(int day, int month, int year)
        {
            return (year * 60) + ((month - 1) * 14) + (day - 1);
        }
    }

    public class CurrentDate
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
        bool              _halfTime = false;
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