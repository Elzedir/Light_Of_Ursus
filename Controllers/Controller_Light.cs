using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Light : MonoBehaviour
{
    Light _light;
    bool _aimLight = false;
    float _meshLength = 20.0f;
    Collider[] _targetsInRange = new Collider[100];
    List<Discoverable> _discoveredObjects = new List<Discoverable>();

    void Awake()
    {
        _getLight();
    }

    void _getLight()
    {
        _light = GetComponent<Light>();
    }

    void FixedUpdate()
    {
        if (_aimLight)
        {
            _moveLightWithMouse();
        }

        CheckTargetsInLight();
    }

    void CheckTargetsInLight()
    {
        int targetsCount = Physics.OverlapSphereNonAlloc(_light.transform.position, _light.range, _targetsInRange, LayerMask.GetMask("Discoverable"));

        _discoveredObjects.Clear();

        for (int i = 0; i < targetsCount; i++)
        {
            Discoverable discoverable = _targetsInRange[i].GetComponent<Discoverable>();
            if (discoverable != null)
            {
                float lightPercentage = _calculateLightPercentage(_light, discoverable.transform);

                if (lightPercentage > 0)
                {
                    _discoveredObjects.Add(discoverable);
                    discoverable.UpdateDiscovery(lightPercentage);
                }
            }
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

    float _calculateLightPercentage(Light spotlight, Transform target)
    {
        Vector3 directionToTarget = target.position - spotlight.transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > spotlight.range)
        {
            return 0f;
        }

        float angle = Vector3.Angle(spotlight.transform.forward, directionToTarget);
        if (angle > spotlight.spotAngle / 2)
        {
            return 0f;
        }

        float distanceFactor = 1 - (distanceToTarget / spotlight.range);
        float lightIntensity = spotlight.intensity * distanceFactor;

        float maxIntensity = spotlight.intensity;
        float lightPercentage = lightIntensity / maxIntensity;

        return Mathf.Clamp01(lightPercentage);
    }
}
