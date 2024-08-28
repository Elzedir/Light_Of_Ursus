using System;
using UnityEngine;

[Serializable]
public class PuzzleData
{
    public PuzzleSet PuzzleSet;
    public int PuzzleID;
    public PuzzleState PuzzleState;
    public PuzzleObjectives PuzzleObjectives;
    public IceWallData IceWallData;

    public PuzzleData
        (
        int puzzleID,
        PuzzleSet puzzleSet,
        PuzzleState puzzleState,
        PuzzleObjectives puzzleObjectives,
        IceWallData iceWallData
        )
    {
        PuzzleID = puzzleID;
        PuzzleSet = puzzleSet;
        PuzzleState = puzzleState;
        PuzzleObjectives = puzzleObjectives;
        IceWallData = iceWallData;
    }

    public PuzzleData(PuzzleData puzzleData)
    {
        PuzzleID = puzzleData.PuzzleID;
        PuzzleSet = puzzleData.PuzzleSet;
        PuzzleState = new PuzzleState(puzzleData.PuzzleState);
        PuzzleObjectives = new PuzzleObjectives(puzzleData.PuzzleObjectives);
        IceWallData = new IceWallData(puzzleData.IceWallData);
    }
}

[Serializable]
public class PuzzleState
{
    public PuzzleType PuzzleType;
    public bool PuzzleRepeatable = false;
    public bool PuzzleCompleted;

    public PuzzleState
        (
        PuzzleType puzzleType,
        bool puzzleRepeatable,
        bool puzzleCompleted
        )
    {
        PuzzleType = puzzleType;
        PuzzleRepeatable = puzzleRepeatable;
        PuzzleCompleted = puzzleCompleted;
    }

    public PuzzleState(PuzzleState puzzleState)
    {
        PuzzleType = puzzleState.PuzzleType;
        PuzzleRepeatable = puzzleState.PuzzleRepeatable;
        PuzzleCompleted = puzzleState.PuzzleCompleted;
    }
}

[Serializable]
public class PuzzleObjectives
{
    [Range(0, 9)] public int PuzzleDifficulty;
    public bool PuzzleObjective;
    public float PuzzleDuration;
    public float PuzzleScore;

    public PuzzleObjectives
        (
        bool puzzleObjective,
        float puzzleDuration,
        float puzzleScore
        )
    {
        PuzzleObjective = puzzleObjective;
        PuzzleDuration = puzzleDuration;
        PuzzleScore = puzzleScore;
    }

    public PuzzleObjectives(PuzzleObjectives puzzleObjectives)
    {
        PuzzleObjective = puzzleObjectives.PuzzleObjective;
        PuzzleDuration = puzzleObjectives.PuzzleDuration;
        PuzzleScore = puzzleObjectives.PuzzleScore;
    }
}

[Serializable]
public class IceWallData
{
    [Range(0, 50)] public int Width = 10;
    [Range(0, 50)] public int Height = 2;
    [Range(0, 50)] public int Depth = 10;
    public Vector3Int StartPosition = new Vector3Int(0, 1, 0);
    public int CellHealthMin = 5;
    public int CellHealthMax = 20;
    public int PlayerExtraStaminaPercentage = 10;

    public IceWallData
        (
        int width,
        int height,
        int depth,
        Vector3Int startPosition,
        int cellHealthMin,
        int cellHealthMax,
        int playerExtraStaminaPercentage
        )
    {
        Width = width;
        Height = height;
        Depth = depth;
        StartPosition = startPosition;
        CellHealthMin = cellHealthMin;
        CellHealthMax = cellHealthMax;
        PlayerExtraStaminaPercentage = playerExtraStaminaPercentage;
    }

    public IceWallData(IceWallData iceWallData)
    {
        Width = iceWallData.Width;
        Height = iceWallData.Height;
        Depth = iceWallData.Depth;
        StartPosition = iceWallData.StartPosition;
        CellHealthMin = iceWallData.CellHealthMin;
        CellHealthMax = iceWallData.CellHealthMax;
        PlayerExtraStaminaPercentage = iceWallData.PlayerExtraStaminaPercentage;
    }

}

