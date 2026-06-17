using UnityEngine;

public class DashingState : PlayerStateBase
{
    protected override void OnStateEnter(PlayerBlackboard board)
    {
        board.DashTimer = board.Data.DashDuration;
    }

    public override void Update(PlayerBlackboard board)
    {
        board.Velocity.x  = board.DashDirection.x * board.Data.DashSpeed;
        board.Velocity.z  = board.DashDirection.z * board.Data.DashSpeed;
        board.Velocity.y  = 0f;

        board.DashTimer  -= Time.deltaTime;

        base.Update(board); // board.Update() + CheckSwitchState (dash expiry)
    }

    protected override void OnStateExit(PlayerBlackboard board)
    {
        board.DashCooldownTimer = board.Data.DashCooldown;
    }

    protected override void HandleDashPressed() { }

    protected override void CheckSwitchState(PlayerBlackboard board)
    {
        if (board.DashTimer <= 0f)
            board.SwitchState(SelectPostDashState(board));
    }

    private static PlayerStateBase SelectPostDashState(PlayerBlackboard board)
    {
        // HasDoubleJump is not touched here by design: grounded states reset it on entry,
        // so ground-dash preserves the jump; air-dash preserves whatever the player had.
        if (!board.IsGrounded) return new AirborneState(0f);
        if (board.MoveInput.sqrMagnitude <= 0.01f) return new IdleState();
        return board.IsSprinting ? (PlayerStateBase)new RunState() : new WalkState();
    }
}
