using Managers;
using TMPro;
using UnityEngine;

namespace Debuggers
{
    // public enum ObjectDataType
    // {    
    //     Prefab,
    //
    //     FullIdentification,
    //     InventoryData,
    //     PriorityData,
    //     ProgressData,
    // }
    //
    // public class ObjectData : MonoBehaviour
    // {
    //     TextMeshProUGUI _objectDataTitle;
    //     public TextMeshProUGUI ObjectDataTitle 
    //     {
    //         get { return _objectDataTitle ??= (_objectDataTitle = Manager_Game.FindTransformRecursively(transform, "ObjectDataTitle").GetComponent<TextMeshProUGUI>()); }
    //         set => _objectDataTitle = value;
    //     }
    //
    //     public ObjectDataType ObjectDataType;
    //     string _objectValue;
    //     public string ObjectValue { get => _objectValue;
    //         set { _objectValue = value; _setName(); } }
    //
    //     public void InitialiseObjectData(ObjectData_Data objectData)
    //     {
    //         ObjectDataType = objectData.ObjectDataType;
    //         ObjectValue = objectData.ObjectValue;
    //     }
    //
    //     void _setName()
    //     {
    //         ObjectDataTitle.text = $"{ObjectValue}";
    //         name = ObjectDataTitle.text;
    //     }
    // }
    //
    // public class ObjectData_Data
    // {
    //     public readonly ObjectDataType ObjectDataType;
    //     public readonly string ObjectValue;
    //
    //     public ObjectData_Data(ObjectDataType objectDataType, string objectValue)
    //     {
    //         ObjectDataType = objectDataType;
    //         ObjectValue = objectValue;
    //     }
    //
    //     public ObjectData_Data(ObjectData_Data objectData)
    //     {
    //         ObjectDataType = objectData.ObjectDataType;
    //         ObjectValue = objectData.ObjectValue;
    //     }
    // }
}