# Technical Preferences

## Engine & Language

| Field | Value |
|---|---|
| **Engine** | Unity 6 (6000.4.9f1) |
| **Language** | C# |
| **Rendering** | URP (Universal Render Pipeline) |
| **Physics** | Unity 3D Physics / PhysX |

## Input & Platform

| Field | Value |
|---|---|
| **Target Platforms** | PC (Windows) |
| **Input Methods** | Keyboard/Mouse, Gamepad |
| **Primary Input** | Keyboard/Mouse |
| **Gamepad Support** | Partial — wired up but not the priority |
| **Touch Support** | None |

**Platform Notes**

All gameplay-critical actions must have keyboard bindings. Gamepad bindings are additive — no action is keyboard-only if a gamepad equivalent can be mapped. Mouse-only interactions are not permitted (must have a keyboard or gamepad alternative).

## Performance Budgets

| Budget | Target | Notes |
|---|---|---|
| **Target Framerate** | 60 fps | Desktop standalone |
| **Frame Budget** | 16.6 ms | Derived from 60 fps |
| **Draw Calls** | TBD — set after first profiling pass | Graybox primitives are low; budget after first scene is built |
| **Memory Ceiling** | TBD — set after first profiling pass | |
| **GC Alloc / Frame** | Zero in steady state | Allocations cause frame spikes; enforce from the start |

## Testing

| Field | Value |
|---|---|
| **Framework** | NUnit (Unity Test Runner) |
| **Test types** | Edit Mode (pure logic), Play Mode (scene/runtime) |
| **Minimum Coverage** | All stateful systems (door/key world-state, camera state transitions) |

**Required Tests** — systems that must have tests before any map is considered done:

- Door/key state machine (locked → unlocked transition)
- Camera logic (state transitions between tracking modes, if applicable)
- Any world-state persistence between room connections

## Forbidden Patterns

- `FindObjectOfType` at runtime — use cached references or dependency injection instead
- `DontDestroyOnLoad` — persistent objects belong in Bootstrap scene only
- `SceneManager.LoadScene` called directly — route through SceneLoader

## Allowed Libraries / Addons

- Unity Input System (com.unity.inputsystem) — keyboard + gamepad input abstraction

## Architecture Decisions Log

- No ADRs yet — create `.claude/docs/other/adr/adr-001-*.md` for the first significant technical decision

## Agent / Specialist Routing

| Task Type | Agent / Skill | Notes |
|---|---|---|
| General C# scripts, scene wiring | `gameplay-programmer` | Default for all Unity work |
| Architecture review, code audit | `technical-director` | Read-only — advises, does not implement |
| Shader / material work | `gameplay-programmer` | URP Lit shader is sufficient for graybox phase |
| UI implementation | `ui-programmer` | Use for any Canvas or HUD work |
| Asset loading | `gameplay-programmer` | No Addressables in scope for Phase 1 |
| Security review | `/security-review` skill | |

### File Extension Routing

| File Type | Agent to Use |
|---|---|
| `.cs` game scripts | `gameplay-programmer` |
| `.shader`, `.shadergraph`, `.mat` | `gameplay-programmer` |
| `.uxml`, `.uss`, Canvas prefabs | `ui-programmer` |
| `.unity`, `.prefab` | `gameplay-programmer` (via coplay MCP) |
| Architecture review | `technical-director` |
