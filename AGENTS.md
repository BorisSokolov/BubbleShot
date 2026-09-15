# BubbleShot - Multi-Agent Operating Protocols & Governance

## 1. Agent & Model Allocation Rules

To maintain high architectural standards, strict code quality, and unbiased verification, tasks are strictly partitioned between agent roles:

| Role | Assigned Model | Responsibility & Constraints |
|------|----------------|------------------------------|
| **Initial Product Architect** | Claude Opus (substitute Gemini 3.8 Flash High if unavailable) | High-level game design, core architecture, decision records, and roadmap specification. Must not perform routine implementation. |
| **Implementation Orchestrator** | Gemini 3.8 Flash High | Feature implementation, refactoring, test authorship, bug fixes, and documentation updates. |
| **Independent Code Reviewer** | Gemini 3.8 Flash High (Fresh Context) | Autonomous code review of complete PR diffs, validation logs, and invariant checks. **Must be a fresh context that did not write the code.** |
| **Verification & CI Runner** | Gemini 3.8 Flash High | Automated compilation, unit testing, PlayMode testing, linting, and environment diagnostics. |

*Substitution Note*: Claude Opus capacity was unavailable on the server at project initialization (HTTP 503 errors); the initial design phase was executed by Gemini 3.8 Flash High as stipulated in repository instructions.

---

## 2. Autonomous Phase Lifecycle Protocol

Each phase must strictly execute the following sequential workflow without requesting routine human intervention:

```
[Main Branch]
      |
      v
1. Create Phase Branch (`phase/XX-...`)
      |
2. Read Governing Docs (GAME_DESIGN.md, ARCHITECTURE.md, Phase Spec)
      |
3. Implement Scoped Phase Features
      |
4. Run Mandatory Automated Validations (Build, Tests, Static Checks)
      |
5. Commit & Push Phase Branch
      |
6. Open Pull Request targeting `main`
      |
7. Spawn Fresh Independent Reviewer Context
      |
8. Reviewer Evaluates Diff, Tests & Validation Evidence
      |
9. Address Findings:
   - If BLOCKING / IMPORTANT: Apply fixes, re-validate, re-request review.
   - If APPROVED: Proceed to merge.
      |
10. Squash Merge PR into `main`
      |
11. Verify `main` post-merge
      |
12. Delete Remote Phase Branch
      |
13. Update AUTONOMOUS_EXECUTION.md
      |
14. Automatically advance to Next Phase
```

---

## 3. Independent Review Protocol & Severity Taxonomy

The independent reviewer evaluates pull requests across 15 mandatory categories:
1. **Correctness**: Logical bugs, race conditions, edge cases.
2. **Scope Compliance**: Strict adherence to the current phase definition of done.
3. **Gameplay Rules**: Conformance to `GAME_DESIGN.md` equations and constants.
4. **Determinism**: Elimination of floating-point drift, unseeded RNG, or frame-rate dependencies.
5. **Architectural Boundaries**: No `UnityEngine` leaks into `BubbleShot.Core`.
6. **Test Quality & Coverage**: Meaningful assertions, edge case coverage, no mocking of business logic.
7. **Unity Serialization Safety**: Proper `.meta` files, stable GUIDs, no broken script references.
8. **Missing References & Metadata**: All new assets have paired `.meta` files committed.
9. **Mobile Lifecycle Safety**: Safe pause, resume, backgrounding, and memory handling.
10. **Performance**: Zero per-frame allocations in core loop, appropriate object reuse.
11. **Accessibility**: Color-blind glyphs, high contrast, reduced-motion compliance.
12. **Security & Persistence**: Safe atomic file writes, corrupt save recovery, no unsanitized IO.
13. **Maintainability**: Clean naming, explicit interfaces, no redundant boilerplate.
14. **Unnecessary Complexity**: Elimination of premature abstractions or dead code.
15. **Documentation Accuracy**: Up-to-date docstrings and markdown records.

### Finding Classifications:
- `BLOCKING`: A defect that breaks compilation, compromises determinism, violates architectural separation, causes data loss, or fails automated tests. **Must be resolved before merge.**
- `IMPORTANT`: Suboptimal implementation, missing edge-case test, or minor lifecycle risk. **Must be resolved unless explicitly justified and documented.**
- `SUGGESTION`: Non-critical polish, aesthetic enhancement, or future optimization. May be deferred to `docs/FUTURE_BACKLOG.md`.

### Verdicts:
- `APPROVED`: Ready to merge immediately.
- `CHANGES REQUIRED`: Must address blocking/important findings and re-verify.
- `STOPPED`: Irrecoverable environmental or architectural blocker.

---

## 4. Git & Branching Policy
- **Branch Naming**:
  - `design/game-and-architecture`
  - `phase/01-unity-foundation`
  - `phase/02-deterministic-engine`
  - `phase/03-first-playable-loop`
  - `phase/04-progression-persistence`
  - `phase/05-special-balls`
  - `phase/06-polish`
  - `phase/07-release-readiness`
- **Commit Format**: Conventional commits (`feat(core):`, `test(engine):`, `fix(review):`, `docs(phase):`).
- **Merge Strategy**: Squash and merge into `main`.
