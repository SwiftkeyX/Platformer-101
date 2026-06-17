# Docs Index

> **Structure:** docs live under exactly four folders — `preproduction/ production/ beta/ other/`. Never add a new top-level folder; see `.claude/rules/docs-structure.md`.
> **Control files at root:** `index.md` (this file), `PIPELINE.md`, `project-snapshot-index.md`.

---

## Pre-production (`preproduction/`)

| File | Status | Purpose |
|---|---|---|
| `preproduction/game-vision.md` | Complete | Game name, concept, feel pillars, difficulty curve, platform, audience |
| `preproduction/design-decisions.md` | Complete | Core mechanic choices, scope constraints, explicit non-goals |
| `preproduction/technical-preferences.md` | Complete | Unity 6, URP, performance budgets, forbidden patterns |
| `preproduction/systems-design.md` | Complete | 11 systems — tier, responsibility, and dependency table |
| `preproduction/architecture.md` | Complete | Script table — one row per script; communication contract |
| `preproduction/best-practices.md` | Complete | Project-critical hard rules + Unity 6 current patterns |

---

## Production — GDDs (`production/gdd/`)

One doc per system. All 11 GDDs are complete; Sub-phase B (coding) not yet started.

### Tier 1 — Foundation

| File | Status | System |
|---|---|---|
| `production/gdd/Bootstrap.md` | GDD complete | Persistent scene; initializes GameManager + SceneLoader |
| `production/gdd/GameManager.md` | GDD complete | Global game state (Playing/Paused); `SetState` + event |
| `production/gdd/SceneLoader.md` | GDD complete | Only authorized scene-loading path; wraps `LoadSceneAsync` |
| `production/gdd/InputReader.md` | GDD complete | ScriptableObject event channel for Move / Jump / Interact |

### Tier 2 — Core Loop

| File | Status | System |
|---|---|---|
| `production/gdd/PlayerController.md` | GDD complete | 3D movement, floaty jump, coyote time, jump buffering |
| `production/gdd/CameraController.md` | GDD complete | Third-person orbit camera; exposes `Forward` for movement |
| `production/gdd/WorldStateManager.md` | GDD complete | Scene-level key/door state store; fires events |
| `production/gdd/KeyPickup.md` | GDD complete | Trigger-based key collection; calls WorldStateManager |
| `production/gdd/Door.md` | GDD complete | Subscribes to WorldStateManager; slides open on unlock |

### Tier 3 — Supporting

| File | Status | System |
|---|---|---|
| `production/gdd/MapSelector.md` | GDD complete | Minimal MainMenu UI to load one of the 3 map scenes |
| `production/gdd/DebugOverlay.md` | GDD complete | Dev-only overlay showing collected keys + unlocked doors |

---

## Beta (`beta/`)

No files yet — Phase 3 has not started.

---

## Other (`other/`)

| File | Status | Purpose |
|---|---|---|
| `other/movement-reference.md` | Complete | Platformer movement technique library — C# patterns, tuning ranges, project status per technique |
