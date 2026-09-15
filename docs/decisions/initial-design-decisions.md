# Architectural Decision Records (ADR) - Initial Design

## ADR 001: Hexagonal Grid Geometry & Coordinate System
- **Status**: Accepted
- **Context**: Bubble shooter games require a regular packing geometry. We evaluated axial coordinates, cube coordinates, and staggered 2D offset coordinates.
- **Decision**: Adopt the **Odd-R Horizontal Staggered Offset Grid**:
  - Even rows have 8 columns ($c \in [0, 7]$).
  - Odd rows have 7 columns ($c \in [0, 6]$) shifted right by half a ball diameter.
- **Rationale**:
  - The odd-r horizontal layout aligns naturally with vertical gravity and mobile portrait screens.
  - Odd rows with $W-1$ columns naturally create boundary recesses that seamlessly nest against left and right vertical walls, eliminating awkward corner gaps.
  - Array storage is compact and coordinates $(r, c)$ directly map to visual rows and columns.

---

## ADR 002: Decoupled Pure C# Authoritative Gameplay Engine
- **Status**: Accepted
- **Context**: Tightly coupling game logic to Unity MonoBehaviours or Unity 2D Physics leads to non-deterministic behavior, frame-rate dependencies, and slow testing cycles requiring scene loads.
- **Decision**: Implement all board state, raycasting, matching, pressure, and scoring in pure C# with zero dependencies on `UnityEngine`.
- **Rationale**:
  - 100% deterministic simulation across platforms and architectures.
  - Tests run in milliseconds via standard .NET unit runners without opening Unity scenes.
  - Presentation (Unity) only reacts to engine events and immutable state snapshots.

---

## ADR 003: Hybrid Pressure Model (Continuous Timer + Shot Penalties)
- **Status**: Accepted
- **Context**: Classic bubble shooters often feel either too static (turn-based with endless time to aim) or too frantic (pure real-time arcade speed without strategic depth).
- **Decision**: Implement a **Hybrid Pressure Model**:
  - A visible pressure countdown timer ($T_{\text{max}} \approx 12\text{s}$) runs continuously during active play.
  - Unsuccessful shots (misses) penalize the timer ($-2.0\text{s}$) and advance a consecutive miss counter (4 misses triggers an immediate row drop).
  - Successful matches grant pressure relief ($+1.0\text{s} + (N-3)\times 0.5\text{s}$).
  - Timer strictly pauses during pause screens, app backgrounding, and blocking resolution animations.
- **Rationale**:
  - Balances thoughtful spatial aiming with urgency.
  - Punishes careless spamming while rewarding accurate, strategic play.
  - Prevents stalls while maintaining readability.

---

## ADR 004: Swept-Circle Continuous Raycasting & Deterministic Tie-Breaking
- **Status**: Accepted
- **Context**: Ray-circle intersection or discrete step simulation can result in tunneling or ambiguous cell attachment when hitting near the border between two adjacent empty cells.
- **Decision**: Implement continuous swept-circle intersection math with rigid wall reflections and explicit deterministic tie-breaking:
  1. Earliest collision parameter $t^*$ is authoritatively selected.
  2. The candidate empty neighbor cell closest to the projectile center at impact is selected.
  3. If distances are identical within numerical epsilon, prefer:
     - Smaller row index (closer to ceiling anchor).
     - Smaller column index (left-most).
- **Rationale**: Completely eliminates non-deterministic placement anomalies and edge-case tunneling.

---

## ADR 005: Wild Ball Multi-Color Resolution
- **Status**: Accepted
- **Context**: When a Wild Ball attaches adjacent to two or more different colored groups, selecting which color it matches could be ambiguous or require player prompts that disrupt action flow.
- **Decision**: The Wild Ball simultaneously evaluates all distinct colors present among its immediate neighbors. If a color forms a connected group $\ge 3$ (including the Wild Ball), **all** qualifying groups are cleared together in a unified multi-color cascade. If no group reaches $\ge 3$, the Wild Ball stays on the board.
- **Rationale**: Fully deterministic, highly rewarding for skillful placement between multiple clusters, and requires zero modal interaction.

---

## ADR 006: Bomb Ball Deterministic Radius
- **Status**: Accepted
- **Context**: Area-of-effect bomb balls need clear, intuitive boundaries.
- **Decision**: The Bomb Ball destroys all balls within hex distance $\le 1$ (the hit cell and its 6 immediate neighbors, up to 7 balls total).
- **Rationale**: Discrete hex distance is completely predictable, easy for players to read visually, and avoids partial circle-overlap edge cases.

---

## ADR 007: Atomic Local Persistence & Versioning
- **Status**: Accepted
- **Context**: Mobile games frequently suffer save file corruption during sudden power loss or OS task termination while writing to disk.
- **Decision**: Implement atomic file writing using temporary files (`savegame.json.tmp`) replaced via atomic file system operations, backed by an `ISaveMigration` versioned pipeline.
- **Rationale**: Zero risk of half-written save files; full backward compatibility across future updates.

---

## ADR 008: High-Contrast Geometric Runes for Accessibility
- **Status**: Accepted
- **Context**: Red-green and blue-yellow color vision deficiencies affect significant portions of mobile players.
- **Decision**: Every ball color is assigned a permanent, high-contrast geometric symbol embossed inside its center (Circle, Square, Diamond, Triangle, Star, Cross).
- **Rationale**: Ensures the game is 100% playable based on shape recognition alone, complying with WCAG and modern accessibility standards.
