using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] int _saveSlotID = 0;
    public int GetSaveSlotID() => _saveSlotID;
    [SerializeField] string _saveSlotName = "";
    public string GetSaveGameName() => _saveSlotName;

    [Header("Content")]
    [SerializeField] SaveData _saveSlotData;
    
    public bool HasData { get; private set; } = false;

    Button _profileIDButton;
    TextMeshProUGUI _profileIDText;
    Button _clearSaveButton;

    public void InitialiseSaveSlot(SaveData saveData, string saveOrLoad, UnityAction saveGameAction, UnityAction loadGameAction, UnityAction clearSaveAction)
    {
        if (!_profileIDButton) _profileIDButton = Manager_Game.FindTransformRecursively(transform, "ProfileIDButton").gameObject.GetComponent<Button>();
        if (!_profileIDText) _profileIDText = Manager_Game.FindTransformRecursively(transform, "ProfileID").gameObject.GetComponent<TextMeshProUGUI>();
        if (!_clearSaveButton) _clearSaveButton = Manager_Game.FindTransformRecursively(transform, "ClearSaveButton").gameObject.GetComponent<Button>();

        _saveSlotID = saveData.SaveDataID;
        _saveSlotName = saveData.SaveDataName;
        _profileIDText.text = saveData.ProfileName;
        
        if (saveData == null) _saveSlotData = new SaveData(_saveSlotData.ProfileID, _saveSlotData.ProfileName);
        
        _saveSlotData = saveData; 
        HasData = true;
        
        if (saveOrLoad == "Save")
        {
            _profileIDButton.onClick.AddListener(() =>
            {
                saveGameAction();
            });
        }

        else if (saveOrLoad == "Load")
        {
            _profileIDButton.onClick.AddListener(() =>
            {
                loadGameAction();
            });
        }
        
        _clearSaveButton.onClick.AddListener(() =>
        {
            clearSaveAction();
        });
    }
}