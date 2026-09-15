# BubbleShot

An offline, single-player mobile action-puzzle game built with Unity 6 LTS and a deterministic pure C# gameplay engine.

---

## Highlights
- **Hybrid Pressure Model**: Board advancement is governed by both real-time countdown pressure and shot efficiency.
- **Deterministic Pure C# Engine**: Authoritative physics, swept-circle trajectory raycasting, wall bounces, and matching logic decoupled entirely from UnityEngine.
- **Color-Blind Accessible**: Every ball features a distinct geometric rune embossed in its center, guaranteeing full readability independent of color.
- **Resilient Persistence**: Atomic file writing, versioned JSON schema, and safe recovery mechanisms.
- **Zero Predatory Mechanics**: No ads, no in-app purchases, no user accounts, no online requirements.

---

## Project Structure
```
Assets/
  Game/
    Core/          # Pure C# game engine (no UnityEngine dependencies)
    Runtime/       # Presentation, Input System, Lifecycle controllers
    UI/            # Canvas UI screens and HUD
    Content/       # Data-driven Level definitions and palettes
    Tests/         # EditMode and PlayMode test suites
    Scenes/        # Bootstrap, MainMenu, Gameplay scenes
Documentation/
  GAME_DESIGN.md   # Core gameplay, pressure math, and mechanics
  ARCHITECTURE.md  # System architecture, hex coordinates, trajectory solver
  ROADMAP.md       # 7-phase implementation roadmap
  AGENTS.md        # Autonomous multi-agent governance & review protocol
  docs/phases/     # Detailed phase execution plans (Phase 1–7)
  docs/decisions/  # Architectural decision records (ADRs)
  docs/reports/    # Autonomous execution reports & validation logs
```

---

## Documentation Quick Links
- [Game Design Document](GAME_DESIGN.md)
- [Architecture & Technical Specification](ARCHITECTURE.md)
- [Project Roadmap](ROADMAP.md)
- [Multi-Agent Governance](AGENTS.md)
- [Architectural Decisions](docs/decisions/initial-design-decisions.md)
- [Autonomous Execution Log](docs/reports/AUTONOMOUS_EXECUTION.md)
