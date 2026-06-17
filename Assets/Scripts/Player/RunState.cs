using UnityEngine;

public class RunState : PlayerStateBase
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

        if (!board.IsSprinting)
        {
            board.SwitchState(new WalkState());
            return;
        }

        if (board.MoveInput.sqrMagnitude <= 0.01f)
            board.SwitchState(new IdleState());
    }

    private static void ApplyMove(PlayerBlackboard board)
    {
        Vector3 move     = board.MoveDirection();
        float   speed    = board.Data.MoveSpeed * board.Data.RunMultiplier;
        board.Velocity.x = move.x * speed;
        board.Velocity.z = move.z * speed;
    }
}
