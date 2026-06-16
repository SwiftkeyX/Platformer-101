using UnityEngine;

[CreateAssetMenu(menuName = "Game/CameraData")]
public class CameraData : ScriptableObject
{
    public float LookSensitivity        = 0.15f;
    public float GamepadLookSensitivity = 3.0f;
    public float OrbitDistance          = 6.0f;
    public float VerticalLookOffset     = 1.0f;
    public float PitchMin               = -20f;
    public float PitchMax               = 60f;
    public float FollowSmoothing        = 8.0f;
}
