using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader      _inputReader;
    [SerializeField] private PlayerData       _data;
    [SerializeField] private CameraController _cameraController;

    private CharacterController _cc;
    private Vector3 _velocity;
    private Vector2 _moveInput;

    private enum MoveState { Grounded, Coyote, Airborne }
    private MoveState _state = MoveState.Airborne;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    // ── Sprint ─────────────────────────────────────────────────────────────────
    private bool _isSprinting;

    // ── Double Jump ────────────────────────────────────────────────────────────
    private bool _hasDoubleJump;

    // ── Dash ───────────────────────────────────────────────────────────────────
    private bool    _isDashing;
    private float   _dashTimer;
    private float   _dashCooldownTimer;
    private Vector3 _dashDirection;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _inputReader.OnMove           += HandleMove;
        _inputReader.OnJumpPressed    += HandleJumpPressed;
        _inputReader.OnSprintHeld     += HandleSprintHeld;
        _inputReader.OnSprintReleased += HandleSprintReleased;
        _inputReader.OnDashPressed    += HandleDash;
    }

    private void OnDisable()
    {
        _inputReader.OnMove           -= HandleMove;
        _inputReader.OnJumpPressed    -= HandleJumpPressed;
        _inputReader.OnSprintHeld     -= HandleSprintHeld;
        _inputReader.OnSprintReleased -= HandleSprintReleased;
        _inputReader.OnDashPressed    -= HandleDash;
    }

    // ── Input handlers ─────────────────────────────────────────────────────────
    private void HandleMove(Vector2 input)    => _moveInput = input;
    private void HandleJumpPressed()          => _jumpBufferTimer = _data.JumpBufferTime;
    private void HandleSprintHeld()           => _isSprinting = true;
    private void HandleSprintReleased()       => _isSprinting = false;

    private void HandleDash()
    {
        if (_dashCooldownTimer > 0f) return;

        _isDashing   = true;
        _dashTimer   = _data.DashDuration;

        // Prefer move-input direction; fall back to camera forward
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 forward = _cameraController != null ? _cameraController.Forward : Vector3.forward;
            Vector3 right   = new Vector3(forward.z, 0f, -forward.x);
            _dashDirection  = (forward * _moveInput.y + right * _moveInput.x).normalized;
        }
        else
        {
            _dashDirection = _cameraController != null ? _cameraController.Forward : Vector3.forward;
        }
    }

    // ── Update ─────────────────────────────────────────────────────────────────
    private void Update()
    {
        UpdateGroundState();
        ApplyGravity();

        if (_isDashing)
            TickDash();
        else
            ApplyHorizontalMovement();

        ConsumeJumpBuffer();
        TickDashCooldown();

        _cc.Move(_velocity * Time.deltaTime);
    }

    // ── Ground state ───────────────────────────────────────────────────────────
    private void UpdateGroundState()
    {
        bool wasAirborne = _state == MoveState.Airborne;

        switch (_state)
        {
            case MoveState.Grounded:
                if (!_cc.isGrounded)
                {
                    _state       = MoveState.Coyote;
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

        // Reset double jump on landing
        if (wasAirborne && _state == MoveState.Grounded && _velocity.y < 0f)
            _hasDoubleJump = true;

        // Also reset on Coyote → Grounded transition
        if (!wasAirborne && _state == MoveState.Grounded)
            _hasDoubleJump = true;
    }

    // ── Gravity ────────────────────────────────────────────────────────────────
    private void ApplyGravity()
    {
        // Suppress gravity during dash
        if (_isDashing) return;

        if (_state == MoveState.Grounded && _velocity.y < 0f)
        {
            _velocity.y = -2f; // prevent accumulation on slopes
            return;
        }

        _velocity.y += _data.Gravity * Time.deltaTime;

        if (_velocity.y < 0f)
            _velocity.y += _data.Gravity * (_data.FallGravityMultiplier - 1f) * Time.deltaTime;
    }

    // ── Horizontal movement ────────────────────────────────────────────────────
    private void ApplyHorizontalMovement()
    {
        Vector3 forward = _cameraController != null ? _cameraController.Forward : Vector3.forward;
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x); // XZ-perpendicular

        Vector3 move = forward * _moveInput.y + right * _moveInput.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speedMult = _state == MoveState.Grounded ? 1f : _data.AirControlMultiplier;
        float runMult   = _isSprinting ? _data.RunMultiplier : 1f;

        _velocity.x = move.x * _data.MoveSpeed * speedMult * runMult;
        _velocity.z = move.z * _data.MoveSpeed * speedMult * runMult;
    }

    // ── Jump (first + double) ──────────────────────────────────────────────────
    private void ConsumeJumpBuffer()
    {
        if (_jumpBufferTimer <= 0f) return;

        _jumpBufferTimer -= Time.deltaTime;

        if (_state == MoveState.Grounded || _state == MoveState.Coyote)
        {
            // First jump — do NOT consume double jump
            _velocity.y      = _data.JumpVelocity;
            _state           = MoveState.Airborne;
            _jumpBufferTimer = 0f;
        }
        else if (_hasDoubleJump)
        {
            // Double jump — consume the token
            _velocity.y      = _data.DoubleJumpVelocity;
            _hasDoubleJump   = false;
            _jumpBufferTimer = 0f;
        }
    }

    // ── Dash tick ──────────────────────────────────────────────────────────────
    private void TickDash()
    {
        _velocity.x  = _dashDirection.x * _data.DashSpeed;
        _velocity.z  = _dashDirection.z * _data.DashSpeed;
        _velocity.y  = 0f;

        _dashTimer  -= Time.deltaTime;
        if (_dashTimer <= 0f)
        {
            _isDashing           = false;
            _dashCooldownTimer   = _data.DashCooldown;
        }
    }

    // ── Dash cooldown tick ─────────────────────────────────────────────────────
    private void TickDashCooldown()
    {
        if (_dashCooldownTimer <= 0f) return;
        _dashCooldownTimer -= Time.deltaTime;
        if (_dashCooldownTimer < 0f) _dashCooldownTimer = 0f;
    }
}
