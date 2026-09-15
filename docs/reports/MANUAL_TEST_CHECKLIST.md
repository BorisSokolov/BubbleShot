# BubbleShot Manual QA Test & Verification Checklist

**Project**: BubbleShot (Unity 6 LTS)  
**Version**: 1.0.0 Release Candidate  
**Target Platforms**: Android (ARM64/ARMv7), iOS, Standalone Desktop (Windows/macOS)  
**Document Purpose**: Step-by-step test plan for human QA testers, device validation, and release sign-off.

---

## 1. Installation, Launch & Safe Area

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 1.1 | **Clean Installation** | App installs cleanly without external permission requests or network warnings. | [ ] |
| 1.2 | **Bootstrap Scene Transition** | App opens with splash/bootstrap and transitions seamlessly to `MainMenu` within 1.5 seconds. | [ ] |
| 1.3 | **Safe Area Fitting** | On notched devices (iPhone 14/15, Samsung Galaxy), UI elements, headers, and launcher stay within `Screen.safeArea` boundaries without being obscured by cutouts. | [ ] |
| 1.4 | **Display Refresh Rate** | Game runs smoothly at 60 FPS (or 120 FPS on high-refresh rate displays) without micro-stutter. | [ ] |

---

## 2. Onboarding & Contextual Tutorials

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 2.1 | **Level 1 First Launch** | Dialog titled "Welcome to BubbleShot!" appears with basic aiming instructions. Game physics and input are paused while dialog is visible. | [ ] |
| 2.2 | **Tutorial Dismissal** | Tapping "Got it!" closes dialog, unpauses game, and enables launcher input. | [ ] |
| 2.3 | **Tutorial Persistence** | Restarting Level 1 or restarting the app does NOT show the Level 1 tutorial again (`SeenTutorials` persists). | [ ] |
| 2.4 | **Level 2 Wall Bounce Intro** | Dialog titled "Wall Bounces" appears upon starting Level 2, explaining bank shots. | [ ] |
| 2.5 | **Level 4 Special Balls Intro** | Dialog titled "Special Balls: Bomb & Wild" appears upon starting Level 4, detailing Bomb blast and Wild multi-color match mechanics. | [ ] |

---

## 3. Aiming & Launcher Controls

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 3.1 | **Touch Drag-to-Aim** | Dragging finger across the lower half of the screen adjusts aiming reticle smoothly with 0 input lag. | [ ] |
| 3.2 | **Trajectory Preview** | Animated dashed preview line projects straight trajectory and up to 2 wall bounces accurately reflecting off left and right walls. | [ ] |
| 3.3 | **Angle Clamping** | Aim angle cannot be dragged below horizontal ($< 15^\circ$ or $> 165^\circ$). Projectile cannot be shot downward. | [ ] |
| 3.4 | **Shot Cancellation** | Dragging finger back down toward launcher base cancels the shot; releasing finger does not fire projectile. | [ ] |
| 3.5 | **Projectile Release** | Releasing finger launches projectile along the exact previewed path with smooth linear speed. | [ ] |

---

## 4. Deterministic Engine & Matching Mechanics

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 4.1 | **3+ Color Match** | Connecting 3 or more bubbles of matching color immediately pops them. Pop animation plays and floating score text rises. | [ ] |
| 4.2 | **Ascending Pentatonic Audio** | Popping consecutive matches plays procedural chimes with increasing pitch (Pentatonic scale: C, D, E, G, A, C'). | [ ] |
| 4.3 | **Cluster Detachment** | Popping a group that leaves unanchored bubbles below disconnects the floating cluster; detached bubbles drop and award +200 pts bonus. | [ ] |
| 4.4 | **Bomb Detonation** | Firing a Bomb projectile into a cluster blasts all bubbles within radius 1 (up to 6 neighbors). Detonation causes haptic feedback. | [ ] |
| 4.5 | **Wild Ball Match** | Firing a Wild projectile connects with all adjacent colors simultaneously, popping any color group having $\ge 2$ adjacent members. | [ ] |
| 4.6 | **Combo Milestones** | Achieving 3 consecutive matches generates a Bomb or Wild projectile for the next shot. | [ ] |

---

## 5. Pressure Mechanics & Danger Threshold

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 5.1 | **Shot Counter Pressure** | Firing shots that result in misses decrements the miss threshold; after 4 misses, an authoritative row descent triggers. | [ ] |
| 5.2 | **Timer Pressure** | On timed levels, leaving launcher idle causes pressure bar to fill; upon timer expiration, a row descent triggers. | [ ] |
| 5.3 | **Row Descent Shift** | New row appears at Row 0, existing rows shift down by 1 row with alternating hexagonal parity. | [ ] |
| 5.4 | **Danger Line Warning** | When any bubble reaches Row 9 or below, the danger line pulses red with an escalating rhythmic sound. | [ ] |
| 5.5 | **Game Over Defeat** | When any bubble crosses Row 11 (the danger line), game transitions immediately to Defeat screen. | [ ] |

---

## 6. Progression, Catalog & Persistence

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 6.1 | **Victory Screen & Stars** | Clearing the board triggers Victory screen, displaying score, stars earned (1–3), and "Next Level" button. | [ ] |
| 6.2 | **Progression Unlocking** | Completing Level $N$ unlocks Level $N+1$ in the Level Catalog. Future levels remain locked. | [ ] |
| 6.3 | **High Score Persistence** | Beating a previous score updates the record; lower scores do not overwrite the record. | [ ] |
| 6.4 | **Force Quit & Relaunch** | Force quitting the app during gameplay or menu, then relaunching, retains all unlocked levels, high scores, and settings. | [ ] |
| 6.5 | **Settings Toggle** | Modifying SFX volume, Music volume, or Haptics immediately applies and persists across app restarts. | [ ] |
| 6.6 | **Colorblind Accessibility** | Enabling Rune/Symbol mode displays distinct geometric runes on bubbles for high-contrast accessibility. | [ ] |

---

## 7. Platform Lifecycle & Stress

| # | Test Scenario | Expected Outcome | Pass/Fail |
|---|---|---|---|
| 7.1 | **Rapid App Switching** | Switching to home screen / other apps and returning immediately resumes gameplay without state desynchronization. | [ ] |
| 7.2 | **Screen Orientation Lock** | App remains firmly locked in Portrait orientation across all device orientations. | [ ] |
| 7.3 | **Low Battery / Notification Banner** | Receiving incoming calls or system alerts pauses game cleanly. | [ ] |
| 7.4 | **Memory Stability** | Continuous play across 10 levels exhibits flat memory consumption (zero garbage collection hitching). | [ ] |

---

## 8. QA Sign-Off

- **Lead QA Engineer**: ___________________________
- **Date Tested**: ___________________________
- **Overall Verdict**: [ ] ACCEPTED FOR RELEASE / [ ] REJECTED
