# InputReader

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Foundation — decouples input hardware from gameplay logic

## Summary

InputReader is a ScriptableObject that wraps Unity's Input System. It enables an Input Action Map in `OnEnable`, subscribes to action callbacks, and fires typed C# events (`OnMove`, `OnJumpPressed`, `OnInteractPressed`) that gameplay scripts subscribe to. No gameplay script reads hardware input directly — all input flows through InputReader.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

---

## Overview

InputReader is created as a project asset (`Assets/Input/InputReader.asset`) and assigned to subscribers (e.g. `PlayerController`) via a serialized Inspector field. When the asset is enabled (loaded into a scene), it activates the `Gameplay` action map from the project's `InputActionAsset` and wires each action to a private callback method. Those callbacks fire the public C# events. When disabled, it removes all callbacks and deactivates the action map. Subscribers (MonoBehaviours) subscribe to InputReader's events in `OnEnable` and unsubscribe in `OnDisable`.

## Player Fantasy

Controls should feel transparent — the player presses a key and the character responds with zero perceptible delay. The InputReader's job is to ensure that input intent is translated to a game event in the same frame the hardware registers it.

---

## Detailed Design

### Core Rules

1. InputReader is a `ScriptableObject` — not a MonoBehaviour. It is a project asset, not a scene object.
2. InputReader references the project's `InputActionAsset` via a private `[SerializeField]` field, assigned in the Inspector on the asset.
3. `OnEnable` (called when the asset is first referenced by an enabled MonoBehaviour): enable the `Gameplay` action map and subscribe all action callbacks.
4. `OnDisable`: unsubscribe all action callbacks and disable the `Gameplay` action map.
5. Three public C# events are exposed:
   - `event Action<Vector2> OnMove` — fired every frame while move input is non-zero; fired once with `Vector2.zero` when input is released.
   - `event Action OnJumpPressed` — fired once on the frame the jump action is pressed (`.performed` phase).
   - `event Action OnInteractPressed` — fired once on the frame the interact action is pressed (`.performed` phase).
6. InputReader never calls any method on GameManager, PlayerController, or any other system — it only fires events.
7. Subscribers must hold a `[SerializeField] InputReader _inputReader` reference assigned in the Inspector. Never use `FindObjectOfType` or `Resources.Load` to find InputReader at runtime.
8. All three actions must have both keyboard and gamepad bindings. Mouse-only bindings are not permitted for gameplay-critical actions.

### Input Action Map: `Gameplay`

| Action | Type | Keyboard Binding | Gamepad Binding | C# Event Fired |
|---|---|---|---|---|
| `Move` | Value (Vector2) | WASD / Arrow Keys | Left Stick | `OnMove(Vector2)` |
| `Jump` | Button | Space | South Button (A / Cross) | `OnJumpPressed()` |
| `Interact` | Button | E | West Button (X / Square) | `OnInteractPressed()` |

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Disabled` | Asset loaded but no MonoBehaviour has referenced it yet | `OnEnable` called | No action map active; no events fire |
| `Enabled` | `OnEnable` called (first subscriber enables) | `OnDisable` called (last subscriber disables) | `Gameplay` action map active; callbacks wired; events fire on input |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| PlayerController | Holds a serialized `[SerializeField] InputReader` reference. Subscribes to `OnMove`, `OnJumpPressed` in `OnEnable`; unsubscribes in `OnDisable`. |
| Any future subscriber | Same pattern — serialized reference, subscribe in OnEnable, unsubscribe in OnDisable. |
| Unity Input System | InputReader holds the `InputActionAsset` and manages its lifecycle. No other script touches the InputActionAsset directly. |

---

## Formulas

None — InputReader passes through raw input values without transformation.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Both keyboard and gamepad input at the same time | Unity Input System resolves this via device priority; the most recently used device wins | Handled by Input System automatically |
| No subscribers when input fires | Events fire with no listeners — no null reference, no error (C# events with no subscribers are safe) | Standard C# event behavior |
| `Move` input released | InputReader fires `OnMove(Vector2.zero)` on the `.canceled` callback phase | Subscribers must handle zero-vector to stop movement |
| InputActionAsset not assigned in Inspector | `NullReferenceException` in `OnEnable` | Configuration error — the asset reference must be set in the Inspector on the InputReader ScriptableObject asset |
| Scene unloads while input is held | `OnDisable` fires, callbacks are removed, action map deactivated — no dangling callbacks | Correct lifecycle behavior from ScriptableObject OnDisable |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Unity Input System package | InputReader depends on it | Rule dependency — must be installed (com.unity.inputsystem) |
| PlayerController | It depends on InputReader | Event subscription — PlayerController reads InputReader events |

---

## Tuning Knobs

None — InputReader passes raw Input System values. Dead zones and sensitivity are configured in the `InputActionAsset` via the Unity Input System editor, not in code.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Any input | None — InputReader is invisible infrastructure | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Celeste's input — every button press registers on the exact frame it is pressed, with no input buffering at the InputReader level. NOT input that skips or double-fires due to polling in Update."
> InputReader uses event callbacks (`.performed`, `.canceled`) — not polling. This guarantees single-frame precision.

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Jump press | Same frame as hardware event (callback fires in Input System update) | 0 extra frames |
| Move direction | Same frame | 0 extra frames |
| Interact press | Same frame | 0 extra frames |

### Animation Feel Targets

None — InputReader has no animations.

### Impact Moments

None.

### Weight and Responsiveness

- **Weight**: Not applicable.
- **Player control**: Direct — no input buffering or smoothing at this layer. Smoothing, if any, belongs in PlayerController.
- **Snap quality**: Binary for buttons (pressed / not pressed), analog for Move (Vector2 value).
- **Failure texture**: If input feels laggy, check that PlayerController is reading from events (not polling), and that the Input System update mode is set to `Process Events In Dynamic Update`.

### Feel Acceptance Criteria

- [ ] Jump fires exactly once per press — no double-fire, no miss
- [ ] Move value reaches the subscriber in the same frame the key is pressed
- [ ] Releasing Move fires `OnMove(Vector2.zero)` within one frame

---

## UI Requirements

None — InputReader has no UI. A rebinding UI would interact with the `InputActionAsset` directly, not through InputReader.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Subscribers use OnEnable/OnDisable | `best-practices.md` | Event subscription pattern | Rule dependency |
| No FindObjectOfType for InputReader | `best-practices.md` | "InputReader is a ScriptableObject — reference via Inspector" rule | Rule dependency |
| PlayerController subscribes to InputReader | `PlayerController.md` | `OnMove`, `OnJumpPressed` subscription | Data dependency |

---

## Acceptance Criteria

- [ ] `InputReader.cs` exists at `Assets/Scripts/InputReader.cs` as a `ScriptableObject`
- [ ] `InputReader.asset` exists at `Assets/Input/InputReader.asset`
- [ ] The `Gameplay` action map is active when any subscriber MonoBehaviour is enabled
- [ ] `OnMove` fires with a non-zero `Vector2` when WASD or arrow keys are held
- [ ] `OnMove` fires with `Vector2.zero` when movement keys are released
- [ ] `OnJumpPressed` fires exactly once per Space/gamepad South press
- [ ] `OnInteractPressed` fires exactly once per E/gamepad West press
- [ ] All three actions have both keyboard and gamepad bindings
- [ ] No gameplay script calls `Input.GetKey`, `Input.GetAxis`, or reads `InputActionAsset` directly

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should InputReader also expose a `OnPausePressed` event for GameManager? | Developer | Before Tier 2 | Recommended yes — add a `Pause` action (Escape / Start) and fire `OnPausePressed`. GameManager subscribes. Not critical for Tier 1 testing. |
| Should Move input use a `CompositeBinding` (four-button WASD) or a `StickControl` binding? | Developer | Before implementation | Use `2D Vector` composite for keyboard (WASD + arrows) and Left Stick for gamepad — standard Input System pattern. |
