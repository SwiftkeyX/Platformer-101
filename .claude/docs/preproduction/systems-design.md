# Systems Design

> Every system in the game, its single responsibility, what it depends on, and which tier it belongs to.

## Systems Table

| System | Responsibility | Depends On | Tier |
|---|---|---|---|
| Bootstrap | Persistent scene; initializes and holds global systems across all scene loads | None | 1 |
| GameManager | Global game state (playing/paused); owns lifecycle events | Bootstrap | 1 |
| SceneLoader | Single entry point for all scene transitions; no direct SceneManager calls permitted elsewhere | GameManager | 1 |
| InputReader | Wraps Unity Input System; fires C# events for Move, Jump, and Interact actions | None | 1 |
| PlayerController | 3D movement, floaty jump, air control; reads from InputReader events | InputReader | 2 |
| CameraController | Third-person follow camera; configurable damping and offset; tracks PlayerController | PlayerController | 2 |
| WorldStateManager | Tracks key collection and door unlock state within the current map | GameManager | 2 |
| KeyPickup | Trigger-based MonoBehaviour; notifies WorldStateManager and removes key from the scene on collect | WorldStateManager | 2 |
| Door | Listens to WorldStateManager for its assigned key; transitions locked → unlocked → open | WorldStateManager | 2 |
| MapSelector | Minimal UI to choose and load one of the 3 map scenes; wraps SceneLoader | SceneLoader | 3 |
| DebugOverlay | Dev-only display of current key/door state; not player-facing; stripped before any release build | WorldStateManager, GameManager | 3 |

## Tier Definitions

| Tier | Label | Must work before… |
|---|---|---|
| 1 | Foundation | Any gameplay can be tested |
| 2 | Core Loop | A map is end-to-end playable (player can navigate, collect keys, open doors) |
| 3 | Supporting | The map can be selected and the developer can observe world state clearly |

## Notes

- Each map is a **single Unity scene**. Rooms are spatial areas within that scene — not separate scenes. Door/key world-state lives entirely in WorldStateManager within the loaded map scene.
- Three map scenes will be built (Map A, Map B, Map C). MapSelector is the mechanism for switching between them during development.
- DebugOverlay is a development tool only. It exists to make playtesting honest — the developer can see exactly what state the world is in without guessing.
