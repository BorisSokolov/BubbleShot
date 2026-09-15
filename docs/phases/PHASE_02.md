# Phase 2: Deterministic Pure C# Engine

## 1. Objective
Implement and rigorously test the authoritative game engine in pure C#, completely decoupled from Unity scene objects, MonoBehaviours, and frame timing.

---

## 2. Scope
- **Data Models**:
  - `BallColor`: Red, Blue, Green, Yellow, Purple, Orange.
  - `BallType`: Normal, Bomb, Wild.
  - `HexCoord`: Struct with `(int Row, int Col)`, boundary validation, and parity calculation.
  - `HexBoard`: Grid storage supporting dynamic row insertions and cell inspection.
- **Hex Mathematics**:
  - 6-neighbor lookup with odd-r horizontal staggering.
  - Metric coordinate conversion (`LocalPositionFromCoord`, `CoordFromLocalPosition`).
- **Trajectory & Reflection**:
  - `TrajectorySolver`: Swept-circle continuous raycasting.
  - Rigid wall reflections with horizontal velocity negation.
  - Ceiling boundary snapping.
  - Deterministic tie-breaking: prefer higher row, then leftmost column.
- **Match & Detached Cluster Resolution**:
  - `MatchResolver`: BFS same-color group collection ($\ge 3$ threshold).
  - `ClusterResolver`: Multi-source BFS from Row 0 ceiling anchors identifying falling clusters.
- **Hybrid Pressure Primitives**:
  - `PressureEngine`: Real-time countdown timer, miss penalties, match relief calculation.
  - Row descent generation using deterministic PRNG.
- **RNG**:
  - `DeterministicRng`: Isolated XorShift32 algorithm producing identical outcomes across platforms for identical seeds.
- **Test Suite**:
  - 100% test coverage of all neighbor parities, reflection angles, snap tie-breaks, group matches, floating clusters, and PRNG seeds.

---

## 3. Explicit Non-Goals
- Visual sprite rendering or particle effects.
- Touch or pointer input management.
- Special ball execution logic (Bomb and Wild reserved for Phase 5).
- Audio and haptic triggering.
- Persistent file storage.

---

## 4. Definition of Done
1. Engine can execute complete game simulations purely in memory without loading Unity scenes.
2. Identical input sequences and random seeds produce identical final states across runs.
3. Zero references to `UnityEngine` in `BubbleShot.Core`.
4. Comprehensive test suite passes with zero failures.
5. Independent review approves without blocking findings.
