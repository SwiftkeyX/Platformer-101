using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] private InputReader      _inputReader;
    [SerializeField] private PlayerData       _data;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private LayerMask        _groundMask = ~0;

    private CharacterController _cc;
    private PlayerStateBase     _currentState;
    private InputProcessor      _inputProcessor;

    internal PlayerStateGlobal Global { get; private set; }

    // ── Pre-allocated state roster (no GC on transitions) ─────────────────────
    private readonly IdleState     _idle     = new IdleState();
    private readonly WalkState     _walk     = new WalkState();
    private readonly RunState      _run      = new RunState();
    private readonly AirborneState _airborne = new AirborneState();
    private readonly DashingState  _dashing  = new DashingState();

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cc    = GetComponent<CharacterController>();
        Global = new PlayerStateGlobal
        {
            Input       = _inputReader,
            Data        = _data,
            CameraCtrl  = _cameraController,
            SwitchState = SetState,
            Idle        = _idle,
            Walk        = _walk,
            Run         = _run,
            Airborne    = _airborne,
            Dashing     = _dashing
        };
        _inputProcessor = new InputProcessor(Global);
    }

    private void OnEnable()
    {
        _inputProcessor.Subscribe(Global.Input);
        if (_currentState == null) SetState(_airborne.Configure(JumpType.Buffered));
    }

    private void OnDisable() => _inputProcessor.Unsubscribe(Global.Input);
    private void OnDestroy() => _inputProcessor.Unsubscribe(Global.Input);

    // ── State machine ──────────────────────────────────────────────────────────
    private void SetState(PlayerStateBase next)
    {
        _currentState?.OnExit();
        _currentState = next;
        _currentState.OnEnter(Global);
    }

    // ── Update ─────────────────────────────────────────────────────────────────
    private void Update()
    {
        Global.IsGrounded = ComputeIsGrounded();
        _currentState.Update();
        _cc.Move(Global.Velocity * Time.deltaTime);
    }

    // cc.isGrounded reflects the previous Move() result; SphereCast catches thin-edge misses
    // (slopes, platform edges) where isGrounded reports false despite surface contact.
    private bool ComputeIsGrounded()
    {
        if (_cc.isGrounded) return true;
        Vector3 center = transform.position + Vector3.up * _cc.radius;
        return Physics.SphereCast(center, _cc.radius * 0.9f, Vector3.down,
            out _, _cc.skinWidth + 0.1f, _groundMask, QueryTriggerInteraction.Ignore);
    }
}
