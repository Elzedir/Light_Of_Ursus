using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debuggers
{
    public class ObjectEntry : MonoBehaviour
    {
        TextMeshProUGUI _objectEntryTitle;
        public TextMeshProUGUI ObjectEntryTitle 
        {
            get { return _objectEntryTitle ??= (_objectEntryTitle = Manager_Game.FindTransformRecursively(transform, "ObjectEntryTitle").GetComponent<TextMeshProUGUI>()); }
            set => _objectEntryTitle = value;
        }

        Transform _allData;
        public Transform AllData
        {
            get { return _allData ??= (_allData = Manager_Game.FindTransformRecursively(transform, "AllData")); }
            set => _allData = value;
        }

        public readonly Dictionary<ObjectDataType, ObjectData> AllObjectData = new();

        bool _entryExpanded = true;

        public void InitialiseObjectPanel(ObjectEntryData objectEntryData)
        {
            ObjectEntryTitle.text = $"{objectEntryData.ObjectEntryKey.ObjectEntryName} - {objectEntryData.ObjectEntryKey.ObjectEntryObject} - {objectEntryData.ObjectEntryKey.ObjectEntryID}";
            name = ObjectEntryTitle.text;

            UpdateObjectEntry(objectEntryData.AllObjectData);

            gameObject.GetComponent<Button>().onClick.AddListener(_toggleEntryExpanded);
        }

        void _toggleEntryExpanded()
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
            foreach (var objectData in allObjectData)
            {
                if (!AllObjectData.TryGetValue(objectData.ObjectDataType, out var value))
                {
                    var newObjectData = Instantiate(ObjectVisualiser.Instance.ObjectDataPrefab, AllData).AddComponent<ObjectData>();
                    newObjectData.InitialiseObjectData(new ObjectData_Data(objectData));
                    AllObjectData.Add(objectData.ObjectDataType, newObjectData);
                    return;
                }

                value.ObjectValue = objectData.ObjectValue;
            }
        }
    }

    public class ObjectEntryKey
    {
        public readonly string ObjectEntryName;
        public readonly string ObjectEntryObject;
        public readonly uint ObjectEntryID;

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

    public class ObjectEntryData
    {
        public readonly ObjectEntryKey ObjectEntryKey;
        public readonly List<ObjectData_Data> AllObjectData;

        public ObjectEntryData(ObjectEntryKey objectEntryKey, List<ObjectData_Data> allObjectData)
        {
            ObjectEntryKey = objectEntryKey;
            AllObjectData = allObjectData;
        }

        public ObjectEntryData(ObjectEntryData objectEntryData)
        {
            ObjectEntryKey = objectEntryData.ObjectEntryKey;
            AllObjectData = objectEntryData.AllObjectData;
        }
    }
}