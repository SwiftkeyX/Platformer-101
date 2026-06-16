# MapSelector

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Supporting — allows the developer to switch between the three map scenes during rapid iteration

## Summary

MapSelector is a MonoBehaviour on a simple Canvas UI in the MainMenu scene. It renders one button per map scene. When a button is pressed, MapSelector calls `GameManager.SetState(Playing)` and `SceneLoader.LoadScene(sceneName)`. It has no game logic — it is purely a developer shortcut for switching between the three graybox map scenes.

> **Quick reference** — Layer: `Supporting` · Priority: `MVP` · Key deps: `SceneLoader`, `GameManager`

---

## Overview

Phase 1 requires three map scenes (Map_A, Map_B, Map_C). A developer needs a fast way to choose which map to enter from a standing start. MapSelector lives in a lightweight MainMenu scene that loads at startup. It presents three buttons — one per map. The developer clicks a button; the map scene loads. No animations, no fades, no "are you sure?" The game opens in the MainMenu scene; MapSelector is the entire UI on that screen.

## Player Fantasy

This system has no player-facing fantasy — it is a developer tool. The design goal is zero friction: one click → map starts. It must never break scene loading for any reason, because it is the only entry path to the maps.

---

## Detailed Design

### Core Rules

1. MapSelector is a MonoBehaviour in the MainMenu scene, on a GameObject named `MapSelector`.
2. Three serialized string fields hold the exact scene names as they appear in Unity's Build Settings:
   - `[SerializeField] string _mapASceneName` — e.g. `"Map_A"`
   - `[SerializeField] string _mapBSceneName` — e.g. `"Map_B"`
   - `[SerializeField] string _mapCSceneName` — e.g. `"Map_C"`
3. Three UI Buttons in the Canvas, one per map. Each button's `onClick` wires to the corresponding `LoadMapA()`, `LoadMapB()`, or `LoadMapC()` method.
4. Each `LoadMapX()` method:
   a. Calls `GameManager.Instance.SetState(GameState.Playing)`.
   b. Calls `SceneLoader.Instance.LoadScene(_mapXSceneName)`.
   c. Returns immediately — scene loading is async; no waiting in MapSelector.
5. If `SceneLoader.Instance` or `GameManager.Instance` is null in a `LoadMapX()` call, log an error and return — do not throw.
6. `OnValidate`: warn in the Inspector if any `_mapXSceneName` is empty.
7. MapSelector has no `Update` loop. It is entirely event-driven (UI button callbacks).
8. MapSelector never calls `WorldStateManager`, `PlayerController`, `CameraController`, `KeyPickup`, or `Door`.
9. Scene architecture: MapSelector lives in `Assets/Scenes/MainMenu.unity`. It is not in Bootstrap or any gameplay scene.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | MainMenu scene loaded | A map button is pressed | Buttons visible and interactable |
| `Loading` | A map button is pressed | Scene transition completes (MainMenu unloads) | MapSelector no longer in scene — no further behavior |

MapSelector has no active "Loading" state logic — it calls SceneLoader and then ceases to exist as the scene unloads.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| SceneLoader | MapSelector calls `SceneLoader.Instance.LoadScene(sceneName)` on button press. Direct call — intentional. |
| GameManager | MapSelector calls `GameManager.Instance.SetState(GameState.Playing)` before scene load. |
| WorldStateManager | No interaction — WorldStateManager lives in map scenes, not MainMenu. |
| PlayerController | No interaction. |

---

## Formulas

None — MapSelector contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Button pressed while SceneLoader is already loading | SceneLoader silently drops the second request (by its own rule). MapSelector fires the call unconditionally — SceneLoader protects against doubles. | Keeps MapSelector simple; SceneLoader handles the guard. |
| `_mapXSceneName` is not in Build Settings | SceneLoader logs a Unity error; scene fails to load. | Configuration error — not MapSelector's responsibility to validate scene names at runtime. |
| `_mapXSceneName` is empty | `OnValidate` warns in Inspector. If `LoadMapX()` is somehow called with an empty name, SceneLoader logs an error and returns. | Guard in SceneLoader handles this. |
| `GameManager.Instance` is null | Log error and return without scene load. | Bootstrap misconfiguration — safe failure. |
| `SceneLoader.Instance` is null | Log error and return. | Bootstrap misconfiguration — safe failure. |
| Player presses a button twice very fast | Second call is dropped by SceneLoader (already loading). | Idempotent. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| SceneLoader | MapSelector depends on it | Direct method call — calls `LoadScene` on button press |
| GameManager | MapSelector depends on it | Direct method call — calls `SetState(Playing)` before scene load |

---

## Tuning Knobs

| Parameter | Default | Notes |
|---|---|---|
| `_mapASceneName` | `"Map_A"` | Must match the scene name in Unity Build Settings exactly |
| `_mapBSceneName` | `"Map_B"` | Must match the scene name in Unity Build Settings exactly |
| `_mapCSceneName` | `"Map_C"` | Must match the scene name in Unity Build Settings exactly |

---

## Visual / Audio Requirements

| Element | Phase 1 Spec | Phase 3 Notes |
|---|---|---|
| Canvas | Screen-space overlay | No change required |
| Buttons | Unity default button style with text labels: "Map A", "Map B", "Map C" | Replace with styled art if needed |
| Background | Solid color (black or dark grey) | Replace with art |
| Audio | None | A UI click SFX could be added in Phase 3 |

Phase 1: functional only. Three labeled buttons centered on screen. No logo, no title text required for graybox iteration.

---

## Game Feel

### Feel Reference

> "Should feel like a file browser — zero ceremony, immediate action. Developer opens the game, sees three buttons, clicks one, map loads. NOT like a shipping game's main menu with animations, transitions, and options screens."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Button press → scene load begins | Same frame as onClick fires | 0 extra frames |

### Animation Feel Targets

None — no animations in Phase 1.

### Impact Moments

None. The only feedback is the scene changing.

### Weight and Responsiveness

- **Weight**: None — this is infrastructure.
- **Player control**: One click per intent. No multi-step confirmation.
- **Snap quality**: Instant call to SceneLoader; async load happens in background.
- **Failure texture**: If nothing happens when a button is pressed, check: Bootstrap initialized, SceneLoader.Instance non-null, scene name is in Build Settings.

### Feel Acceptance Criteria

- [ ] Clicking a map button begins scene loading with no perceptible delay
- [ ] The MainMenu scene is visible for as short a time as possible before the map loads
- [ ] No button state stays "clicked" or disabled after a press — the scene should load so fast this isn't an issue

---

## UI Requirements

| Element | Value | Notes |
|---|---|---|
| Canvas render mode | Screen Space — Overlay | No camera required |
| Button count | 3 | One per map scene |
| Button labels | "Map A", "Map B", "Map C" | Plain text; no icons required |
| Button layout | Vertical stack, centered | No specific spacing requirement |
| Font | Unity default or project default | Phase 1: default is fine |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Calls SceneLoader.LoadScene | `SceneLoader.md` | `LoadScene(string sceneName)` | Direct dependency |
| Calls GameManager.SetState(Playing) | `GameManager.md` | `SetState(GameState.Playing)` | Direct dependency |
| Never calls WorldStateManager | `best-practices.md` | Architecture contract | Rule dependency |
| Lives in MainMenu scene | `unity-editor.md` | Scene architecture (Bootstrap / MainMenu / GameLogic / HUD split) | Rule dependency |

---

## Acceptance Criteria

- [ ] `MapSelector.cs` exists at `Assets/Scripts/MapSelector.cs`
- [ ] MainMenu scene exists at `Assets/Scenes/MainMenu.unity` with a Canvas containing three map buttons
- [ ] Clicking "Map A" loads Map_A; clicking "Map B" loads Map_B; clicking "Map C" loads Map_C
- [ ] `GameManager.SetState(Playing)` is called before each scene load
- [ ] Empty `_mapXSceneName` logs a warning in `OnValidate`
- [ ] Null `SceneLoader.Instance` or `GameManager.Instance` logs an error without throwing
- [ ] No calls to `WorldStateManager`, `PlayerController`, `CameraController`, `KeyPickup`, or `Door`

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the MainMenu scene also be the scene that Bootstrap loads on startup? | Developer | Before Tier 3 implementation | Recommended yes — set Bootstrap's `StartupScene` to `"MainMenu"`. This way the game always boots into the selector. |
| Should the map buttons show a "loading..." state while a scene is in flight? | Developer | Before Tier 3 implementation | Not recommended for Phase 1 — graybox maps load in under 1 second. Skip the state; add a loading indicator in Phase 3 if needed. |
| Should there be a "Quit" button? | Designer | Before Tier 3 implementation | Recommended yes — add a fourth button that calls `Application.Quit()`. Low cost; expected in any standalone build. |
