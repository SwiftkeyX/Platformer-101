# Platformer Movement Reference

Practical technique library derived from studying Mario, Celeste, Hollow Knight, and other platformers. Each entry has a Unity C# pattern, tuning range, and the current project's status.

**Audience:** Anyone implementing or tuning `PlayerController.cs` or `PlayerData.cs`.  
**Project feel target:** Floaty · Precise · Exploratory (A Short Hike feel pillar — light, full air control, generous forgiveness windows).

---

## Jump Architecture — Start Here

Before setting any constants, derive them from what you *want*, not the other way around. Given a desired peak height `h` and time to apex `t`:

```csharp
// PlayerData derivation (run this once in a scratch script or calculator)
float gravity      = -(2f * h) / (t * t);
float jumpVelocity =  (2f * h) / t;
```

**Current project values:** `JumpVelocity = 9`, `Gravity = -18`
→ Peak height: `9² / (2 × 18) = 2.25 m`  
→ Time to apex: `9 / 18 = 0.5 s`

Change `h` and `t` in the formula, copy the outputs into `PlayerData`. Never tune `Gravity` and `JumpVelocity` independently — they are linked by this equation.

---

## Techniques

### 1. Asymmetric Gravity (Fall Multiplier)

**What:** Apply extra downward gravity while falling so the descent is faster than the ascent.  
**Why:** Symmetric gravity produces a slow, floaty arc that feels out of control. A heavier fall snaps the player back to the ground, making them feel decisive.

```csharp
// In ApplyGravity() — runs every Update
_velocity.y += _data.Gravity * Time.deltaTime;

if (_velocity.y < 0f)   // falling
    _velocity.y += _data.Gravity * (_data.FallGravityMultiplier - 1f) * Time.deltaTime;
```

| Parameter | Typical range | Project value |
|---|---|---|
| `FallGravityMultiplier` | 1.2 – 3.5 | **1.4** (light/floaty end) |

Celeste uses ~3.0 (punchy). Hollow Knight uses ~2.5 (weighty). A Short Hike uses ~1.3 (cloud-like). Stay near 1.4 for the project's feel pillar.

**Project status:** ✅ Implemented.

---

### 2. Jump Cut (Variable Jump Height)

**What:** When the jump button is released while `velocity.y > 0`, clamp upward velocity immediately. Tap = small hop. Hold = full arc.  
**Why:** Without this, every jump is identical. Jump cut gives the player two heights from one button, which is the basis for precision platforming skill.

```csharp
// Wire to InputReader.OnJumpReleased event
private void OnJumpReleased()
{
    if (_velocity.y > 0f)
        _velocity.y *= _data.JumpCutMultiplier;   // e.g. 0.4f
}
```

| Parameter | Typical range | Project value |
|---|---|---|
| `JumpCutMultiplier` | 0.3 – 0.6 | ❌ Not in PlayerData yet |

Lower value = more dramatic height difference between tap and hold. 0.4 is a good starting point for a floaty game.

**Project status:** ❌ Missing. `InputReader` fires `OnJumpPressed` but not `OnJumpReleased`. Both the event and the cut multiplier need to be added.

---

### 3. Coyote Time

**What:** After walking off a ledge, allow a short window where the jump is still valid — as if the player were still grounded.  
**Why:** Players press jump the moment they *intend* to jump, which is often 2–4 frames after they've visually left the edge. Without this, the jump silently fails and the player feels cheated.

```csharp
// State machine: Grounded → Coyote → Airborne
// In Update, when the controller loses ground contact:
if (wasGrounded && !_controller.isGrounded)
{
    _state = MoveState.Coyote;
    _coyoteTimer = _data.CoyoteTime;
}

// Count down while Coyote
if (_state == MoveState.Coyote)
{
    _coyoteTimer -= Time.deltaTime;
    if (_coyoteTimer <= 0f) _state = MoveState.Airborne;
}
```

| Parameter | Typical range | Project value |
|---|---|---|
| `CoyoteTime` | 0.08 – 0.20 s | **0.12 s** |

**Project status:** ✅ Implemented via the `Grounded → Coyote → Airborne` state machine.

---

### 4. Jump Buffering

**What:** When the jump button is pressed before landing, store the press for a short window. Execute the jump the moment the player touches ground.  
**Why:** Players anticipate the landing and press early. Without buffering, they land, the jump is silently discarded, and they must press again — which feels sluggish.

```csharp
// On jump pressed — start the buffer timer
private void OnJumpPressed() => _jumpBufferTimer = _data.JumpBufferTime;

// In Update — drain the timer and fire if grounded
private void ConsumeJumpBuffer()
{
    if (_jumpBufferTimer <= 0f) return;
    _jumpBufferTimer -= Time.deltaTime;

    if (_state == MoveState.Grounded || _state == MoveState.Coyote)
    {
        _velocity.y = _data.JumpVelocity;
        _state = MoveState.Airborne;
        _jumpBufferTimer = 0f;
    }
}
```

| Parameter | Typical range | Project value |
|---|---|---|
| `JumpBufferTime` | 0.10 – 0.20 s | **0.15 s** |

**Project status:** ✅ Implemented.

---

### 5. Ground Snapping (`-2f`)

**What:** When grounded and `velocity.y < 0`, clamp it to a small negative constant rather than `0`.  
**Why:** `CharacterController.isGrounded` returns false on the first frame after landing when `velocity.y` is exactly `0` (no downward force means the controller floats by one physics step). This causes single-frame coyote state flickers on steps and slopes.

```csharp
if (_state == MoveState.Grounded && _velocity.y < 0f)
{
    _velocity.y = -2f;   // pressed against ground; not 0
    return;
}
```

The value `-2f` is the standard Unity fix. Do not change it to `0` or a large negative number.

**Project status:** ✅ Implemented.

---

### 6. Air Control

**What:** Allow full (or partial) horizontal directional input while airborne.  
**Why:** 3D platformers have a depth-perception problem — players misjudge ledges. Generous air control lets them course-correct mid-flight instead of committing on take-off.

```csharp
// In horizontal movement calculation:
float controlScale = _state == MoveState.Grounded
    ? 1f
    : _data.AirControlMultiplier;

Vector3 move = (forward * input.y + right * input.x) * _data.MoveSpeed * controlScale;
_controller.Move(move * Time.deltaTime);
```

| Parameter | Typical range | Project value |
|---|---|---|
| `AirControlMultiplier` | 0.3 – 1.0 | **1.0** (full) |

`1.0` = A Short Hike feel (turn around freely mid-air). `0.3` = Hollow Knight feel (commit to your jump). Matching the floaty feel pillar: keep at 1.0.

**Project status:** ✅ Implemented.

---

### 7. Apex Modifier (Optional)

**What:** Near the peak of a jump (where `|velocity.y|` is small), temporarily reduce gravity. The player "hangs" at the top of the arc.  
**Why:** Hang time is the signature of floaty platformers (Kirby, A Short Hike). It gives the player more time to aim their landing and makes the jump feel generous.

```csharp
// Add to ApplyGravity(), after the fall multiplier
float apexFactor = 1f - Mathf.InverseLerp(
    _data.ApexThreshold, 0f, Mathf.Abs(_velocity.y));

float gravityScale = Mathf.Lerp(1f, _data.ApexGravityScale, apexFactor);
_velocity.y += _data.Gravity * gravityScale * Time.deltaTime;
```

| Parameter | Typical range | Project value |
|---|---|---|
| `ApexThreshold` | 1.0 – 3.0 m/s | ❌ Not in PlayerData |
| `ApexGravityScale` | 0.2 – 0.5 | ❌ Not in PlayerData |

`ApexThreshold` defines the velocity band near the peak. `ApexGravityScale` sets how much gravity is reduced there (0.3 = 70% gravity reduction at the very top).

**Project status:** ❌ Not implemented. Optional; adds hang-time feel on top of the existing `FallGravityMultiplier`.

---

### 8. Corner Correction

**What:** When a jump is slightly blocked by a ledge corner, test small horizontal offsets. If one offset clears the obstacle, nudge the player by that amount.  
**Why:** Players aim for the center of a platform but the capsule clips a corner. Without correction, they bonk and fall; the jump looked like it should have worked. Corner correction silently fixes this.

```csharp
// After a failed upward Move(), try lateral offsets
private bool TryCornerCorrection(Vector3 upward, float radius)
{
    float[] offsets = { 0.2f, -0.2f, 0.4f, -0.4f };
    foreach (float offset in offsets)
    {
        Vector3 nudge = transform.right * offset;
        if (!Physics.CapsuleCast(/* ... */, upward, radius))
        {
            transform.position += nudge;
            return true;
        }
    }
    return false;
}
```

| Parameter | Typical range | Project value |
|---|---|---|
| Max correction radius | 0.2 – 0.5 units | ❌ Not implemented |

**Project status:** ❌ Not implemented. Add during tuning phase if players report "almost made it" frustration on tight platforms.

---

### 9. Momentum / Skidding (Optional)

**What:** When the player reverses horizontal direction at speed, decelerate before accelerating the opposite way — instead of instant velocity flip.  
**Why:** This is the defining feel of Mario. Momentum preservation creates skill expression: learn to plan direction changes, and the character rewards precision. Without it, the character feels weightless or "twitchy."

```csharp
// Instead of direct velocity assignment:
float targetSpeed = input.x * _data.MoveSpeed;
float currentSpeed = _horizontalVelocity.x;

bool reversing = Mathf.Sign(input.x) != Mathf.Sign(currentSpeed) && input.x != 0f;
float accel = reversing ? _data.SkidDeceleration : _data.Acceleration;

_horizontalVelocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);
```

| Parameter | Typical range | Project value |
|---|---|---|
| `Acceleration` | 20 – 80 units/s² | ❌ Not in PlayerData |
| `SkidDeceleration` | 40 – 120 units/s² | ❌ Not in PlayerData |

**Project status:** ❌ Not implemented. Current movement uses direct velocity assignment (instant direction changes). Adding momentum would shift the game toward Mario-style weight — evaluate against the "floaty" feel pillar before adding.

---

## Quick-Reference Tuning Table

| Technique | Parameter | Implemented | Project value | Typical range |
|---|---|---|---|---|
| Jump height | `JumpVelocity` | ✅ | 9 m/s | 6 – 14 |
| Gravity | `Gravity` | ✅ | -18 m/s² | -12 – -30 |
| Fall gravity | `FallGravityMultiplier` | ✅ | 1.4× | 1.2 – 3.5 |
| Jump cut | `JumpCutMultiplier` | ❌ | — | 0.3 – 0.6 |
| Coyote time | `CoyoteTime` | ✅ | 0.12 s | 0.08 – 0.20 |
| Jump buffer | `JumpBufferTime` | ✅ | 0.15 s | 0.10 – 0.20 |
| Move speed | `MoveSpeed` | ✅ | 6 m/s | 4 – 10 |
| Air control | `AirControlMultiplier` | ✅ | 1.0 | 0.3 – 1.0 |
| Sprint | `RunMultiplier` | ✅ | 1.6× | 1.3 – 2.0 |
| Double jump | `DoubleJumpVelocity` | ✅ | 9 m/s | same as jump |
| Dash speed | `DashSpeed` | ✅ | 18 m/s | 15 – 25 |
| Dash duration | `DashDuration` | ✅ | 0.2 s | 0.15 – 0.3 |
| Dash cooldown | `DashCooldown` | ✅ | 1.0 s | 0.5 – 2.0 |
| Apex threshold | `ApexThreshold` | ❌ | — | 1.0 – 3.0 m/s |
| Apex gravity scale | `ApexGravityScale` | ❌ | — | 0.2 – 0.5 |

---

## 3D-Specific Notes

### Camera-relative movement
All horizontal input is interpreted relative to the camera, not the world. `PlayerController` reads `CameraController.Forward` and `CameraController.Right` each frame:

```csharp
Vector3 move = (_cam.Forward * input.y + _cam.Right * input.x).normalized * speed;
```

Never use `Vector3.forward` or `transform.forward` for player input — the player faces the camera, not world north.

### The ledge depth-perception problem
In 3D, players misjudge platform distances because the Z axis is compressed visually. The three standard solutions:

| Solution | How | This project |
|---|---|---|
| Full air control | Let players steer mid-jump | ✅ `AirControlMultiplier = 1.0` |
| Wide platforms | Level design absorbs the error | Level designer responsibility |
| Ledge magnetism | Snap player onto ledge edge within N units | ❌ Not implemented |

Full air control is already in place, which is the cheapest and most effective solution for an exploratory feel game.

### `Update()` vs `FixedUpdate()`

`CharacterController.Move()` belongs in **`Update()`**, not `FixedUpdate()`. It is not a physics operation — it is a collision-aware position delta. Running it in `FixedUpdate()` decouples movement from input by a frame and causes micro-stutters at non-60Hz frame rates.

Rule of thumb: `Rigidbody.AddForce` → `FixedUpdate`. `CharacterController.Move` → `Update`.

---

## Priority Order for Future Work

If adding missing techniques, do them in this order (highest ROI first):

1. **Jump cut** — single biggest feel improvement; one event + one multiply
2. **Apex modifier** — adds hang-time on top of existing fall multiplier; matches feel pillar
3. **Corner correction** — add only if testers report "almost made it" frustration
4. **Momentum / skidding** — changes the feel significantly; validate against feel pillar first
