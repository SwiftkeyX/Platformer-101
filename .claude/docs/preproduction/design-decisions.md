# Design Decisions — Platformer 101

## Core Mechanic Constraints

**What this game IS:**

1. **3D traversal is the mechanic.** The game is about moving through a 3D space. Every system exists to support or gate that movement. Nothing else.

2. **Door/key world-state is the gating mechanic.** Finding a key opens a door. This is the only form of progression lock in Phase 1. No other state-change mechanics are in scope.

3. **Camera is a first-class design concern.** The camera is not a default Unity follow cam left on auto. It must be explicitly designed, implemented, and playtested as part of layout validation.

4. **Map layout is the primary deliverable.** Three maps will be built and playtested. Weak maps are discarded. The surviving map is the output of Phase 1 — not code, not assets.

---

## Explicit Non-Goals

**What this game is NOT:**

1. **Not an art project.** Phase 1 uses primitives only. Art does not ship until layout is proven. Art never compensates for bad layout.

2. **Not a combat or enemy game.** No enemies, no health system, no damage in Phase 1. These are out of scope for the layout learning goal.

3. **Not a score or reward game.** No points, no timers, no collectibles beyond keys. Reward systems would distract from evaluating whether the space itself is enjoyable.

4. **Not a shipped product.** This is a personal learning project. Success is measured by developer understanding and layout quality — not player count, ratings, or release readiness.

---

## Non-Obvious Choices

**Why these decisions over the alternatives:**

1. **Build 3 maps and discard the bad ones — not refine one map endlessly.**
   Iteration over a single map creates attachment bias. The developer optimizes toward making the existing layout work rather than questioning whether the layout is fundamentally good. Building separate maps and discarding forces honest evaluation.

2. **"Fun without art" as the Phase 1 exit gate.**
   Polish is never applied to Phase 1 output. If a map is not enjoyable as gray boxes, art will not fix it — it will only delay discovering the layout problem. The graybox gate forces the decision before investment compounds.

3. **Floaty movement feel over snappy or weighty.**
   Forgiving air control ensures that playtests measure map quality, not player skill. If movement is punishing, playtest feedback becomes noise ("I kept dying at that gap") rather than signal ("that room felt dead-end-ish"). Floaty keeps the feedback clean.

---

## Scope Boundaries

| Category | In scope (Phase 1) | Out of scope (Phase 1) |
|---|---|---|
| Core systems | Movement, jump, camera, door, key | Enemies, combat, health |
| Content | 3 map iterations (→ 1 surviving) | Art, textures, audio |
| UI / feedback | None beyond dev aids | HUD, score, menus |
| Platform | PC (Windows), standalone | WebGL, mobile, multiplayer |
| Goal | Developer learning + surviving map | Release, audience, polish |
