# Phase 1: Unity Foundation

## 1. Objective
Establish a clean, buildable, well-tested Unity 6 LTS mobile project foundation with proper assembly definitions, runtime/test separation, bootstrap architecture, and command-line validation.

---

## 2. Scope
- **Unity 6 LTS Configuration**: Project settings configured for 2D Mobile (portrait 1080x1920 reference).
- **Assembly Definitions**:
  - `BubbleShot.Core.asmdef` (Pure C# logic, zero engine dependencies).
  - `BubbleShot.Runtime.asmdef` (Engine integration, presentation, input).
  - `BubbleShot.UI.asmdef` (UI screens, HUD, canvas).
  - `BubbleShot.Core.Tests.asmdef` (EditMode unit tests).
  - `BubbleShot.Runtime.Tests.asmdef` (PlayMode integration tests).
- **Directory Structure**: Structured folders under `Assets/Game/` for Core, Runtime, UI, Content, Tests, Scenes, Prefabs, Art, and Audio.
- **Scenes**:
  - `Bootstrap.unity` (Initialization, service registration, persistent root).
  - `MainMenu.unity` (Navigation entry point, level selection hook).
  - `Gameplay.unity` (Active play space, HUD, presentation root).
- **Input System**: New Unity Input System configured for mobile touch and mouse pointer.
- **Testing Scaffolding**: Test assembly definitions linked to NUnit and Unity Test Framework.
- **Git Hygiene**: Comprehensive `.gitignore` and `.gitattributes` avoiding binary LFS leaks and text merge conflicts.
- **Verification Scripts**: Command-line build and test runner scripts.

---

## 3. Explicit Non-Goals
- Authoritative gameplay mechanics or matching logic (deferred to Phase 2).
- Final production art, textures, or animations (deferred to Phase 6).
- Special balls implementation (deferred to Phase 5).
- Progression and persistent save files (deferred to Phase 4).
- Production audio sound effects or music (deferred to Phase 6).

---

## 4. Definition of Done
1. Project opens and compiles cleanly with zero warnings or errors.
2. Assembly references are properly isolated without circular dependencies.
3. EditMode and PlayMode test runners execute and pass.
4. Scenes can be transitioned sequentially: Bootstrap -> MainMenu -> Gameplay.
5. All `.meta` files are generated, valid, and tracked in git.
6. Independent review approves without blocking findings.
