# BubbleShot - Game Design Document

## 1. Executive Summary & Product Vision

**BubbleShot** is a premium, offline, single-player mobile action-puzzle game developed in Unity 6 LTS. It combines the tactile spatial aiming of classic bubble shooters with an active **hybrid pressure model**, where board descent is governed by both continuous real-time pressure and shot efficiency.

The game is strictly offline, deterministic, self-contained, and devoid of predatory monetization, forced ads, external analytics, or online account requirements.

---

## 2. Core Gameplay Mechanics

### 2.1 The Board
- **Geometry**: Staggered hexagonal grid (odd-r horizontal offset).
- **Dimensions**:
  - Width: 8 columns on even rows ($r \in \{0, 2, 4, \dots\}$), 7 columns on odd rows ($r \in \{1, 3, 5, \dots\}$).
  - Visible Board Height: 12 rows.
  - Danger Line: Located between Row 10 and Row 11. Any ball coming to rest at Row 11 or below triggers a loss condition.
- **Anchor**: Row 0 is the ceiling anchor. Balls are physically anchored if there is an unbroken chain of adjacent balls connecting them to Row 0.

### 2.2 Ball Palette & Special Balls
1. **Normal Balls**:
   - Palette: Up to 6 distinct colors:
     - Red (`#FF3B30`)
     - Blue (`#007AFF`)
     - Green (`#34C759`)
     - Yellow (`#FFCC00`)
     - Purple (`#AF52DE`)
     - Orange (`#FF9500`)
   - Every color includes unique interior geometric glyphs/symbols for complete color-blind accessibility (Shape + Color identification).

2. **Bomb Ball**:
   - Destroys all balls within hex distance $\le 1$ (the hit cell plus all 6 immediate neighbors, up to 7 balls total).
   - Detonation is deterministic and triggers immediately upon attachment.
   - Cleared balls count toward score and pressure relief.

3. **Wild Ball**:
   - Acts as a universal wildcard matching any normal color.
   - **Deterministic Resolution**:
     - Evaluates all distinct colors among its immediate hex neighbors.
     - For each color $C$, it calculates the size of the connected group of color $C$ that includes the Wild Ball.
     - If one or more colors achieve a match size $\ge 3$, **all** such qualifying groups are popped simultaneously in a single multi-color cascade.
     - If no neighbor color achieves a match size $\ge 3$, the Wild Ball remains placed on the board, retaining its wild property for subsequent shots.

### 2.3 Aiming, Reflection & Launch
- **Aiming**: Touch/pointer drag or direct pointing from the launcher pivot (bottom center of screen).
- **Clamp**: Aim angle restricted to $[10^\circ, 170^\circ]$ (measured from the horizontal axis) to prevent backward or horizontal-loop shots.
- **Wall Reflections**:
  - The board has rigid left and right boundaries ($x = x_{\text{min}}$ and $x = x_{\text{max}}$).
  - Trajectory reflects horizontally ($v_x' = -v_x$) upon hitting a wall boundary.
  - Supports up to 2 wall bounces. Trajectory preview accurately displays up to 2 segments.
- **Flight & Attachment**:
  - Authoritative trajectory calculation computes the earliest sweep-circle collision against occupied cells or the ceiling (Row 0).
  - Upon collision at time $t$, the projectile snaps into the nearest legal, empty hex cell.
  - **Deterministic Tie-Breaking**: If the projectile center is equidistant to two or more legal neighbor cells:
    1. Prefer the cell with the smaller row index (higher up, closer to ceiling).
    2. If row indices are identical, prefer the cell with the smaller column index (left-most).

### 2.4 Matching & Detached Clusters
1. **Connected Component Search**: Breadth-First Search (BFS) explores all same-color neighbors from the newly placed cell.
2. **Pop Condition**: If the group size $N \ge 3$, all balls in the group are marked for popping.
3. **Cluster Fall (Floating Balls)**:
   - Run multi-source BFS from all balls currently in Row 0.
   - Any ball not visited during this traversal is disconnected from the ceiling anchor.
   - All disconnected balls immediately detach and drop as bonus score and pressure relief.

---

## 3. Hybrid Pressure Model

The board continuously works against the player through a combined time-and-shot pressure system.

### 3.1 Pressure Timer
- A visible progress meter (Pressure Bar) counts down from $T_{\text{max}}$ (default: 12.0 seconds, configurable by level).
- When the timer reaches $0.0$, **Row Descent** is triggered:
  - A new top row is inserted at Row 0 with seeded procedural colors.
  - All existing rows shift down by 1 ($r \leftarrow r + 1$).
  - The parity of row staggering alternates cleanly.
  - Timer resets to $T_{\text{max}}$.

### 3.2 Shot Modifiers (Misses and Matches)
- **Unsuccessful Shot (Miss)**: A shot that does not result in popping $\ge 3$ balls.
  - Each miss penalizes the player by immediately reducing the remaining pressure timer by $\Delta T_{\text{miss}} = 2.0\text{s}$.
  - In addition, a miss counter increments. Every 4 consecutive misses triggers an instant row drop regardless of timer.
- **Successful Match Relief**:
  - Popping a match group of size $N \ge 3$ restores time to the pressure bar:
    $$\Delta T_{\text{relief}} = 1.0\text{s} + (N - 3) \times 0.5\text{s}$$
  - Detached balls falling restore additional time: $+0.4\text{s}$ per detached ball.
  - Total time is capped at $T_{\text{max}}$.
  - The consecutive miss counter resets to 0.

### 3.3 Lifecycle & Animations Pause
- The pressure timer strictly freezes when:
  - Game is paused.
  - Application is backgrounded/minimized.
  - Active blocking resolution animations are playing (pop cascades, row drop animations, level transition).
  - No inputs are processed and no hidden timer ticks occur while animations resolve.

---

## 4. Scoring, Combos & Objectives

### 4.1 Scoring Formula
- **Matched Balls**: $100 \text{ pts per ball}$.
- **Size Multiplier**: For matches larger than 3:
  $$\text{Score}_{\text{match}} = N \times 100 \times (1 + (N - 3) \times 0.2)$$
- **Detached Balls**: High-value exponential reward:
  $$\text{Score}_{\text{detached}} = M \times 200 \times 2^{\min(M-1, 4)}$$
  where $M$ is the number of falling balls.
- **Combo Multiplier**: Consecutive successful shots increment the Combo counter ($1\times, 1.5\times, 2\times, 2.5\times, \dots$ up to $4\times$). Any miss resets the combo counter to $1\times$.

### 4.2 Level Objectives
1. **Clear All**: Remove all balls from the board.
2. **Clear Anchor**: Remove all balls directly attached to the ceiling row (Row 0), causing everything below to drop.
3. **Target Score**: Reach target score threshold before time or shot capacity runs out.
4. **Survive Rows**: Successfully clear or survive $K$ advancing rows without breaching the danger line.

### 4.3 Star Ratings
- 1 Star: Complete the primary objective.
- 2 Stars: Complete objective with Score $\ge \text{Threshold}_2$.
- 3 Stars: Complete objective with Score $\ge \text{Threshold}_3$ and 0 Danger alerts.

---

## 5. Difficulty & Progression System

Progression across levels introduces challenge via structured, non-arbitrary parameters:
1. **Color Count**: Starts with 3 colors (Levels 1–3), expands to 4 (Levels 4–7), 5 (Levels 8–12), and 6 (Levels 13+).
2. **Pressure Velocity**: $T_{\text{max}}$ scales from 15.0s down to 8.0s on master stages.
3. **Starting Formations**: Pre-authored hex layouts with color clusters, pockets, and strategic obstacles.
4. **Miss Tolerance**: Strictness of consecutive miss allowances.
5. **Special Ball Availability**: Pre-loaded queue or earned via high combos ($\ge 3$ consecutive matches).

---

## 6. Accessibility & Controls

1. **Color Accessibility**: High-contrast color palette paired with distinct geometric runes inside each ball (Circle, Square, Diamond, Triangle, Star, Cross).
2. **Aim Line Visuals**: High-contrast dashed line with end-reticle showing exact landing cell highlight.
3. **Motion Settings**: Reduced-motion toggle disables screen shakes, reduces particle velocity, and switches to gentle fades.
4. **Audio & Haptics**: Independent volume sliders for Music and SFX, dedicated haptic feedback toggle.
5. **Safe Area & Aspect Ratios**: Fully responsive layout adapting to aspect ratios from 16:9 to 21:9 with safe-area notch and home-indicator padding.
