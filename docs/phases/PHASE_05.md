# Phase 5: Bomb and Wild Special Balls

## 1. Objective
Introduce the initial special balls—Bomb Ball and Wild Ball—into the authoritative engine and presentation layer with deterministic ambiguity resolution, unique visual identifiers, and comprehensive test coverage.

---

## 2. Scope
- **Bomb Ball Mechanics**:
  - `BallType.Bomb`: Detonates immediately upon attachment.
  - Blast Radius: Deterministic radius $\le 1$ hex distance (clears the target cell and up to 6 immediate adjacent neighbors).
  - Cleared balls are awarded points and relieve pressure.
  - Cascades detached clusters after detonation.
- **Wild Ball Mechanics**:
  - `BallType.Wild`: Universal wildcard matching any color.
  - Ambiguous Resolution Rule: Inspects all adjacent neighbor colors. For every color $C$ that forms a connected group $\ge 3$ including the Wild Ball, all such qualifying groups pop simultaneously in a multi-color sweep.
  - If no neighboring group reaches $\ge 3$, the Wild Ball remains parked on the board as a wild wildcard.
- **Acquisition & Spawning**:
  - High combo rewards (e.g. 3 consecutive matches loads a special ball into the launcher preview).
  - Level-configured pre-placed special balls on the starting board.
- **Presentation & Accessibility**:
  - Distinctive visual styling: Bomb has an unmistakable fuse/spark glyph and deep crimson tint; Wild has a prismatic rainbow sheen and star icon.
  - Identifiable instantly without color reliance.
  - Bomb explosion particle burst and screen shake (respecting reduced-motion settings).
- **Unit & Integration Tests**:
  - Tests covering blast radius boundaries, multi-color simultaneous wild pops, wild balls acting as bridges between disparate groups, and interaction ordering with falling clusters.

---

## 3. Explicit Non-Goals
- Any additional special ball types (Lightning, Color Wipe, Laser are logged in future backlog).
- Paid power-up stores or consumables.
- Non-deterministic or probability-based special effects.

---

## 4. Definition of Done
1. Bomb and Wild balls resolve authoritatively with 100% determinism.
2. Wild multi-color ambiguous placement resolves all qualifying groups without nondeterministic bias.
3. Both special balls are instantly recognizable for color-blind players.
4. Comprehensive test coverage passes with zero regressions.
5. Independent review approves without blocking findings.
