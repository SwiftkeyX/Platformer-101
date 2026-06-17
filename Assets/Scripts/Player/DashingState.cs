using UnityEngine;

public class DashingState : PlayerStateBase
{
    protected override void OnStateEnter()
    {
        _global.DashActiveTimer = _global.Data.DashDuration;
        _global.DashRequested   = false; // consume the request that triggered this dash
    }

    public override void Update()
    {
        _global.Velocity.x       = _global.DashDirection.x * _global.Data.DashSpeed;
        _global.Velocity.z       = _global.DashDirection.z * _global.Data.DashSpeed;
        _global.Velocity.y       = 0f;
        _global.DashActiveTimer -= Time.deltaTime;

        base.Update(); // _global.Update() + CheckSwitchState (dash expiry)
    }

    protected override void OnStateExit()
    {
        _global.DashCooldownTimer = _global.Data.DashCooldown;
        _global.DashRequested     = false; // discard any dash pressed mid-dash
    }

    protected override void CheckSwitchState()
    {
        if (_global.DashActiveTimer <= 0f)
            _global.SwitchState(SelectPostDashState());
    }

    private PlayerStateBase SelectPostDashState()
    {
        // HasDoubleJump is not reset here by design: grounded states reset it on entry,
        // so ground-dash preserves the double-jump; air-dash preserves whatever the player had.
        if (!_global.IsGrounded) return _global.Airborne.Configure(JumpType.Buffered);
        if (_global.MoveInput.sqrMagnitude <= 0.01f) return _global.Idle;
        return _global.IsSprinting ? (PlayerStateBase)_global.Run : _global.Walk;
    }
}
