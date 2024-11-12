using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debuggers
{
    public enum ObjectSectionType
    {
        Prefab,
    
        Testing,
        Hauling,
    }

    public class ObjectSection : MonoBehaviour
    {
        TextMeshProUGUI _sectionTitle;
        public TextMeshProUGUI SectionTitle
        {
            get { return _sectionTitle ??= _sectionTitle = Manager_Game.FindTransformRecursively(transform, "ObjectSectionTitle").GetComponent<TextMeshProUGUI>(); }
            set => _sectionTitle = value;
        }

        public readonly Dictionary<string, ObjectEntry> AllObjectEntries = new();

        bool _sectionExpanded = true;

        public void InitialiseObjectSection(ObjectSection_Data objectSectionData)
        {
            SectionTitle.text = objectSectionData.ObjectSectionType.ToString();
            name = SectionTitle.text;

            gameObject.GetComponent<Button>().onClick.AddListener(_toggleSectionExpanded);

            UpdateObjectSection(objectSectionData.AllEntryData);
        }

        void _toggleSectionExpanded()
        {
            _sectionExpanded = !_sectionExpanded;
        
            if (_sectionExpanded)
            {
                foreach (var entry in AllObjectEntries)
                {
                    entry.Value.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (var entry in AllObjectEntries)
                {
                    entry.Value.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateObjectSection(List<ObjectEntryData> allEntryData)
        {
            foreach (var objectEntryData in allEntryData)
            {
                if (!AllObjectEntries.ContainsKey(objectEntryData.ObjectEntryKey.GetID()))
                {
                    var newObjectEntry = Instantiate(ObjectVisualiser.Instance.ObjectEntryPrefab, transform).AddComponent<ObjectEntry>();
                    Destroy(Manager_Game.FindTransformRecursively(newObjectEntry.transform, "ObjectDataPrefab").gameObject);
                    newObjectEntry.InitialiseObjectPanel(new ObjectEntryData(objectEntryData));
                    AllObjectEntries.Add(objectEntryData.ObjectEntryKey.GetID(), newObjectEntry);
                    return;
                }

                AllObjectEntries[objectEntryData.ObjectEntryKey.GetID()].UpdateObjectEntry(objectEntryData.AllObjectData);
            }
        }
    }

    public class ObjectSection_Data
    {
        public readonly ObjectSectionType ObjectSectionType;
        public readonly List<ObjectEntryData> AllEntryData;

        public ObjectSection_Data(ObjectSectionType objectSectionType, List<ObjectEntryData> allEntryData)
        {
            ObjectSectionType = objectSectionType;
            AllEntryData = allEntryData;
        }

        public ObjectSection_Data(ObjectSection_Data objectSectionData)
        {
            ObjectSectionType = objectSectionData.ObjectSectionType;
            AllEntryData = objectSectionData.AllEntryData;
        }
    }
}