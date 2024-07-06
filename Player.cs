using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class Player : Controller, IDataPersistence
{
    Actor_Base _actor;
    public float SpeedIncrease;
    CapsuleCollider _coll_Body;
    BoxCollider _coll_Head;
    Animator _animator;
    Animation _animation;
    bool _moved;
    RaycastHit2D _hit;
    public Interactable_Base ClosestInteractableObject; //public Interactable ClosestInteractableObject { get { return _closestInteractableObject; } }
    List<Interactable_Base> _interactableObjects = new();
    [SerializeField] bool _hasStaff;
    bool _aim = false;
    Vector2 _move;
    Rigidbody _testBody;
    bool _inAir = false;

    int _playerID = 0;
    public BoxCollider _fireflyWanderZone; public BoxCollider FireflyWanderZone { get { return _fireflyWanderZone; } }

    public void Start()
    {
        _actor = GetComponent<Actor_Base>();
        _coll_Body = GetComponent<CapsuleCollider>();
        _coll_Head = Manager_Game.FindTransformRecursively(transform, "PlayerHead").GetComponent<BoxCollider>();
        _animator = GetComponent<Animator>();
        _animation = GameObject.Find("TestActor").GetComponent<Animation>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        _testBody = GameObject.Find("TestBody").GetComponent<Rigidbody>();

        //StartCoroutine(_testAbility("Eagle Stomp"));
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
        data.PlayerPosition = _rigidBody.transform.position;
    }

    public void LoadData(GameData data)
    {
        //_rigidbody.transform.position = data.PlayerPosition;
    }

    protected void Update()
    {
        _moved = false;

        PlayerMove();

        if (_hasStaff != Manager_Game.Instance.PlayerHasStaff) { _hasStaff = Manager_Game.Instance.PlayerHasStaff; _animator.SetBool("HasStaff", _hasStaff); }

        if (Manager_Game.Instance.CurrentState != GameState.Playing) return;

        // if (!_moved) _animator.SetFloat("Speed", 0);
        TargetCheck();
    }

    IEnumerator _testAbility(string abilityName)
    {
        Manager_Ability.SetCharacter(_playerID, (_testBody, false));

        var ability = Manager_Ability.GetAbility(abilityName);

        if (ability == null) { Debug.Log($"Ability: {abilityName} is null"); yield break; }

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(5);

            Debug.Log($"{ability} performed.");

            ability.GetAction("Eagle Stomp")?.Invoke(_playerID);

            _inAir = true;

            Cursor.lockState = CursorLockMode.None;

            yield return new WaitForSeconds(3);

            _inAir = false;

            Cursor.lockState = CursorLockMode.Locked;

            if (ability.AnimationClip != null)
            {
                ability.AnimationClip.legacy = true;
                yield return StartCoroutine(PlayAnimation(ability.AnimationClip));
                ability.AnimationClip.legacy = false;
            }
        }
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

            _rigidBody.transform.Translate(desiredMoveDirection * _speed * Time.deltaTime, Space.World);
        }

        _animator.SetFloat("Speed", _move.magnitude);
    }

    public void Move(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    Coroutine _attackChainCoroutine;
    float _attackChainWindow;
    bool _ableToContinueAttackChain = false;
    bool _continuedAttackChain = false;

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_aim) _animator.SetTrigger("Attack");
            if (_inAir) Manager_Ability.SetCharacter(0, (_testBody, true));
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
                    Manager_Time.DecreaseTimeScale();
                    break;
                case "numpadPlus":
                    Manager_Time.IncreaseTimeScale();
                    break;
                case "numpad0":
                    Manager_Time.ResetTimeScale();
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
            ClosestInteractableObject.Interact(gameObject);
        }
    }

    public void TargetCheck()
    {
        float closestDistance = float.PositiveInfinity;
        Collider[] triggerHits = Physics.OverlapSphere(transform.position, 100);

        foreach (Collider hit in triggerHits)
        {
            if (hit.gameObject == null) continue;

            if (hit.gameObject.TryGetComponent<Interactable_Base>(out Interactable_Base interactable))
            {
                _interactableObjects.Add(interactable);
            }
        }

        foreach (Interactable_Base interactable in _interactableObjects)
        {
            if (interactable != null)
            {
                float targetDistance = Vector3.Distance(transform.position, interactable.transform.position);

                if (targetDistance < closestDistance)
                {
                    closestDistance = targetDistance;
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
