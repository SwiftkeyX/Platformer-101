using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed             = 6f;
    public float JumpVelocity          = 9f;
    public float Gravity               = -18f;
    public float FallGravityMultiplier = 1.4f;
    public float AirControlMultiplier  = 1.0f;
    public float CoyoteTime            = 0.12f;
    public float JumpBufferTime        = 0.15f;

    // ── Run ────────────────────────────────────────────────────────────────────
    /// <summary>Speed multiplier applied to horizontal movement while sprinting.</summary>
    public float RunMultiplier         = 1.6f;

    // ── Double Jump ────────────────────────────────────────────────────────────
    /// <summary>Vertical velocity applied when the double jump is consumed mid-air.</summary>
    public float DoubleJumpVelocity    = 9.0f;

    // ── Dash ───────────────────────────────────────────────────────────────────
    /// <summary>Horizontal speed applied each frame during an active dash.</summary>
    public float DashSpeed             = 18.0f;

    /// <summary>Duration in seconds that the dash lasts.</summary>
    public float DashDuration          = 0.2f;

    /// <summary>Cooldown in seconds before the next dash can be triggered.</summary>
    public float DashCooldown          = 1.0f;
}
