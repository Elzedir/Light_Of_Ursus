using System.Collections;
using UnityEngine;

public class Lux : MonoBehaviour
{
    Light fireflyLight;

    Controller_Agent _agent;

    [SerializeField] float _minLightIntensity = 0.6f;
    [SerializeField] float _maxLightIntensity = 0.9f;
    float _duration = 2f;

    private float timer = 0f;

    void Start()
    {
        fireflyLight = GetComponent<Light>();
        _agent = gameObject.AddComponent<Controller_Agent>();
        _subscribeToEvents();
        StartCoroutine(_testWanderUrsus());
    }

    IEnumerator _testWanderUrsus()
    {
        yield return new WaitForSeconds(2);

        WanderAroundUrsus();
    }

    void _subscribeToEvents()
    {
        Manager_Dialogue.Instance.luxIntroEvent?.AddListener(LuxIntro);
    }

    void Update()
    {
        _flicker();
    }

    void _flicker()
    {
        timer += Time.deltaTime;

        float lerpRatio = (Mathf.Sin(timer / _duration * Mathf.PI * 2f) + 1f) / 2f;

        fireflyLight.intensity = Mathf.Lerp(_minLightIntensity, _maxLightIntensity, lerpRatio);
        //fireflyLight.shadowRadius = Mathf.Lerp(_max, _min, lerpRatio);

        if (timer > _duration) timer = 0f;
    }

    void OnDestroy()
    {
        Manager_Dialogue.Instance.luxIntroEvent?.RemoveListener(LuxIntro);
    }

    void LuxIntro()
    {
        WanderAroundUrsus();
    }

    void WanderAroundUrsus()
    {
        //Debug.Log("Wander around Ursus called");
        //_agent.SetAgentDetails(targetGO: Manager_Game.Instance.Player.gameObject, speed: 5);

        VoxelGrid.InitialiseVoxelGridTest();
    }
}
