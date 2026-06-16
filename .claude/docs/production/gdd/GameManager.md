# GameManager

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Foundation — owns global game state and lifecycle events

## Summary

GameManager is a singleton MonoBehaviour that owns the `GameState` enum and fires `OnGameStateChanged` whenever the state transitions. It is the single source of truth for whether the game is playing or paused. All systems that need to react to state changes subscribe to its event; no system polls state directly.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `Bootstrap`

---

## Overview

Bootstrap instantiates GameManager in Awake. GameManager sets `GameManager.Instance` in its own Awake, making it available to every subsequent script. It exposes `SetState(GameState)` as the only way to change game state — this method updates the internal state and fires the `OnGameStateChanged` C# event. GameManager also holds a reference to `SceneLoader` (set after Bootstrap instantiates it) and can call `SceneLoader.LoadScene()` when a programmatic scene transition is needed.

## Player Fantasy

GameManager is invisible to the player. Pausing the game should feel instantaneous and reliable — the player presses pause and everything freezes without lag or partial-update artifacts.

---

## Detailed Design

### Core Rules

1. `GameManager.Instance` is set in `Awake`. It is available from any script whose `Awake` or `Start` runs after Bootstrap's `Awake` (which is guaranteed by build order since Bootstrap is scene index 0).
2. State is changed exclusively via `SetState(GameState newState)`. No external code sets `_currentState` directly.
3. `SetState` is a no-op if `newState == _currentState` — it does not fire the event for identical transitions.
4. When state transitions to `Paused`, GameManager sets `Time.timeScale = 0f`.
5. When state transitions to `Playing`, GameManager sets `Time.timeScale = 1f`.
6. `OnGameStateChanged` is a C# `event Action<GameState>` — subscribers receive the new state.
7. GameManager does not subscribe to any events from other systems — it is only a source, never a listener.
8. GameManager holds a direct reference to `SceneLoader` (set by Bootstrap after both are instantiated). It calls `SceneLoader.LoadScene(sceneName)` when a programmatic scene change is required.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Uninitialized` | GameManager.Awake runs | `SetState(Playing)` called | No timeScale change; initial state before any gameplay begins |
| `Playing` | `SetState(Playing)` called | `SetState(Paused)` called | `Time.timeScale = 1f`; normal gameplay |
| `Paused` | `SetState(Paused)` called | `SetState(Playing)` called | `Time.timeScale = 0f`; all physics and Update loops using `deltaTime` freeze |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Bootstrap | Bootstrap instantiates GameManager in Awake and holds the GameObject reference. GameManager.Awake sets Instance. |
| SceneLoader | GameManager holds a direct `SceneLoader` reference (injected by Bootstrap). Calls `SceneLoader.LoadScene(name)` for programmatic transitions. |
| Any subscriber | Subscribes to `GameManager.Instance.OnGameStateChanged` in `OnEnable`; unsubscribes in `OnDisable`. GameManager never holds subscriber references directly. |

---

## Formulas

None — GameManager contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `SetState` called with current state | No-op — event not fired, timeScale unchanged | Prevents redundant subscribers from double-processing identical states |
| Scene loads while Paused | `Time.timeScale` remains 0 until `SetState(Playing)` is explicitly called | Scene transitions do not auto-resume gameplay — the system that triggers the load is responsible for resuming |
| GameManager.Instance accessed before Bootstrap runs | Null reference — callers must not access Instance before Bootstrap initializes | Build order (Bootstrap = index 0) guarantees this does not happen in production |
| Duplicate GameManager instantiated | Duplicate destroys itself in Awake after detecting Instance is already set | Defensive guard matching Bootstrap's own duplicate-protection pattern |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Bootstrap | Bootstrap depends on GameManager | Ownership handoff — Bootstrap creates it; GameManager owns its own state |
| SceneLoader | GameManager depends on SceneLoader | Direct reference — GameManager calls LoadScene when it needs to trigger a transition |

---

## Tuning Knobs

None — GameManager has no tunable values. `Time.timeScale` values (0 and 1) are fixed by Unity convention.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| State → Paused | Handled by any UI subscriber to OnGameStateChanged | Audio pause handled by any audio subscriber | MVP |
| State → Playing | Handled by any UI subscriber to OnGameStateChanged | Audio resume handled by any audio subscriber | MVP |

GameManager fires the event; visual and audio responses are the responsibility of their respective systems.

---

## Game Feel

### Feel Reference

> Pausing should feel like flipping a switch — instant and complete. No partial frames, no physics settling after freeze. Every Update loop using `Time.deltaTime` must stop the same frame the pause fires.

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Pause toggle | 1 frame (next Update cycle after input detected) | 1 frame |

### Animation Feel Targets

None — GameManager has no animations.

### Impact Moments

None.

### Weight and Responsiveness

- **Weight**: Not applicable.
- **Player control**: Pause is committed — no partial-pause state.
- **Snap quality**: Binary. State is either Playing or Paused with no interpolation.
- **Failure texture**: If pause doesn't feel instant, check that all time-sensitive systems use `Time.deltaTime` (not `Time.unscaledDeltaTime`) so they respect `timeScale = 0`.

### Feel Acceptance Criteria

- [ ] Pausing freezes all movement and physics in the same frame the input is processed
- [ ] Resuming restores movement immediately with no stutter

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Paused indicator | Overlay (handled by a future UI system) | On state change | When `GameState == Paused` |

GameManager does not directly manage UI — it fires `OnGameStateChanged` and any UI system subscribes.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| GameManager calls SceneLoader | `SceneLoader.md` | `SceneLoader.LoadScene(string)` | Direct dependency |
| Bootstrap creates GameManager | `Bootstrap.md` | Bootstrap.Awake instantiation | Ownership handoff |
| Event subscription pattern | `best-practices.md` | OnEnable/OnDisable subscription rule | Rule dependency |

---

## Acceptance Criteria

- [ ] `GameManager.cs` exists at `Assets/Scripts/GameManager.cs`
- [ ] `GameManager.Instance` is non-null from any scene after Bootstrap has initialized
- [ ] Calling `SetState(Paused)` sets `Time.timeScale` to `0` and fires `OnGameStateChanged(Paused)`
- [ ] Calling `SetState(Playing)` sets `Time.timeScale` to `1` and fires `OnGameStateChanged(Playing)`
- [ ] Calling `SetState` with the current state fires no event and changes no timeScale
- [ ] A duplicate GameManager instantiated at runtime destroys itself without error
- [ ] Zero GC allocations in `SetState` and the event fire path

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should GameManager own a pause-input binding, or should a future InputReader action fire SetState? | Developer | Before Tier 2 | Defer — for Phase 1 testing, call SetState directly from a dev key binding |
