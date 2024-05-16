using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Light : MonoBehaviour
{
    Light _light;
    MeshCollider _meshCollider;
    bool _aimLight = false;
    float _meshLength = 20.0f;

    void Awake()
    {
        _getLight();
        _getCollider();
    }

    void _getLight()
    {
        _light = GetComponent<Light>();
    }

    void _getCollider() 
    {
        _meshCollider = GetComponentInChildren<MeshCollider>();
        _meshCollider.sharedMesh = _createPyramidMesh();
    }

    Mesh _createPyramidMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),   // Top vertex at the origin
            new Vector3(-5, -1, _meshLength),  // Bottom front-left
            new Vector3(5, -1, _meshLength),   // Bottom front-right
            new Vector3(5, 1, _meshLength),  // Bottom back-right
            new Vector3(-5, 1, _meshLength)  // Bottom back-left
        };

        // Define the triangles of the pyramid
        int[] triangles = new int[]
        {
            0, 1, 2,  // Front face
            0, 2, 3,  // Right face
            0, 3, 4,  // Back face
            0, 4, 1,  // Left face
            1, 4, 3,  // Bottom face (part 1)
            1, 3, 2   // Bottom face (part 2)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Calculate normals for lighting
        mesh.RecalculateNormals();

        return mesh;
    }

    void Update()
    {
        if (_aimLight)
        {
            _moveLightWithMouse();
        }
    }

    void _moveLightWithMouse()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void AimLight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _aimLight = true;
        }
        else if (context.canceled)
        {
            _aimLight = false;
            _resetLight();
        }
    }

    void _resetLight()
    {
        transform.localRotation = Quaternion.identity;
    }

    void OnDrawGizmos()
    {
        if (_meshCollider == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireMesh(_meshCollider.sharedMesh, transform.position, transform.rotation, transform.localScale);
    }
}
