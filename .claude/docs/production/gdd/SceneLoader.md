# SceneLoader

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Foundation — the only authorized path for all scene transitions

## Summary

SceneLoader is a singleton MonoBehaviour that wraps `SceneManager.LoadSceneAsync`. It is the only script in the project permitted to call Unity's scene-loading API. All other systems (GameManager, MapSelector, Bootstrap) call `SceneLoader.LoadScene(sceneName)` — never `SceneManager` directly. It prevents double-loads by rejecting new requests while a load is in progress.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `GameManager`

---

## Overview

Bootstrap instantiates SceneLoader in Awake alongside GameManager. SceneLoader sets `SceneLoader.Instance` in its own Awake. When any system needs to change the active gameplay scene, it calls `SceneLoader.LoadScene(string sceneName)`. SceneLoader starts a coroutine that calls `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)`. Because Bootstrap's root GameObject has `DontDestroyOnLoad`, the Bootstrap scene (and SceneLoader itself) persists across all transitions; only the gameplay scene is replaced.

## Player Fantasy

Scene transitions are invisible infrastructure. The player should experience them as a seamless cut — the old scene disappears and the new one is ready. No double-loads, no partial states, no flashes of empty geometry.

---

## Detailed Design

### Core Rules

1. `SceneLoader.Instance` is set in `Awake`. Available from any script after Bootstrap initializes.
2. `LoadScene(string sceneName)` is the only public method. It accepts a scene name string that must exist in Unity's Build Settings.
3. If a load is already in progress (`_isLoading == true`), `LoadScene` logs a warning and returns immediately — the new request is silently dropped.
4. `LoadScene` starts a private coroutine `LoadSceneRoutine(string sceneName)` which calls `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)`.
5. `LoadSceneMode.Single` replaces the current gameplay scene. The Bootstrap scene persists because its root has `DontDestroyOnLoad`.
6. `_isLoading` is set to `true` at the start of `LoadSceneRoutine` and reset to `false` when `AsyncOperation.isDone == true`.
7. `OnSceneLoaded` C# event (optional) fires after the async operation completes, passing the loaded scene name. Subscribers can use this to run post-load setup.
8. SceneLoader never calls `GameManager.SetState` — it loads scenes without imposing a game state. The caller is responsible for state management.
9. SceneLoader never receives input and has no `Update` loop.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | Awake completes; or previous load finishes | `LoadScene()` called while not loading | Accepts new load requests |
| `Loading` | `LoadScene()` called | `AsyncOperation.isDone == true` | Rejects new requests; coroutine awaiting async op |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Bootstrap | Bootstrap instantiates SceneLoader in Awake. Bootstrap calls `SceneLoader.LoadScene(StartupScene)` in Start if configured. |
| GameManager | GameManager calls `SceneLoader.LoadScene(sceneName)` for programmatic scene transitions. SceneLoader holds no reference to GameManager. |
| MapSelector (Tier 3) | MapSelector calls `SceneLoader.LoadScene(sceneName)` when the player selects a map. |
| Any subscriber | May subscribe to `SceneLoader.Instance.OnSceneLoaded` (in OnEnable, unsubscribe in OnDisable) to run post-load setup. |

---

## Formulas

None — SceneLoader contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `LoadScene` called while already loading | Warning logged; new request dropped | Prevents race conditions from double-loads; callers should not fire load requests before checking state |
| Scene name not in Build Settings | Unity logs an error; `AsyncOperation` fails silently; `_isLoading` resets to false via the coroutine completing | This is a configuration error — SceneLoader does not validate scene names at call time |
| `LoadScene("")` called with empty string | Treated as invalid; log error and return without starting coroutine | Empty string would cause Unity to throw; validate before starting the load |
| Duplicate SceneLoader instantiated | Duplicate destroys itself in Awake after detecting Instance is already set | Mirrors Bootstrap's duplicate guard |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | SceneLoader depends on GameManager | Dependency is light — SceneLoader does not call GameManager, but it is instantiated after GameManager by Bootstrap convention |
| Bootstrap | Bootstrap depends on SceneLoader | Ownership handoff — Bootstrap creates it |

---

## Tuning Knobs

None — SceneLoader has no tunable values. Scene names are passed by callers.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Scene load begins | None in Phase 1 (no loading screen) | None | MVP |
| Scene load completes | Gameplay scene appears | None | MVP |

A loading screen or fade transition is out of scope for Phase 1. If added later, it would be driven by a subscriber to `OnSceneLoaded` — SceneLoader itself remains unchanged.

---

## Game Feel

### Feel Reference

> Scene transitions should feel like a clean cut in video editing — the old scene ends, the new one begins. No black flash, no visible Bootstrap scene between maps. The async load should complete fast enough (primitives-only maps) that no loading indicator is needed.

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Scene load triggered | Immediate coroutine start (same frame) | 1 frame to begin |

### Animation Feel Targets

None — SceneLoader has no animations.

### Impact Moments

None.

### Weight and Responsiveness

- **Weight**: Not applicable.
- **Player control**: Not applicable.
- **Snap quality**: Async load runs in background; gameplay scene appears as soon as Unity is ready.
- **Failure texture**: If a scene fails to load, the game sits on an empty state. Phase 1 has no error recovery screen — fix the scene name.

### Feel Acceptance Criteria

- [ ] No visible flash of the Bootstrap scene between map loads
- [ ] Load requests made during an active load are silently dropped without error

---

## UI Requirements

None — SceneLoader has no UI. A future loading screen would be implemented as a subscriber to `OnSceneLoaded`.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| SceneLoader is the only SceneManager caller | `best-practices.md` | "Never call SceneManager.LoadScene directly" rule | Rule dependency |
| Bootstrap calls LoadScene(StartupScene) | `Bootstrap.md` | Bootstrap.Start startup scene field | Data dependency |
| GameManager calls LoadScene | `GameManager.md` | GameManager.SceneLoader reference | Direct dependency |

---

## Acceptance Criteria

- [ ] `SceneLoader.cs` exists at `Assets/Scripts/SceneLoader.cs`
- [ ] `SceneLoader.Instance` is non-null from any scene after Bootstrap has initialized
- [ ] Calling `LoadScene("ValidScene")` loads that scene asynchronously and replaces the current gameplay scene
- [ ] Bootstrap scene (and all DontDestroyOnLoad objects) persists across the transition
- [ ] Calling `LoadScene` while already loading logs a warning and does not start a second load
- [ ] Calling `LoadScene("")` logs an error and does not start a load
- [ ] No other script in the project calls `SceneManager.LoadScene` or `SceneManager.LoadSceneAsync` directly
- [ ] A duplicate SceneLoader destroys itself without error

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should SceneLoader fire OnSceneLoaded before or after the first frame renders? | Developer | Before Tier 2 | Unity's AsyncOperation completes at the start of the frame after the load — OnSceneLoaded fires then; first frame of new scene renders after |
| Should a loading screen / fade be added between maps? | Designer | Before Tier 3 | Deferred — out of scope for Phase 1 graybox |
