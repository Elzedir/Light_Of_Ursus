using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using DataPersistence;
using Managers;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Spawner : MonoBehaviour
{
    public static Manager_Spawner Instance;
    List<GameObject> _spawners = new();
    public GameObject PuzzleSpawner { get; private set; }

    public delegate void OnPuzzleStatesRestoredDelegate();
    public static event OnPuzzleStatesRestoredDelegate OnPuzzleStatesRestored;

    void Awake()
    {
        Instance = this;
    }

    public void OnSceneLoaded()
    {

    }

    void Update()
    {
        _refreshSongParts();
    }

    public void SaveData(SaveData data)
    {
        // AssignSpawners();

        // if (!PuzzleSpawner) return;

        // foreach (Transform child in PuzzleSpawner.transform)
        // {
        //     if (child.TryGetComponent<Interactable_Puzzle>(out Interactable_Puzzle puzzle))
        //     {
        //         data.PuzzleSaveData[puzzle.PuzzleData.PuzzleID] = JsonConvert.SerializeObject(puzzle.PuzzleData.PuzzleState, Formatting.None);
        //     }
        // }
    }

    public void LoadData(SaveData data)
    {
        StartCoroutine(OnLoadNumerator(data));
    }

    IEnumerator OnLoadNumerator(SaveData data)
    {
        yield return new WaitForSeconds(0.1f);

        AssignSpawners();

        RestorePuzzleStates(data);
    }

    void AssignSpawners()
    {
        GameObject spawnersParent = GameObject.Find("Spawners");

        if (spawnersParent == null) return;

        foreach (Transform child in spawnersParent.transform)
        {
            if (!_spawners.Contains(child.gameObject)) _spawners.Add(child.gameObject);

            if (child.gameObject.name == "PuzzleSpawner") PuzzleSpawner = child.gameObject;
        }
    }

    public void RestorePuzzleStates(SaveData data)
    {
        if (PuzzleSpawner == null) { Debug.Log("Puzzle Spawner not present in scene."); return; }

        foreach (Transform child in PuzzleSpawner.transform)
        {
            if (child.TryGetComponent(out Interactable_Puzzle puzzle))
            {
                puzzle.PuzzleData = new PuzzleData(data.PuzzleData[puzzle.PuzzleData.PuzzleID]);

                if (puzzle.PuzzleData.PuzzleState.PuzzleCompleted) puzzle.CompletePuzzle();

            }
        }

        OnPuzzleStatesRestored?.Invoke();
    }
    public void CompletePuzzle(int puzzleID)
    {
        //Manager_Puzzle.GetPuzzle(puzzleID).PuzzleData.PuzzleState.PuzzleCompleted = true;
        //GameObject.Find(puzzleID).GetComponent<Interactable_Puzzle>().CompletePuzzle();
    }

    void _refreshSongParts()
    {
        if (Manager_Game.Instance.SceneName == "Main_Menu" || Manager_Game.Instance.SceneName == "Puzzle") return;

        if (PuzzleSpawner == null) return;

        int i = 0;

        foreach (Transform child in PuzzleSpawner.transform)
        {
            if (child.GetComponent<Interactable_Puzzle>().PuzzleData.PuzzleState.PuzzleCompleted)
            {
                Manager_Game.Instance.Manager_Audio.LocalParameters[i].SetValue(1);
            }

            i++;
        }
    }
}
