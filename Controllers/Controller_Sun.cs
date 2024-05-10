using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Sun : MonoBehaviour
{
    public static Controller_Sun Instance;
    Light _light;

    void Awake()
    {
        Instance = this;
        _light = GetComponent<Light>();
    }

    public void SetLightPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
