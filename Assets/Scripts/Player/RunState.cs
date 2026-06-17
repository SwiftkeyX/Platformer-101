using UnityEngine;

public class RunState : PlayerStateBase
{
    protected override void OnStateEnter()
    {
        _global.HasDoubleJump = true;
    }

    public override void Update()
    {
        if (_global.IsGrounded && _global.Velocity.y < 0f)
            _global.Velocity.y = -2f;

        base.Update();
        ApplyMove();
    }

    protected override void CheckSwitchState()
    {
        if (_global.DashRequested)
        {
            _global.DashRequested = false;
            _global.SwitchState(_global.Dashing);
            return;
        }

        if (!_global.IsGrounded)
        {
            _global.SwitchState(_global.Airborne.Configure(JumpType.Coyote));
            return;
        }

        if (_global.IsJumpBuffer)
        {
            _global.Velocity.y      = _global.Data.JumpVelocity;
            _global.JumpBufferTimer = 0f;
            _global.SwitchState(_global.Airborne.Configure(JumpType.Buffered));
            return;
        }

        if (!_global.IsSprinting)
        {
            _global.SwitchState(_global.Walk);
            return;
        }

        if (_global.MoveInput.sqrMagnitude <= 0.01f)
            _global.SwitchState(_global.Idle);
    }

    private void ApplyMove()
    {
        Vector3 move       = _global.MoveDirection();
        float   speed      = _global.Data.MoveSpeed * _global.Data.RunMultiplier;
        _global.Velocity.x = move.x * speed;
        _global.Velocity.z = move.z * speed;
    }
}
