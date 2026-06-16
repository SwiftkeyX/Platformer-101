# Architecture Contract

> The authoritative record of how scripts in this project communicate. Claude reads this before touching any existing script.
>
> For system responsibilities, dependencies, and tier assignments, see `.claude/docs/preproduction/systems-design.md`.
> For coding conventions and anti-patterns, see `.claude/docs/preproduction/best-practices.md`.

---

## Script Table

| System | Script | Responsibility |
|---|---|---|
| Bootstrap | `Bootstrap.cs` | Entry point MonoBehaviour in the persistent Bootstrap scene; instantiates and holds GameManager and SceneLoader |
| GameManager | `GameManager.cs` | Singleton; owns game state enum (Playing/Paused); fires `OnGameStateChanged` event |
| SceneLoader | `SceneLoader.cs` | Singleton; the only permitted caller of `SceneManager.LoadScene`; exposes `LoadScene(string name)` |
| InputReader | `InputReader.cs` | ScriptableObject; wraps Unity Input System; fires C# events `OnMove`, `OnJumpPressed`, `OnInteractPressed` |
| PlayerController | `PlayerController.cs` | MonoBehaviour; 3D movement and floaty jump via CharacterController; subscribes to InputReader events |
| CameraController | `CameraController.cs` | MonoBehaviour; third-person follow camera; holds a direct reference to the player Transform |
| WorldStateManager | `WorldStateManager.cs` | MonoBehaviour (on a persistent or scene-level manager GameObject); tracks key/door state; fires `OnKeyCollected` and `OnDoorUnlocked` |
| KeyPickup | `KeyPickup.cs` | MonoBehaviour; `OnTriggerEnter` calls `WorldStateManager.CollectKey(keyId)` and disables the key GameObject |
| Door | `Door.cs` | MonoBehaviour; subscribes to `WorldStateManager.OnDoorUnlocked`; plays open sequence when its `keyId` matches |
| MapSelector | `MapSelector.cs` | MonoBehaviour (dev UI); calls `SceneLoader.LoadScene(sceneName)` to switch between the three map scenes |
| DebugOverlay | `DebugOverlay.cs` | MonoBehaviour; dev-only; reads WorldStateManager and GameManager state each frame via direct reference; rendered with `OnGUI` |

---

## Communication Patterns

| From | To | Method | Notes |
|---|---|---|---|
| Bootstrap | GameManager | Direct instantiation | Bootstrap creates GameManager on Awake and holds the reference |
| Bootstrap | SceneLoader | Direct instantiation | Bootstrap creates SceneLoader on Awake and holds the reference |
| GameManager | SceneLoader | Direct method call | `SceneLoader.LoadScene()` — GameManager is the only non-UI caller allowed to trigger scene loads |
| InputReader | PlayerController | C# event (`OnMove`, `OnJumpPressed`) | PlayerController subscribes in OnEnable, unsubscribes in OnDisable |
| PlayerController | InputReader | Event subscription only | PlayerController holds a serialized reference to the InputReader ScriptableObject; never calls methods on it |
| CameraController | PlayerController | Direct Transform reference | CameraController holds a serialized reference to the player Transform; no events |
| KeyPickup | WorldStateManager | Direct method call (`CollectKey`) | KeyPickup calls WorldStateManager directly on trigger; WorldStateManager fires the resulting event |
| WorldStateManager | Door | C# event (`OnDoorUnlocked`) | Door subscribes in OnEnable, unsubscribes in OnDisable; WorldStateManager never holds a reference to Door |
| WorldStateManager | DebugOverlay | Direct field read | DebugOverlay reads public state from WorldStateManager via serialized reference; polling in OnGUI |
| MapSelector | SceneLoader | Direct method call (`LoadScene`) | MapSelector is the dev UI entry point for scene switching |
| DebugOverlay | GameManager | Direct field read | DebugOverlay reads `GameManager.CurrentState` via serialized reference |

---

## Inter-System Communication Contract

### Direct calls — explicitly permitted

| Caller | Callee | Rationale |
|---|---|---|
| Bootstrap | GameManager, SceneLoader | Bootstrap owns these singletons; direct instantiation at startup |
| GameManager | SceneLoader | One authoritative path for programmatic scene loads |
| KeyPickup | WorldStateManager | Tight coupling is correct here — KeyPickup has no purpose without WorldStateManager |
| MapSelector | SceneLoader | Dev tool; direct scene-switch call is appropriate |
| DebugOverlay | WorldStateManager, GameManager | Read-only polling for display; acceptable in dev-only code |
| CameraController | PlayerController.Transform | Camera must track the player; reference is set in Inspector |

### Event-only — no direct calls permitted

| From | To | Events |
|---|---|---|
| InputReader | PlayerController | `OnMove(Vector2)`, `OnJumpPressed()`, `OnInteractPressed()` |
| WorldStateManager | Door | `OnDoorUnlocked(string keyId)` |
| GameManager | Any subscriber | `OnGameStateChanged(GameState)` |

### Forbidden

- `PlayerController` must never call `WorldStateManager` — interaction is handled exclusively by `KeyPickup`
- `CameraController` must never call `WorldStateManager` or `GameManager` — it only tracks the player Transform
- `Door` must never call `PlayerController` or `InputReader`
- Any script using `FindObjectOfType`, `GameObject.Find`, or `Resources.Load` at runtime
- Any script calling `SceneManager.LoadScene` directly — always go through `SceneLoader`
- Any script calling `DontDestroyOnLoad` — only Bootstrap is permitted to do this, and it does so for itself only
