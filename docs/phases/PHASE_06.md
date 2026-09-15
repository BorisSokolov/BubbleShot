# Phase 6: Visual, Audio, Haptic, and UX Polish

## 1. Objective
Elevate the game feel, aesthetic cohesion, tactile feedback, audio atmosphere, and accessibility to commercial standards while ensuring rock-solid performance on mobile devices.

---

## 2. Scope
- **Visual Polish**:
  - Polished ball rendering with soft specular highlights and subtle depth shadows.
  - Projectile aim guide with animated dashed flow and distinct impact reticle.
  - Particle effects for matches, bomb blasts, and detached ball drops.
  - Dynamic danger feedback: rhythmic red warning pulse when any ball reaches Row 9 or lower.
  - Combo callouts ("Great!", "Super!", "Unstoppable!") with floating text.
- **Audio Architecture**:
  - `AudioManager`: Synthesized / licensed sound effects for aim clicks, wall bounces, ball snaps, melodic ascending pitch match pops, and bomb blasts.
  - Ambient background music loop with smooth transitions between menus and gameplay.
  - Dedicated Volume controls with real-time muting.
- **Haptic Feedback**:
  - `HapticService`: Distinct mobile vibration patterns for light taps (aiming), medium pulses (wall bounce, match pop), and heavy impacts (row descent, bomb burst).
  - Can be toggled on/off in settings.
- **Accessibility Suite**:
  - High-contrast geometric runes inside all balls for complete color-blind support.
  - Reduced-Motion toggle: replaces screen shakes and violent particle bursts with gentle fades.
- **Mobile Responsive Layout**:
  - Full safe-area support adapting to notches, punch-holes, and rounded corners on iOS and Android devices.
  - Dynamic canvas scaling across aspect ratios from 16:9 to 21:9.
- **Performance Optimization**:
  - Object pooling for balls, particle effects, and floating score labels to eliminate runtime garbage collection allocations.

---

## 3. Explicit Non-Goals
- Unlicensed or copyrighted art/music assets.
- Heavy post-processing shaders that impact mobile battery life.
- Social sharing or online leaderboards.

---

## 4. Definition of Done
1. Game looks vibrant, premium, and visually polished without visual clutter.
2. Gameplay remains completely legible during intense particle effects.
3. Audio and haptics provide crisp, satisfying feedback and can be individually disabled.
4. Reduced-motion mode functions cleanly across all effects.
5. Zero GC allocations during active gameplay shooting loop.
6. Independent review approves without blocking findings.
