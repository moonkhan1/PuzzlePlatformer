using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
using CASP.CameraManager;
public class PlayerController : MonoBehaviour
{
    private IInputReader _inputs;
    private IMover _mover;
    private IJump _jumper;
    private IDevice _devices;
    private IMonth _month;
    private IPlayerCamera _playerCamera;
    private IAnimation _animation;
    private ILevelControl _levelControl;
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _runSpeed = 23f;
    [SerializeField] public Transform _monthPoint;

    // JUMP PROPS
    private float _gravity = -9.81f;
    private float _groundedGravity = -0.05f;

    private float _jumpForce;
    private float _maxJumpHeight = 6.0f;
    private float _maxJumpTime = 0.75f;
    private Vector3 _direction;
    private Vector3 CameraRelativeMove;
    private int jumpCount = 5;
    private int realJumpCount = 5;
    private bool isGrounded = true;

    int isJumpingHash;
    bool isJumpAnimating = false;
    bool isJump = false;
    private bool isDead = false;
    public bool IsPlayerDead => isDead;
    public event System.Action<int> IsJumpedAction;
    public event System.Action isPlayerDead;
    public event System.Action isAngelTriggered;
    public DeviceController _deviceCont;
    int LevelTwoPic = 0;


    private void Awake()
    {
        _inputs = GetComponent<IInputReader>();
        _mover = new MoveWithCharCont(this);
        _jumper = new JumpWithCharCont(this);
        _animation = new AnimationController(this);
        _devices = new DeviceController(this);
        _month = new MonthItem(this);
        _playerCamera = new PlayerCameraController(this);
        _levelControl = new LevelUpController(this);
        SetupJumpProps();
        isJumpingHash = Animator.StringToHash("Jump");
        string word = "Level2";
        Debug.Log(word.Substring(word.Length-1));

    }

    private void OnEnable()
    {
        _devices.isPicTriggered += isPicCorrect;
        _levelControl.isLeveltriggered += LevelUpStats;
    }
    private void OnDisable()
    {
        _devices.isPicTriggered -= isPicCorrect;
        _levelControl.isLeveltriggered -= LevelUpStats;
    }

    private void LevelUpStats(int index)
    {
        realJumpCount = realJumpCount + (index*6);
         jumpCount = realJumpCount;
        IsJumpedAction?.Invoke(realJumpCount);
    }

    private void isPicCorrect()
    {
        Debug.Log("Tamam");
        DeviceManager.Instance.Level2Platforms.SetActive(true);
        CameraManager.Instance.OpenCamera("PlatformCamera", 0.4f, CameraEaseStates.EaseOut);
        
    }

    void Update()
    {
        if (!isDead)
        {
            _direction = _inputs.Direction;
            HandleGravity();
            HandleJump();
            HandleMove();
            Debug.Log(LevelTwoPic);
        }

    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
        _mover.MoveAction(CameraRelativeMove, _speed);
        _mover.HandleRotation(CameraRelativeMove, _inputs.isMovingPressed);
        if (_inputs.isRunPressed)
            _mover.RunAction(CameraRelativeMove, _runSpeed);
        }
    }
    private void LateUpdate()
    {
        if (!isDead)
        {
        _animation.MoveAnimation(_inputs.isMovingPressed);
        _animation.RunAnimation(_inputs.isRunPressed);
        }

    }
    private void OnTriggerEnter(Collider other)
    {

        _devices.WhenTriggerInteractable(other);
        _playerCamera.TriggerForCamera(other);
         if (other.gameObject.tag == "AngelChat")
        {
            isAngelTriggered?.Invoke();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        _month.GetItemToMonth(other.collider);

        _levelControl.IsLeveltriggered(other.collider);



        if (other.gameObject.tag == "Ground")
        {
           
            isGrounded = true;
        }
        if (other.gameObject.tag == "Enemy")
        {
            isPlayerDead?.Invoke();
            isDead = true;

        }

       
    }

    private void OnCollisionStay(Collision other)
    {

        _devices.WhenTriggerInteractable(this.transform, CameraRelativeMove, other.collider, Quaternion.Euler(0f, 90f, 0f));

    }
   
    public void HandleMove()
    {
        Vector3 ForwardRelativeToCamera = Camera.main.transform.forward;
        Vector3 RightRelativeToCamera = Camera.main.transform.right;
        ForwardRelativeToCamera.y = 0;
        RightRelativeToCamera.y = 0;
        ForwardRelativeToCamera = ForwardRelativeToCamera.normalized;
        RightRelativeToCamera = RightRelativeToCamera.normalized;

        Vector3 _playerMoveRelativeCamerasForward = _direction.z * ForwardRelativeToCamera;
        Vector3 _playerMoveRelativeCamerasRight = _direction.x * RightRelativeToCamera;
        CameraRelativeMove = _playerMoveRelativeCamerasForward + _playerMoveRelativeCamerasRight;
    }

    private void SetupJumpProps()
    {
        float timeForMaxHeight = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeForMaxHeight, 2);
        _jumpForce = (2f * _maxJumpHeight) / timeForMaxHeight;
    }



    private void HandleJump()
    {
        if (!isJump && _inputs.isJumpPressed && jumpCount != 0 && isGrounded)
        {
            // _animation.JumpAnimation(true);
            _animation._animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJump = true;
            jumpCount--;
            _jumper.Jump(_inputs.isJumpPressed, _jumpForce);
            isGrounded = false;
            // realJumpCount--;
            IsJumpedAction?.Invoke(jumpCount);
        }
        else if (isJump && !_inputs.isJumpPressed && jumpCount != 0 && isGrounded)
        {
            isJump = false;
        }
    }

    private void HandleGravity()
    {
        bool isFalling = _direction.y <= 0.0f || _inputs.isJumpPressed;
        float fallMultipl = 2.0f;
        if (isGrounded)
        {
            if (isJumpAnimating)
            {
                //  _animation.JumpAnimation(false);
                _animation._animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;

            }

            _direction.y = _groundedGravity;
        }
        else if (isFalling)
        {
            float previousYVelocity = _direction.y;
            float newYVelocity = _direction.y + (_gravity * fallMultipl * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            _direction.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = _direction.y;
            float newYVelocity = _direction.y + (_gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            _direction.y = nextYVelocity;
        }
    }


}
