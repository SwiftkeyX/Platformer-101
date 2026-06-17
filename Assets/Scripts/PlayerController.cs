using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader      _inputReader;
    [SerializeField] private PlayerData       _data;
    [SerializeField] private CameraController _cameraController;

    private CharacterController _cc;
    private PlayerStateBase     _currentState;

    internal PlayerBlackboard Board { get; private set; }

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Board = new PlayerBlackboard
        {
            Data        = _data,
            CameraCtrl  = _cameraController,
            SwitchState = SetState
        };
        SetState(new AirborneState(0f));
    }

    private void OnEnable()
    {
        _inputReader.OnMove           += HandleMove;
        _inputReader.OnJumpPressed    += HandleJumpPressed;
        _inputReader.OnSprintHeld     += HandleSprintHeld;
        _inputReader.OnSprintReleased += HandleSprintReleased;
        _inputReader.OnDashPressed    += HandleDash;
    }

    private void OnDisable()
    {
        _inputReader.OnMove           -= HandleMove;
        _inputReader.OnJumpPressed    -= HandleJumpPressed;
        _inputReader.OnSprintHeld     -= HandleSprintHeld;
        _inputReader.OnSprintReleased -= HandleSprintReleased;
        _inputReader.OnDashPressed    -= HandleDash;
    }

    // ── Input handlers ─────────────────────────────────────────────────────────
    private void HandleMove(Vector2 input)    => Board.MoveInput = input;
    private void HandleJumpPressed()          => Board.JumpBufferTimer = _data.JumpBufferTime;
    private void HandleSprintHeld()           => Board.IsSprinting = true;
    private void HandleSprintReleased()       => Board.IsSprinting = false;

    private void HandleDash()
    {
        if (Board.DashCooldownTimer > 0f) return;

        if (Board.MoveInput.sqrMagnitude > 0.01f)
        {
            Vector3 forward     = _cameraController != null ? _cameraController.Forward : Vector3.forward;
            Vector3 right       = new Vector3(forward.z, 0f, -forward.x);
            Board.DashDirection = (forward * Board.MoveInput.y + right * Board.MoveInput.x).normalized;
        }
        else
        {
            Board.DashDirection = _cameraController != null ? _cameraController.Forward : Vector3.forward;
        }

        SetState(new DashingState());
    }

    // ── State machine ──────────────────────────────────────────────────────────
    private void SetState(PlayerStateBase next)
    {
        _currentState?.OnExit(Board);
        _currentState = next;
        _currentState.OnEnter(Board);
    }

    // ── Update ─────────────────────────────────────────────────────────────────
    private void Update()
    {
        Board.IsGrounded = _cc.isGrounded;
        _currentState.Update(Board);
        _cc.Move(Board.Velocity * Time.deltaTime);
    }
}
