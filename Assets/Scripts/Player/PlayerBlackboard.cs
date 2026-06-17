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

    // ── Per-frame engine snapshot (refreshed by PlayerController each Update) ──
    public bool IsGrounded;

    // ── Read-only refs (set once at construction) ──────────────────────────────
    public PlayerData       Data;
    public CameraController CameraCtrl;

    // ── State machine hook ─────────────────────────────────────────────────────
    public Action<PlayerStateBase> SwitchState;

    // ── Per-frame tick (called by PlayerStateBase.Update) ─────────────────────
    public void Update()
    {
        if (JumpBufferTimer > 0f)
            JumpBufferTimer -= Time.deltaTime;

        if (DashCooldownTimer > 0f)
        {
            DashCooldownTimer -= Time.deltaTime;
            if (DashCooldownTimer < 0f) DashCooldownTimer = 0f;
        }
    }
}
