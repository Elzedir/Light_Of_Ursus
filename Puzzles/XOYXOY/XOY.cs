using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XOY : MonoBehaviour
{
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    public Spawner_XOY Spawner;
    public int CurrentSpriteIndex;

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<Controller_Puzzle_XOY>()) ChangeXOY();
    }

    void ChangeXOY()
    {
        if (_meshRenderer == null) { Debug.Log("SpriteRenderer is null;"); return; }

        CurrentSpriteIndex = (CurrentSpriteIndex + 1) % Spawner.XoyMaterials.Length;

        _meshRenderer.material = Spawner.XoyMaterials[CurrentSpriteIndex];
        
        switch (CurrentSpriteIndex)
        {
            case 0:
                transform.parent = Spawner.XParent;
                break;
            case 1:
                transform.parent = Spawner.OParent;
                break;
            case 2:
                transform.parent = Spawner.YParent;
                break;
        }
    }
}
