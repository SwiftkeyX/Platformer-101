public class IdleState : PlayerStateBase
{
    protected override void OnStateEnter()
    {
        _global.HasDoubleJump = true;
    }

    public override void Update()
    {
        if (_global.IsGrounded && _global.Velocity.y < 0f)
            _global.Velocity.y = -2f;

        _global.Velocity.x = 0f;
        _global.Velocity.z = 0f;

        base.Update();
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

        if (_global.MoveInput.sqrMagnitude > 0.01f)
            _global.SwitchState(_global.IsSprinting ? (PlayerStateBase)_global.Run : _global.Walk);
    }
}
