using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell_Base : MonoBehaviour
{
    protected MeshFilter _meshFilter;
    protected Mesh _previousMesh;
    protected MeshRenderer _meshRenderer;
    protected BoxCollider _boxCollider;
    public Vector3Int Position { get; protected set; }

    public virtual void Show()
    {
        _meshRenderer.enabled = true;
    }

    public virtual void Hide()
    {
        _meshRenderer.enabled = false;
    }

    public virtual void Enable()
    {
        _meshFilter.mesh = _previousMesh;
    }

    public virtual void Disable()
    {
        _previousMesh = _meshFilter.mesh;
        _meshFilter.mesh = null;
    }

    public virtual void MarkCell(Material material)
    {
        _meshRenderer.material = material;
    }
}
