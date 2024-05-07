using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] string _saveGameName = "";

    [Header("Content")]
    [SerializeField] GameData _saveGameData;
    
    public bool HasData { get; private set; } = false;

    Button _profileIDButton;
    TextMeshProUGUI _profileIDText;
    Button _clearSaveButton;

    public void InitialiseSaveSlot(string saveGameName, GameData gameData, string saveOrLoad, UnityAction saveGameAction, UnityAction loadGameAction, UnityAction clearSaveAction)
    {
        if (!_profileIDButton) _profileIDButton = Manager_Game.FindTransformRecursively(transform, "ProfileIDButton").gameObject.GetComponent<Button>();
        if (!_profileIDText) _profileIDText = Manager_Game.FindTransformRecursively(transform, "ProfileID").gameObject.GetComponent<TextMeshProUGUI>();
        if (!_clearSaveButton) _clearSaveButton = Manager_Game.FindTransformRecursively(transform, "ClearSaveButton").gameObject.GetComponent<Button>();

        _saveGameName = saveGameName;
        _profileIDText.text = saveGameName;
        if (gameData != null) { _saveGameData = gameData; HasData = true; }
        else _saveGameData = new GameData(_saveGameData.CurrentProfileName);

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

    public string GetSaveGameName()
    {
        return _saveGameName;
    }
}