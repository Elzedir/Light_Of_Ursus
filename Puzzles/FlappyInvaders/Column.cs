using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Column : MonoBehaviour
{
    public Vector3 Move;
    public float Speed;

    public void Initialise(float columnHeight, Transform spawner, Mesh mesh, Material material,  float speed)
    {
        transform.parent = spawner.transform;
        transform.localPosition = new Vector3(0, spawner.localPosition.y + columnHeight / 2, 0);
        transform.localScale = new Vector3(1, Mathf.Abs(columnHeight), 1);

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
        gameObject.AddComponent<BoxCollider>();

        //Rigidbody columnBody = gameObject.AddComponent<Rigidbody>();
        //columnBody.useGravity = false;
        //columnBody.mass = 1000;

        Speed = speed;
    }

    void Update()
    {
        if (Move == Vector3.zero) Move = Vector3.left; transform.position += (Move.normalized * Speed * Time.deltaTime);
    }

    public void DestroyColumn()
    {
        Destroy(gameObject);
    }
}
