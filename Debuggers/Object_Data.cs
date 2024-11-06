using TMPro;
using UnityEngine;

public enum ObjectDataType
{    
    Prefab,

    FullIdentification
}

public class ObjectData : MonoBehaviour
{
    TextMeshProUGUI _ObjectDataTitle;
    public TextMeshProUGUI ObjectDataTitle 
    {
        get { return _ObjectDataTitle ??= (_ObjectDataTitle = Manager_Game.FindTransformRecursively(transform, "ObjectDataTitle").GetComponent<TextMeshProUGUI>()); }
        set { _ObjectDataTitle = value; }
    }

    public ObjectDataType ObjectDataType;
    public string _ObjectValue;
    public string ObjectValue { get { return _ObjectValue; } set { _ObjectValue = value; _setName(); } }

    public void InitialiseObjectData(ObjectData_Data ObjectData)
    {
        ObjectDataType = ObjectData.ObjectDataType;
        ObjectValue = ObjectData.ObjectValue;
    }

    void _setName()
    {
        ObjectDataTitle.text = $"{ObjectDataType} - {ObjectValue}";
        name = ObjectDataTitle.text;
    }
}

public class ObjectData_Data
{
    public ObjectDataType ObjectDataType;
    public string ObjectValue;

    public ObjectData_Data(ObjectDataType objectDataType, string objectValue)
    {
        ObjectDataType = objectDataType;
        ObjectValue = objectValue;
    }

    public ObjectData_Data(ObjectData_Data ObjectData)
    {
        ObjectDataType = ObjectData.ObjectDataType;
        ObjectValue = ObjectData.ObjectValue;
    }
}
