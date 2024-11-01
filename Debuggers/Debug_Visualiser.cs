using System.Collections.Generic;
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

    Button _xButton;
    Button XButton
    {
        get { return _xButton ??= (_xButton = Manager_Game.FindTransformRecursively(transform, "XPanel").GetComponent<Button>()); }
    }
    GameObject _debugPrefab;
    public GameObject DebugPrefab
    {
        get { return _debugPrefab ??= (_debugPrefab = Manager_Game.FindTransformRecursively(transform, "DebugPanel").gameObject); }
    }
    GameObject _dataPrefab;
    public GameObject DataPrefab
    {
        get { return _dataPrefab ??= (_dataPrefab = Manager_Game.FindTransformRecursively(transform, "DataPanel").gameObject); }
    }

    public Dictionary<uint, DebugVisual> DebugVisuals = new();

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        DebugPrefab.SetActive(false);
        gameObject.SetActive(true);
    }

    public void UpdateDebugVisualiser(uint debugID, DebugPanelType debugPanelType, Dictionary<DebugDataType, DebugData> data)
    {
        if (!DebugVisuals.ContainsKey(debugID))
        {
            DebugVisuals.Add(debugID, new DebugVisual(debugPanelType, data));
            _updatePanels();
        }

        DebugVisuals[debugID].UpdateData(data);
    }

    public void RemoveDebugPanel(DebugPanelType debugPanelType)
    {
        DebugVisuals.Remove(debugPanelType);
        _updatePanels();
    }

    void _updatePanels()
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Debug") && child.gameObject != DebugPrefab)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (var debugPanel in DebugVisuals)
        {
            var panelGO = Instantiate(DebugPrefab, transform);
            panelGO.SetActive(true);
            var panel = panelGO.AddComponent<DebugPanel>();
            panel.InitialiseDebugPanel(debugPanel.Value.Title.text);
            panel.name = debugPanel.Value.Title.text;

            foreach (var existingData in debugPanel.Value.AllData)
            {
                var dataPanel = Instantiate(DataPrefab, panelGO.transform);
                dataPanel.SetActive(true);
                var data = dataPanel.AddComponent<DebugData>();
                data.InitialiseDebugData(existingData.Key, existingData.Value.Value);
                data.name = existingData.Key.ToString();
            }
        }
    }
}

public class DebugVisual
{
    public DebugPanelType DebugPanelType;
    public Dictionary<DebugDataType, DebugData> DebugData;

    public DebugVisual(DebugPanelType debugPanelType, Dictionary<DebugDataType, DebugData> debugData)
    {
        DebugPanelType = debugPanelType;
        DebugData = debugData;
    }
}

public enum DebugPanelType
{
    HaulTo,
    HaulFrom,
}


public class DebugPanel : MonoBehaviour
{
    TextMeshProUGUI _title;
    public TextMeshProUGUI Title 
    {
        get { return _title ??= (_title = Manager_Game.FindTransformRecursively(transform, "Title").GetComponent<TextMeshProUGUI>()); }
        set { _title = value; }
    }
    public Dictionary<DebugDataType, DebugData> AllData;

    public void InitialiseDebugPanel(string title)
    {
        Title.text = title;
    }

    public void UpdateData(Dictionary<uint, (DebugData)> data)
    {
        foreach (var debugData in data)
        {
            if (AllData.ContainsKey(debugData.Key))
            {
                AllData[debugData.Key].Value = debugData.Value.Value;
            }
            else
            {
                AllData.Add(debugData.Key, debugData.Value);
            }
        }
    }

    public void RemoveData(DebugDataType debugDataType)
    {
        AllData.Remove(debugDataType);
    }
}

public enum DebugDataType
{
    Priority_Item,
    Priority_Distance,
    Priority_Total,
}

public class DebugData : MonoBehaviour
{
    public DebugDataType DebugDataType;
    public string Value;

    public void InitialiseDebugData(DebugDataType debugDataType, string value)
    {
        DebugDataType = debugDataType;
        Value = value;
    }
}