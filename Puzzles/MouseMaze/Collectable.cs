using UnityEngine;

public class Collectable : MonoBehaviour
{
    Spawner_Maze _spawner;
    BoxCollider _collider;
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;

    public void SpawnCollectable(Spawner_Maze spawner, Mesh mesh, Material material)
    {
        _spawner = spawner;

        _collider = gameObject.AddComponent<BoxCollider>();
        _collider.isTrigger = true; 
        _collider.size = new Vector2(0.4f, 0.4f);

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = mesh;
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = material;

        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name != "Focus") return;
        _spawner.CollectableCollected(this);
    }
}
