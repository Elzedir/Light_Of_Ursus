using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    TextMeshProUGUI _scoreText;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            _scoreText = child.GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        Manager_Puzzle.Instance.OnAddScore += AddScore;
    }

    void OnDestroy()
    {
        Manager_Puzzle.Instance.OnAddScore -= AddScore;
    }

    public void AddScore(string score)
    {
        Debug.Log("Added Score");
        _scoreText.text = score;
    }
}
