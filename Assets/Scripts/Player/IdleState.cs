public class IdleState : PlayerStateBase
{
    protected override void OnStateEnter(PlayerBlackboard board)
    {
        board.HasDoubleJump = true;
    }

    public override void Update(PlayerBlackboard board)
    {
        if (board.IsGrounded && board.Velocity.y < 0f)
            board.Velocity.y = -2f;

        board.Velocity.x = 0f;
        board.Velocity.z = 0f;

        base.Update(board);
    }

    protected override void CheckSwitchState(PlayerBlackboard board)
    {
        if (!board.IsGrounded)
        {
            board.SwitchState(new AirborneState(board.Data.CoyoteTime));
            return;
        }

        if (board.JumpBufferTimer > 0f)
        {
            board.Velocity.y      = board.Data.JumpVelocity;
            board.JumpBufferTimer = 0f;
            board.SwitchState(new AirborneState(0f));
            return;
        }

        if (board.MoveInput.sqrMagnitude > 0.01f)
            board.SwitchState(board.IsSprinting ? (PlayerStateBase)new RunState() : new WalkState());
    }
}
