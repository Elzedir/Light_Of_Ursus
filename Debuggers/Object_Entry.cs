using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectEntry : MonoBehaviour
{
    TextMeshProUGUI _ObjectEntryTitle;
    public TextMeshProUGUI ObjectEntryTitle 
    {
        get { return _ObjectEntryTitle ??= (_ObjectEntryTitle = Manager_Game.FindTransformRecursively(transform, "ObjectEntryTitle").GetComponent<TextMeshProUGUI>()); }
        set { _ObjectEntryTitle = value; }
    }

    Transform _allData;
    public Transform AllData
    {
        get { return _allData ??= (_allData = Manager_Game.FindTransformRecursively(transform, "AllData")); }
        set { _allData = value; }
    }

    public Dictionary<ObjectDataType, ObjectData> AllObjectData = new();

    bool _entryExpanded = true;

    public void InitialiseObjectPanel(ObjectEntry_Data ObjectEntryData)
    {
        ObjectEntryTitle.text = $"{ObjectEntryData.ObjectEntryKey.ObjectEntryName} - {ObjectEntryData.ObjectEntryKey.ObjectEntryObject} - {ObjectEntryData.ObjectEntryKey.ObjectEntryID}";
        name = ObjectEntryTitle.text;

        UpdateObjectEntry(ObjectEntryData.AllObjectData);

        gameObject.GetComponent<Button>().onClick.AddListener(ToggleEntryExpanded);
    }

    public void ToggleEntryExpanded()
    {
        _entryExpanded = !_entryExpanded;
        
        if (_entryExpanded)
        {
            foreach (var data in AllObjectData)
            {
                data.Value.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (var data in AllObjectData)
            {
                data.Value.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateObjectEntry(List<ObjectData_Data> allObjectData)
    {
        foreach (var ObjectData in allObjectData)
        {
            if (!AllObjectData.ContainsKey(ObjectData.ObjectDataType))
            {
                var newObjectData = Instantiate(Object_Visualiser.Instance.ObjectDataPrefab, AllData).AddComponent<ObjectData>();
                newObjectData.InitialiseObjectData(new ObjectData_Data(ObjectData));
                AllObjectData.Add(ObjectData.ObjectDataType, newObjectData);
                return;
            }

            AllObjectData[ObjectData.ObjectDataType].ObjectValue = ObjectData.ObjectValue;
        }
    }
}

public class ObjectEntryKey
{
    public string ObjectEntryName;
    public string ObjectEntryObject;
    public uint ObjectEntryID;

    public ObjectEntryKey(string objectEntryName, string objectEntryObject, uint objectEntryID)
    {
        ObjectEntryName = objectEntryName;
        ObjectEntryObject = objectEntryObject;
        ObjectEntryID = objectEntryID;
    }

    public string GetID()
    {
        return $"{ObjectEntryName} - {ObjectEntryObject} - {ObjectEntryID}";
    }
}

public class ObjectEntry_Data
{
    public ObjectEntryKey ObjectEntryKey;
    public List<ObjectData_Data> AllObjectData;

    public ObjectEntry_Data(ObjectEntryKey objectEntryKey, List<ObjectData_Data> allObjectData)
    {
        ObjectEntryKey = objectEntryKey;
        AllObjectData = allObjectData;
    }

    public ObjectEntry_Data(ObjectEntry_Data ObjectEntryData)
    {
        ObjectEntryKey = ObjectEntryData.ObjectEntryKey;
        AllObjectData = ObjectEntryData.AllObjectData;
    }
}
