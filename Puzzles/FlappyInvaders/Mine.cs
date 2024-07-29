using UnityEngine;

public class Mine : MonoBehaviour
{
    Vector3 _move;
    float _speed;
    float _reloadTime;
    float _shootInterval = 3;

    Transform _bulletParent;
    
    public void Initialise(Transform bulletParent, Mesh mesh, Material material, float speed)
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
        gameObject.AddComponent<BoxCollider>();
        Rigidbody mineBody = gameObject.AddComponent<Rigidbody>();
        mineBody.mass = 1000;
        mineBody.useGravity = false;

        _speed = speed;
        _bulletParent = bulletParent;
    }

    void Update()
    {
        if (_move == Vector3.zero) _move = Vector3.left; transform.position += (_move.normalized * _speed * UnityEngine.Time.deltaTime);

        if (_reloadTime >= _shootInterval)
        {
            Shoot();

            _reloadTime = 0;
        }

        _reloadTime += UnityEngine.Time.deltaTime;

        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("1"); if (collision.gameObject.GetComponent<Bullet>()) DestroyMine();
    }

    void Shoot()
    {
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.transform.parent = _bulletParent;
        Bullet bullet = bulletGO.AddComponent<Bullet>();
        bullet.Initialise(
            Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
            Resources.Load<Material>("Materials/Material_Green"),
            Vector3.left, 
            transform.position
            );
    }

    void DestroyMine()
    {
        Destroy(gameObject);
    }
}
