using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using Time = UnityEngine.Time;

namespace Debuggers
{
    public class DebugVisualiser : MonoBehaviour
    {
        // static DebugVisualiser _instance;
        //
        // public static DebugVisualiser Instance =>
        //     _instance ??= Manager_Game.FindTransformRecursively(GameObject.Find("UI").transform, "Debug_Visualiser").GetComponent<DebugVisualiser>();
        //
        // GameObject _debugPanelParent;
        // GameObject DebugPanelParent => _debugPanelParent ??= _debugPanelParent =
        //     Manager_Game.FindTransformRecursively(transform, "DebugPanelParent").gameObject;
        // RectTransform _debugPanelParentRect;
        // RectTransform DebugPanelParentRect => _debugPanelParentRect ??= _debugPanelParentRect =
        //     DebugPanelParent.GetComponent<RectTransform>();
        //
        // Button _xButton;
        // Button XButton => _xButton ??=
        //     _xButton = Manager_Game.FindTransformRecursively(transform, "XPanel").GetComponent<Button>();
        //
        // GameObject _debugSectionPrefab;
        // GameObject DebugSectionPrefab => _debugSectionPrefab ??= _debugSectionPrefab =
        //     Manager_Game.FindTransformRecursively(transform, "DebugSectionPrefab").gameObject;
        //
        // GameObject _debugEntryPrefab;
        // public GameObject DebugEntryPrefab => _debugEntryPrefab ??= _debugEntryPrefab =
        //     Manager_Game.FindTransformRecursively(transform, "DebugEntryPrefab").gameObject;
        //
        // GameObject _debugDataPrefab;
        // public GameObject DebugDataPrefab => _debugDataPrefab ??= _debugDataPrefab =
        //     Manager_Game.FindTransformRecursively(transform, "DebugDataPrefab").gameObject;
        //
        // readonly Dictionary<DebugSectionType, DebugSection> _allDebugSections = new();
        //
        // void Start()
        // {
        //     _togglePrefabs(false);
        // }
        //
        // const float _tickRate = 0.1f;
        // float       _nextTickTime;
        // public void Update()
        // {
        //     if (!Input.GetKeyDown(KeyCode.F1)) return;
        //
        //     if (gameObject.activeSelf)
        //     {
        //         _closePanel();
        //     }
        //     else
        //     {
        //         _openPanel();
        //     }
        //     
        //     if (Time.time < _nextTickTime) return;
        //     
        //     _nextTickTime = Time.time + _tickRate;
        //     
        //     _onTick();
        // }
        //
        // void _onTick()
        // {
        //     LayoutRebuilder.ForceRebuildLayoutImmediate(DebugPanelParentRect);
        // }
        //
        // void _togglePrefabs(bool toggle)
        // {
        //     DebugSectionPrefab.SetActive(toggle);
        //     DebugEntryPrefab.SetActive(toggle);
        //     DebugDataPrefab.SetActive(toggle);
        // }
        //
        // void _closePanel()
        // {
        //     gameObject.SetActive(false);
        // }
        //
        // void _openPanel()
        // {
        //     gameObject.SetActive(true);
        // }
        //
        // void _updateDebugSection(DebugSectionData debugSectionData)
        // {
        //     _togglePrefabs(true);
        //
        //     if (!_allDebugSections.TryGetValue(debugSectionData.DebugSectionType, out var section))
        //     {
        //         var newDebugSection = Instantiate(DebugSectionPrefab, DebugPanelParent.transform).AddComponent<DebugSection>();
        //         Destroy(Manager_Game.FindTransformRecursively(newDebugSection.transform, "DebugEntryPrefab").gameObject);
        //         newDebugSection.InitialiseDebugSection(new DebugSectionData(debugSectionData));
        //         _allDebugSections.Add(debugSectionData.DebugSectionType, newDebugSection);
        //     
        //         _togglePrefabs(false);
        //     
        //         return;
        //     }
        //
        //     section.UpdateDebugSection(debugSectionData.AllEntryData);
        //
        //     _togglePrefabs(false);
        // }
        //
        // public void UpdateDebugEntry(DebugSectionType debugSectionType, DebugEntry_Data debugEntryData)
        // {
        //     _togglePrefabs(true);
        //
        //     if (!_allDebugSections.ContainsKey(debugSectionType))
        //     {
        //         var newDebugEntryDataList = new List<DebugEntry_Data> { debugEntryData };
        //         var newDebugSectionData = new DebugSectionData(debugSectionType, newDebugEntryDataList);
        //
        //         _updateDebugSection(newDebugSectionData);
        //
        //         _togglePrefabs(false);
        //
        //         return;
        //     }
        //
        //     if (!_allDebugSections[debugSectionType].AllDebugEntries.ContainsKey(debugEntryData.DebugEntryKey.GetID()))
        //     {
        //         var newDebugEntryDataList = new List<DebugEntry_Data> { debugEntryData };
        //
        //         _allDebugSections[debugSectionType].UpdateDebugSection(newDebugEntryDataList);
        //
        //         _togglePrefabs(false);
        //
        //         return;
        //     }
        //
        //     _allDebugSections[debugSectionType].AllDebugEntries[debugEntryData.DebugEntryKey.GetID()].UpdateDebugEntry(debugEntryData.AllDebugData);
        //
        //     _togglePrefabs(false);
        // }
        //
        // public void UpdateDebugData(DebugSectionType debugSectionType, DebugEntryKey debugEntryTitle, DebugData_Data debugData)
        // {
        //     _togglePrefabs(true);
        //
        //     if (!_allDebugSections.TryGetValue(debugSectionType, out var section))
        //     {
        //         var newDebugDataList = new List<DebugData_Data> { debugData };
        //         var newDebugEntry = new DebugEntry_Data(debugEntryTitle, newDebugDataList);
        //         var newDebugEntryDataList = new List<DebugEntry_Data> { newDebugEntry };
        //         var newDebugSectionData = new DebugSectionData(debugSectionType, newDebugEntryDataList);
        //
        //         _updateDebugSection(newDebugSectionData);
        //
        //         _togglePrefabs(false);
        //
        //         return;
        //     }
        //
        //     if (!section.AllDebugEntries.ContainsKey(debugEntryTitle.GetID()))
        //     {
        //         var newDebugDataList = new List<DebugData_Data> { debugData };
        //         var newDebugEntry = new DebugEntry_Data(debugEntryTitle, newDebugDataList);
        //
        //         UpdateDebugEntry(debugSectionType, newDebugEntry);
        //
        //         _togglePrefabs(false);
        //
        //         return;
        //     }
        //
        //     if (!_allDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].AllDebugData.ContainsKey(debugData.DebugDataType))
        //     {
        //         var newDebugDataList = new List<DebugData_Data> { debugData };
        //
        //         _allDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].UpdateDebugEntry(newDebugDataList);
        //
        //         _togglePrefabs(false);
        //
        //         return;
        //     }
        //
        //     _allDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].AllDebugData[debugData.DebugDataType].DebugValue = debugData.DebugValue;
        //
        //     _togglePrefabs(false);
        // }
        //
        // public void RemoveDebugSection(DebugSectionType debugSectionType)
        // {
        //     if (!_allDebugSections.TryGetValue(debugSectionType, out var section))
        //     {
        //         Debug.LogWarning($"Debug Section {debugSectionType} does not exist.");
        //         return;
        //     }
        //
        //     Destroy(section.gameObject);
        //     _allDebugSections.Remove(debugSectionType);
        // }
    }
}