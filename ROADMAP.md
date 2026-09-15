# BubbleShot - Product Roadmap

This document outlines the sequential phases of development for BubbleShot, defining clear objectives, deliverables, dependencies, and exit criteria.

---

## Roadmap Overview

```
[Design Phase]  --> Complete Game Design, Architecture, Decisions, and Agent Roles
       |
[Phase 1]       --> Unity Foundation, Assembly Definitions, Scenes & Test Infrastructure
       |
[Phase 2]       --> Pure C# Deterministic Engine, Hex Grid, Raycasting, Matches & Tests
       |
[Phase 3]       --> First Playable Hybrid Loop (Aim, Launch, Bounce, Pressure, Win/Loss)
       |
[Phase 4]       --> Level Progression, Data-Driven Content, Scoring & Atomic Persistence
       |
[Phase 5]       --> Bomb & Wild Special Balls, Deterministic Ambiguity Resolution
       |
[Phase 6]       --> Audio, Haptics, Visual FX, Accessibility & Safe-Area Polishing
       |
[Phase 7]       --> Balancing, Fuzz Simulation, Build Packaging & Release Readiness
```

---

## Detailed Phase Breakdown

### Phase 1: Unity Foundation
- **Goal**: Establish a pristine, buildable Unity 6 LTS mobile project with assembly definitions, clean folder hierarchy, input mapping, and testing scaffolding.
- **Key Deliverables**:
  - Unity 6 LTS project settings (portrait orientation, 2D mobile renderer).
  - Clean Assembly Definitions (`BubbleShot.Core`, `BubbleShot.Runtime`, `BubbleShot.UI`, and Test assemblies).
  - Bootstrap, MainMenu, and Gameplay scene skeletons.
  - Safe `.gitignore` and `.gitattributes` for Unity LFS and text serialization.
  - EditMode and PlayMode test runners verified.

### Phase 2: Deterministic Pure C# Engine
- **Goal**: Implement the authoritative game engine in pure C# with zero Unity dependencies, full hex-grid mathematics, wall reflections, collision tie-breaking, match logic, and exhaustive unit tests.
- **Key Deliverables**:
  - `HexCoordinates`: Odd-r horizontal coordinates, neighbor lookups, boundary checks.
  - `HexBoard`: Grid storage, cell queries, ceiling anchors, row insertions.
  - `TrajectorySolver`: Continuous swept-circle collision, wall reflections, deterministic snap tie-breaking.
  - `MatchResolver`: BFS same-color group collection ($\ge 3$ threshold).
  - `ClusterResolver`: Multi-source BFS from ceiling identifying detached balls.
  - `PressureEngine`: Real-time timer, miss penalties, match relief calculations.
  - `DeterministicRng`: Seeded random number generator.
  - Comprehensive unit test suite covering all boundary and collision cases.

### Phase 3: First Playable Hybrid Loop
- **Goal**: Connect the pure C# engine to Unity presentation to deliver an end-to-end playable single-level loop.
- **Key Deliverables**:
  - Touch & pointer aim input with trajectory preview line renderer.
  - Interpolated projectile flight and snapping visualization.
  - Pop animations and detached ball drop effects.
  - HUD: Pressure gauge, consecutive miss indicators, score counter, danger line.
  - Full game lifecycle: Pause, Resume, Restart, Defeat, Victory screens.
  - Invariant protection: Input blocked during animations, no timer leaks while paused/backgrounded.

### Phase 4: Progression, Scoring & Persistence
- **Goal**: Transform the playable loop into a multi-level structured game with data-driven levels and resilient local persistence.
- **Key Deliverables**:
  - Data-driven level definitions (`LevelData` ScriptableObjects and JSON configs).
  - Multi-objective engine (Clear All, Clear Anchor, Score Target, Survive Rows).
  - Combo system with exponential detached multipliers and star ratings (1–3 stars).
  - Level select screen showing unlocked stages and earned stars.
  - Local save system with atomic file writing (`.tmp` swap), versioned schema, and migration pipeline.
  - User settings persistence (SFX, Music, Haptics, Accessibility).

### Phase 5: Bomb & Wild Special Balls
- **Goal**: Introduce Bomb and Wild mechanics with strict determinism and accessibility.
- **Key Deliverables**:
  - Bomb Ball: Deterministic radial burst clearing all balls at distance $\le 1$.
  - Wild Ball: Simultaneous multi-color group evaluation and cascade popping.
  - Special ball queue integration and combo reward generation.
  - Unique high-contrast glyph overlays ensuring immediate recognition without color alone.
  - Comprehensive unit tests covering all multi-color ambiguous wild placement configurations.

### Phase 6: Visual, Audio, Haptic & UX Polish
- **Goal**: Elevate tactile feel and presentation to commercial quality while ensuring accessibility and responsiveness.
- **Key Deliverables**:
  - Visuals: Ball depth, high-contrast runes, projectile glow trail, pop particle bursts, subtle board danger pulsations.
  - Audio: Procedurally synthesized / royalty-free melodic pop tones scaling with combo chains.
  - Haptics: Distinct mobile haptic feedback pulses for wall bounces, matches, and row drops.
  - Accessibility: Reduced-motion toggle (replaces shakes/explosions with gentle fades).
  - Safe Area: Adaptive UI layout respecting notches and dynamic device aspect ratios.

### Phase 7: Balancing, Robustness & Release Readiness
- **Goal**: Validate difficulty curve, stress-test stability, automate simulation fuzzing, and prepare build packaging.
- **Key Deliverables**:
  - Interactive onboarding tutorial introducing aiming, wall bounces, and pressure mechanics.
  - Difficulty balancing across 15+ curated stages.
  - Automated headless simulation running 10,000 randomized shots validating no hangs or invariant breaches.
  - Android build scripts and APK generation validation.
  - Final execution reports, known-issues audit, and human handoff guide.
