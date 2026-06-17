using UnityEngine;

public class AirborneState : PlayerStateBase
{
    private float _coyoteTimer;
    private bool  _coyoteActive;

    public AirborneState(float coyoteTime)
    {
        _coyoteTimer  = coyoteTime;
        _coyoteActive = coyoteTime > 0f;
    }

    public override void Update(PlayerBlackboard board)
    {
        if (_coyoteActive)
        {
            _coyoteTimer -= Time.deltaTime;
            if (_coyoteTimer <= 0f)
                _coyoteActive = false;
        }

        // Full gravity — fall multiplier on descent
        board.Velocity.y += board.Data.Gravity * Time.deltaTime;
        if (board.Velocity.y < 0f)
            board.Velocity.y += board.Data.Gravity * (board.Data.FallGravityMultiplier - 1f) * Time.deltaTime;

        base.Update(board); // board.Update() + CheckSwitchState (landing)

        // Coyote or double jump — velocity modification, no state change
        if (board.JumpBufferTimer > 0f)
        {
            if (_coyoteActive)
            {
                board.Velocity.y      = board.Data.JumpVelocity;
                board.JumpBufferTimer = 0f;
                _coyoteActive         = false;
            }
            else if (board.HasDoubleJump)
            {
                board.Velocity.y      = board.Data.DoubleJumpVelocity;
                board.HasDoubleJump   = false;
                board.JumpBufferTimer = 0f;
            }
        }

        // Air-control horizontal movement
        Vector3 move     = board.MoveDirection();
        float   speed    = board.Data.MoveSpeed * board.Data.AirControlMultiplier;
        board.Velocity.x = move.x * speed;
        board.Velocity.z = move.z * speed;
    }

    protected override void CheckSwitchState(PlayerBlackboard board)
    {
        if (board.IsGrounded)
            board.SwitchState(SelectGroundedState(board));
    }

    private static PlayerStateBase SelectGroundedState(PlayerBlackboard board)
    {
        if (board.MoveInput.sqrMagnitude <= 0.01f) return new IdleState();
        return board.IsSprinting ? (PlayerStateBase)new RunState() : new WalkState();
    }
}
