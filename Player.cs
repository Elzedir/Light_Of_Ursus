using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : Controller, IDataPersistence
{
    public float SpeedIncrease;
    BoxCollider2D _coll;
    Animator _animator;
    bool _moved;
    RaycastHit2D _hit;
    public Interactable ClosestInteractableObject; //public Interactable ClosestInteractableObject { get { return _closestInteractableObject; } }
    List<Interactable> _interactableObjects = new();
    [SerializeField] bool _hasStaff;
    bool _strafe = false;
    Vector2 _move;

    public BoxCollider2D _fireflyWanderZone; public BoxCollider2D FireflyWanderZone { get { return _fireflyWanderZone; } }

    public void Start()
    {
        _coll = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SaveData(GameData data)
    {
        data.PlayerPosition = transform.position;
    }

    public void LoadData(GameData data)
    {
        //transform.position = data.PlayerPosition;
    }

    protected override void FixedUpdate()
    {
        _moved = false;
        base.FixedUpdate();

        PlayerMove();

        if (_hasStaff != Manager_Game.Instance.PlayerHasStaff) { _hasStaff = Manager_Game.Instance.PlayerHasStaff; _animator.SetBool("HasStaff", _hasStaff); }

        if (Manager_Game.Instance.CurrentState != GameState.Playing) return;

        // if (!_moved) _animator.SetFloat("Speed", 0);
        TargetCheck();
    }

    public void OnPlayerChange()
    {
        //_playerActor = GameManager.Instance.Player.PlayerActor;
    }

    public virtual void PlayerMove()
    {
        _moved = true;
        //Vector3 move = new Vector3(x, y, 0).normalized * SpeedIncrease;
        //transform.localScale = new Vector3(Mathf.Sign(move.x), 1, 1);
        //_playerActor.ActorScripts.Actor_VFX.transform.localScale = new Vector3(Mathf.Sign(lookDirection.x), 1, 1);
        //transform.Translate(move.x * Time.deltaTime, move.y * Time.deltaTime, 0);

        Vector3 movement = new Vector3(_move.x, 0, _move.y);

        if (movement != Vector3.zero && !_strafe) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        else if (_strafe) _playerStrafe();

        //transform.Translate(movement * _speed * Time.deltaTime, Space.World);

        _rigidbody.velocity = new Vector3(_move.x, 0, _move.y) * _speed;

        _animator.SetFloat("Speed", _move.magnitude);

        void _playerStrafe()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Vector3 direction = (hit.point - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    public void Strafe(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _strafe = true;
        }
        else if (context.canceled)
        {
            _strafe = false;
        }
    }

    public virtual void TargetCheck()
    {
        float maxTargetDistance = 1000;
        Collider2D[] triggerHits = Physics2D.OverlapCircleAll(transform.position, 1000);

        foreach (Collider2D hit in triggerHits)
        {
            if (hit.gameObject == null) continue;

            if (hit.gameObject.TryGetComponent<Interactable>(out Interactable interactable))_interactableObjects.Add(interactable);
        }

        foreach (Interactable interactable in _interactableObjects)
        {
            if (interactable != null)
            {
                float targetDistance = Vector3.Distance(transform.position, interactable.transform.position);

                if (targetDistance < maxTargetDistance)
                {
                    maxTargetDistance = targetDistance;
                    ClosestInteractableObject = interactable;
                }
            }
        }
    }

    public IEnumerator PickUpStaffAction()
    {
        _animator.SetTrigger("PickupStaff");

        yield return new WaitForSeconds(2);

        _animator.SetBool("HasStaff", _hasStaff);
    }
}
