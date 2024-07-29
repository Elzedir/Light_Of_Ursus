using System.Collections;
using UnityEngine;

public enum DiscoverState { Undiscovered, Discovered, Revealed }
public enum DiscoverStyle { Time, Pattern }

public class Discoverable : MonoBehaviour
{
    [SerializeField] DiscoverState _discoveredState = DiscoverState.Undiscovered;
    [SerializeField] DiscoverStyle _discoveredStyle = DiscoverStyle.Time;

    Renderer _objectRenderer;
    Coroutine _discoveryCoroutine;
    MaterialPropertyBlock _propertyBlock;

    bool _inLight = false;
    Coroutine _outOfLightTimer;
    [SerializeField] [Range(0, 1)] float _discoverProgress = 0f;

    void Start()
    {
        _objectRenderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        _setAlpha();
    }

    void Update()
    {
        if (_discoverProgress > 0 && _discoveredState != DiscoverState.Undiscovered && !_inLight)
        {
            switch(_discoveredStyle)
            {
                case DiscoverStyle.Time:
                    ResetDiscoveryTime();
                    break;
            }
        }
    }

    void ResetDiscoveryTime()
    {
        _discoverProgress -= UnityEngine.Time.deltaTime;

        if (_discoverProgress <= 0)
        {
            _discoverProgress = 0;
            _discoveredState = DiscoverState.Undiscovered;
            _setAlpha();
        }
        else
        {
            _setAlpha();
        }
    }

    public void UpdateDiscovery(float lightPercentage)
    {
        if (_discoverProgress >= 1 && _discoveredState == DiscoverState.Revealed) return;

        _inLight = true;

        if (_discoveredState == DiscoverState.Undiscovered) _discoveredState = DiscoverState.Discovered;

        _discoverProgress += lightPercentage * UnityEngine.Time.deltaTime;

        if (_discoverProgress >= 1)
        {
            _discoverProgress = 1;
            _discoveredState = DiscoverState.Revealed;
        }

        _setAlpha();

        if (_outOfLightTimer != null) { StopCoroutine(_outOfLightTimer); }
        _outOfLightTimer = StartCoroutine(_startOutOfLightTimer());
    }

    IEnumerator _startOutOfLightTimer()
    {
        yield return new WaitForSeconds(0.5f);
        _inLight = false;
    }

    void _setAlpha()
    {
        _propertyBlock.SetFloat("_Alpha", _discoverProgress);
        _objectRenderer.SetPropertyBlock(_propertyBlock);
    }
}
