using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using z_Abandoned;

public class Lux : MonoBehaviour
{
    Light fireflyLight;

    Controller_Agent _agent;

    [SerializeField] float _minLightIntensity = 0.6f;
    [SerializeField] float _maxLightIntensity = 0.9f;
    float _duration = 2f;

    private float timer = 0f;
    Collider _collider;

    [SerializeField] int _pathID = -1;
    [SerializeField] int _currentPathIndex = -1;
    [SerializeField] List<Vector3> _path = new();
    [SerializeField] float _distance = 0;
    
    void Start()
    {
        fireflyLight = GetComponent<Light>();
        _agent = gameObject.AddComponent<Controller_Agent>();
        _collider = gameObject.GetComponent<Collider>();
        _subscribeToEvents();
        StartCoroutine(_testWanderUrsus());
    }

    IEnumerator _testWanderUrsus()
    {
        yield return new WaitForSeconds(0.5f);

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
        timer += UnityEngine.Time.deltaTime;

        float lerpRatio = (Mathf.Sin(timer / _duration * Mathf.PI * 2f) + 1f) / 2f;

        fireflyLight.intensity = Mathf.Lerp(_minLightIntensity, _maxLightIntensity, lerpRatio);
        //fireflyLight.shadowRadius = Mathf.Lerp(_max, _min, lerpRatio);

        if (timer > _duration) timer = 0f;
    }

    void OnDestroy()
    {
        //Manager_Dialogue.Instance.luxIntroEvent?.RemoveListener(LuxIntro);
    }

    void LuxIntro()
    {
        WanderAroundUrsus();
    }

    void WanderAroundUrsus()
    {
        //Debug.Log("Wander around Ursus called");
        //_agent.SetAgentDetails(targetGO: Manager_Game.Instance.Player.gameObject, speed: 5);

        VoxelGrid_Deprecated.InitialiseVoxelGridTest();
        //_agent.SetAgentDetails(new List<MoverType> { MoverType.Ground }, targetGO: Manager_Game.Instance.Player.gameObject, speed: 1f, lux: this);
    }

    public void TestPath(List<(Vector3 position, Collider)> path, float distance, int pathID = -1)
    {
        _path.Clear();

        foreach (var point in path)
        {
            _path.Add(point.position);
        }

        _distance = distance;
        _pathID = pathID;
    }

    public void TestPathIndex(int currentPathIndex)
    {
        _currentPathIndex = currentPathIndex;
    }
}
