# Bootstrap

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Foundation — enables all other systems to exist

## Summary

Bootstrap is the persistent entry-point scene and its sole MonoBehaviour. It runs before any gameplay scene loads, instantiates GameManager and SceneLoader, and calls `DontDestroyOnLoad` on itself so those singletons survive all subsequent scene transitions. No other script in the project may call `DontDestroyOnLoad`.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

---

## Overview

When the game launches, Unity loads the Bootstrap scene (build index 0). `Bootstrap.cs` runs immediately — it creates GameManager and SceneLoader as child GameObjects, marks the Bootstrap root as persistent, and then optionally loads a startup scene. All subsequent scene loads go through SceneLoader. Bootstrap never unloads; it exists for the lifetime of the application.

## Player Fantasy

Bootstrap is invisible to the player. Its purpose is to guarantee that global systems (game state, scene transitions) are available from the first frame of any scene, without requiring each scene to carry its own manager prefabs.

---

## Detailed Design

### Core Rules

1. Bootstrap scene must be build index 0 — Unity loads it first on launch.
2. Bootstrap scene is **never unloaded**. All subsequent scenes are loaded additively on top of it via `SceneLoader`. This is what makes its objects persistent — not `DontDestroyOnLoad`.
3. `GameManager` and `SceneLoader` are placed in the Bootstrap scene as child GameObjects of the Bootstrap root **in the editor** — they are not instantiated at runtime. Their own `Awake` methods set `GameManager.Instance` and `SceneLoader.Instance`.
4. `Bootstrap.cs` sets `Bootstrap.Instance` in `Awake`. It never calls `DontDestroyOnLoad` — that call is unnecessary and forbidden project-wide (see `unity-editor.md`).
5. If `StartupScene` (Inspector field) is non-empty, Bootstrap calls `SceneLoader.LoadScene(StartupScene)` in `Start` — after all `Awake` calls have completed.
6. If `StartupScene` is empty, Bootstrap does nothing after initialization — this is the valid state for Tier 1 testing.
7. Bootstrap never destroys itself. It never reloads. It has no update loop.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| Initializing | Application launch (Awake) | Awake complete | Instantiates GameManager and SceneLoader, calls DontDestroyOnLoad |
| Ready | Awake complete | Never — Bootstrap is permanent | Holds references; optionally loads startup scene in Start |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| GameManager | Bootstrap instantiates it as a child GameObject in Awake. GameManager.Awake then sets `GameManager.Instance`. Bootstrap holds a private reference but does not call methods on it after creation. |
| SceneLoader | Bootstrap instantiates it as a child GameObject in Awake. SceneLoader.Awake sets `SceneLoader.Instance`. Bootstrap calls `SceneLoader.LoadScene(StartupScene)` in Start if the field is set. |

---

## Formulas

None — Bootstrap contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Bootstrap scene is not build index 0 | Other scenes load before Bootstrap; GameManager and SceneLoader do not exist | Build settings must be correct — this is a configuration error, not a runtime error to handle |
| StartupScene field is empty | Bootstrap initializes but loads nothing — game sits on empty Bootstrap scene | Valid state for Tier 1 integration testing; MapSelector loads scenes in Tier 3 |
| Bootstrap scene is loaded a second time (e.g., by accident) | DontDestroyOnLoad prevents a second Bootstrap from persisting; duplicate is destroyed | Bootstrap.Awake checks for existing Instance and self-destructs if one already exists |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | Bootstrap creates it | Ownership handoff — Bootstrap instantiates, GameManager owns its own state thereafter |
| SceneLoader | Bootstrap creates it | Ownership handoff — Bootstrap instantiates, SceneLoader owns its own logic thereafter |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `StartupScene` (string, Inspector) | `""` (empty) | Any valid scene name in build settings | Loads that scene on start | Empty = no auto-load |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Bootstrap loads | None — invisible to player | None | MVP |

---

## Game Feel

### Feel Reference

> Bootstrap has no player-facing feel. It must be invisible — the player should never notice it exists. The target is zero-latency initialization: gameplay scene appears without a visible hitch or flash of the Bootstrap scene.

### Input Responsiveness

None — Bootstrap has no input.

### Animation Feel Targets

None — Bootstrap has no animations.

### Impact Moments

None.

### Weight and Responsiveness

- **Weight**: Not applicable — Bootstrap is infrastructure.
- **Player control**: Not applicable.
- **Snap quality**: Initialization must complete in a single frame (all in Awake/Start). No async operations.
- **Failure texture**: If Bootstrap fails, the game is broken — no graceful degradation. Fix the configuration.

### Feel Acceptance Criteria

- [ ] Player never sees the Bootstrap scene — the startup scene loads before the first rendered frame is visible to the player (or the Bootstrap scene is visually blank)
- [ ] No frame hitch during initialization — Bootstrap Awake completes in < 1ms

---

## UI Requirements

None — Bootstrap has no UI.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Bootstrap creates GameManager | `GameManager.md` | `GameManager.Instance` static property | Ownership handoff |
| Bootstrap creates SceneLoader | `SceneLoader.md` | `SceneLoader.Instance` static property | Ownership handoff |
| DontDestroyOnLoad restriction | `best-practices.md` | "Never call DontDestroyOnLoad outside Bootstrap" | Rule dependency |

---

## Acceptance Criteria

- [ ] Bootstrap scene exists at `Assets/Scenes/Bootstrap.unity` and is build index 0
- [ ] `Bootstrap.cs` exists at `Assets/Scripts/Bootstrap.cs`
- [ ] After launch, `GameManager.Instance` is non-null from any scene
- [ ] After launch, `SceneLoader.Instance` is non-null from any scene
- [ ] No script anywhere in the project calls `DontDestroyOnLoad`
- [ ] If a second Bootstrap scene were somehow loaded, the duplicate destroys itself without error
- [ ] `StartupScene` field is empty by default; setting it to a valid scene name causes that scene to load on Start

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should Bootstrap show a loading screen during startup scene load? | Designer | Before Tier 3 | Deferred — no loading screen in scope for Phase 1 |
