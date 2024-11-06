using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        get { return _sectionTitle ??= (_sectionTitle = Manager_Game.FindTransformRecursively(transform, "ObjectSectionTitle").GetComponent<TextMeshProUGUI>()); }
        set { _sectionTitle = value; }
    }

    public Dictionary<string, ObjectEntry> AllObjectEntries = new();

    bool _sectionExpanded = true;

    public void InitialiseObjectSection(ObjectSection_Data ObjectSection_Data)
    {
        SectionTitle.text = ObjectSection_Data.ObjectSectionType.ToString();
        name = SectionTitle.text;

        gameObject.GetComponent<Button>().onClick.AddListener(ToggleSectionExpanded);

        UpdateObjectSection(ObjectSection_Data.AllEntryData);
    }

    public void ToggleSectionExpanded()
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

    public void UpdateObjectSection(List<ObjectEntry_Data> allEntryData)
    {
        foreach (var ObjectEntryData in allEntryData)
        {
            if (!AllObjectEntries.ContainsKey(ObjectEntryData.ObjectEntryKey.GetID()))
            {
                var newObjectEntry = Instantiate(Object_Visualiser.Instance.ObjectEntryPrefab, transform).AddComponent<ObjectEntry>();
                Destroy(Manager_Game.FindTransformRecursively(newObjectEntry.transform, "ObjectDataPrefab").gameObject);
                newObjectEntry.InitialiseObjectPanel(new ObjectEntry_Data(ObjectEntryData));
                AllObjectEntries.Add(ObjectEntryData.ObjectEntryKey.GetID(), newObjectEntry);
                return;
            }

            AllObjectEntries[ObjectEntryData.ObjectEntryKey.GetID()].UpdateObjectEntry(ObjectEntryData.AllObjectData);
        }
    }
}

public class ObjectSection_Data
{
    public ObjectSectionType ObjectSectionType;
    public List<ObjectEntry_Data> AllEntryData;

    public ObjectSection_Data(ObjectSectionType objectSectionType, List<ObjectEntry_Data> allEntryData)
    {
        ObjectSectionType = objectSectionType;
        AllEntryData = allEntryData;
    }

    public ObjectSection_Data(ObjectSection_Data ObjectSection_Data)
    {
        ObjectSectionType = ObjectSection_Data.ObjectSectionType;
        AllEntryData = ObjectSection_Data.AllEntryData;
    }
}