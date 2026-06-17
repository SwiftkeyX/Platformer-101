using UnityEngine;

public class WalkState : PlayerStateBase
{
    protected override void OnStateEnter(PlayerBlackboard board)
    {
        board.HasDoubleJump = true;
    }

    public override void Update(PlayerBlackboard board)
    {
        if (board.IsGrounded && board.Velocity.y < 0f)
            board.Velocity.y = -2f;

        base.Update(board);

        ApplyMove(board);
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

        if (board.IsSprinting)
        {
            board.SwitchState(new RunState());
            return;
        }

        if (board.MoveInput.sqrMagnitude <= 0.01f)
            board.SwitchState(new IdleState());
    }

    internal static void ApplyMove(PlayerBlackboard board)
    {
        Vector3 move     = board.MoveDirection();
        board.Velocity.x = move.x * board.Data.MoveSpeed;
        board.Velocity.z = move.z * board.Data.MoveSpeed;
    }
}
