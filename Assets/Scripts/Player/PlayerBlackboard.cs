using System;
using UnityEngine;

public class PlayerBlackboard
{
    // ── Runtime state ──────────────────────────────────────────────────────────
    public Vector3 Velocity;
    public Vector2 MoveInput;
    public bool    IsSprinting;
    public float   JumpBufferTimer;
    public bool    HasDoubleJump;
    public float   DashCooldownTimer;
    public float   DashTimer;
    public Vector3 DashDirection;

    // ── Per-frame engine snapshot (refreshed by PlayerStateManager each Update) ──
    public bool IsGrounded;

    // ── Read-only refs (set once at construction) ──────────────────────────────
    public InputReader      Input;
    public PlayerData       Data;
    public CameraController CameraCtrl;

    // ── State machine hook ─────────────────────────────────────────────────────
    public Action<PlayerStateBase> SwitchState;

    // ── Per-frame tick (called by PlayerStateBase.Update) ─────────────────────
    public void Update()
    {
        if (JumpBufferTimer > 0f)
        {
            JumpBufferTimer -= Time.deltaTime;
            if (JumpBufferTimer < 0f) JumpBufferTimer = 0f;
        }

        if (DashCooldownTimer > 0f)
        {
            DashCooldownTimer -= Time.deltaTime;
            if (DashCooldownTimer < 0f) DashCooldownTimer = 0f;
        }
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
