using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Controller_Camera : MonoBehaviour
{
    public bool PlayerCameraEnabled = true;

    public static Controller_Camera Instance;
    Transform _lookAt;
    public Player _player;
    public float boundX = 0.15f;
    public float boundY = 0.05f;
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    protected Vector3 _originalPosition;
    protected Vector3 _lastPos;
    protected Vector3 _nextPos;
    protected float _lastFoV;
    protected float _nextFoV;
    protected float _shakeTime;
    public bool DeltaMovement = true;
    public Vector3 Amount = new Vector3(1f, 1f, 0);
    public float Duration;
    public float Speed;
    protected Camera _camera;
    public bool IsCoroutineRunning { get; private set; } = false;

    [SerializeField] float _smoothTime = 0.3f;
    [SerializeField] Vector3 _offsetPosition;
    [SerializeField] Quaternion _targetRotation;
    Vector3 _velocity = Vector3.one;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _camera = GetComponent<Camera>();
    }

    public void SetOffset(Vector3 position, Quaternion rotation)
    {
        
            _offsetPosition = position;
            _targetRotation = rotation;
        
    }

    private void LateUpdate()
    {
        if (Manager_Game.Instance.CurrentState == GameState.Cinematic && _lookAt != null) _lookAt = null;

        if (Manager_Game.Instance.CurrentState == GameState.Playing)
        {
            if ((_player == null || _lookAt == null || _player.gameObject != Manager_Game.Instance.Player.gameObject))
            {
                if (Manager_Game.Instance.Player == null) Manager_Game.Instance.SetPlayer();
                _player = Manager_Game.Instance.Player;
                _lookAt = _player.transform;
            }
        }
        
        if (Manager_Game.Instance.CurrentState == GameState.Puzzle)
        {
            if (_lookAt == null) _lookAt = GameObject.Find("Focus").transform;
        }

        if (_lookAt != null && PlayerCameraEnabled)
        {
            //Vector3 delta = Vector3.zero;

            //float deltaX = _lookAt.position.x - transform.position.x;

            //if (deltaX > boundX || deltaX < -boundX)
            //{
            //    if (transform.position.x < _lookAt.position.x)
            //    {
            //        delta.x = deltaX - boundX;
            //    }
            //    else
            //    {
            //        delta.x = deltaX + boundX;
            //    }
            //}

            //float deltaY = _lookAt.position.y - transform.position.y;

            //if (deltaY > boundY || deltaY < -boundY)
            //{
            //    if (transform.position.y < _lookAt.position.y)
            //    {
            //        delta.y = deltaY - boundY;
            //    }
            //    else
            //    {
            //        delta.y = deltaY + boundY;
            //    }
            //}

            //transform.position += new Vector3(delta.x, delta.y, 0);

            transform.position = Vector3.SmoothDamp(transform.position, _lookAt.position + _offsetPosition, ref _velocity, _smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _smoothTime);
        }

        if (_shakeTime > 0)
        {
            _shakeTime -= Time.deltaTime;

            if (_shakeTime > 0)
            {
                _nextPos = (Mathf.PerlinNoise(_shakeTime * Speed, _shakeTime * Speed * 2) - 0.5f) * Amount.x * transform.right * Curve.Evaluate(1f - _shakeTime / Duration) +
                              (Mathf.PerlinNoise(_shakeTime * Speed * 2, _shakeTime * Speed) - 0.5f) * Amount.y * transform.up * Curve.Evaluate(1f - _shakeTime / Duration);

                _nextFoV = (Mathf.PerlinNoise(_shakeTime * Speed * 2, _shakeTime * Speed * 2) - 0.5f) * Amount.z * Curve.Evaluate(1f - _shakeTime / Duration);

                _camera.fieldOfView += (_nextFoV - _lastFoV);
                transform.Translate(DeltaMovement ? (_nextPos - _lastPos) : _nextPos);
                transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

                _lastPos = _nextPos;
                _lastFoV = _nextFoV;
            }
            else
            {
                ResetCameraShake();
            }
        }
    }

    public void ManualMove(Vector2 direction)
    {
        float moveSpeed = 5.0f;
        Vector3 move = new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.unscaledDeltaTime;
        transform.position += move;
    }

    public void ShakeOnce(float duration = 0.5f, float speed = 5f, Vector3? amount = null, Camera camera = null, bool deltaMovement = true, AnimationCurve curve = null)
    {
        _originalPosition = transform.position;
        Duration = duration;
        Speed = speed;
        if (amount != null)
            Amount = (Vector3)amount;
        if (curve != null)
            Curve = curve;
        DeltaMovement = deltaMovement;

        ResetCameraShake();
        _shakeTime = Duration;
    }

    private void ResetCameraShake()
    {
        transform.Translate(DeltaMovement ? _lastPos : _originalPosition);
        _camera.fieldOfView -= _lastFoV;

        _lastPos = _nextPos = _originalPosition;
        _lastFoV = _nextFoV = 0f;
    }

    IEnumerator RotateCamera(Vector3 newPosition, Quaternion newRotation, float duration)
    {
        float startTime = Time.time;
        Vector3 startPosition = _camera.transform.position;
        Quaternion startRotation = _camera.transform.rotation;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            _camera.transform.position = Vector3.Lerp(startPosition, newPosition, t);
            _camera.transform.rotation = Quaternion.Lerp(startRotation, newRotation, t);
            yield return null;
        }

        _camera.transform.position = newPosition;
        _camera.transform.rotation = newRotation;

        IsCoroutineRunning = false;
    }
}
