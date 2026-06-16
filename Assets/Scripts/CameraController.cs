using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform  _playerTransform;
    [SerializeField] private CameraData _data;

    public Vector3 Forward { get; private set; } = Vector3.forward;
    public Vector3 Right   { get; private set; } = Vector3.right;

    private float _yaw   = 0f;
    private float _pitch = 20f;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void LateUpdate()
    {
        if (_playerTransform == null || _data == null) return;

        // Accumulate look input from mouse and gamepad
        Vector2 lookInput = Vector2.zero;

        if (Mouse.current != null)
            lookInput += Mouse.current.delta.ReadValue() * _data.LookSensitivity;

        if (Gamepad.current != null)
            lookInput += Gamepad.current.rightStick.ReadValue() * _data.GamepadLookSensitivity;

        _yaw   += lookInput.x;
        _pitch -= lookInput.y;
        _pitch  = Mathf.Clamp(_pitch, _data.PitchMin, _data.PitchMax);

        // Orbit position via spherical coordinates
        float yawRad   = _yaw   * Mathf.Deg2Rad;
        float pitchRad = _pitch * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            Mathf.Sin(yawRad)  * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad)  * Mathf.Cos(pitchRad)
        ) * _data.OrbitDistance;

        Vector3 playerPivot = _playerTransform.position + Vector3.up * _data.VerticalLookOffset;
        Vector3 orbitTarget = playerPivot + orbitOffset;

        // Smooth position follow, then look at player pivot
        transform.position = Vector3.Lerp(transform.position, orbitTarget, _data.FollowSmoothing * Time.deltaTime);
        transform.LookAt(playerPivot);

        // Expose XZ-plane forward/right for PlayerController camera-relative movement
        Vector3 f = transform.forward;
        f.y = 0f;
        if (f.sqrMagnitude > 0.001f)
        {
            Forward = f.normalized;
            Right   = new Vector3(Forward.z, 0f, -Forward.x);
        }
    }
}
