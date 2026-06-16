# Game Vision — Platformer 101

## Concept

**Name:** Platformer 101

**One-sentence concept:** A 3D graybox platformer built to learn layout design, camera logic, and door/key world-state mechanics through rapid prototyping and honest playtesting.

---

## Feel Pillars

**Floaty · Precise · Exploratory**

- **Floaty** — jumps feel airy; the player spends meaningful time in the air. Gravity is not punishing.
- **Precise** — air control is generous but the player's intent is respected. Movement does what you expect.
- **Exploratory** — the space rewards curiosity. Rooms invite the player to look around before committing to a path.

---

## Intended Player Experience

Each session should feel like moving through a space that *makes sense*. The player should feel:

- In control of their movement at all times — never fighting the camera or physics.
- Curious about what's behind the next door or around the next corner.
- Satisfied when they discover the correct path — not frustrated by obscurity.

The experience is intrinsic. There are no score counters, no timers, no enemies (Phase 1). The only feedback is whether the space feels good to move through.

---

## Difficulty Curve

This game uses **map iteration** rather than a linear difficulty progression. Phase 1 targets layout quality, not enemy or mechanical escalation.

| Iteration | Map | Mechanic introduced | Exit gate |
|---|---|---|---|
| 1 | Map A | Basic camera tracking, jump traversal | Playtest: does movement feel correct? |
| 2 | Map B | Door/key world-state, room connections | Playtest: does the map flow feel logical? |
| 3 | Map C | Combined — camera + door/key + map flow | Playtest: is this map genuinely fun to move through? |

**Escalation pattern:** Each map adds one new system on top of the last. No map is discarded until it has been playtested cold (by someone who did not build it, or after a break).

**Pacing target per map:** A first-time player should reach every key interaction (a door, a locked area, a high platform) within 3–5 minutes of natural exploration.

**Exit condition for Phase 1:** One surviving map that a cold player finds genuinely enjoyable with zero art. If none of the three maps pass this gate, redesign — do not proceed to art or polish.

**Movement speed target:** Character moves at a pace where the player can observe the environment while moving — not so fast that they overshoot doorways, not so slow that traversal feels like a chore.

---

## Scope (Phase 1)

- Primitives only — no custom art, no textures beyond flat colors for readability (e.g., floor vs. wall distinction).
- No enemies, no score, no HUD beyond what aids development.
- Three maps will be built. The bad ones are discarded. Only the surviving map advances to Phase 2.
- Primary learning targets: camera logic, door/key world-state, map flow, room connections.

---

## Target Platform

**PC (Windows)** — standalone build, keyboard + mouse or gamepad.

## Audience

**Personal learning project.** No external audience. Success is measured by the developer's own understanding and the quality of the surviving map — not by player count or polish.
