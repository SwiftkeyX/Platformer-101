using UnityEngine;

public class InputProcessor
{
    private readonly PlayerStateGlobal _global;

    public InputProcessor(PlayerStateGlobal global) => _global = global;

    public void Subscribe(InputReader input)
    {
        input.OnMove           += HandleMove;
        input.OnJumpPressed    += HandleJumpPressed;
        input.OnSprintHeld     += HandleSprintHeld;
        input.OnSprintReleased += HandleSprintReleased;
        input.OnDashPressed    += HandleDashPressed;
    }

    public void Unsubscribe(InputReader input)
    {
        input.OnMove           -= HandleMove;
        input.OnJumpPressed    -= HandleJumpPressed;
        input.OnSprintHeld     -= HandleSprintHeld;
        input.OnSprintReleased -= HandleSprintReleased;
        input.OnDashPressed    -= HandleDashPressed;
    }

    private void HandleMove(Vector2 v)  => _global.MoveInput = v;
    private void HandleJumpPressed()    => _global.JumpBufferTimer = _global.Data.JumpBufferTime;
    private void HandleSprintHeld()     => _global.IsSprinting = true;
    private void HandleSprintReleased() => _global.IsSprinting = false;
    private void HandleDashPressed()    => _global.RequestDash();
}
