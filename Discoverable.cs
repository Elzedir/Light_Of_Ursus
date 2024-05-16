using System.Collections;
using UnityEngine;

public class Discoverable : MonoBehaviour
{
    public enum DiscoverState { Undiscovered, Discovered, Revealed }
    public enum DiscoverStyle { Time, Pattern }

    [SerializeField] DiscoverState _discoveredState = DiscoverState.Undiscovered;
    [SerializeField] DiscoverStyle _discoveredStyle = DiscoverStyle.Time;

    Renderer _objectRenderer;
    Coroutine _discoveryCoroutine;
    MaterialPropertyBlock _propertyBlock;

    float _discoverProgress = 0f;

    void Start()
    {
        _objectRenderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        SetMaterial();
    }

    void Update()
    {
        if (IsLightShiningOnObject() && _discoveredState == DiscoverState.Undiscovered)
        {
            StartDiscovery();
        }
        else if (!IsLightShiningOnObject() && _discoveredState != DiscoverState.Undiscovered)
        {
            if (_discoveredStyle == DiscoverStyle.Time)
            {
                ResetDiscovery();
            }
        }
    }

    public void SetDiscoverStyle(DiscoverStyle discoverStyle)
    {
        _discoveredStyle = discoverStyle;
    }

    void ResetDiscovery()
    {
        _discoverProgress -= Time.deltaTime;

        if (_discoverProgress <= 0)
        {
            _discoveredState = DiscoverState.Undiscovered;
        }
    }

    bool IsLightShiningOnObject()
    {
        // Find a way to set true
        return false;
    }

    void StartDiscovery()
    {
        if (_discoveryCoroutine != null) StopCoroutine(_discoveryCoroutine);
        _discoveryCoroutine = StartCoroutine(DiscoverObject());
    }

    IEnumerator DiscoverObject()
    {
        _discoveredState = DiscoverState.Discovered;
        SetMaterial();

        yield return new WaitForSeconds(2f);

        _discoveredState = DiscoverState.Revealed;
        SetMaterial();
    }

    private void SetMaterial()
    {
        switch (_discoveredState)
        {
            case DiscoverState.Undiscovered:
                SetMaterialProperties(0f, 0f);
                break;
            case DiscoverState.Discovered:
                SetMaterialProperties(0.5f, 1f);
                break;
            case DiscoverState.Revealed:
                SetMaterialProperties(1f, 0f);
                break;
        }
    }

    private void SetMaterialProperties(float alpha, float twinkle)
    {
        _propertyBlock.SetFloat("_Alpha", alpha);
        _propertyBlock.SetFloat("_Twinkle", twinkle);
        _objectRenderer.SetPropertyBlock(_propertyBlock);
    }
}
