using UnityEngine;

public abstract class PlayerStateBase
{
    protected PlayerBlackboard _board;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public void OnEnter(PlayerBlackboard board)
    {
        _board = board;
        SubscribeInput(board);
        OnStateEnter(board);
    }

    public void OnExit(PlayerBlackboard board)
    {
        OnStateExit(board);
        UnsubscribeInput(board);
        _board = null;
    }

    protected virtual void OnStateEnter(PlayerBlackboard board) { }
    protected virtual void OnStateExit(PlayerBlackboard board)  { }

    // ── Frame loop ─────────────────────────────────────────────────────────────
    public virtual void Update(PlayerBlackboard board)
    {
        board.Update();
        CheckSwitchState(board);
    }

    protected abstract void CheckSwitchState(PlayerBlackboard board);

    // ── Input subscription ─────────────────────────────────────────────────────
    internal void SubscribeInput(PlayerBlackboard board)
    {
        board.Input.OnMove           += HandleMove;
        board.Input.OnJumpPressed    += HandleJumpPressed;
        board.Input.OnSprintHeld     += HandleSprintHeld;
        board.Input.OnSprintReleased += HandleSprintReleased;
        board.Input.OnDashPressed    += HandleDashPressed;
    }

    internal void UnsubscribeInput(PlayerBlackboard board)
    {
        board.Input.OnMove           -= HandleMove;
        board.Input.OnJumpPressed    -= HandleJumpPressed;
        board.Input.OnSprintHeld     -= HandleSprintHeld;
        board.Input.OnSprintReleased -= HandleSprintReleased;
        board.Input.OnDashPressed    -= HandleDashPressed;
    }

    // ── Default input handlers (states override only what differs) ─────────────
    protected virtual void HandleMove(Vector2 input)  => _board.MoveInput = input;
    protected virtual void HandleJumpPressed()         => _board.JumpBufferTimer = _board.Data.JumpBufferTime;
    protected virtual void HandleSprintHeld()          => _board.IsSprinting = true;
    protected virtual void HandleSprintReleased()      => _board.IsSprinting = false;

    protected virtual void HandleDashPressed()
    {
        if (_board.DashCooldownTimer > 0f) return;

        _board.DashDirection = _board.MoveInput.sqrMagnitude > 0.01f
            ? _board.MoveDirection()
            : (_board.CameraCtrl != null ? _board.CameraCtrl.Forward : Vector3.forward);

        _board.SwitchState(new DashingState());
    }
}
