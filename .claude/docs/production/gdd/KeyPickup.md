# KeyPickup

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Exploratory — keys are the rewards for exploring the space; collecting one should feel satisfying and immediately meaningful

## Summary

KeyPickup is a MonoBehaviour placed on each key object in the map. When the player's collider enters its trigger, it calls `WorldStateManager.CollectKey(_keyId)` and disables its own GameObject. It has no Update loop, fires no events of its own, and knows nothing about doors or the player's state — those are WorldStateManager's concern.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `WorldStateManager`

---

## Overview

Each key in a map is a GameObject with a trigger `Collider`, a visual mesh (graybox primitive in Phase 1), and a `KeyPickup` component. The Inspector field `_keyId` identifies which door this key corresponds to. When the player walks through the trigger, `OnTriggerEnter` fires, KeyPickup verifies the collider belongs to the player (by tag), calls `WorldStateManager.Instance.CollectKey(_keyId)`, then calls `gameObject.SetActive(false)` to remove the key from the scene. No further logic lives here.

## Player Fantasy

Picking up a key should feel like a small but clear reward — the key vanishes, and somewhere in the map a door responds. The act of collection is immediate and unambiguous: touch the key, it's yours, something changed.

---

## Detailed Design

### Core Rules

1. KeyPickup requires a `Collider` component on the same GameObject with `isTrigger = true`.
2. `[SerializeField] string _keyId` must be set in the Inspector. It must exactly match the `_requiredKeyId` on the corresponding `Door` (case-sensitive).
3. `OnTriggerEnter(Collider other)` is the only logic method:
   a. Check `other.CompareTag("Player")` — if false, return immediately.
   b. Call `WorldStateManager.Instance.CollectKey(_keyId)`.
   c. Call `gameObject.SetActive(false)`.
4. KeyPickup has no `Update`, `FixedUpdate`, or `Start` loop — it is entirely event-driven.
5. KeyPickup never references `PlayerController`, `CameraController`, `GameManager`, or `SceneLoader`.
6. KeyPickup calls `WorldStateManager.Instance` — it does not hold a cached serialized reference. `WorldStateManager.Instance` is safe to call in `OnTriggerEnter` (not in Update, so no per-frame allocation concern).
7. The key GameObject is disabled (not destroyed) on collection — `SetActive(false)` avoids triggering physics re-bake and is sufficient for Phase 1.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Active` | Scene loads; GameObject is active | Player trigger enters | Collider is live; key is visible and collectable |
| `Collected` | Player trigger enters and tag matches | Never (permanent in scene lifetime) | `gameObject.SetActive(false)`; key is invisible and non-collidable |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| WorldStateManager | `OnTriggerEnter` calls `WorldStateManager.Instance.CollectKey(_keyId)`. Direct call — tight coupling is correct and intentional. |
| PlayerController | No direct interaction. KeyPickup only checks the entering collider's tag ("Player"). PlayerController is never referenced. |

---

## Formulas

None — KeyPickup contains no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Non-player collider enters trigger (e.g., a physics object) | `CompareTag("Player")` returns false; method returns immediately | Prevents accidental collection by non-player objects |
| Player enters trigger while key is already disabled | `OnTriggerEnter` does not fire on disabled GameObjects — no double-collect | Unity's trigger system only fires on active colliders |
| `WorldStateManager.Instance` is null when trigger fires | Null reference exception — this should not happen if Bootstrap and scene loading are correct | Guard with a null-check and a `Debug.LogError` for safety: `if (WorldStateManager.Instance == null) { Debug.LogError(...); return; }` |
| `_keyId` is empty string in Inspector | `CollectKey("")` is called; WorldStateManager stores an empty-string key | Configuration error — log a warning in `OnValidate` if `_keyId` is empty |
| Scene reloads | Key GameObject is re-instantiated from the scene with `SetActive(true)` — key reappears | Correct behavior: state resets with scene reload since WorldStateManager also resets |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| WorldStateManager | KeyPickup depends on it | Direct method call — calls `CollectKey` on collection |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `_keyId` (string, Inspector) | `""` (must be set) | Any non-empty string | N/A — not a numeric knob | N/A |

No numeric tuning knobs. The only configurable value is `_keyId`, which is a design parameter set per key instance.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Key collected (`SetActive(false)`) | Key mesh disappears instantly (Phase 1) | Key collect SFX (Phase 3) | Phase 1: instant disappear. Phase 3: pop/sparkle effect |

Phase 1: the key simply disappears. A particle burst or animation is Phase 3 scope.

---

## Game Feel

### Feel Reference

> "Should feel like collecting a coin in Mario — immediate, clean removal from the world, with a corresponding door response. NOT like a key that sits in an inventory and requires separate 'use' input."
> Collection is automatic on touch. No button press required.

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Key collected on touch | Same frame as `OnTriggerEnter` (physics update) | 0 extra frames |

### Animation Feel Targets

Phase 1 — no animation. Key disappears instantly via `SetActive(false)`.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Key collected | Immediate | Key disappears; WorldStateManager fires events; Door begins open sequence |

### Weight and Responsiveness

- **Weight**: Light — the act of collection is weightless; the consequence (door opening) carries the weight.
- **Player control**: Automatic on contact. No player decision required.
- **Snap quality**: Instant removal. No linger.
- **Failure texture**: If a key doesn't collect, check: Player tag set to "Player", Collider is trigger, `_keyId` is non-empty and matches Door's `_requiredKeyId`.

### Feel Acceptance Criteria

- [ ] Key disappears on the same frame the player touches it
- [ ] No key can be collected twice (WorldStateManager's idempotency handles this, but verify visually)

---

## UI Requirements

None — KeyPickup has no UI. A "key collected" HUD indicator is out of scope for Phase 1.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Calls WorldStateManager.CollectKey | `WorldStateManager.md` | `CollectKey(string keyId)` method | Direct dependency |
| Player identified by tag | Unity Tag system | Tag "Player" on Player GameObject | Rule dependency |
| _keyId must match Door._requiredKeyId | `Door.md` | `_requiredKeyId` Inspector field | Data dependency |

---

## Acceptance Criteria

- [ ] `KeyPickup.cs` exists at `Assets/Scripts/KeyPickup.cs`
- [ ] Player entering the trigger collider calls `WorldStateManager.CollectKey(_keyId)` and disables the key GameObject
- [ ] Non-player colliders entering the trigger have no effect
- [ ] `_keyId` empty string triggers a warning in `OnValidate`
- [ ] Null `WorldStateManager.Instance` logs an error and returns without crashing
- [ ] Key reappears (re-enabled) after scene reload

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the key have a visual idle animation (floating/rotating) in Phase 1? | Developer | Before implementation | Recommended: a simple `transform.Rotate` in Update for readability in graybox. Low cost, makes keys visible during movement. |
| Should keys have a magnetic pull toward the player when nearby? | Designer | Phase 3 | Deferred — out of scope for Phase 1. |
