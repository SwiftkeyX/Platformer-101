using UnityEngine;

public class WalkState : PlayerStateBase
{
    public override void OnEnter(PlayerBlackboard board)
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
        Vector3 forward = board.CameraCtrl != null ? board.CameraCtrl.Forward : Vector3.forward;
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x);
        Vector3 move    = forward * board.MoveInput.y + right * board.MoveInput.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        board.Velocity.x = move.x * board.Data.MoveSpeed;
        board.Velocity.z = move.z * board.Data.MoveSpeed;
    }
}
