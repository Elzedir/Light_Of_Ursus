using TMPro;
using UnityEngine;

namespace Debuggers
{
    public enum DebugDataType
    {    
        Prefab,

        Priority_Item,
        Priority_Distance,
        Priority_Total,

        StationType,
    }

    public class DebugData : MonoBehaviour
    {
        TextMeshProUGUI _debugDataTitle;
        public TextMeshProUGUI DebugDataTitle 
        {
            get { return _debugDataTitle ??= (_debugDataTitle = Manager_Game.FindTransformRecursively(transform, "DebugDataTitle").GetComponent<TextMeshProUGUI>()); }
            set => _debugDataTitle = value;
        }

        public DebugDataType DebugDataType;
        string _debugValue;
        public string DebugValue { get => _debugValue;
            set { _debugValue = value; _setName(); } }

        public void InitialiseDebugData(DebugData_Data debugData)
        {
            DebugDataType = debugData.DebugDataType;
            DebugValue = debugData.DebugValue;
        }

        void _setName()
        {
            DebugDataTitle.text = $"{DebugDataType} - {DebugValue}";
            name = DebugDataTitle.text;
        }
    }

    public class DebugData_Data
    {
        public readonly DebugDataType DebugDataType;
        public readonly string DebugValue;

        public DebugData_Data(DebugDataType debugDataType, string debugValue)
        {
            DebugDataType = debugDataType;
            DebugValue = debugValue;
        }

        public DebugData_Data(DebugData_Data debugData)
        {
            DebugDataType = debugData.DebugDataType;
            DebugValue = debugData.DebugValue;
        }
    }
}