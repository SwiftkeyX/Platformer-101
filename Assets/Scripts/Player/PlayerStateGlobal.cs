using System;
using UnityEngine;

public class PlayerStateGlobal
{
    // ── Runtime state ──────────────────────────────────────────────────────────
    public Vector3 Velocity;
    public Vector2 MoveInput;
    public bool    IsSprinting;
    public float   JumpBufferTimer;
    public bool    HasDoubleJump;
    public float   DashCooldownTimer;
    public float   DashActiveTimer;   // counts down while a dash is in progress
    public Vector3 DashDirection;
    public bool    DashRequested;

    // ── Per-frame engine snapshot (refreshed by PlayerStateManager each Update) ──
    public bool IsGrounded;

    // ── Computed properties ────────────────────────────────────────────────────
    public bool IsJumpBuffer => JumpBufferTimer > 0f;

    // ── Read-only refs (set once at construction) ──────────────────────────────
    public InputReader      Input;
    public PlayerData       Data;
    public CameraController CameraCtrl;

    // ── State machine hook ─────────────────────────────────────────────────────
    public Action<PlayerStateBase> SwitchState;

    // ── Pre-allocated state roster ─────────────────────────────────────────────
    public IdleState     Idle;
    public WalkState     Walk;
    public RunState      Run;
    public AirborneState Airborne;
    public DashingState  Dashing;

    // ── Per-frame tick (called by PlayerStateBase.Update) ─────────────────────
    public void Update()
    {
        TickJumpBuffer();
        TickDashCooldown();
    }

    private void TickJumpBuffer()
    {
        if (JumpBufferTimer > 0f)
        {
            JumpBufferTimer -= Time.deltaTime;
            if (JumpBufferTimer < 0f) JumpBufferTimer = 0f;
        }
    }

    private void TickDashCooldown()
    {
        if (DashCooldownTimer > 0f)
        {
            DashCooldownTimer -= Time.deltaTime;
            if (DashCooldownTimer < 0f) DashCooldownTimer = 0f;
        }
    }

    // ── Dash request ───────────────────────────────────────────────────────────
    public void RequestDash()
    {
        if (DashCooldownTimer > 0f || DashActiveTimer > 0f) return;
        DashDirection = MoveInput.sqrMagnitude > 0.01f
            ? MoveDirection()
            : (CameraCtrl != null ? CameraCtrl.Forward : Vector3.forward);
        DashRequested = true;
    }

    // ── Movement helpers ───────────────────────────────────────────────────────
    public Vector3 MoveDirection()
    {
        Vector3 forward = CameraCtrl != null ? CameraCtrl.Forward : Vector3.forward;
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x);
        Vector3 dir     = forward * MoveInput.y + right * MoveInput.x;
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        return dir;
    }
}
