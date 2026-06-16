# Door

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Exploratory — a door opening reveals new space; it is the map's primary reward for exploration

## Summary

Door is a MonoBehaviour on each door GameObject. It subscribes to `WorldStateManager.OnDoorUnlocked` in `OnEnable` and responds when the event fires with a matching `_requiredKeyId` by disabling its collider and playing a slide-open animation. A `Start` check handles the case where the key was collected before the Door subscribed. It never calls `PlayerController` or `InputReader`.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `WorldStateManager`

---

## Overview

Each door in a map is a GameObject with a `Collider`, a visual mesh (graybox primitive), and a `Door` component. The Inspector field `_requiredKeyId` must match the `_keyId` on the corresponding `KeyPickup`. When `WorldStateManager` fires `OnDoorUnlocked(_requiredKeyId)`, Door immediately disables its collider (player can pass through) then runs a slide-open coroutine that moves the door mesh upward over `OpenDuration` seconds. After the coroutine completes, the door is fully open and passable. The state is permanent for the scene's lifetime.

## Player Fantasy

The door opening is the moment of payoff — the player found the key, collected it, and now a barrier they couldn't cross before swings (slides) open. The door should move visibly and take just long enough to feel like a real door, not an instant disappear. The player should be able to walk through while it's still opening.

---

## Detailed Design

### Core Rules

1. Door requires a `Collider` component on the same GameObject (or a child "blocker" object) for the locked barrier.
2. `[SerializeField] string _requiredKeyId` must be set in the Inspector. Must match a `KeyPickup._keyId` exactly (case-sensitive).
3. `OnEnable`: subscribe to `WorldStateManager.Instance.OnDoorUnlocked += HandleDoorUnlocked`.
4. `OnDisable`: unsubscribe — `WorldStateManager.Instance.OnDoorUnlocked -= HandleDoorUnlocked`.
5. `Start`: call `WorldStateManager.Instance.IsDoorUnlocked(_requiredKeyId)` — if true, open immediately without animation (handles late subscription after key was already collected).
6. `HandleDoorUnlocked(string keyId)`:
   a. If `keyId != _requiredKeyId`, return immediately.
   b. If already in `Open` or `Opening` state, return (idempotent).
   c. Transition to `Opening`: disable the Collider immediately (player can walk through at once).
   d. Start `OpenCoroutine`.
7. `OpenCoroutine`: lerp the door's local Y position from 0 to `+OpenDistance` over `OpenDuration` seconds using `Time.deltaTime`. On completion, set state to `Open`.
8. When the door opens instantly (late-subscription case in `Start`): teleport to open position immediately (`transform.localPosition += Vector3.up * OpenDistance`), no coroutine.
9. Door never calls `PlayerController`, `CameraController`, `GameManager`, `KeyPickup`, or `SceneLoader`.
10. Door has no Update loop — behavior is entirely driven by events and coroutines.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Locked` | Scene loads | `HandleDoorUnlocked` fires with matching keyId | Door mesh blocks passage; Collider active |
| `Opening` | `HandleDoorUnlocked` fires | `OpenCoroutine` completes | Collider disabled (passable); mesh lerping upward |
| `Open` | `OpenCoroutine` completes, OR instant-open in `Start` | Never — permanent | Collider disabled; mesh at open position |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| WorldStateManager | Door subscribes to `OnDoorUnlocked` in `OnEnable`; unsubscribes in `OnDisable`. Calls `IsDoorUnlocked` in `Start` for late-subscription guard. |
| PlayerController | No interaction — Door only tracks world state, not player position. |

---

## Formulas

### Door Slide (Lerp)

```
float elapsed = 0f;
Vector3 startPos = transform.localPosition;
Vector3 endPos   = startPos + Vector3.up * OpenDistance;

while (elapsed < OpenDuration) {
    elapsed += Time.deltaTime;
    float t = Mathf.Clamp01(elapsed / OpenDuration);
    transform.localPosition = Vector3.Lerp(startPos, endPos, t);
    yield return null;
}
transform.localPosition = endPos;
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `OpenDistance` | float | 1–6 m | Inspector | How far the door slides upward |
| `OpenDuration` | float | 0.3–3.0 s | Inspector | Time for the slide to complete |

**Expected output**: Door mesh moves from `startPos` to `startPos + (0, OpenDistance, 0)` over `OpenDuration` seconds.
**Edge cases**: If `OpenDuration` is set to 0, a division-by-zero occurs in `t = elapsed / OpenDuration`. Clamp minimum to 0.05s in Inspector or guard in code.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `HandleDoorUnlocked` fires while already `Opening` | Return immediately (state guard) | Prevents second coroutine from starting; idempotent |
| `HandleDoorUnlocked` fires while already `Open` | Return immediately (state guard) | Key collected twice (impossible per WorldStateManager, but guard anyway) |
| Door's key was collected before Door's `OnEnable` ran | `Start` calls `IsDoorUnlocked`; instant-open without animation | Handles unusual scene activation order |
| `WorldStateManager.Instance` is null in `OnEnable` | Log error and skip subscription | Bootstrap misconfiguration; door stays locked |
| `_requiredKeyId` is empty string | Door never opens (no event matches empty string); log warning in `OnValidate` | Configuration error |
| `OpenDuration` = 0 | Guard: clamp to minimum 0.05s | Prevents divide-by-zero in lerp calculation |
| Game paused during open animation | `Time.deltaTime == 0`; coroutine stalls mid-animation | Expected: door pauses mid-slide while game is paused. Acceptable for Phase 1. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| WorldStateManager | Door depends on it | Event subscription — subscribes to `OnDoorUnlocked`; queries `IsDoorUnlocked` |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `_requiredKeyId` (string, Inspector) | `""` (must be set) | Any non-empty string matching a KeyPickup | N/A | N/A |
| `OpenDistance` | 3.0 m | 1–6 | Door slides further up; more clearance | Door barely clears; player may clip top |
| `OpenDuration` | 0.8 s | 0.05–3.0 | Slower, more dramatic open | Faster, snappier open |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Door unlocked (`Opening` state begins) | Collider disables; mesh begins sliding up | Door open SFX (Phase 3) | Phase 1: visible slide. Phase 3: creak SFX |
| Door fully open | Mesh at open position | None | MVP |
| Door locked (initial state) | Solid mesh blocking passage | None | MVP |

Phase 1: door is a colored primitive (grey or red = locked, green = open — use a material swap). Phase 3: replace with art.

---

## Game Feel

### Feel Reference

> "Should feel like a dungeon door in Zelda: Link's Awakening — the barrier slides away with a clear, deliberate motion. The player knows it's done. NOT like a door that pops open instantly or takes so long it feels like a loading screen."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Collider disabled on unlock | Same frame as event (OnDoorUnlocked callback) | 0 extra frames |
| Visual slide begins | Same frame as collider disable | 0 extra frames |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Door slide | 0 (instant start) | ~48 (at 0.8s × 60fps) | 0 | Deliberate, satisfying, unhurried |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Door opens | 0.8s slide | Physical barrier removed; new area revealed |

### Weight and Responsiveness

- **Weight**: Medium — the door takes `OpenDuration` to travel, giving it physical presence.
- **Player control**: The player doesn't control the door — they observe it. The payoff is the revealed space beyond.
- **Snap quality**: Collider disables instantly (player can pass through immediately); visual is smooth lerp.
- **Failure texture**: If the door doesn't open, check: `_requiredKeyId` matches the KeyPickup exactly, WorldStateManager.Instance is non-null, Door's OnEnable subscribed before the key was collected (or Start guard handled it).

### Feel Acceptance Criteria

- [ ] Door begins sliding the same frame the key is collected (no perceptible delay)
- [ ] Player can walk through the door opening while it is still in motion
- [ ] Door open animation completes cleanly at `OpenDistance` without overshooting
- [ ] A door that was already unlocked (scene loaded after key collection impossible — same scene) opens correctly via Start guard

---

## UI Requirements

None — Door has no UI. A "door unlocked" indicator in the HUD is out of scope for Phase 1.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Subscribes to OnDoorUnlocked | `WorldStateManager.md` | `OnDoorUnlocked(string keyId)` event | Event dependency |
| Calls IsDoorUnlocked in Start | `WorldStateManager.md` | `IsDoorUnlocked(string keyId)` method | Data dependency |
| _requiredKeyId must match KeyPickup._keyId | `KeyPickup.md` | `_keyId` Inspector field | Data dependency |
| Never calls PlayerController or InputReader | `best-practices.md` | "Door must never call PlayerController or InputReader" rule | Rule dependency |
| Subscribe in OnEnable / OnDisable | `best-practices.md` | Event subscription pattern | Rule dependency |

---

## Acceptance Criteria

- [ ] `Door.cs` exists at `Assets/Scripts/Door.cs`
- [ ] Collecting the matching key causes the door's Collider to disable on the same frame
- [ ] Door mesh slides from start position to `start + (0, OpenDistance, 0)` over `OpenDuration` seconds
- [ ] Player can walk through the door while the animation is in progress
- [ ] `HandleDoorUnlocked` called twice does not start a second coroutine
- [ ] `_requiredKeyId` empty string logs a warning in `OnValidate`
- [ ] `WorldStateManager.Instance` null in `OnEnable` logs an error without crashing
- [ ] `OpenDuration = 0` does not cause a divide-by-zero (guarded)
- [ ] No calls to `PlayerController` or `InputReader` in this script

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the door use a material swap (red=locked, green=open) for graybox readability? | Developer | Before implementation | Recommended yes — swap to a green material when `Opening` begins. Requires two materials on the door prefab. |
| Should `OpenDistance` automatically match the door mesh height, or be set manually? | Developer | Before implementation | Set manually in Inspector. Automatic sizing requires reading mesh bounds, which adds complexity not needed for Phase 1. |
| Should the door slide up (disappear into ceiling) or rotate open (swing)? | Designer | Before implementation | Default is slide-up (simpler to implement, no rotation math). Swing requires a pivot point and quaternion lerp — defer to Phase 3 if desired. |
