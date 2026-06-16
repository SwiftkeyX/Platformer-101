# WorldStateManager

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Foundation — the single source of truth for key/door state within a map

## Summary

WorldStateManager is a scene-level MonoBehaviour (not persistent) that tracks which keys have been collected and which doors have been unlocked in the current map. It exposes `CollectKey(string keyId)` for `KeyPickup` to call, and fires `OnKeyCollected` and `OnDoorUnlocked` C# events that `Door` subscribes to. Its state resets automatically on scene reload.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `GameManager`

---

## Overview

Each map scene contains one `WorldStateManager` GameObject. When the player collects a key, `KeyPickup` calls `WorldStateManager.CollectKey(keyId)`. WorldStateManager records the collection, fires `OnKeyCollected(keyId)`, then fires `OnDoorUnlocked(keyId)` — any `Door` MonoBehaviour subscribed with a matching `_requiredKeyId` opens in response. Because WorldStateManager lives in the map scene (not Bootstrap), its state is destroyed and reset when the scene unloads. No save system exists in Phase 1.

## Player Fantasy

When the player picks up a key, a corresponding door somewhere in the map should open — possibly audible from a distance, reinforcing that collecting the key had a world effect. The connection between key and door is explicit: find the matching key, the door opens, a new area is revealed.

---

## Detailed Design

### Core Rules

1. WorldStateManager is a regular MonoBehaviour in the map scene — not `DontDestroyOnLoad`. One per map scene.
2. `WorldStateManager.Instance` is a static property, set in `Awake`, cleared in `OnDestroy`. It is scene-level: valid only while the map scene is loaded.
3. State is stored in two private `HashSet<string>` fields: `_collectedKeys` and `_unlockedDoors`.
4. `CollectKey(string keyId)` is the only write method:
   a. If `keyId` is already in `_collectedKeys`, return immediately (idempotent — no double-fire).
   b. Add `keyId` to `_collectedKeys`. Fire `OnKeyCollected(keyId)`.
   c. Add `keyId` to `_unlockedDoors`. Fire `OnDoorUnlocked(keyId)`.
5. One key ID maps to one `OnDoorUnlocked` event. Any Door subscribed with a matching `_requiredKeyId` responds. Multiple doors can share the same `_requiredKeyId` (one key, many doors).
6. `IsKeyCollected(string keyId)` → `bool`: read-only query, used by DebugOverlay and Door's `Start` check.
7. `IsDoorUnlocked(string keyId)` → `bool`: read-only query, used by Door's `Start` to handle the case where a key was collected before a Door subscribed (scene load order edge case).
8. Two public read-only collection properties for DebugOverlay: `IReadOnlyCollection<string> CollectedKeys` and `IReadOnlyCollection<string> UnlockedDoors`.
9. WorldStateManager never calls `PlayerController`, `CameraController`, `KeyPickup`, or `Door` directly — communication is one-way (events out, direct calls in from KeyPickup only).
10. WorldStateManager has no `Update` loop. All logic runs in `CollectKey` callbacks.
11. WorldStateManager does not listen to `GameManager.OnGameStateChanged` — key/door state is not paused (no mechanism to collect keys while paused exists in Phase 1 anyway).

### States and Transitions

WorldStateManager has no state machine — it is a pure data store with events. The state at any point is fully described by the contents of `_collectedKeys` and `_unlockedDoors`.

| Data | Initial State | Change Trigger | Change |
|---|---|---|---|
| `_collectedKeys` | Empty set | `CollectKey(keyId)` called | keyId added |
| `_unlockedDoors` | Empty set | `CollectKey(keyId)` called | keyId added |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| KeyPickup | Calls `WorldStateManager.Instance.CollectKey(keyId)` in `OnTriggerEnter`. Direct call — tight coupling is correct here. |
| Door | Subscribes to `OnDoorUnlocked` in `OnEnable`; unsubscribes in `OnDisable`. Calls `IsDoorUnlocked(keyId)` in `Start` to handle late subscription. |
| DebugOverlay | Reads `CollectedKeys` and `UnlockedDoors` properties via direct serialized reference. Read-only. |
| GameManager | No direct interaction. WorldStateManager depends on GameManager only in the sense that Bootstrap must have initialized before any map scene loads. |

---

## Formulas

None — WorldStateManager contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Same key collected twice (e.g., two triggers on same key) | Idempotent — `CollectKey` returns immediately if `keyId` already in `_collectedKeys`; no double event | Prevents doors from toggling or double-opening |
| Door subscribes to `OnDoorUnlocked` after the key has already been collected | Door calls `IsDoorUnlocked(_requiredKeyId)` in `Start` and opens immediately if already unlocked | Handles any scene-load order where Door initializes after a key was collected at scene start |
| No door exists for a collected key | `OnDoorUnlocked` fires with no subscribers — no error; key is still recorded | Valid map design — not every key needs a matching door in every map iteration |
| No key exists for a door's `_requiredKeyId` | Door stays locked forever (no matching key in the map) | Map design error — DebugOverlay will make this visible |
| Scene unloads | `WorldStateManager.OnDestroy` clears `Instance` to null. `_collectedKeys` and `_unlockedDoors` are GC'd with the scene | State correctly resets; no stale instance |
| `WorldStateManager.Instance` accessed from Bootstrap scene (before map loads) | Null — callers must null-check before accessing | WorldStateManager is scene-level, not global |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | WorldStateManager depends on it | Execution-order dependency only — GameManager must exist (via Bootstrap) before map scene loads; no direct calls |
| KeyPickup | It depends on WorldStateManager | Direct method call — KeyPickup calls `CollectKey` |
| Door | It depends on WorldStateManager | Event subscription + query — Door subscribes to `OnDoorUnlocked` and calls `IsDoorUnlocked` |
| DebugOverlay | It depends on WorldStateManager | Read-only data dependency |

---

## Tuning Knobs

None — WorldStateManager has no tunable values. Key/door pairings are determined by the keyId strings set in KeyPickup and Door Inspector fields.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `OnKeyCollected` fires | Handled by Door and DebugOverlay; WorldStateManager produces no visuals | None at WorldStateManager level | MVP |
| `OnDoorUnlocked` fires | Door handles its own open animation | Door handles its own open SFX | MVP |

WorldStateManager fires events; visual and audio responses are the responsibility of the subscribing systems.

---

## Game Feel

### Feel Reference

> "Should feel like Zelda's dungeon keys — collect the small key, hear the matching door unlock somewhere in the room. The state change is immediate and permanent. NOT like a fetch-quest where collecting a key does nothing visible until you walk to the door."
> The `OnDoorUnlocked` event allows doors to respond immediately — even if off-screen — so the player hears/sees feedback the moment the key is collected.

### Input Responsiveness

None — WorldStateManager has no direct input.

### Animation Feel Targets

None at this layer — Door handles its own open animation.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Key collected | Immediate (same frame) | `OnDoorUnlocked` fires; Door begins open sequence |

### Weight and Responsiveness

- **Weight**: Not applicable — data store.
- **Player control**: Not applicable.
- **Snap quality**: State change is instant and permanent. Keys cannot be uncollected.
- **Failure texture**: If a door doesn't open when a key is collected, check keyId strings match exactly (case-sensitive) between KeyPickup and Door Inspector fields. DebugOverlay displays the collected key IDs for comparison.

### Feel Acceptance Criteria

- [ ] Collecting a key causes its matching door to begin opening in the same frame
- [ ] Collecting the same key twice produces no second event or second door trigger
- [ ] After scene reload, all keys and doors return to their initial locked state

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Collected key IDs | DebugOverlay (dev-only) | On `OnKeyCollected` event | Development builds only |
| Unlocked door IDs | DebugOverlay (dev-only) | On `OnDoorUnlocked` event | Development builds only |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| KeyPickup calls CollectKey | `KeyPickup.md` | `KeyPickup.OnTriggerEnter` → `CollectKey` | Direct dependency |
| Door subscribes to OnDoorUnlocked | `Door.md` | `Door.OnEnable` subscription | Event dependency |
| State does not persist across scene loads | `best-practices.md` | "WorldStateManager state does not persist between map loads" rule | Rule dependency |
| DebugOverlay reads state | `DebugOverlay.md` | `CollectedKeys`, `UnlockedDoors` properties | Data dependency |

---

## Acceptance Criteria

- [ ] `WorldStateManager.cs` exists at `Assets/Scripts/WorldStateManager.cs`
- [ ] `WorldStateManager.Instance` is non-null while the map scene is loaded; null after scene unloads
- [ ] `CollectKey("key_A")` fires `OnKeyCollected("key_A")` and `OnDoorUnlocked("key_A")`
- [ ] Calling `CollectKey("key_A")` twice fires events only once
- [ ] `IsKeyCollected("key_A")` returns `true` after collection; `false` before
- [ ] `IsDoorUnlocked("key_A")` returns `true` after `CollectKey("key_A")`
- [ ] A Door that subscribes after its key was collected still opens (via `IsDoorUnlocked` check in `Start`)
- [ ] After scene reload (`SceneLoader.LoadScene`), a fresh WorldStateManager has empty state
- [ ] Zero GC allocations per frame (no Update loop; all work is event-driven)
- [ ] Edit Mode test: `CollectKey` → `IsKeyCollected` returns true; `IsDoorUnlocked` returns true; double-call fires event once

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should multiple keys be required to open one door (AND-gate)? | Designer | Before Tier 2 implementation | Deferred — out of scope for Phase 1. The current design is 1-key-to-N-doors. Multi-key doors would require a door to track a list of required key IDs and check if all are collected. |
| Should key/door state be logged to console on change (for debugging without DebugOverlay)? | Developer | Before Tier 2 testing | Recommended: add `Debug.Log` calls in `CollectKey` gated by `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. |
