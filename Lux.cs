using UnityEngine;

public class Lux : Controller_Agent
{
    Light fireflyLight;

    [SerializeField] float _minLightIntensity = 0.6f;
    [SerializeField] float _maxLightIntensity = 0.9f;
    float _duration = 2f;

    private float timer = 0f;

    protected override void Start()
    {
        base.Start();

        fireflyLight = GetComponent<Light>();
    }

    protected override void SubscribeToEvents()
    {
        Manager_Dialogue.Instance.luxIntroEvent?.AddListener(LuxIntro);
    }

    protected override void Update()
    {
        base.Update();

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
        SetAgentDetails(targetGO: Manager_Game.Instance.Player.gameObject);
    }
}
