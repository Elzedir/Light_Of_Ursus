using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class Player : Controller, IDataPersistence
{
    ActorComponent _actor;
    public float SpeedIncrease;
    Collider _coll_Body;
    Animator _animator;
    Animator _weaponAnimator;
    Animation _animation;
    bool _moved;
    RaycastHit2D _hit;
    public IInteractable ClosestInteractableObject; //public Interactable ClosestInteractableObject { get { return _closestInteractableObject; } }
    List<IInteractable> _interactableObjects = new();
    [SerializeField] bool _hasStaff;
    bool _aim = false;
    Vector2 _move;
    Rigidbody _testBody;
    //bool _inAir = false;
    public BoxCollider _fireflyWanderZone; public BoxCollider FireflyWanderZone { get { return _fireflyWanderZone; } }

    public void Start()
    {
        _actor = GetComponent<ActorComponent>();
        _coll_Body = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        //_animation = GameObject.Find("TestActor").GetComponent<Animation>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        //_testBody = GameObject.Find("TestBody").GetComponent<Rigidbody>();
        // _weaponAnimator = Manager_Game.FindTransformRecursively(transform.parent, "Slot_4").GetComponent<Animator>();

        _rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SaveData(SaveData data)
    {
        //data.PlayerPosition = _rigidBody.transform.position;
    }

    public void LoadData(SaveData data)
    {
        //_rigidbody.transform.position = data.PlayerPosition;
    }

    protected void Update()
    {
        _moved = false;

        if (_moved == false) _moved = false;

        PlayerMove();

        if (_hasStaff != Manager_Game.Instance.PlayerHasStaff) { _hasStaff = Manager_Game.Instance.PlayerHasStaff; _animator.SetBool("HasStaff", _hasStaff); }

        if (Manager_Game.Instance.CurrentState != GameState.Playing) return;

        // if (!_moved) _animator.SetFloat("Speed", 0);
        TargetCheck();
    }

    public IEnumerator PlayAnimation(AnimationClip animationClip)
    {
        _animation.AddClip(animationClip, animationClip.name);
        _animation.Play(animationClip.name);

        while (_animation.isPlaying) yield return null;

        _animation.RemoveClip(animationClip);
    }

    public void OnPlayerChange()
    {
        //_playerActor = GameManager.Instance.Player.PlayerActor;
    }

    bool _movementLocked = false;

    public virtual void PlayerMove()
    {
        _moved = true;

        Vector3 movement = new Vector3(_move.x, 0, _move.y);

        //if (thirdPerson)

        if (movement != Vector3.zero)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 desiredMoveDirection = cameraForward * movement.z + cameraRight * movement.x;

            if (_aim)
            {
                _rigidBody.transform.rotation = Quaternion.Slerp(_rigidBody.transform.rotation, Quaternion.LookRotation(cameraForward), 0.15f);

                _animator.SetBool("Block", true);
            }
            else
            {
                _rigidBody.transform.rotation = Quaternion.Slerp(_rigidBody.transform.rotation, Quaternion.LookRotation(desiredMoveDirection), 0.15f);
            }

            if (!_movementLocked) _rigidBody.velocity = desiredMoveDirection * _speed;
        }

        _animator.SetFloat("Speed", _move.magnitude);
    }

    public void Move(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    bool _eagleStompActive = false;

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_aim)
            {
                // Replace with an action eventually, or an event.
                _animator.SetTrigger("Attack");
                //_weaponAnimator.SetTrigger("Attack");
            }

            if (_eagleStompActive) StartCoroutine(_eagleStomp());
        }
    }

    IEnumerator _eagleStomp()
    {
        if (_actor.IsGrounded())
        {
            Debug.Log("Breaking");
            yield break;
        }

        _movementLocked = true;

        float elapsedTime = 0;

        while(elapsedTime < 1 && !_actor.IsGrounded())
        {
            elapsedTime += UnityEngine.Time.deltaTime;
            yield return null;
        }

        _eagleStompActive = false;

        _rigidBody.velocity = Vector3.zero;

        _movementLocked = false;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !_eagleStompActive && _actor.IsGrounded())
        {
            StartCoroutine(_testAbility("Eagle Stomp"));
        }
    }

    IEnumerator _testAbility(string abilityName)
    {
        var ability = Manager_Ability.GetAbility(abilityName);

        if (ability == null) { Debug.Log($"Ability: {abilityName} is null"); yield break; }

        Debug.Log($"{ability} performed.");

        _movementLocked = true;

        yield return StartCoroutine(ability.GetAction("Eagle Stomp"));

        _eagleStompActive = true;

        Cursor.lockState = CursorLockMode.None;

        float elapsedTime = 0;

        while (elapsedTime < 3)
        {
            elapsedTime += UnityEngine.Time.deltaTime;

            if (elapsedTime > 0.25 && _actor.IsGrounded())
            {
                break;
            }

            yield return null;
        }

        Cursor.lockState = CursorLockMode.Locked;

        _movementLocked = false;

        _eagleStompActive = false;

        if (ability.AnimationClip != null)
        {
            ability.AnimationClip.legacy = true;
            yield return StartCoroutine(PlayAnimation(ability.AnimationClip));
            ability.AnimationClip.legacy = false;
        }
    }

    public void EnableRootMotionForBody()
    {
        _animator.applyRootMotion = true;
    }

    public void DisableRootMotionForBody()
    {
        _animator.applyRootMotion = false;
    }

    public void SetTime(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (context.control.name)
            {
                case "numpadMinus":
                    Manager_Date_And_Time.DecreaseTimeScale();
                    break;
                case "numpadPlus":
                    Manager_Date_And_Time.IncreaseTimeScale();
                    break;
                case "numpad0":
                    Manager_Date_And_Time.ResetTimeScale();
                    break;
                default:
                    Debug.LogWarning($"{context.control.name} key pressed");
                    break;
            }
        }
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _aim = true;
        }
        else if (context.canceled)
        {
            _aim = false;
            _animator.SetBool("Block", false);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (ClosestInteractableObject == null)
        {
            Debug.Log("ClosestInteractableObject is null");
            return;
        }

        if (context.performed)
        {
            ClosestInteractableObject.Interact(_actor);
        }
    }

    public void TargetCheck()
    {
        float closestDistance = float.PositiveInfinity;
        IInteractable closestInteractable = null;

        Collider[] triggerHits = Physics.OverlapSphere(transform.position, 100);

        foreach (Collider hit in triggerHits)
        {
            if (hit.gameObject == null) continue;

            if (hit.gameObject.TryGetComponent(out IInteractable interactable))
            {
                float targetDistance = Vector3.Distance(transform.position, hit.transform.position);

                if (targetDistance < closestDistance)
                {
                    closestDistance = targetDistance;
                    closestInteractable = interactable;
                }
            }
        }

        ClosestInteractableObject = closestInteractable;
    }

    public IEnumerator PickUpStaffAction()
    {
        _animator.SetTrigger("PickupStaff");

        yield return new WaitForSeconds(2);

        _animator.SetBool("HasStaff", _hasStaff);
    }
}
