using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateManager : MonoBehaviour
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
            Input       = _inputReader,
            Data        = _data,
            CameraCtrl  = _cameraController,
            SwitchState = SetState
        };
    }

    private void OnEnable()
    {
        // Initial enable: enter first state (subscribes input via OnEnter).
        // Re-enable after disable: resubscribe without reinitialising state.
        if (_currentState == null) SetState(new AirborneState(0f));
        else _currentState.SubscribeInput(Board);
    }

    private void OnDisable() => _currentState?.UnsubscribeInput(Board);
    private void OnDestroy() => _currentState?.UnsubscribeInput(Board);

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
