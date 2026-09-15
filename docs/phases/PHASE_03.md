# Phase 3: First Playable Hybrid Loop

## 1. Objective
Bridge the pure C# engine to Unity presentation to deliver an end-to-end playable single-level loop with real-time pressure, aiming, wall reflections, and win/loss states.

---

## 2. Scope
- **Presentation Architecture**:
  - `BoardView`: Instantiates and maintains ball views corresponding to authoritative `HexBoard` state.
  - `BallView`: Visual representation with color tinting and distinct high-contrast geometric symbol overlay.
  - `LauncherController`: Touch and pointer drag aiming with clamped angle restrictions ($[10^\circ, 170^\circ]$).
  - `TrajectoryPreview`: Multi-segment dashed trajectory line visualizing bounces off walls and target landing reticle.
- **Flight & Snapping Presentation**:
  - Interpolated flight along the authoritative path.
  - Smooth snap into target hex grid position upon collision.
- **Dynamic Board Feedback**:
  - Pop animation sequence for matched ball groups ($\ge 3$).
  - Detached cluster gravity drop animation.
  - Smooth downward translation of existing rows when new row descends.
- **HUD & Pressure Feedback**:
  - Real-time pressure gauge visualizing remaining countdown time.
  - Miss indicators showing progress toward forced row drop.
  - Red danger line placed above Row 11 with warning pulsation when balls breach Row 9.
  - Running score and combo multiplier text.
- **Game Lifecycle**:
  - Pause menu (freezes pressure timer and projectile flight).
  - Backgrounding/unfocus handling (auto-pauses game safely).
  - Victory and Defeat modal screens with Restart and Menu actions.
- **Integration Tests**:
  - PlayMode tests verifying aim input, shot resolution, pause freeze, and win/loss transitions.

---

## 3. Explicit Non-Goals
- Multi-level progression or campaign map (Phase 4).
- Persistent disk storage (Phase 4).
- Bomb and Wild special balls (Phase 5).
- Final production audio mixing and custom shaders (Phase 6).

---

## 4. Definition of Done
1. Player can launch balls, bounce off walls, clear groups, and experience pressure row drops in real time.
2. Aiming is intuitive and visual landing matches authoritative snap cell with zero discrepancy.
3. Pausing or minimizing application strictly stops board progression.
4. Input is completely locked during active pop/drop animations.
5. Win and loss conditions trigger correctly on objective completion or danger line breach.
6. Independent review approves without blocking findings.
