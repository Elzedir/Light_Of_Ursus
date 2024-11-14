using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debuggers
{
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
            set => _sectionTitle = value;
        }

        public readonly Dictionary<string, DebugEntry> AllDebugEntries = new();

        bool _sectionExpanded = true;

        public void InitialiseDebugSection(DebugSectionData debugSectionData)
        {
            SectionTitle.text = debugSectionData.DebugSectionType.ToString();
            name = SectionTitle.text;

            gameObject.GetComponent<Button>().onClick.AddListener(ToggleSectionExpanded);

            UpdateDebugSection(debugSectionData.AllEntryData);
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
                    var newDebugEntry = Instantiate(DebugVisualiser.Instance.DebugEntryPrefab, transform).AddComponent<DebugEntry>();
                    Destroy(Manager_Game.FindTransformRecursively(newDebugEntry.transform, "DebugDataPrefab").gameObject);
                    newDebugEntry.InitialiseDebugPanel(new DebugEntry_Data(debugEntryData));
                    AllDebugEntries.Add(debugEntryData.DebugEntryKey.GetID(), newDebugEntry);
                    return;
                }

                AllDebugEntries[debugEntryData.DebugEntryKey.GetID()].UpdateDebugEntry(debugEntryData.AllDebugData);
            }
        }
    }

    public class DebugSectionData
    {
        public readonly DebugSectionType DebugSectionType;
        public readonly List<DebugEntry_Data> AllEntryData;

        public DebugSectionData(DebugSectionType debugSectionType, List<DebugEntry_Data> allEntryData)
        {
            DebugSectionType = debugSectionType;
            AllEntryData = allEntryData;
        }

        public DebugSectionData(DebugSectionData debugSectionData)
        {
            DebugSectionType = debugSectionData.DebugSectionType;
            AllEntryData = debugSectionData.AllEntryData;
        }
    }
}