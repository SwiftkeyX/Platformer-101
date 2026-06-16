using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject that wraps Unity Input System (Assets/Input/GameInput.inputactions).
/// Enables the Gameplay action map in OnEnable, wires action callbacks, and fires
/// typed C# events that gameplay scripts subscribe to.
///
/// Usage: assign this asset to subscribers via [SerializeField] in the Inspector.
/// Subscribers must subscribe to events in OnEnable and unsubscribe in OnDisable.
/// Never retrieve this asset at runtime via FindObjectOfType or Resources.Load.
/// </summary>
[CreateAssetMenu(menuName = "Game/InputReader")]
public class InputReader : ScriptableObject
{
    // ── Public events ──────────────────────────────────────────────────────────

    /// <summary>
    /// Fired every frame while move input is non-zero.
    /// Fired once with Vector2.zero when input is released (canceled phase).
    /// </summary>
    public event Action<Vector2> OnMove;

    /// <summary>
    /// Fired exactly once on the frame the jump action is pressed (performed phase).
    /// </summary>
    public event Action OnJumpPressed;

    /// <summary>
    /// Fired exactly once on the frame the interact action is pressed (performed phase).
    /// </summary>
    public event Action OnInteractPressed;

    // ── Inspector-assigned asset ───────────────────────────────────────────────
    [SerializeField] private InputActionAsset _inputActions;

    // ── Cached action map and actions ──────────────────────────────────────────
    private InputActionMap _gameplayMap;
    private InputAction    _moveAction;
    private InputAction    _jumpAction;
    private InputAction    _interactAction;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        if (_inputActions == null)
        {
            Debug.LogError("[InputReader] InputActionAsset is not assigned. " +
                           "Assign Assets/Input/GameInput.inputactions in the Inspector.");
            return;
        }

        _gameplayMap = _inputActions.FindActionMap("Gameplay");
        if (_gameplayMap == null)
        {
            Debug.LogError("[InputReader] Action map 'Gameplay' not found in " + _inputActions.name);
            return;
        }

        _moveAction     = _gameplayMap.FindAction("Move");
        _jumpAction     = _gameplayMap.FindAction("Jump");
        _interactAction = _gameplayMap.FindAction("Interact");

        if (_moveAction == null || _jumpAction == null || _interactAction == null)
        {
            Debug.LogError("[InputReader] One or more actions (Move/Jump/Interact) not found in Gameplay map.");
            return;
        }

        _moveAction.performed     += HandleMove;
        _moveAction.canceled      += HandleMove;
        _jumpAction.performed     += HandleJump;
        _interactAction.performed += HandleInteract;

        _gameplayMap.Enable();
    }

    private void OnDisable()
    {
        if (_gameplayMap == null) return;

        _moveAction.performed     -= HandleMove;
        _moveAction.canceled      -= HandleMove;
        _jumpAction.performed     -= HandleJump;
        _interactAction.performed -= HandleInteract;

        _gameplayMap.Disable();
    }

    // ── Private callbacks (zero GC alloc — named methods, not lambdas) ─────────
    private void HandleMove(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        OnJumpPressed?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        OnInteractPressed?.Invoke();
    }
}
