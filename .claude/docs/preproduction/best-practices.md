# Best Practices

<!-- Agents read this before writing any code. Project-critical patterns override everything else. -->

---

## Project-Critical Patterns

> These are hard constraints, not suggestions. Every rule below was derived from the architecture contract in `architecture.md`. Violating any of them breaks the inter-system communication design.

---

### No direct cross-system calls outside the permitted list

**Rule:** Only the communication paths listed in `architecture.md` are permitted. No ad-hoc cross-system calls.

**Why:** The architecture contract defines which systems may call which directly and which must use events. Any call outside that list creates hidden coupling that breaks the design.

**Enforcement:** Before writing any cross-script call, check `architecture.md` — if the path isn't listed under "Direct calls — explicitly permitted", it must use an event instead.

---

### PlayerController must never call WorldStateManager

**Rule:** `PlayerController.cs` must not reference or call `WorldStateManager`.

**Why:** Key collection is `KeyPickup`'s responsibility. If `PlayerController` also reacts to keys, the responsibility is split and world-state becomes unpredictable.

**Instead:** The trigger collision lives entirely in `KeyPickup.cs`. `PlayerController` knows nothing about keys or doors.

---

### CameraController must never call WorldStateManager or GameManager

**Rule:** `CameraController.cs` reads only the player Transform. No references to `WorldStateManager` or `GameManager`.

**Why:** Camera logic is purely spatial. Mixing game-state reads into camera code creates coupling that makes both systems harder to modify independently.

---

### Door must never call PlayerController or InputReader

**Rule:** `Door.cs` only subscribes to `WorldStateManager.OnDoorUnlocked`. It never references the player or input.

**Why:** Doors respond to world-state events, not to player actions directly. Keeping Door decoupled from the player allows door logic to be driven by any future trigger source without modification.

---

### Never call SceneManager.LoadScene directly

**Rule:** `SceneManager.LoadScene` and `SceneManager.LoadSceneAsync` must never be called outside of `SceneLoader.cs`.

**Why:** `SceneLoader` is the single entry point for all scene transitions. Direct calls bypass any transition logic or loading state tracking added later.

**Instead:** Always call `SceneLoader.LoadScene(sceneName)`.

---

### Never call DontDestroyOnLoad anywhere

**Rule:** `DontDestroyOnLoad` must never be called in any script in this project.

**Why:** All persistent objects live in `Bootstrap.unity`, which is loaded at build index 0 and never unloaded — all other scenes load additively on top of it. The Bootstrap scene persists naturally without `DontDestroyOnLoad`. Calling it creates an untracked persistent object that is difficult to debug and violates the additive scene architecture.

---

### Never use FindObjectOfType, GameObject.Find, or Resources.Load at runtime

**Rule:** These methods are banned at runtime (in `Update`, `Start`, triggered callbacks, etc.).

**Why:** Runtime lookups are O(n) over all scene objects, allocate, and create implicit coupling. They also break in multi-scene setups.

**Instead:** Cache references in `Awake` via serialized `[SerializeField]` fields or dependency injection through Bootstrap.

---

### Subscribe in OnEnable, unsubscribe in OnDisable

**Rule:** All C# event subscriptions (InputReader events, WorldStateManager events) must be made in `OnEnable` and removed in `OnDisable`.

**Why:** `Start` runs only once. `OnEnable`/`OnDisable` fire on every activation cycle. Scripts on disabled GameObjects must not receive events — failing to unsubscribe causes null-reference callbacks on destroyed objects.

```csharp
// ✅ Correct
void OnEnable()  => _inputReader.OnJumpPressed += HandleJump;
void OnDisable() => _inputReader.OnJumpPressed -= HandleJump;

// ❌ Wrong — leaks subscription when object is disabled
void Start() => _inputReader.OnJumpPressed += HandleJump;
```

---

### Each map is a single scene — no multi-scene room loading

**Rule:** Rooms are spatial areas within one Unity scene. Do not design systems that assume rooms are separate scenes loaded additively.

**Why:** The design decision is that each of the three maps is one scene. `WorldStateManager` is scene-level. Splitting rooms into scenes would require cross-scene state persistence, which is out of scope.

---

### WorldStateManager state does not persist between map loads

**Rule:** `WorldStateManager` is a scene-level MonoBehaviour. Its state resets when the scene is reloaded. Do not attempt to preserve key/door state across scene transitions.

**Why:** Phase 1 has no save system and no cross-map state. If state needs to persist in a future phase, that is a new architectural decision — route it through `GameManager`, not `WorldStateManager`.

---

### DebugOverlay is dev-only — strip before release

**Rule:** `DebugOverlay.cs` must never appear in a release build. Gate it with `#if UNITY_EDITOR || DEVELOPMENT_BUILD` or disable the GameObject before building.

**Why:** The overlay exposes internal world state (key counts, door IDs). It is a development tool, not player-facing UI.

```csharp
// ✅ Correct
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    _debugOverlay.SetActive(true);
#endif
```

---

### PlayerController uses the State Pattern — states own horizontal velocity only

**Rule:** `PlayerController` is a state machine host. All per-state behavior lives in a concrete `PlayerStateBase` subclass. Vertical velocity (gravity, jump, fall multiplier) and `CharacterController.Move` are shared steps in `PlayerController.Update` — never inside a state class.

**Why:** Separating per-state horizontal logic from the shared vertical-physics steps keeps gravity and `CharacterController.Move` in one place and prevents state classes from accidentally fighting each other over vertical velocity.

**State files:** `Assets/Scripts/Player/PlayerStateBase.cs` (abstract) + `IdleState`, `WalkState`, `RunState`, `AirborneState`, `DashingState` in the same folder.

---

### Zero GC alloc per frame in steady state

**Rule:** `Update`, `FixedUpdate`, and `LateUpdate` must not allocate on the managed heap during normal gameplay (no `new`, no LINQ, no string concatenation, no boxing).

**Why:** Allocations trigger the GC, causing frame spikes. The target is 60 fps; a GC spike at any point breaks that budget.

**Instead:** Pre-allocate in `Awake`/`Start`. Use object pools for anything created at runtime.

---

### InputReader is a ScriptableObject — reference via Inspector, never at runtime

**Rule:** `InputReader` must be assigned via `[SerializeField]` in the Inspector. Never call `FindObjectOfType<InputReader>()` or `Resources.Load` to retrieve it.

**Why:** `InputReader` is a ScriptableObject asset, not a scene object. Runtime lookup of ScriptableObjects is unreliable and violates the no-Find rule above.

---

## Unity 6 LTS — Current Patterns

**Last verified:** 2026-05-30

> These patterns differ from older Unity versions that may appear in LLM training data. Follow these. Do not revert to the legacy column.

### Input

| Use This | Not This | Why |
|---|---|---|
| `UnityEngine.InputSystem` package | `Input.GetKey()` / `Input.GetAxis()` | Rebindable, cross-platform, event-driven |

```csharp
// ✅ Correct
controls.Gameplay.Jump.performed += ctx => Jump();

// ❌ Legacy
if (Input.GetKeyDown(KeyCode.Space)) Jump();
```

---

### UI

| Use This | Not This | Why |
|---|---|---|
| UI Toolkit (`.uxml` + `.uss`) | UGUI Canvas + `Text`/`Image` components | Production-ready in Unity 6, HTML/CSS workflow |

```csharp
// ✅ Correct
var root = GetComponent<UIDocument>().rootVisualElement;
root.Q<Button>("play-button").clicked += StartGame;

// ❌ Legacy
GetComponent<Button>().onClick.AddListener(StartGame);
```

---

### Asset Loading

| Use This | Not This | Why |
|---|---|---|
| `Addressables` | `Resources.Load` | Async, memory-efficient, supports remote delivery |

---

### Tunable Data

| Use This | Not This | Why |
|---|---|---|
| `ScriptableObject` assets | Hardcoded values or config files | Inspector-editable, designer-friendly, no recompile |

```csharp
// ✅ Correct
[CreateAssetMenu(menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject {
    public float MoveSpeed;
    public float JumpForce;
    public float Gravity;
}
```

---

### Timers (when timeScale may change)

| Use This | Not This | Why |
|---|---|---|
| `Time.unscaledDeltaTime` / `WaitForSecondsRealtime` | `Time.deltaTime` / `WaitForSeconds` | `Time.timeScale = 0` (pause) freezes scaled time |

```csharp
// ✅ Survives pause
_timer += Time.unscaledDeltaTime;
yield return new WaitForSecondsRealtime(duration);
```

---

### Rendering (URP custom passes)

| Use This | Not This | Why |
|---|---|---|
| `RenderGraph` API | `CommandBuffer.Execute()` | Required in Unity 6 URP; old API deprecated |

---

### Testing

| Use This | Not This | Why |
|---|---|---|
| NUnit + Unity Test Runner | Manual Play Mode testing only | Repeatable, automated, catches regressions |
| Edit Mode tests for pure logic | Play Mode tests for everything | Faster iteration |

```csharp
// ✅ Edit Mode test — no scene needed
[Test]
public void WorldState_KeyCollected_DoorUnlocks() {
    var state = new WorldStateManager();
    state.CollectKey("key_A");
    Assert.IsTrue(state.IsDoorUnlocked("door_A"));
}
```

---

### Summary Reference

| Feature | Use (2026) | Avoid (Legacy) |
|---|---|---|
| Input | Input System package | `Input` class |
| UI | UI Toolkit | UGUI Canvas |
| Assets | Addressables | `Resources` |
| Tunable data | `ScriptableObject` | Hardcoded constants |
| Rendering | URP + RenderGraph | Built-in pipeline |
| Timers (pause-safe) | `unscaledDeltaTime` | `deltaTime` |
| Testing | NUnit + Test Runner | Manual only |
| Event subscription | `OnEnable` / `OnDisable` | `Start` / `OnDestroy` |
