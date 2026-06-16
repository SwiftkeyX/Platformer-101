using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed           = 6f;
    public float JumpVelocity        = 9f;
    public float Gravity             = -18f;
    public float FallGravityMultiplier = 1.4f;
    public float AirControlMultiplier  = 1.0f;
    public float CoyoteTime          = 0.12f;
    public float JumpBufferTime      = 0.15f;
}
