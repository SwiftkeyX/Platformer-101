# PlayerController

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Floaty · Precise · Exploratory — every movement decision serves these three words

## Summary

PlayerController is a MonoBehaviour on the Player prefab that drives all 3D movement via a `CharacterController`. It reads `InputReader` events (never polls input directly), applies custom gravity each frame, and implements a floaty jump with full air control, coyote time, and jump buffering. It also handles run (sprint speed modifier), double jump (second mid-air jump), and dash (short gravity-suppressed velocity burst). It never communicates with `WorldStateManager` — key collection belongs to `KeyPickup`.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `InputReader`

---

## Overview

The player character is a capsule controlled by Unity's `CharacterController` component. Each `Update`, PlayerController reads the accumulated horizontal input vector (from `InputReader.OnMove`), applies custom gravity to a vertical velocity, handles jump requests, and calls `CharacterController.Move(velocity * Time.deltaTime)`. The feel target is airy and forgiving — jumps last long, air control is immediate and generous, and the player always feels in command of their trajectory.

## Player Fantasy

The player should feel like they're moving through the space effortlessly. Jumps feel light and floaty — the character hangs in the air for a noticeable moment before descending. Every jump lands where the player intended. The controls never fight back.

---

## Detailed Design

### Core Rules

1. PlayerController uses a `CharacterController` component, never a `Rigidbody`. Physics simulation is not used.
2. Input is received exclusively via `InputReader` events — never via `Input.GetKey`, `Input.GetAxis`, or polling.
3. Horizontal movement is calculated from the latest `OnMove(Vector2)` value each frame. The Y axis of the input vector maps to world Z (forward/back); X maps to world X (left/right).
4. Gravity is applied manually each frame: `_velocity.y += _gravity * Time.deltaTime`. Unity's Physics gravity (`Physics.gravity`) has no effect on CharacterController.
5. When `CharacterController.isGrounded` is true and `_velocity.y < 0`, clamp `_velocity.y = -2f` (prevents gravity accumulation while walking on slopes).
6. Jump fires when: a jump request is pending AND (`CharacterController.isGrounded` is true OR coyote time is active). Jump sets `_velocity.y = JumpVelocity`.
7. During descent (`_velocity.y < 0`), apply an additional fall gravity multiplier: `_velocity.y += _gravity * (FallGravityMultiplier - 1f) * Time.deltaTime`. This makes the fall slightly faster than the rise without making it feel heavy.
8. Air control is fully permitted — horizontal movement uses the same speed in the air as on the ground, multiplied by `AirControlMultiplier`.
9. **Coyote time**: After leaving a grounded surface without jumping, the player retains the ability to jump for `CoyoteTime` seconds. A timer starts when `isGrounded` goes false; if it expires before a jump fires, the jump window closes.
10. **Jump buffering**: If jump is pressed up to `JumpBufferTime` seconds before landing, the jump fires on the first grounded frame. Store the time of the last jump press and check it on grounding.
11. PlayerController never calls `WorldStateManager`, `GameManager`, or `SceneLoader`. It is completely isolated from game state.
12. All movement is applied in `Update` (not `FixedUpdate`) — `CharacterController.Move` is safe to call in Update.
13. **Run**: While `OnSprintHeld` is active, `_isSprinting = true`. Horizontal speed is multiplied by `RunMultiplier`. Run applies on ground and in air with no additional state — it is a speed modifier on the existing movement path.
14. **Double jump**: `_hasDoubleJump` (bool) resets to `true` on the first grounded frame each time the player lands. When a jump request fires while Airborne AND `_hasDoubleJump` is `true`: set `_velocity.y = DoubleJumpVelocity`, set `_hasDoubleJump = false`. Coyote jumps use the first-jump path and do not consume `_hasDoubleJump`. Jump buffer (`JumpBufferTime`) applies to the double jump identically to the first jump.
15. **Dash**: `OnDashPressed` triggers a dash if `_dashCooldown <= 0`. Dash direction = current movement input direction flattened to XZ; if input is zero, fall back to `CameraController.Forward`. During dash (`_isDashing == true`), horizontal velocity is locked to `DashDirection * DashSpeed` and `_velocity.y` is held at `0` (gravity suppressed). Dash runs for `DashDuration` seconds via `_dashTimer`. On dash end, `_isDashing = false`, `_dashCooldown = DashCooldown`. `_dashCooldown` decrements by `Time.deltaTime` each frame. Dash does not consume or reset `_hasDoubleJump`.
16. Subscribe to `OnSprintHeld`, `OnSprintReleased`, and `OnDashPressed` in `OnEnable`; unsubscribe in `OnDisable` (same pattern as existing events).

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Grounded` | `CharacterController.isGrounded == true` | Character leaves ground | Horizontal movement at full speed; coyote timer reset; `_hasDoubleJump` reset to `true` on landing; jump available |
| `Coyote` | `isGrounded` goes false without a jump; `_coyoteTimer > 0` | `_coyoteTimer` expires OR jump fires | Jump still available; horizontal movement continues; `_hasDoubleJump` still `true` |
| `Airborne` | Coyote timer expires OR jump fires | `isGrounded` becomes true | First jump unavailable; double jump available if `_hasDoubleJump`; air control active; fall multiplier applies during descent |
| `Dashing` | `OnDashPressed` fires and `_dashCooldown <= 0` | `_dashTimer` expires | `_isDashing = true`; horizontal velocity locked to `DashDirection * DashSpeed`; `_velocity.y = 0` (gravity suppressed); `_dashCooldown` set to `DashCooldown` on exit |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| InputReader | PlayerController subscribes to `OnMove`, `OnJumpPressed`, `OnSprintHeld`, `OnSprintReleased`, `OnDashPressed` in `OnEnable`; unsubscribes in `OnDisable`. Holds a `[SerializeField] InputReader _inputReader` reference — never resolved at runtime. |
| CameraController | CameraController holds a reference to the player's Transform. PlayerController reads `CameraController.Forward` for dash direction fallback when movement input is zero. PlayerController never calls CameraController methods. |
| KeyPickup | No interaction — KeyPickup's `OnTriggerEnter` handles key collection independently. PlayerController knows nothing about keys. |

---

## Formulas

### Horizontal Movement (with Run)

```
moveDirection = new Vector3(inputVector.x, 0, inputVector.y).normalized
float runMult = _isSprinting ? RunMultiplier : 1f
horizontalVelocity = moveDirection * MoveSpeed * airMultiplier * runMult
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `inputVector` | Vector2 | -1 to 1 per axis | InputReader.OnMove | Raw move input; normalized in 3D |
| `MoveSpeed` | float | 3–10 m/s | Inspector (ScriptableObject) | Base horizontal speed |
| `airMultiplier` | float | 0–1 | Computed: 1.0 if Grounded, `AirControlMultiplier` if Airborne | Reduces (or maintains) speed in air |
| `runMult` | float | 1.0 or RunMultiplier | Computed from `_isSprinting` | Sprint speed multiplier |

**Expected output range**: 0 to MoveSpeed × RunMultiplier m/s

### Dash

```
// On OnDashPressed, if _dashCooldown <= 0:
Vector3 dashDir = _moveInput.sqrMagnitude > 0.01f
    ? new Vector3(_moveInput.x, 0f, _moveInput.y).normalized
    : _cameraController.Forward;

// Each frame while _isDashing:
_velocity.x = dashDir.x * DashSpeed;
_velocity.z = dashDir.z * DashSpeed;
_velocity.y = 0f;   // gravity suppressed during dash

_dashTimer -= Time.deltaTime;
if (_dashTimer <= 0f) { _isDashing = false; _dashCooldown = DashCooldown; }
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `DashSpeed` | float | 10–30 m/s | Inspector (ScriptableObject) | Horizontal burst speed during dash |
| `DashDuration` | float | 0.1–0.5 s | Inspector (ScriptableObject) | How long the dash lasts |
| `DashCooldown` | float | 0.3–3.0 s | Inspector (ScriptableObject) | Time before next dash is allowed |

**Expected output**: Instant lateral burst covering `DashSpeed × DashDuration` metres.

### Vertical Velocity (Gravity + Jump)

```
// Each frame:
_velocity.y += Gravity * Time.deltaTime

// If descending:
_velocity.y += Gravity * (FallGravityMultiplier - 1f) * Time.deltaTime

// On grounded with downward velocity:
_velocity.y = -2f   // ground clamp

// On jump:
_velocity.y = JumpVelocity
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `Gravity` | float | -10 to -30 m/s² | Inspector | Upward-flight gravity (negative = downward) |
| `FallGravityMultiplier` | float | 1.0–2.5 | Inspector | Extra gravity during descent |
| `JumpVelocity` | float | 5–15 m/s | Inspector | Initial upward speed at jump start |

**Expected air time** (at defaults Gravity = -18, JumpVelocity = 9): ~1.0s total, ~0.5s ascending

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Jump pressed just before landing | Jump fires on first grounded frame (buffer) | Feels responsive — player intent is honoured |
| Jump pressed just after walking off a ledge | Jump fires during coyote window | Prevents frustrating "I was right at the edge" misses |
| Player holds jump key | Jump fires once on press, not held (`.performed` phase, not `.ReadValue`) | No charge-jump in Phase 1; holding should not extend air time |
| Diagonal movement input | Input vector is normalized before multiplying by MoveSpeed | Prevents faster diagonal movement (45° should equal forward speed) |
| Player lands on a moving platform | CharacterController handles basic platform riding; no special logic in Phase 1 | Moving platforms are out of scope |
| Player walks off a slope into the air | isGrounded may flicker on slopes; coyote time absorbs single-frame glitches | Coyote time exists partly to mask this |
| Player tries to move while Paused (timeScale = 0) | `Time.deltaTime == 0`; `CharacterController.Move` with zero delta = no movement | Pause handled automatically via timeScale |
| Dash pressed while on cooldown | No-op — `_dashCooldown > 0` guard; no feedback beyond doing nothing | Prevents spam |
| Dash pressed with zero movement input | Dash direction falls back to `CameraController.Forward` | Never dashes in place |
| Dash pressed mid-air | Dash fires normally; `_velocity.y` held at 0 during duration | Valid — air dash for exploration |
| Run + Dash at the same time | During `_isDashing`, horizontal velocity is fixed to `DashSpeed`; run multiplier is not applied on top | Dash speed is absolute, not additive |
| Double jump pressed with no movement input | Fires straight up (`_velocity.y = DoubleJumpVelocity`); horizontal velocity unaffected | Valid vertical escape jump |
| Double jump while dashing | Dash ends immediately (or resolves normally); `_hasDoubleJump` is checked after `_isDashing` clears in the same frame | Dash and double jump are independent resources |
| `_dashCooldown` while paused (timeScale = 0) | `_dashCooldown` decrements by `Time.deltaTime = 0`; cooldown pauses | Dash cooldown freezes during pause — acceptable |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| InputReader | PlayerController depends on it | Event subscription — reads Move and Jump events |
| CharacterController | PlayerController depends on it | Component dependency — must be on same GameObject |

---

## Tuning Knobs

All values exposed via a `PlayerData` ScriptableObject asset (`Assets/Data/PlayerData.asset`) — no magic numbers in code.

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MoveSpeed` | 6.0 m/s | 3–10 | Faster movement; harder to stop at targets | Slower; easier to navigate tight spaces |
| `JumpVelocity` | 9.0 m/s | 5–14 | Higher jump; longer air time | Lower jump; faster return to ground |
| `Gravity` | -18.0 m/s² | -10 to -30 | Snappier jump arc; less floaty | More floaty; longer air time |
| `FallGravityMultiplier` | 1.4 | 1.0–2.5 | Faster fall; more arcade weight | Same rise and fall speed; very floaty |
| `AirControlMultiplier` | 1.0 | 0.5–1.0 | Full air control (1.0 = same as ground) | Restricted air correction; momentum-based |
| `CoyoteTime` | 0.12 s | 0.0–0.25 | Larger ledge-jump grace window | Tighter; must jump before leaving ledge |
| `JumpBufferTime` | 0.15 s | 0.0–0.3 | More forgiving pre-land jump input | Must press jump closer to landing |
| `RunMultiplier` | 1.6 | 1.2–2.5 | Faster sprint; covers ground quickly | Sprint barely faster than walk |
| `DoubleJumpVelocity` | 9.0 m/s | 5–14 | Higher second jump; more air time | Lower second jump; shorter extension |
| `DashSpeed` | 18.0 m/s | 10–30 | Longer burst; crosses larger gaps | Short snappy burst |
| `DashDuration` | 0.2 s | 0.1–0.5 s | Longer dash window | Very quick burst |
| `DashCooldown` | 1.0 s | 0.3–3.0 s | Fewer dashes per encounter | Spammable dash |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Jump | Squash/stretch on Player mesh (Phase 3) | Jump SFX (Phase 3) | Polish |
| Land | Squash on landing (Phase 3) | Land SFX (Phase 3) | Polish |
| Walk | Footstep animation (Phase 3) | Footstep SFX (Phase 3) | Polish |

All visual and audio feedback is Phase 3 scope. Phase 1 uses an untextured capsule with no animations.

---

## Game Feel

### Feel Reference

> "Should feel like A Short Hike's walking — light, floaty, fully air-controllable, with generous coyote time. The player can course-correct in the air at any point. NOT like early Sonic where horizontal momentum carries past your intent."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Move direction change | Same frame (Update reads latest OnMove value) | 0 extra frames |
| Jump press | Same frame (jump fires in the Update that receives the event) | 0 extra frames |

### Animation Feel Targets

Phase 1 — no animations. The capsule moves. That is sufficient for layout playtesting.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Landing | Phase 3 | Squash on Player mesh |

### Weight and Responsiveness

- **Weight**: Light and reactive. The player should feel like they weigh very little in the air.
- **Player control**: Full course-correction mid-air at any point. No committed arcs.
- **Snap quality**: Instant direction change on ground. Air direction change is immediate but slightly smoother due to AirControlMultiplier.
- **Failure texture**: When the player misses a platform, they should know it was their mistake — not the control's. The coyote time and jump buffer exist to eliminate ambiguous failures.

### Feel Acceptance Criteria

- [ ] A playtest participant, unprompted, uses the word "floaty" or "light" to describe the jump
- [ ] No playtest participant uses the word "sticky" or "heavy"
- [ ] Coyote time: player can jump 2–3 frames after walking off a ledge without noticing a gap
- [ ] Jump buffer: pressing jump 2–3 frames before landing fires the jump on landing without the player noticing the buffer

---

## UI Requirements

None — PlayerController has no UI. Health and state display are out of scope for Phase 1.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| InputReader event subscription | `InputReader.md` | `OnMove`, `OnJumpPressed` events | Data dependency |
| PlayerData ScriptableObject | `best-practices.md` | Tunable data via ScriptableObject rule | Rule dependency |
| Never calls WorldStateManager | `best-practices.md` | "PlayerController must never call WorldStateManager" rule | Rule dependency |
| CameraController tracks player Transform | `CameraController.md` | Player Transform reference | State trigger |

---

## Acceptance Criteria

- [ ] `PlayerController.cs` exists at `Assets/Scripts/PlayerController.cs`
- [ ] `PlayerData.asset` exists at `Assets/Data/PlayerData.asset` with all tuning knobs
- [ ] Player moves horizontally at `MoveSpeed` on flat ground
- [ ] Diagonal movement speed equals forward movement speed (normalized input)
- [ ] Jump fires once per press; vertical velocity matches `JumpVelocity` on the jump frame
- [ ] Player descends faster than it ascends (FallGravityMultiplier effect is visible)
- [ ] Coyote time: player can jump within `CoyoteTime` seconds of leaving a ledge
- [ ] Jump buffer: pressing jump `JumpBufferTime` seconds before landing fires the jump on landing
- [ ] Air control: player can fully change horizontal direction while airborne
- [ ] Player does not accumulate falling velocity while grounded on a slope
- [ ] No calls to `WorldStateManager`, `GameManager`, or `SceneLoader` in this script
- [ ] Zero GC allocations in `Update`
- [ ] Holding Sprint (Left Shift / Left Trigger) increases horizontal speed by `RunMultiplier`; releasing returns to base speed
- [ ] Player can perform exactly one double jump per grounded landing; `_hasDoubleJump` resets on landing
- [ ] Double jump fires in the same frame jump is pressed while airborne (no delay)
- [ ] Coyote jump does not consume `_hasDoubleJump`
- [ ] Dash fires in the direction of movement input; falls back to camera-forward when input is zero
- [ ] During dash, `_velocity.y` is held at 0 (no gravity)
- [ ] Dash does not fire while `_dashCooldown > 0`
- [ ] `_dashCooldown` starts decrementing immediately after dash ends
- [ ] Dash does not reset or consume `_hasDoubleJump`
- [ ] All five InputReader events subscribed in `OnEnable`, unsubscribed in `OnDisable`

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should sprint (hold Shift / trigger) be added in Phase 1? | Developer | Before implementation | Deferred — out of scope for layout testing. Add in Phase 2 Tier 3 or Phase 3 if the map needs it. |
| Should the player have a fall death plane (respawn if Y < threshold)? | Developer | Before Tier 2 testing | Recommended yes — add a `DeathPlane` Y value in PlayerData; call `SceneLoader.LoadScene(currentScene)` to respawn. Simple and necessary for map testing. |
| Should CameraController input affect movement direction (camera-relative movement)? | Developer | Before implementation | Recommended yes — movement should be camera-relative so WASD always feels like "forward relative to where I'm looking." Requires PlayerController to read the camera's forward vector. |
