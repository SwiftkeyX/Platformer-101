# PIPELINE.md

## Phase 1 — Pre-production

- [x] Fill out `docs/design/game-vision.md`
- [x] Fill out `docs/design/design-decisions.md`
- [x] Fill out `docs/technical/technical-preferences.md` (engine, platform, performance budgets)
- [x] Fill out `docs/design/systems-design.md` — list every system, tier, and dependencies
- [x] Fill out `docs/technical/architecture.md` with finalized script table
- [x] Fill out `docs/technical/best-practices.md` — add project-critical patterns section
- [x] Milestone 0 — vision complete, all systems tiered, architecture and tech stack finalized

## Phase 2 — Production

### Sub-phase A — Design (GDDs)
- [x] GDD: Bootstrap
- [x] GDD: GameManager
- [x] GDD: SceneLoader
- [x] GDD: InputReader
- [x] GDD: PlayerController
- [x] GDD: CameraController
- [x] GDD: WorldStateManager
- [x] GDD: KeyPickup
- [x] GDD: Door
- [x] GDD: MapSelector
- [x] GDD: DebugOverlay
- [x] Milestone 1 — all system GDDs written and approved

### Sub-phase B — Code (Implementation)

#### Tier 1 — Foundation
- [x] Code: Bootstrap
- [x] Code: GameManager
- [x] Code: SceneLoader
- [x] Code: InputReader
- [x] 🧪 Test Gate 1 — Tier 1 systems compile and Bootstrap scene initializes without errors

#### Tier 2 — Core Loop
- [x] Code: PlayerController
- [x] Code: CameraController
- [x] Code: WorldStateManager
- [x] Code: KeyPickup
- [x] Code: Door
- [x] 🧪 Test Gate 2 — player can move, collect a key, and open a door end-to-end

#### Tier 3 — Supporting Systems
- [x] Code: MapSelector
- [x] Code: DebugOverlay
- [x] 🧪 Test Gate 3 — all systems working, map selectable, debug overlay visible

- [x] Milestone 2 — core loop playable end-to-end (Test Gates 1 & 2 passed)
- [x] Milestone 3 — all features in, all test gates passed
- [ ] Architecture pass

## Phase 3 — Beta

- [ ] Juice pass — screen shake, particles, hit-stop, SFX, music, UI animations
- [ ] Feel tuning — tweak values via ScriptableObjects/Inspector
- [ ] Difficulty tuning — curve, pacing, escalation
- [ ] Bug pass — all known issues fixed (`docs/process/known-issues.md` clear)
- [ ] Performance pass — GC allocs and frame rate within budgets (`docs/technical/technical-preferences.md`)
- [ ] Ship — final build, smoke test, release (`docs/process/build-notes.md` checklist)
