using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Actor;
using Actors;
using Managers;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Interactable_Puzzle : MonoBehaviour, IInteractable
{
    public PuzzleSet PuzzleSet;
    public List<IceWallType> IceWallTypes;
    public MazeType MazeType;
    public PuzzleData PuzzleData;

    BoxCollider2D _collider;

    public float InteractRange {  get; private set; }

    protected virtual void Awake()
    {
        // Load from data, don't assign on awake. if (PuzzleData.PuzzleID == 0) PuzzleData.PuzzleID = load;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void SetInteractRange(float interactRange)
    {
        InteractRange = interactRange;
    }

    public bool WithinInteractRange(Actor_Component interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }

    public IEnumerator Interact(Actor_Component actor)
    {
        if (actor.TryGetComponent(out Player player) && !PuzzleData.PuzzleState.PuzzleCompleted)
        { 
            Manager_Game.S_Instance.LoadScene("Puzzle", this); 
        }

        yield break;
    }

    public void CompletePuzzle()
    {
        if (PuzzleData.PuzzleState.PuzzleRepeatable) return;

        PuzzleData.PuzzleState.PuzzleCompleted = true;
        GetComponent<BoxCollider2D>().isTrigger = true;
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Blue ground");

        switch (PuzzleSet)
        {
            case PuzzleSet.Directional:
                break;
            default:
                break;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Interactable_Puzzle))]
[CanEditMultipleObjects]
public class PuzzleDataEditor : Editor
{
    SerializedProperty _puzzleData;
    SerializedProperty _puzzleSet;

    void OnEnable()
    {
        _puzzleData = serializedObject.FindProperty("PuzzleData");
        _puzzleSet = serializedObject.FindProperty("PuzzleSet");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_puzzleSet, false);

        if (_puzzleSet.enumValueIndex != (int)PuzzleSet.None)
        {
            if (_puzzleSet.enumValueIndex == (int)PuzzleSet.IceWall)
            {
                SerializedProperty iceWallTypes = serializedObject.FindProperty("IceWallTypes");
                EditorGUILayout.PropertyField(iceWallTypes, true);

                if (iceWallTypes.isArray && iceWallTypes.arraySize > 0)
                {
                    
                    EditorGUILayout.PropertyField(_puzzleData.FindPropertyRelative("IceWallData"), true);
                }
            }

            // When puzzle stlye is selected, then you show these.
            EditorGUILayout.PropertyField(_puzzleData.FindPropertyRelative("PuzzleObjectives"), true);
            EditorGUILayout.PropertyField(_puzzleData.FindPropertyRelative("PuzzleState"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif