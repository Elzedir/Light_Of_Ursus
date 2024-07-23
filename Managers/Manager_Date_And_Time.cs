using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Date_And_Time : MonoBehaviour
{
    public static Date CurrentDate {  get; private set; } = new Date(1, 1 , 100);
    static float _currentTimeScale = 1f;

    public float GetTimeScale()
    {
        return _currentTimeScale;
    }

    public static void SetTimeScale(float timeScale)
    {
        if (timeScale < 0) { Debug.LogError($"Timescale: {timeScale} cannot be less than 0.");  return; }

        _currentTimeScale = timeScale;
        Time.timeScale = _currentTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public static IEnumerator SetTimeScaleGradual(float targetTimeScale, float duration)
    {
        float start = _currentTimeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
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

    public float GetAge(Date birthDate)
    {
        return CurrentDate - birthDate;
    }
}


public enum Day { Daymon, Suedyat, Dawyed, Thraydus, Faydri, Sadtay, Sandun }

public class Date
{
    public int Day;
    public int Month;
    public int Year;

    public Date(int day, int month, int year)
    {
        Day = day;
        Month = month;
        Year = year;
    }

    public static float operator -(Date a, Date b)
    {
        DateTime dateA = new DateTime(a.Year, a.Month, a.Day);
        DateTime dateB = new DateTime(b.Year, b.Month, b.Day);
        TimeSpan difference = dateA - dateB;
        return (float)difference.TotalDays;
    }
}

