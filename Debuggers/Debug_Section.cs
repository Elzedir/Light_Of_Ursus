using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DebugSectionType
{
    Prefab,
    
    Testing,
    Hauling,
}

public class DebugSection : MonoBehaviour
{
    TextMeshProUGUI _sectionTitle;
    public TextMeshProUGUI SectionTitle
    {
        get { return _sectionTitle ??= (_sectionTitle = Manager_Game.FindTransformRecursively(transform, "DebugSectionTitle").GetComponent<TextMeshProUGUI>()); }
        set { _sectionTitle = value; }
    }

    public Dictionary<string, DebugEntry> AllDebugEntries = new();

    bool _sectionExpanded = true;

    public void InitialiseDebugSection(DebugSection_Data debugSection_Data)
    {
        SectionTitle.text = debugSection_Data.DebugSectionType.ToString();
        name = SectionTitle.text;

        gameObject.GetComponent<Button>().onClick.AddListener(ToggleSectionExpanded);

        UpdateDebugSection(debugSection_Data.AllEntryData);
    }

    public void ToggleSectionExpanded()
    {
        _sectionExpanded = !_sectionExpanded;
        
        if (_sectionExpanded)
        {
            foreach (var entry in AllDebugEntries)
            {
                entry.Value.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (var entry in AllDebugEntries)
            {
                entry.Value.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateDebugSection(List<DebugEntry_Data> allEntryData)
    {
        foreach (var debugEntryData in allEntryData)
        {
            if (!AllDebugEntries.ContainsKey(debugEntryData.DebugEntryKey.GetID()))
            {
                var newDebugEntry = Instantiate(Debug_Visualiser.Instance.DebugEntryPrefab, transform).AddComponent<DebugEntry>();
                Destroy(Manager_Game.FindTransformRecursively(newDebugEntry.transform, "DebugDataPrefab").gameObject);
                newDebugEntry.InitialiseDebugPanel(new DebugEntry_Data(debugEntryData));
                AllDebugEntries.Add(debugEntryData.DebugEntryKey.GetID(), newDebugEntry);
                return;
            }

            AllDebugEntries[debugEntryData.DebugEntryKey.GetID()].UpdateDebugEntry(debugEntryData.AllDebugData);
        }
    }
}

public class DebugSection_Data
{
    public DebugSectionType DebugSectionType;
    public List<DebugEntry_Data> AllEntryData;

    public DebugSection_Data(DebugSectionType debugSectionType, List<DebugEntry_Data> allEntryData)
    {
        DebugSectionType = debugSectionType;
        AllEntryData = allEntryData;
    }

    public DebugSection_Data(DebugSection_Data debugSection_Data)
    {
        DebugSectionType = debugSection_Data.DebugSectionType;
        AllEntryData = debugSection_Data.AllEntryData;
    }
}