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
    public Vector3 ShakeAmount = new Vector3(1f, 1f, 0);
    public float ShakeDuration;
    public float ShakeSpeed;
    protected Camera _camera;
    public bool IsCoroutineRunning { get; private set; } = false;

    [SerializeField] float _smoothTime = 0.1f;
    [SerializeField] Vector3 _offsetPosition = new Vector3(0, 2, -4);
    //[SerializeField] Quaternion _offsetRotation;
    [SerializeField] float _xMouseSensitivity = 500f;
    [SerializeField] float _yMouseSensitivity = 50f;
    [SerializeField] float _orbitRadius = 5f;
    [SerializeField] Quaternion _targetRotation;
    Vector3 _velocity = Vector3.one;

    float _yaw;
    float _pitch;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _camera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetOffset(Vector3 position, Quaternion rotation)
    {
        _offsetPosition = position;
        // _targetRotation = rotation;
    }

    void Update()
    {
        if (Manager_Game.Instance.CurrentState == GameState.Cinematic && _lookAt != null) _lookAt = null;

        //if (Manager_Game.Instance.CurrentState == GameState.Playing)
        //{
        //    if ((_player == null || _lookAt == null || _player.gameObject != Manager_Game.Instance.Player.gameObject))
        //    {
        //        if (Manager_Game.Instance.Player == null) Manager_Game.Instance.SetPlayer();
        //        _player = Manager_Game.Instance.Player;
        //        _lookAt = _player.transform;
        //    }

        //    _handleCameraRotation();
        //}
        
        if (Manager_Game.Instance.CurrentState == GameState.Puzzle)
        {
            if (_lookAt == null) _lookAt = GameObject.Find("Focus").transform;
        }

        if (_lookAt != null && PlayerCameraEnabled)
        {
            transform.position = Vector3.SmoothDamp(transform.position, CalculateCameraPosition(), ref _velocity, _smoothTime);

            transform.LookAt(_lookAt);

            //transform.position = Vector3.SmoothDamp(transform.position, _lookAt.position + _lookAt.TransformDirection(_offsetPosition), ref _velocity, _smoothTime);
            //transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _smoothTime);
        }

        if (_shakeTime > 0)
        {
            _shakeTime -= UnityEngine.Time.deltaTime;

            if (_shakeTime > 0)
            {
                _nextPos = (Mathf.PerlinNoise(_shakeTime * ShakeSpeed, _shakeTime * ShakeSpeed * 2) - 0.5f) * ShakeAmount.x * transform.right * Curve.Evaluate(1f - _shakeTime / ShakeDuration) +
                              (Mathf.PerlinNoise(_shakeTime * ShakeSpeed * 2, _shakeTime * ShakeSpeed) - 0.5f) * ShakeAmount.y * transform.up * Curve.Evaluate(1f - _shakeTime / ShakeDuration);

                _nextFoV = (Mathf.PerlinNoise(_shakeTime * ShakeSpeed * 2, _shakeTime * ShakeSpeed * 2) - 0.5f) * ShakeAmount.z * Curve.Evaluate(1f - _shakeTime / ShakeDuration);

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

    void _handleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * _xMouseSensitivity * UnityEngine.Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _yMouseSensitivity * UnityEngine.Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -35, 60);

        _targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _smoothTime);
    }

    Vector3 CalculateCameraPosition()
    {
        Vector3 direction = new Vector3(0, 0, -_orbitRadius);
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        return _lookAt.position + rotation * direction;
    }

    public void ManualMove(Vector2 direction)
    {
        float moveSpeed = 5.0f;
        Vector3 move = new Vector3(direction.x, direction.y, 0) * moveSpeed * UnityEngine.Time.unscaledDeltaTime;
        transform.position += move;
    }

    public void ShakeOnce(float duration = 0.5f, float speed = 5f, Vector3? amount = null, Camera camera = null, bool deltaMovement = true, AnimationCurve curve = null)
    {
        _originalPosition = transform.position;
        ShakeDuration = duration;
        ShakeSpeed = speed;
        if (amount != null)
            ShakeAmount = (Vector3)amount;
        if (curve != null)
            Curve = curve;
        DeltaMovement = deltaMovement;

        ResetCameraShake();
        _shakeTime = ShakeDuration;
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
        float startTime = UnityEngine.Time.time;
        Vector3 startPosition = _camera.transform.position;
        Quaternion startRotation = _camera.transform.rotation;

        while (UnityEngine.Time.time < startTime + duration)
        {
            float t = (UnityEngine.Time.time - startTime) / duration;
            _camera.transform.position = Vector3.Lerp(startPosition, newPosition, t);
            _camera.transform.rotation = Quaternion.Lerp(startRotation, newRotation, t);
            yield return null;
        }

        _camera.transform.position = newPosition;
        _camera.transform.rotation = newRotation;

        IsCoroutineRunning = false;
    }
}
