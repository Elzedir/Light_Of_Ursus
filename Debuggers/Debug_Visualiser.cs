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
        get { return _xButton ??= _xButton = Manager_Game.FindTransformRecursively(transform, "XPanel").GetComponent<Button>(); }
    }

    GameObject _debugPrefab;
    public GameObject DebugPrefab
    {
        get { return _debugPrefab ??= _debugPrefab = Manager_Game.FindTransformRecursively(transform, "DebugPanel").gameObject; }
    }

    GameObject _dataPrefab;
    public GameObject DataPrefab
    {
        get { return _dataPrefab ??= _dataPrefab = Manager_Game.FindTransformRecursively(transform, "DataPanel").gameObject; }
    }

    GameObject _debugPanelParent;
    public GameObject DebugPanelParent
    {
        get { return _debugPanelParent ??= _debugPanelParent = Manager_Game.FindTransformRecursively(transform, "DebugPanelParent").gameObject; }
    }

    public Dictionary<(ActionName, object, uint), List<(DebugDataType DebugDataType, string DebugValue)>> AllDebugData = new();

    void Start()
    {
        DebugPrefab.SetActive(false);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        DebugPrefab.SetActive(false);
        gameObject.SetActive(true);
    }

    public void UpdateDebugVisualiser((ActionName, object, uint) debugID, List<(DebugDataType, string)> data)
    {
        if (!AllDebugData.ContainsKey(debugID))
        {
            AllDebugData.Add(debugID, data);
            _updatePanels();
        }

        AllDebugData[debugID] = data;
    }

    public void RemoveDebugPanel((ActionName, object, uint) debugID)
    {
        AllDebugData.Remove(debugID);
        _updatePanels();
    }

    void _updatePanels()
    {
        foreach (Transform child in DebugPanelParent.transform)
        {
            if (child.name.Contains("Station"))
            {
                Destroy(child.gameObject);
            }
            else
            {
                Debug.LogError($"Child not debug. Child: {child.name}");
            }
        }

        foreach (var debugObject in AllDebugData)
        {
            var panelGO = Instantiate(DebugPrefab, DebugPanelParent.transform);
            panelGO.SetActive(true);
            var panel = panelGO.AddComponent<DebugPanel>();
            panel.InitialiseDebugPanel(debugObject.Key.ToString());
            Transform dataParent = Manager_Game.FindTransformRecursively(panelGO.transform, "AllData");

            foreach (var debugData in debugObject.Value)
            {
                var dataPanel = Instantiate(DataPrefab, dataParent);
                dataPanel.SetActive(true);
                var data = dataPanel.AddComponent<DebugData>();
                data.InitialiseDebugData(debugData.DebugDataType, debugData.DebugValue);
            }
        }

        DebugPrefab.SetActive(true);
        DebugPrefab.SetActive(false);
    }
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
        name = title;
    }

    public void UpdateData(List<DebugData> data)
    {
        foreach (var debugData in data)
        {
            if (AllData.ContainsKey(debugData.DebugDataType))
            {
                AllData[debugData.DebugDataType].DebugValue = debugData.DebugValue;
            }
            else
            {
                AllData.Add(debugData.DebugDataType, debugData);
            }
        }
    }

    public void RemoveData(DebugDataType debugDataType)
    {
        AllData.Remove(debugDataType);
    }
}

public enum DebugDataType
{    Priority_Item,
    Priority_Distance,
    Priority_Total,
}

public class DebugData : MonoBehaviour
{
    TextMeshProUGUI _dataTitle;
    public TextMeshProUGUI DataTitle 
    {
        get { return _dataTitle ??= (_dataTitle = Manager_Game.FindTransformRecursively(transform, "DataTitle").GetComponent<TextMeshProUGUI>()); }
        set { _dataTitle = value; }
    }

    public DebugDataType DebugDataType;
    public string _debugValue;
    public string DebugValue { get { return _debugValue; } set { _debugValue = value; _setName(); } }

    public void InitialiseDebugData(DebugDataType debugDataType, string debugValue)
    {
        DebugDataType = debugDataType;
        DebugValue = debugValue;
    }

    void _setName()
    {
        DataTitle.text = $"{DebugDataType} - {DebugValue}";
        name = DataTitle.text;
    }
}