using UnityEngine;

public class AirborneState : PlayerStateBase
{
    private float    _coyoteTimer;
    private bool     _coyoteActive;
    private JumpType _pendingType = JumpType.Buffered;

    public AirborneState Configure(JumpType type) { _pendingType = type; return this; }

    protected override void OnStateEnter()
    {
        _coyoteTimer  = _pendingType == JumpType.Coyote ? _global.Data.CoyoteTime : 0f;
        _coyoteActive = _pendingType == JumpType.Coyote;
    }

    public override void Update()
    {
        if (_coyoteActive)
        {
            _coyoteTimer -= Time.deltaTime;
            if (_coyoteTimer <= 0f)
                _coyoteActive = false;
        }

        // Full gravity — fall multiplier on descent
        _global.Velocity.y += _global.Data.Gravity * Time.deltaTime;
        if (_global.Velocity.y < 0f)
            _global.Velocity.y += _global.Data.Gravity * (_global.Data.FallGravityMultiplier - 1f) * Time.deltaTime;

        base.Update(); // _global.Update() + CheckSwitchState (landing)

        // Coyote or double jump — velocity modification, no state change
        if (_global.IsJumpBuffer)
        {
            if (_coyoteActive)
            {
                _global.Velocity.y      = _global.Data.JumpVelocity;
                _global.JumpBufferTimer = 0f;
                _coyoteActive           = false;
            }
            else if (_global.HasDoubleJump)
            {
                _global.Velocity.y      = _global.Data.DoubleJumpVelocity;
                _global.HasDoubleJump   = false;
                _global.JumpBufferTimer = 0f;
            }
        }

        // Air-control horizontal movement
        Vector3 move       = _global.MoveDirection();
        float   speed      = _global.Data.MoveSpeed * _global.Data.AirControlMultiplier;
        _global.Velocity.x = move.x * speed;
        _global.Velocity.z = move.z * speed;
    }

    protected override void CheckSwitchState()
    {
        if (_global.DashRequested)
        {
            _global.DashRequested = false;
            _global.SwitchState(_global.Dashing);
            return;
        }

        if (_global.IsGrounded)
            _global.SwitchState(SelectGroundedState());
    }

    private PlayerStateBase SelectGroundedState()
    {
        if (_global.MoveInput.sqrMagnitude <= 0.01f) return _global.Idle;
        return _global.IsSprinting ? (PlayerStateBase)_global.Run : _global.Walk;
    }
}
