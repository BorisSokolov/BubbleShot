# Phase 4: Levels, Progression, Scoring, and Persistence

## 1. Objective
Transform the single playable loop into a cohesive offline game with data-driven levels, difficulty progression, scoring mechanics, star ratings, and resilient local persistence.

---

## 2. Scope
- **Data-Driven Levels**:
  - `LevelData` ScriptableObjects and JSON definitions.
  - Level parameters: Starting board layout, allowed color palette, base pressure timer ($T_{\text{max}}$), miss penalty delta, and objective type.
  - Objectives: `ClearAll`, `ClearAnchor`, `TargetScore`, `SurviveRows`.
  - Representative suite of initial levels (Levels 1–10) with demonstrable progression in difficulty and color count.
- **Scoring & Combo System**:
  - `ScoreKeeper`: Match size bonuses, exponential falling cluster score, combo chain multipliers ($1.0\times$ up to $4.0\times$).
  - Star rating thresholds calculated per level (1, 2, or 3 stars).
- **Level Select & Flow**:
  - Level selection grid displaying locked/unlocked state, highest score, and stars earned.
  - Sequential unlocking upon level completion.
- **Local Persistence**:
  - `SaveSystem`: Versioned JSON save schema (`SaveDataV1`).
  - Atomic writing via temporary file swap to eliminate corruption risks.
  - Automated migration pipeline (`ISaveMigration`) for future schema evolutions.
  - Fallback safe defaults if file is damaged, preserving backup copy.
  - Persistent settings: Audio volumes, haptic toggle, high-contrast runes, reduced-motion option.
- **Automated Validation**:
  - Unit tests for save/load serialization, schema migration, corrupt file recovery, and score calculations.

---

## 3. Explicit Non-Goals
- Remote backend, cloud saves, or online leaderboards.
- Bomb and Wild special balls (Phase 5).
- Production particle effects and custom shaders (Phase 6).
- Monetization or ad SDKs.

---

## 4. Definition of Done
1. Multiple levels can be selected, played, won, and unlocked in sequence.
2. Progression and high scores reliably persist across full game restarts.
3. Save file corruption triggers safe defaults without crashing.
4. Difficulty escalates smoothly through color count and pressure rather than unreadable speeds.
5. Independent review approves without blocking findings.
