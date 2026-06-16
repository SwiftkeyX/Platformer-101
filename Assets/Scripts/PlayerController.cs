using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerData _data;
    [SerializeField] private CameraController _cameraController;

    private CharacterController _cc;
    private Vector3 _velocity;
    private Vector2 _moveInput;

    private enum MoveState { Grounded, Coyote, Airborne }
    private MoveState _state = MoveState.Airborne;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _inputReader.OnMove       += HandleMove;
        _inputReader.OnJumpPressed += HandleJumpPressed;
    }

    private void OnDisable()
    {
        _inputReader.OnMove       -= HandleMove;
        _inputReader.OnJumpPressed -= HandleJumpPressed;
    }

    private void HandleMove(Vector2 input)    => _moveInput = input;
    private void HandleJumpPressed()          => _jumpBufferTimer = _data.JumpBufferTime;

    private void Update()
    {
        UpdateGroundState();
        ApplyGravity();
        ApplyHorizontalMovement();
        ConsumeJumpBuffer();
        _cc.Move(_velocity * Time.deltaTime);
    }

    private void UpdateGroundState()
    {
        switch (_state)
        {
            case MoveState.Grounded:
                if (!_cc.isGrounded)
                {
                    _state = MoveState.Coyote;
                    _coyoteTimer = _data.CoyoteTime;
                }
                break;

            case MoveState.Coyote:
                if (_cc.isGrounded)
                {
                    _state = MoveState.Grounded;
                }
                else
                {
                    _coyoteTimer -= Time.deltaTime;
                    if (_coyoteTimer <= 0f)
                        _state = MoveState.Airborne;
                }
                break;

            case MoveState.Airborne:
                if (_cc.isGrounded)
                    _state = MoveState.Grounded;
                break;
        }
    }

    private void ApplyGravity()
    {
        if (_state == MoveState.Grounded && _velocity.y < 0f)
        {
            _velocity.y = -2f; // prevent accumulation on slopes
            return;
        }

        _velocity.y += _data.Gravity * Time.deltaTime;

        if (_velocity.y < 0f)
            _velocity.y += _data.Gravity * (_data.FallGravityMultiplier - 1f) * Time.deltaTime;
    }

    private void ApplyHorizontalMovement()
    {
        Vector3 forward = _cameraController != null ? _cameraController.Forward : Vector3.forward;
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x); // XZ-perpendicular

        Vector3 move = forward * _moveInput.y + right * _moveInput.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speedMult = _state == MoveState.Grounded ? 1f : _data.AirControlMultiplier;
        _velocity.x = move.x * _data.MoveSpeed * speedMult;
        _velocity.z = move.z * _data.MoveSpeed * speedMult;
    }

    private void ConsumeJumpBuffer()
    {
        if (_jumpBufferTimer <= 0f) return;

        _jumpBufferTimer -= Time.deltaTime;

        if (_state == MoveState.Grounded || _state == MoveState.Coyote)
        {
            _velocity.y      = _data.JumpVelocity;
            _state           = MoveState.Airborne;
            _jumpBufferTimer = 0f;
        }
    }
}
