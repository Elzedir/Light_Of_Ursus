using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debuggers
{
    // public class DebugEntry : MonoBehaviour
    // {
    //     TextMeshProUGUI _debugEntryTitle;
    //     public TextMeshProUGUI DebugEntryTitle 
    //     {
    //         get { return _debugEntryTitle ??= (_debugEntryTitle = Manager_Game.FindTransformRecursively(transform, "DebugEntryTitle").GetComponent<TextMeshProUGUI>()); }
    //         set => _debugEntryTitle = value;
    //     }
    //
    //     Transform _allData;
    //     public Transform AllData
    //     {
    //         get { return _allData ??= (_allData = Manager_Game.FindTransformRecursively(transform, "AllData")); }
    //         set => _allData = value;
    //     }
    //
    //     public readonly Dictionary<DebugDataType, DebugData> AllDebugData = new();
    //
    //     bool _entryExpanded = true;
    //
    //     public void InitialiseDebugPanel(DebugEntry_Data debugEntryData)
    //     {
    //         DebugEntryTitle.text = $"{debugEntryData.DebugEntryKey.DebugEntryName} - {debugEntryData.DebugEntryKey.DebugEntryObject} - {debugEntryData.DebugEntryKey.DebugEntryID}";
    //         name = DebugEntryTitle.text;
    //
    //         UpdateDebugEntry(debugEntryData.AllDebugData);
    //
    //         gameObject.GetComponent<Button>().onClick.AddListener(_toggleEntryExpanded);
    //     }
    //
    //     void _toggleEntryExpanded()
    //     {
    //         _entryExpanded = !_entryExpanded;
    //     
    //         if (_entryExpanded)
    //         {
    //             foreach (var data in AllDebugData)
    //             {
    //                 data.Value.gameObject.SetActive(true);
    //             }
    //         }
    //         else
    //         {
    //             foreach (var data in AllDebugData)
    //             {
    //                 data.Value.gameObject.SetActive(false);
    //             }
    //         }
    //     }
    //
    //     public void UpdateDebugEntry(List<DebugData_Data> allDebugData)
    //     {
    //         foreach (var debugData in allDebugData)
    //         {
    //             if (!AllDebugData.TryGetValue(debugData.DebugDataType, out var value))
    //             {
    //                 var newDebugData = Instantiate(DebugVisualiser.Instance.DebugDataPrefab, AllData).AddComponent<DebugData>();
    //                 newDebugData.InitialiseDebugData(new DebugData_Data(debugData));
    //                 AllDebugData.Add(debugData.DebugDataType, newDebugData);
    //                 return;
    //             }
    //
    //             value.DebugValue = debugData.DebugValue;
    //         }
    //     }
    // }
    //
    // public class DebugEntryKey
    // {
    //     public readonly string DebugEntryName;
    //     public readonly string DebugEntryObject;
    //     public readonly uint DebugEntryID;
    //
    //     public DebugEntryKey(string debugEntryName, string debugEntryObject, uint debugEntryID)
    //     {
    //         DebugEntryName = debugEntryName;
    //         DebugEntryObject = debugEntryObject;
    //         DebugEntryID = debugEntryID;
    //     }
    //
    //     public string GetID()
    //     {
    //         return $"{DebugEntryName} - {DebugEntryObject} - {DebugEntryID}";
    //     }
    // }
    //
    // public class DebugEntry_Data
    // {
    //     public readonly DebugEntryKey DebugEntryKey;
    //     public readonly List<DebugData_Data> AllDebugData;
    //
    //     public DebugEntry_Data(DebugEntryKey debugEntryKey, List<DebugData_Data> allDebugData)
    //     {
    //         DebugEntryKey = debugEntryKey;
    //         AllDebugData = allDebugData;
    //     }
    //
    //     public DebugEntry_Data(DebugEntry_Data debugEntryData)
    //     {
    //         DebugEntryKey = debugEntryData.DebugEntryKey;
    //         AllDebugData = debugEntryData.AllDebugData;
    //     }
    // }
}