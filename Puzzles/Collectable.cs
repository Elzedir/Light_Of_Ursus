using UnityEngine;

public class Collectable : MonoBehaviour
{
    protected Spawner_Maze _spawner;
    protected BoxCollider _collider;
    protected MeshFilter _meshFilter;
    protected MeshRenderer _meshRenderer;

    public virtual void SpawnCollectable(Spawner_Maze spawner, Mesh mesh, Material material)
    {
        _spawner = spawner;

        _collider = gameObject.AddComponent<BoxCollider>();
        _collider.isTrigger = true;

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = mesh;
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = material;

        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    protected virtual void OnTriggerEnter(Collider collision)
    {

    }
}
