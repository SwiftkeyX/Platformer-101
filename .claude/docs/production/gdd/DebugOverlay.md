# DebugOverlay

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Supporting — makes playtesting honest; the developer can see exact world state without guessing

## Summary

DebugOverlay is a dev-only MonoBehaviour that displays the current key/door state on screen during play. It subscribes to `WorldStateManager.OnKeyCollected` and `WorldStateManager.OnDoorUnlocked`, then refreshes two text panels showing collected key IDs and unlocked door IDs. It is compiled out of non-development builds using `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. It never modifies game state.

> **Quick reference** — Layer: `Supporting` · Priority: `MVP` · Key deps: `WorldStateManager`, `GameManager`

---

## Overview

During map playtesting, the developer needs to know: "which keys have I collected, and which doors are now unlocked?" DebugOverlay answers this question visually, in real time, without opening the Inspector. It lives on a permanent Canvas in the HUD scene (or the same gameplay scene). When a key is collected, the key ID appears in the "Collected Keys" panel. When a door unlocks, the door ID appears in the "Unlocked Doors" panel. The overlay can be toggled on/off with a keyboard shortcut.

## Player Fantasy

DebugOverlay has no player-facing fantasy — it is a developer tool. The design goal is information density: a glanceable display that tells the developer exactly what the WorldStateManager knows, at all times, with no delay.

---

## Detailed Design

### Core Rules

1. DebugOverlay is wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` at the class level — the entire MonoBehaviour compiles out in release builds.
2. `[SerializeField] WorldStateManager _worldStateManager` — direct Inspector reference to the WorldStateManager in the same map scene. Never resolves via `WorldStateManager.Instance` at runtime. Assigned in the Editor before play.
3. `[SerializeField] TextMeshProUGUI _keyListText` — text component displaying collected key IDs.
4. `[SerializeField] TextMeshProUGUI _doorListText` — text component displaying unlocked door IDs.
5. `[SerializeField] KeyCode _toggleKey = KeyCode.BackQuote` — keyboard key to toggle the overlay panel on/off (default: backtick/grave, the traditional debug key).
6. `OnEnable`: subscribe to `_worldStateManager.OnKeyCollected += HandleKeyCollected` and `_worldStateManager.OnDoorUnlocked += HandleDoorUnlocked`. If `_worldStateManager` is null, log an error and skip subscription.
7. `OnDisable`: unsubscribe from both events.
8. `Start`: call `RefreshDisplay()` to initialize text panels with any state already present in WorldStateManager (handles late instantiation).
9. `HandleKeyCollected(string keyId)` and `HandleDoorUnlocked(string keyId)` both call `RefreshDisplay()`.
10. `RefreshDisplay()`: reads `_worldStateManager.CollectedKeys` and `_worldStateManager.UnlockedDoors` and rebuilds the text for each panel. Zero GC in steady state is not required for a dev tool — string concatenation in RefreshDisplay is acceptable.
11. `Update`: check `Input.GetKeyDown(_toggleKey)` — if pressed, toggle the Canvas group / root panel's `SetActive`.
12. DebugOverlay never calls `SceneLoader`, `PlayerController`, `CameraController`, `KeyPickup`, or `Door`.
13. DebugOverlay is read-only — it reads WorldStateManager data but never modifies it.
14. `GameManager` dependency is a soft dependency: if `GameManager.Instance.CurrentState == GameState.Paused`, the overlay remains visible (useful for debugging while paused). No calls to GameManager are needed — the overlay simply doesn't respond to pause.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Visible` | Scene loads (default); or toggle key pressed while Hidden | Toggle key pressed | Panels visible; text updated on each event |
| `Hidden` | Toggle key pressed while Visible | Toggle key pressed | Panels hidden; event subscriptions still active; display refreshes silently |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| WorldStateManager | Subscribes to `OnKeyCollected` and `OnDoorUnlocked` in `OnEnable`. Reads `CollectedKeys` and `UnlockedDoors` properties in `RefreshDisplay`. Holds a `[SerializeField]` direct reference — not Instance. |
| GameManager | No direct interaction — DebugOverlay intentionally ignores pause state to remain visible during debugging. |
| All other systems | No interaction. |

---

## Formulas

### RefreshDisplay text format

```
// Collected Keys panel:
string keys = "Keys collected: " + _worldStateManager.CollectedKeys.Count + "\n";
foreach (string k in _worldStateManager.CollectedKeys) keys += "  • " + k + "\n";
_keyListText.text = keys;

// Unlocked Doors panel:
string doors = "Doors unlocked: " + _worldStateManager.UnlockedDoors.Count + "\n";
foreach (string d in _worldStateManager.UnlockedDoors) doors += "  • " + d + "\n";
_doorListText.text = doors;
```

This builds a simple bulleted list. String allocation here is acceptable — this is a debug tool, not steady-state gameplay code.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `_worldStateManager` is null (scene missing WorldStateManager) | Log error in `OnEnable`; skip subscription; panels show "—" | Map scene misconfiguration; dev can see the error in console |
| `_keyListText` or `_doorListText` is null (Canvas not wired) | `RefreshDisplay` null-guards both; logs a warning | Avoids null reference crash if overlay is half-configured |
| Scene loads with zero keys collected | Panels show "Keys collected: 0" / "Doors unlocked: 0" | Expected initial state |
| Toggle key conflicts with gameplay input | Backtick is not used by gameplay (Move/Jump/Interact use WASD/Space/E) | Backtick is the conventional debug toggle key; no conflict |
| Overlay toggled off while key is collected | Event subscription is still active; `RefreshDisplay` runs; panel text updates silently; panel remains hidden | Correct — the overlay only hides the panel, not the event subscription |
| Release build | Entire class compiles out via `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD` | No performance cost in release |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| WorldStateManager | DebugOverlay depends on it | Event subscription + read-only data query |
| GameManager | DebugOverlay depends on it | Indirect — GameManager must exist (Bootstrap) before WorldStateManager is valid |

---

## Tuning Knobs

| Parameter | Default | Notes |
|---|---|---|
| `_toggleKey` | `KeyCode.BackQuote` | Change to any key that doesn't conflict with gameplay |
| Panel visibility on start | Visible | The overlay starts visible by default; the developer can toggle it during play |

---

## Visual / Audio Requirements

| Element | Phase 1 Spec |
|---|---|
| Panel background | Semi-transparent dark rectangle (black, alpha 0.6) |
| Text color | White |
| Font | TextMeshPro default |
| Position | Top-left corner of screen |
| Panel size | Fits content (auto-size) |
| Audio | None |

No artistic requirements. Functional legibility is the only goal.

---

## Game Feel

### Feel Reference

> "Should feel like a game's built-in console output — always on, always accurate, minimal visual noise. Developers shouldn't need to look long to get the information they need. NOT like a polished UI element — this is raw data for internal use only."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Key collected → text updates | Same frame as OnKeyCollected event | 0 extra frames |
| Toggle key pressed → panel visible/hidden | Same frame as Input.GetKeyDown | 0 extra frames |

### Animation Feel Targets

None — instant show/hide, no transitions.

### Weight and Responsiveness

- **Weight**: None — it is a read-only display.
- **Player control**: Toggle key is the only interaction.
- **Snap quality**: Instant text refresh on each event.
- **Failure texture**: If the overlay shows no data, check `_worldStateManager` is assigned in Inspector, and that `OnEnable` subscribed correctly (check console for the null-reference warning).

### Feel Acceptance Criteria

- [ ] Text panels update in the same frame a key is collected
- [ ] Toggle key shows/hides the panel without affecting gameplay
- [ ] Panels are legible against any map background (semi-transparent dark backing)

---

## UI Requirements

| Element | Value | Notes |
|---|---|---|
| Canvas render mode | Screen Space — Overlay | Dev overlay must always be on top of the game view |
| Panel count | 2 (keys, doors) | Can be combined into one panel if desired |
| Text component | TextMeshProUGUI | Project standard for all UI text |
| Z-order | Top of Canvas hierarchy | Must not be occluded by other UI elements |
| Toggle | `KeyCode.BackQuote` | Dev key; not visible to players |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Subscribes to OnKeyCollected | `WorldStateManager.md` | `OnKeyCollected(string keyId)` event | Event dependency |
| Subscribes to OnDoorUnlocked | `WorldStateManager.md` | `OnDoorUnlocked(string keyId)` event | Event dependency |
| Reads CollectedKeys, UnlockedDoors | `WorldStateManager.md` | `CollectedKeys`, `UnlockedDoors` properties | Data dependency |
| Compiled out in release | `best-practices.md` | "DebugOverlay stripped before release builds" rule | Rule dependency |
| Subscribe in OnEnable / OnDisable | `best-practices.md` | Event subscription pattern | Rule dependency |

---

## Acceptance Criteria

- [ ] `DebugOverlay.cs` exists at `Assets/Scripts/DebugOverlay.cs`
- [ ] Class is wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
- [ ] Collecting a key causes the key ID to appear in the keys panel on the same frame
- [ ] Unlocking a door causes the door ID to appear in the doors panel on the same frame
- [ ] Toggle key (`BackQuote`) shows and hides the overlay panel
- [ ] Null `_worldStateManager` logs an error in `OnEnable` without crashing
- [ ] Null `_keyListText` or `_doorListText` logs a warning without crashing
- [ ] Overlay remains visible during `GameManager.Paused` state
- [ ] No calls to `SceneLoader`, `PlayerController`, `CameraController`, `KeyPickup`, or `Door`
- [ ] Class does not compile in a release build (verify by checking compiled assembly)

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the overlay also display `GameManager.CurrentState`? | Developer | Before Tier 3 implementation | Recommended yes — adds "State: Playing/Paused" above the key/door lists. One additional read. |
| Should the overlay list ALL keys/doors that SHOULD exist in the map (to show uncollected ones)? | Developer | Before Tier 3 testing | Deferred — would require a "registered keys" list, which doesn't exist yet. For Phase 1, showing only collected items is sufficient. |
| Should DebugOverlay have a keyboard shortcut to reload the current scene? | Developer | Before Tier 3 | Recommended yes — a dev reset shortcut (e.g. `KeyCode.R`) that calls `SceneLoader.Instance.LoadScene(currentScene)` saves time during map iteration. Keep it in DebugOverlay since it's dev-only infrastructure. |
