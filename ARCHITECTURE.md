# BubbleShot - Software Architecture & Technical Specification

## 1. Architectural Principles & System Boundary

The core architecture strictly enforces a unidirectional, decoupled data flow between authoritative gameplay logic and Unity presentation:

```
+-------------------------------------------------------------+
|                     Pure C# Engine                          |
|  - Hex Grid Coordinates & Board State                       |
|  - Swept-Circle Deterministic Raycasting                    |
|  - Reflection & Snap Point Tie-Breaking                     |
|  - Match & Detached Cluster Resolvers                       |
|  - Pressure, Miss Count & Row Advancement                   |
|  - Scoring, Combos & Win/Loss Conditions                    |
|  - Seeded Deterministic PRNG                                |
+-------------------------------------------------------------+
                              |
                     [Engine Events & Snapshots]
                              v
+-------------------------------------------------------------+
|                 Unity Presentation Layer                    |
|  - Ball Renderers & Interpolated Flight                     |
|  - Trajectory Line Renderer & Reticle                       |
|  - Pop Particles, Screen Shake & Floating Scores            |
|  - Audio SFX & Haptics Triggering                           |
|  - Touch/Pointer Input Routing via New Input System         |
|  - Responsive Canvas UI & Safe Area Adaptation              |
+-------------------------------------------------------------+
```

### 1.1 Separation Invariants
- The **Pure C# Engine** has zero dependencies on `UnityEngine`, `MonoBehaviour`, `GameObject`, `Transform`, `Rigidbody2D`, `Collider2D`, or frame timing.
- Every state mutation occurs via deterministic command execution:
  `GameCommand -> Engine.Execute(command) -> IEnumerable<EngineEvent>`
- Presentation classes only observe state changes and render animations. Animation durations or visual frame rates **never** dictate or alter gameplay outcomes.

---

## 2. Hexagonal Grid Coordinate System

### 2.1 Coordinate Space
We utilize an **Odd-R Horizontal Staggered Hexagonal Grid**:
- Coordinates are discrete integer pairs: $\mathbf{c} = (r, c)$, where $r$ is the row index ($0 \le r < R_{\text{max}}$) and $c$ is the column index ($0 \le c < C_r$).
- For even rows ($r \pmod 2 == 0$): $C_r = W = 8$, columns $c \in [0, 7]$.
- For odd rows ($r \pmod 2 == 1$): $C_r = W - 1 = 7$, columns $c \in [0, 6]$.

### 2.2 Grid to Metric Space Conversion
Let $d$ be the ball diameter ($d = 2 \times \text{radius}$).
The vertical spacing between adjacent rows is:
$$\Delta y = d \times \frac{\sqrt{3}}{2} \approx 0.8660254 \times d$$
The position of cell $(r, c)$ in local board space (with $(0,0)$ at top-left anchor origin) is:
$$x(r, c) = \begin{cases} c \cdot d + \frac{d}{2} & \text{if } r \pmod 2 == 0 \\ (c + 1) \cdot d & \text{if } r \pmod 2 == 1 \end{cases}$$
$$y(r, c) = -r \cdot \Delta y$$

### 2.3 Six-Neighbor Connectivity
Given cell $(r, c)$, its 6 immediate hexagonal neighbors are:
- **If $r$ is even**:
  1. Top-Left: $(r-1, c-1)$
  2. Top-Right: $(r-1, c)$
  3. Left: $(r, c-1)$
  4. Right: $(r, c+1)$
  5. Bottom-Left: $(r+1, c-1)$
  6. Bottom-Right: $(r+1, c)$
- **If $r$ is odd**:
  1. Top-Left: $(r-1, c)$
  2. Top-Right: $(r-1, c+1)$
  3. Left: $(r, c-1)$
  4. Right: $(r, c+1)$
  5. Bottom-Left: $(r+1, c)$
  6. Bottom-Right: $(r+1, c+1)$

A neighbor is **valid** if and only if $0 \le r < R_{\text{max}}$ and $0 \le c < C_r$.

---

## 3. Deterministic Trajectory & Collision Resolution

### 3.1 Wall Bounce Simulation
- Board width: $X_{\text{max}} = W \times d = 8 \cdot d$.
- Left boundary reflection at $x = \text{radius}$.
- Right boundary reflection at $x = X_{\text{max}} - \text{radius}$.
- Trajectory is simulated as a sequence of 2D line segments with horizontal velocity negation:
  $$\mathbf{v}' = (-v_x, v_y)$$
- Max reflections permitted: 2 bounces before ceiling or ball contact.

### 3.2 Continuous Swept-Circle Collision
For each trajectory segment $\mathbf{p}(t) = \mathbf{p}_0 + \mathbf{v} \cdot t$, we compute intersection against:
1. Every currently occupied board cell $(r, c)$ with center $\mathbf{c}_i$ and collision radius $2 \times \text{radius} = d$.
2. The ceiling anchor line $y = y(0) + \text{radius}$.

The earliest collision parameter $t^* = \min_i(t_i) \ge 0$ is authoritatively selected.

### 3.3 Snap Cell Selection & Tie-Breaking
At collision point $\mathbf{p}(t^*)$, we test all empty valid neighbor cells surrounding the hit ball (or the ceiling cells if ceiling collision).
- Candidate cell $k$ minimizes Euclidean distance $||\mathbf{p}(t^*) - \mathbf{c}_k||$.
- **Tie-Breaking Rule**: If $||\mathbf{p} - \mathbf{c}_a|| == ||\mathbf{p} - \mathbf{c}_b||$:
  1. Primary: Prefer smaller row index (higher up).
  2. Secondary: Prefer smaller column index (left-most).

---

## 4. Game Rules & Match Resolution Algorithm

### 4.1 Step-by-Step Resolution Pipeline
1. **Place Ball**: Insert projectile ball type/color into target cell $(r, c)$.
2. **Special Ball Branching**:
   - If ball is **Bomb**: Collect all valid neighbors within distance $\le 1$. Add hit cell. Clear all collected balls.
   - If ball is **Wild**: Find all distinct colors among neighbors. For each color $C$, run BFS group detection. If any group size $\ge 3$, mark all balls in qualifying groups for removal.
   - If ball is **Normal**: Run BFS for same-color neighbors. If group size $\ge 3$, mark group for removal.
3. **Cluster Resolution**:
   - Run multi-source BFS starting from all occupied cells in Row 0.
   - Any occupied cell unreached during this search is identified as **Disconnected Cluster**.
   - Clear all disconnected balls from the board.
4. **Pressure & Miss Updates**:
   - If balls were removed:
     - Relieve pressure: $\Delta T = 1.0\text{s} + (N_{\text{matched}} - 3) \times 0.5\text{s} + N_{\text{disconnected}} \times 0.4\text{s}$.
     - Reset consecutive miss counter to 0.
     - Increment combo multiplier.
   - If no balls were removed:
     - Apply miss penalty: $-2.0\text{s}$ to pressure timer.
     - Increment miss counter. If miss counter $\ge 4$, trigger immediate row descent.
     - Reset combo multiplier to 1.
5. **Row Descent Check**:
   - If pressure timer $\le 0$ or miss drop triggered:
     - Shift all rows downward: cell $(r, c) \rightarrow (r+1, c')$ accounting for parity shift.
     - Generate new procedural top row at $r=0$ using seeded PRNG.
6. **Danger & Victory Evaluation**:
   - If any remaining ball occupies row $r \ge 11$, emit `GameOverEvent(Result.Defeat)`.
   - Else if level objective satisfied (e.g. board empty), emit `GameOverEvent(Result.Victory)`.

---

## 5. Seeded Random Number Generator

All procedural row generation and randomized level layouts rely on an isolated, deterministic pseudo-random number generator (`DeterministicRng` using XorShift32 or PCG32):
```csharp
public class DeterministicRng
{
    private uint _state;
    public DeterministicRng(uint seed) => _state = seed == 0 ? 0x853c49e6 : seed;
    public uint Next() {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }
    public int NextRange(int min, int max) => min + (int)(Next() % (uint)(max - min));
}
```
This guarantees 100% reproducible board layouts, row additions, and test scenarios.

---

## 6. Assembly Definitions & Packaging

The project is structured into clear, decoupled assembly definitions:

| Assembly Name | Target | Dependencies | Constraints |
|---------------|--------|--------------|-------------|
| `BubbleShot.Core` | `Assets/Game/Core` | None | Pure C# only, no `UnityEngine` |
| `BubbleShot.Runtime` | `Assets/Game/Runtime` | `BubbleShot.Core`, `Unity.InputSystem`, `Unity.TextMeshPro` | Presentation & Platform |
| `BubbleShot.UI` | `Assets/Game/UI` | `BubbleShot.Core`, `BubbleShot.Runtime`, `Unity.TextMeshPro` | Canvas Screens & HUD |
| `BubbleShot.Core.Tests` | `Assets/Game/Tests/EditMode` | `BubbleShot.Core`, `nunit.framework` | Fast unit & simulation tests |
| `BubbleShot.Runtime.Tests` | `Assets/Game/Tests/PlayMode` | `BubbleShot.Core`, `BubbleShot.Runtime`, `UnityEngine.TestRunner` | Integration tests |

---

## 7. Persistence Architecture

Save data is maintained locally using a versioned JSON schema:
- **Location**: `Application.persistentDataPath/savegame.json`.
- **Atomic File Writing**: Writes first to temporary file (`savegame.json.tmp`) and swaps atomically via `File.Replace` / `File.Move` to prevent corruption on crash or abrupt power loss.
- **Migration Pipeline**:
  - `ISaveMigration` chain upgrades schemas (e.g., Version 1 $\rightarrow$ Version 2).
  - Corrupt or unparseable files trigger graceful fallback to defaults with `.bak` preservation.
- **Data Model**:
  - Highest unlocked level.
  - Level stars and high scores.
  - User preferences (SFX volume, Music volume, Haptics toggle, Reduced Motion toggle, High-Contrast glyph toggle).
