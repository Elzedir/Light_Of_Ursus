using System;
using UnityEditor;
using UnityEngine;

namespace DateAndTime
{
    [CreateAssetMenu(fileName = "DateAndTime_SO", menuName = "SOList/DateAndTime_SO")]
    [Serializable]
    public class DateAndTime_SO : ScriptableObject
    {
        // Get CurrentTotalDays and CurrentTotalMinutes from save on load.
        
        bool _initialised;

        public uint CurrentTotalDays = 6000;
        public uint GetDate()        => CurrentTotalDays;
        
        public string Date { get; private set; }
        public           void   SetDate() => Date = Manager_DateAndTime.GetCurrentDateAsString();

        public uint CurrentTotalMinutes = 240;
        public uint GetTime()           => CurrentTotalMinutes;
        
        public string Time { get; private set; }
        public           void   SetTime() => Time = CurrentTime.GetCurrentTimeAsString();
    }
    
    [CustomEditor(typeof(DateAndTime_SO))]
    public class DateAndTime_SOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dateAndTimeSO = (DateAndTime_SO)target;
            
            EditorGUILayout.LabelField("Date",   $"{dateAndTimeSO.Date}");
            
            EditorGUILayout.LabelField("Time", $"{dateAndTimeSO.Time}");
        }
    }
}
