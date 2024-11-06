using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Debug_Visualiser : MonoBehaviour
{
    static Debug_Visualiser _instance;
    public static Debug_Visualiser Instance
    {
        get { return _instance ??= GameObject.Find("Debug_Visualiser").GetComponent<Debug_Visualiser>(); }
    }

    GameObject _debugPanelParent;
    public GameObject DebugPanelParent
    {
        get { return _debugPanelParent ??= _debugPanelParent = Manager_Game.FindTransformRecursively(transform, "DebugPanelParent").gameObject; }
    }

    Button _xButton;
    Button XButton
    {
        get { return _xButton ??= _xButton = Manager_Game.FindTransformRecursively(transform, "XPanel").GetComponent<Button>(); }
    }

    GameObject _debugSectionPrefab;
    GameObject DebugSectionPrefab
    {
        get { return _debugSectionPrefab ??= _debugSectionPrefab = Manager_Game.FindTransformRecursively(transform, "DebugSectionPrefab").gameObject; }
    }

    GameObject _debugEntryPrefab;
    public GameObject DebugEntryPrefab
    {
        get { return _debugEntryPrefab ??= _debugEntryPrefab = Manager_Game.FindTransformRecursively(transform, "DebugEntryPrefab").gameObject; }
    }

    GameObject _debugDataPrefab;
    public GameObject DebugDataPrefab
    {
        get { return _debugDataPrefab ??= _debugDataPrefab = Manager_Game.FindTransformRecursively(transform, "DebugDataPrefab").gameObject; }
    }

    public Dictionary<DebugSectionType, DebugSection> AllDebugSections = new();

    void Start()
    {
        TogglePrefabs(false);
    }

    public void Initialise()
    {
        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneTenthSecond);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (gameObject.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }

    public void OnTick()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(DebugPanelParent.GetComponent<RectTransform>());
    }

    public void TogglePrefabs(bool toggle)
    {
        DebugSectionPrefab.SetActive(toggle);
        DebugEntryPrefab.SetActive(toggle);
        DebugDataPrefab.SetActive(toggle);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void UpdateDebugSection(DebugSection_Data debugSectionData)
    {
        TogglePrefabs(true);

        if (!AllDebugSections.ContainsKey(debugSectionData.DebugSectionType))
        {
            var newDebugSection = Instantiate(DebugSectionPrefab, DebugPanelParent.transform).AddComponent<DebugSection>();
            Destroy(Manager_Game.FindTransformRecursively(newDebugSection.transform, "DebugEntryPrefab").gameObject);
            newDebugSection.InitialiseDebugSection(new DebugSection_Data(debugSectionData));
            AllDebugSections.Add(debugSectionData.DebugSectionType, newDebugSection);
            
            TogglePrefabs(false);
            
            return;
        }

        AllDebugSections[debugSectionData.DebugSectionType].UpdateDebugSection(debugSectionData.AllEntryData);

        TogglePrefabs(false);
    }

    public void UpdateDebugEntry(DebugSectionType debugSectionType, DebugEntry_Data debugEntryData)
    {
        TogglePrefabs(true);

        if (!AllDebugSections.ContainsKey(debugSectionType))
        {
            var newDebugEntryDataList = new List<DebugEntry_Data> { debugEntryData };
            var newDebugSectionData = new DebugSection_Data(debugSectionType, newDebugEntryDataList);

            UpdateDebugSection(newDebugSectionData);

            TogglePrefabs(false);

            return;
        }

        if (!AllDebugSections[debugSectionType].AllDebugEntries.ContainsKey(debugEntryData.DebugEntryKey.GetID()))
        {
            var newDebugEntryDataList = new List<DebugEntry_Data> { debugEntryData };

            AllDebugSections[debugSectionType].UpdateDebugSection(newDebugEntryDataList);

            TogglePrefabs(false);

            return;
        }

        AllDebugSections[debugSectionType].AllDebugEntries[debugEntryData.DebugEntryKey.GetID()].UpdateDebugEntry(debugEntryData.AllDebugData);

        TogglePrefabs(false);
    }

    public void UpdateDebugData(DebugSectionType debugSectionType, DebugEntryKey debugEntryTitle, DebugData_Data debugData)
    {
        TogglePrefabs(true);

        if (!AllDebugSections.ContainsKey(debugSectionType))
        {
            var newDebugDataList = new List<DebugData_Data> { debugData };
            var newDebugEntry = new DebugEntry_Data(debugEntryTitle, newDebugDataList);
            var newDebugEntryDataList = new List<DebugEntry_Data> { newDebugEntry };
            var newDebugSectionData = new DebugSection_Data(debugSectionType, newDebugEntryDataList);

            UpdateDebugSection(newDebugSectionData);

            TogglePrefabs(false);

            return;
        }

        if (!AllDebugSections[debugSectionType].AllDebugEntries.ContainsKey(debugEntryTitle.GetID()))
        {
            var newDebugDataList = new List<DebugData_Data> { debugData };
            var newDebugEntry = new DebugEntry_Data(debugEntryTitle, newDebugDataList);

            UpdateDebugEntry(debugSectionType, newDebugEntry);

            TogglePrefabs(false);

            return;
        }

        if (!AllDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].AllDebugData.ContainsKey(debugData.DebugDataType))
        {
            var newDebugDataList = new List<DebugData_Data> { debugData };

            AllDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].UpdateDebugEntry(newDebugDataList);

            TogglePrefabs(false);

            return;
        }

        AllDebugSections[debugSectionType].AllDebugEntries[debugEntryTitle.GetID()].AllDebugData[debugData.DebugDataType].DebugValue = debugData.DebugValue;

        TogglePrefabs(false);
    }

    public void RemoveDebugSection(DebugSectionType debugSectionType)
    {
        if (!AllDebugSections.ContainsKey(debugSectionType))
        {
            Debug.LogWarning($"Debug Section {debugSectionType} does not exist.");
            return;
        }

        Destroy(AllDebugSections[debugSectionType].gameObject);
        AllDebugSections.Remove(debugSectionType);
    }
}