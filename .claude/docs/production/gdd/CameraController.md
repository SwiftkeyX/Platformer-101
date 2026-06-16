# CameraController

> **Status**: Approved
> **Last Updated**: 2026-06-16
> **Implements Pillar**: Exploratory — the camera is the player's window into the space; it must never fight the player or hide geometry they need to see

## Summary

CameraController is a MonoBehaviour that drives a third-person orbit camera around the player. The player rotates the view with mouse look (or right stick); the camera orbits the player at a fixed distance, damped smoothly. It exposes its `forward` vector so `PlayerController` can compute camera-relative movement. It never calls `WorldStateManager` or `GameManager`.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `PlayerController`

---

## Overview

The camera sits at an orbit distance from the player, defined by a yaw (horizontal rotation) and pitch (vertical tilt). Each frame, the player steers the camera with mouse delta or right stick. `CameraController` updates `_yaw` and `_pitch`, computes the ideal orbit position, then smoothly lerps the camera toward that position. The camera always looks at the player pivot. `PlayerController` reads `CameraController.Forward` (the camera's flattened world-forward vector) to compute camera-relative horizontal movement — so WASD always means "forward relative to where I'm looking."

## Player Fantasy

The camera should feel like an extension of the player's eyes — it follows where the player directs it without any resistance. Rotating the view should reveal new parts of the space naturally. The camera should never clip inside geometry or lose the player from frame.

---

## Detailed Design

### Core Rules

1. CameraController is a MonoBehaviour placed on the Camera GameObject (a child of an empty `CameraRig` GameObject, or the Camera GameObject itself).
2. The player's Transform is assigned via `[SerializeField] Transform _playerTransform` — never resolved at runtime.
3. Mouse look input is read directly via `Mouse.current.delta.ReadValue()` (Unity Input System). Right stick is read via `Gamepad.current?.rightStick.ReadValue()`. Camera look is not routed through `InputReader` — it is continuous analog input, not a discrete game action.
4. Each frame: `_yaw += lookInput.x * LookSensitivity`. `_pitch -= lookInput.y * LookSensitivity`. `_pitch` is clamped to `[PitchMin, PitchMax]`. `_yaw` wraps freely (no clamping).
5. Orbit position is computed from `_yaw`, `_pitch`, and `OrbitDistance` using spherical coordinates (see Formulas).
6. Camera position is smoothly lerped toward the orbit target: `transform.position = Vector3.Lerp(transform.position, orbitTarget, FollowSmoothing * Time.deltaTime)`.
7. Camera always looks at the player pivot point (player position + `Vector3.up * VerticalLookOffset`): `transform.LookAt(playerPivot)`.
8. `CameraController.Forward` is a public read-only property: the camera's forward vector projected onto the XZ plane, normalized. `PlayerController` reads this for camera-relative movement.
9. Camera runs in `LateUpdate` — always after `PlayerController.Update` has moved the player. This prevents a one-frame lag where the camera trails behind after the player has already moved.
10. CameraController never calls `WorldStateManager`, `GameManager`, or `SceneLoader`.
11. No camera collision in Phase 1 — camera may clip through geometry. This is acceptable for graybox layout testing.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Following` | Always (no other states in Phase 1) | Never | Reads look input, updates orbit angles, lerps to orbit position, looks at player |

CameraController has no state machine in Phase 1 — it runs a single continuous follow behavior.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| PlayerController | CameraController holds `[SerializeField] Transform _playerTransform`. PlayerController reads `CameraController.Forward` (the static or scene-referenced property) for camera-relative movement direction. CameraController never calls PlayerController. |
| Unity Input System | Reads `Mouse.current.delta` and `Gamepad.current?.rightStick` directly. Not routed through InputReader. |

---

## Formulas

### Orbit Position (Spherical Coordinates)

```
float yawRad   = _yaw   * Mathf.Deg2Rad;
float pitchRad = _pitch * Mathf.Deg2Rad;

Vector3 orbitOffset = new Vector3(
    Mathf.Sin(yawRad)  * Mathf.Cos(pitchRad),
    Mathf.Sin(pitchRad),
    Mathf.Cos(yawRad)  * Mathf.Cos(pitchRad)
) * OrbitDistance;

Vector3 orbitTarget = _playerTransform.position
                    + Vector3.up * VerticalLookOffset
                    + orbitOffset;
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `_yaw` | float | 0–360 (wraps) | Accumulated from mouse X | Horizontal orbit angle |
| `_pitch` | float | PitchMin–PitchMax | Accumulated from mouse Y | Vertical tilt angle |
| `OrbitDistance` | float | 3–12 m | Inspector (PlayerData or CameraData SO) | Distance from player pivot |
| `VerticalLookOffset` | float | 0–2 m | Inspector | Height above player origin the camera looks at |

**Expected output**: Camera position on a sphere of radius `OrbitDistance` centered at the player pivot + vertical offset.

### Camera-Relative Forward (for PlayerController)

```
Vector3 forward = transform.forward;
forward.y = 0f;
forward.Normalize();
// Exposed as CameraController.Forward
```

PlayerController computes movement as:
```
Vector3 move = CameraController.Forward * inputVector.y
             + CameraController.Right   * inputVector.x;
move.Normalize();
horizontalVelocity = move * MoveSpeed;
```

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Player is directly below or above the orbit center | `Mathf.Cos(pitchRad)` approaches 0; orbit offset collapses toward the Y axis | Clamping pitch to `[-80°, 80°]` prevents gimbal collapse at poles |
| Camera inside geometry (Phase 1) | Clips through — no correction | Camera collision is Phase 3 scope; graybox walls are no-clip for now |
| No mouse connected; no gamepad connected | `Mouse.current` and `Gamepad.current` are null; null-check before reading | Camera simply doesn't rotate if no look input device is present |
| `_playerTransform` is null (player destroyed) | Null reference — guard with null-check in LateUpdate; skip update if null | Player destruction is not in scope for Phase 1, but the guard prevents crashes |
| Game is paused (timeScale = 0) | Mouse delta still registers (input is not time-scaled); `Time.deltaTime = 0` means lerp does not move | Camera rotation continues during pause. Acceptable — cursor lock should be released on pause by a future UI system |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| PlayerController | CameraController depends on PlayerController's Transform | Data dependency — reads player position each LateUpdate |
| Unity Input System | CameraController depends on it | Data dependency — reads Mouse and Gamepad devices directly |

---

## Tuning Knobs

All values exposed via a `CameraData` ScriptableObject asset (`Assets/Data/CameraData.asset`) — no magic numbers in code.

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `LookSensitivity` | 0.15 | 0.05–0.5 | Faster camera rotation | Slower, more deliberate rotation |
| `OrbitDistance` | 6.0 m | 3–12 | Wider view, player appears smaller | Closer, more intimate; harder to see surroundings |
| `VerticalLookOffset` | 1.0 m | 0–2 | Camera looks higher on player (more sky visible) | Camera looks at player feet (more ground visible) |
| `PitchMin` | -20° | -80 to 0 | More floor visible at minimum | Less floor visible |
| `PitchMax` | 60° | 0 to 80 | More overhead view available | Less aerial view |
| `FollowSmoothing` | 8.0 | 1–20 | Faster camera catch-up (snappier) | Slower catch-up (more lag behind player) |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Camera rotating | None — camera is the viewport | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like A Short Hike's camera — orbits naturally, follows without fighting, reveals the space at the player's pace. NOT like early 3D Mario games where the camera constantly auto-corrects and interrupts player intent."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Mouse look | Same frame (Mouse.current.delta read in LateUpdate) | 0 extra frames |
| Camera follow player | 1–3 frames of lerp lag at FollowSmoothing = 8 | Intentional — gives the camera a "weight" |

### Animation Feel Targets

None — camera is not animated beyond lerp smoothing.

### Impact Moments

None in Phase 1. A camera shake on landing could be added in Phase 3.

### Weight and Responsiveness

- **Weight**: The camera has a slight lag (lerp smoothing) so it doesn't feel bolted to the player — it trails slightly, giving a sense of motion.
- **Player control**: Immediate. Mouse look updates `_yaw` and `_pitch` instantly; the lerp only affects the position follow, not the look angle.
- **Snap quality**: Smooth and analog — no snapping.
- **Failure texture**: If the camera feels like it "fights" the player, reduce `FollowSmoothing`. If it feels detached, increase it.

### Feel Acceptance Criteria

- [ ] Rotating the camera 360° feels smooth with no jitter at any angle
- [ ] Camera never loses sight of the player during normal movement
- [ ] WASD movement direction corresponds to camera-relative forward (confirmed by playtest)
- [ ] Camera pitch never inverts or flips (clamping working correctly)

---

## UI Requirements

None — CameraController has no UI. A crosshair or reticle, if ever added, belongs to a separate UI system.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| PlayerController reads CameraController.Forward | `PlayerController.md` | Camera-relative movement formula | State trigger |
| No WorldStateManager or GameManager calls | `best-practices.md` | "CameraController must never call WorldStateManager or GameManager" | Rule dependency |
| Mouse.current / Gamepad.current read directly | `InputReader.md` | Camera look is NOT routed through InputReader | Rule dependency (explicit exclusion) |

---

## Acceptance Criteria

- [ ] `CameraController.cs` exists at `Assets/Scripts/CameraController.cs`
- [ ] `CameraData.asset` exists at `Assets/Data/CameraData.asset` with all tuning knobs
- [ ] Camera orbits the player when mouse is moved; right stick also works
- [ ] Pitch is clamped to `[PitchMin, PitchMax]` — no flip at poles
- [ ] Camera position smoothly follows player with `FollowSmoothing` lag
- [ ] `CameraController.Forward` returns a normalized XZ-plane vector matching camera facing
- [ ] PlayerController movement is camera-relative (WASD moves relative to camera forward)
- [ ] Camera runs in `LateUpdate` — no one-frame position lag behind player
- [ ] No calls to `WorldStateManager`, `GameManager`, or `SceneLoader` in this script

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should cursor be locked (`Cursor.lockState = CursorLockMode.Locked`) during gameplay? | Developer | Before implementation | Recommended yes — lock cursor in CameraController.OnEnable, unlock in OnDisable or when game is Paused. Without lock, mouse exits the window. |
| Should right stick look sensitivity differ from mouse sensitivity? | Developer | Before implementation | Recommended yes — gamepad stick is typically lower sensitivity than mouse. Add a separate `GamepadLookSensitivity` tuning knob. |
| Camera collision (prevent clipping through walls)? | Developer | Phase 3 | Deferred — out of scope for graybox Phase 1. Add a spherecast from player to camera in Phase 3. |
