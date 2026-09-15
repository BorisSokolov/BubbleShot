using System;
using System.Collections;
using UnityEngine;
using BubbleShot.Core;
using BubbleShot.Runtime.Presentation;
using BubbleShot.Runtime.Audio;
using BubbleShot.Runtime.Haptics;
using BubbleShot.UI.HUD;
using BubbleShot.UI.Screens;

namespace BubbleShot.Runtime.Lifecycle
{
    /// <summary>
    /// Master coordinator for the gameplay loop in Gameplay.unity.
    /// Bridges the pure C# AuthoritativeEngine to presentation views, HUD, input, and lifecycle.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private BoardView? _boardView;
        [SerializeField] private LauncherController? _launcher;
        [SerializeField] private DangerLineView? _dangerLine;
        [SerializeField] private GameplayHUD? _hud;
        [SerializeField] private AudioFeedbackPlaceholder? _audio;
        [SerializeField] private HapticFeedbackPlaceholder? _haptics;
        [SerializeField] private ResultsScreenPlaceholder? _resultsScreen;
        [SerializeField] private FloatingCalloutManager? _callouts;

        public AuthoritativeEngine Engine { get; private set; } = null!;
        public bool IsResolvingAnimation { get; private set; }
        public bool IsGamePaused { get; private set; }

        private void Awake()
        {
            InitializeGameplay();
        }

        private void OnEnable()
        {
            if (_hud != null) _hud.PauseRequested += OnPauseRequested;
        }

        private void OnDisable()
        {
            if (_hud != null) _hud.PauseRequested -= OnPauseRequested;
        }

        public LevelDefinition CurrentLevel { get; private set; } = null!;
        public int RowsSurvivedCount { get; private set; }

        public void InitializeGameplay(int levelNumber = 0, uint seedOverride = 0)
        {
            int lvlNum = levelNumber > 0 ? levelNumber : LevelSelectScreen.SelectedLevelNumber;
            CurrentLevel = LevelCatalog.GetLevel(lvlNum);

            uint seed = seedOverride != 0 ? seedOverride : CurrentLevel.Seed;
            var geometry = new BoardGeometry(evenWidth: 8, maxRows: 12, dangerRow: 11);
            var board = new HexBoard(geometry);
            var pressure = new PressureEngine(
                maxTime: CurrentLevel.BasePressureTime,
                missPenalty: CurrentLevel.MissPenalty,
                missThreshold: 4);

            Engine = new AuthoritativeEngine(seed, board, pressure);
            Engine.ActiveColorCount = CurrentLevel.ActiveColorCount;
            RowsSurvivedCount = 0;

            // Populate starting formation from LevelDefinition
            PopulateBoardFromLevel(Engine, CurrentLevel);

            if (_boardView != null)
            {
                _boardView.BindBoard(board);
            }

            if (_dangerLine != null)
            {
                _dangerLine.Initialize(geometry);
            }

            // Seed launcher queue
            BallInfo ball1 = BallInfo.CreateNormal(Engine.Rng.NextColor(Engine.ActiveColorCount));
            BallInfo ball2 = BallInfo.CreateNormal(Engine.Rng.NextColor(Engine.ActiveColorCount));

            if (_launcher != null)
            {
                _launcher.Initialize(board, ball1, ball2);
            }

            UpdateHUD();
            GameLogger.LogInfo("GameplayController", $"Level {CurrentLevel.LevelNumber} ('{CurrentLevel.LevelName}') initialized successfully.");
        }

        private void PopulateBoardFromLevel(AuthoritativeEngine engine, LevelDefinition level)
        {
            for (int i = 0; i < level.StartingBalls.Count; i++)
            {
                var pair = level.StartingBalls[i];
                engine.Board.SetBall(pair.Coord, pair.Ball);
            }
        }

        private void Update()
        {
            if (IsGamePaused || Engine == null || Engine.Status != GameResult.Ongoing) return;

            // Tick real-time pressure timer if not currently animating
            if (!IsResolvingAnimation)
            {
                bool rowTriggered = Engine.Tick(Time.deltaTime);
                if (rowTriggered)
                {
                    StartCoroutine(ExecuteRowDescentSequence());
                }
            }

            // Update pressure bar in HUD
            _hud?.UpdatePressure(Engine.Pressure.RemainingTime, Engine.Pressure.MaxTime);
        }

        public void HandleAimInput(Vector2 screenPosition)
        {
            if (IsResolvingAnimation || IsGamePaused || _launcher == null) return;

            Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam == null) return;

            Vector3 worldTarget = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cam.transform.position.z));
            _launcher.UpdateAim(worldTarget);
        }

        public void HandleFireInput()
        {
            if (IsResolvingAnimation || IsGamePaused || _launcher == null || !_launcher.IsAiming) return;

            StartCoroutine(ExecuteShotSequence());
        }

        private IEnumerator ExecuteShotSequence()
        {
            IsResolvingAnimation = true;
            Engine.Pressure.IsPaused = true;

            var projectile = _launcher!.CurrentProjectile;
            var trajectory = TrajectorySolver.Solve(
                new Vector2D(_launcher.transform.localPosition.x, _launcher.transform.localPosition.y),
                _launcher.AimDirection,
                Engine.Board);

            _audio?.PlayLaunch();

            // 1. Play in-flight projectile animation
            bool flightDone = false;
            StartCoroutine(_launcher.LaunchProjectile(trajectory, projectile, _ => flightDone = true));

            while (!flightDone) yield return null;

            _audio?.PlayAttach();

            // 2. Authoritative engine resolves attachment, matches, clusters, and pressure
            var shotResult = Engine.ExecuteShot(
                new Vector2D(_launcher.transform.localPosition.x, _launcher.transform.localPosition.y),
                _launcher.AimDirection,
                projectile);

            // Spawn attached ball view
            _boardView?.SpawnBallView(shotResult.AttachedCoord, projectile);

            // 3. Animate matches if any
            if (shotResult.MatchedCoords.Count > 0)
            {
                if (shotResult.IsBombDetonation)
                {
                    _audio?.PlayBombExplosion(shotResult.MatchedCoords.Count);
                    _haptics?.TriggerHeavyPulse();

                    bool reducedMotion = SaveSystem.CurrentSave?.Settings.ReducedMotion ?? false;
                    if (!reducedMotion)
                    {
                        StartCoroutine(AnimateScreenShake(0.2f, 0.15f));
                    }
                }
                else if (projectile.Type == BallType.Wild)
                {
                    _audio?.PlayWildMatch(shotResult.MatchedCoords.Count);
                    _haptics?.TriggerMediumPulse();
                }
                else
                {
                    _audio?.PlayMatchPop(shotResult.MatchedCoords.Count);
                    _haptics?.TriggerMediumPulse();
                }

                bool matchesDone = false;
                StartCoroutine(_boardView!.AnimateMatches(shotResult.MatchedCoords, () => matchesDone = true));
                while (!matchesDone) yield return null;

                // Trigger floating combo callout
                if (shotResult.ComboMultiplier >= 1.5f && _callouts != null)
                {
                    string callout = FloatingCalloutManager.GetComboCalloutText(shotResult.ComboMultiplier);
                    if (!string.IsNullOrEmpty(callout))
                    {
                        Vector3 pos = _boardView != null ? _boardView.transform.position + new Vector3(0f, -2f, 0f) : Vector3.zero;
                        _callouts.ShowCallout(pos, callout, new Color(1f, 0.85f, 0.2f));
                    }
                }
            }

            // 4. Animate detached falling clusters if any
            if (shotResult.DetachedCoords.Count > 0)
            {
                _audio?.PlayClusterDrop(shotResult.DetachedCoords.Count);

                bool clustersDone = false;
                StartCoroutine(_boardView!.AnimateDetachedClusters(shotResult.DetachedCoords, () => clustersDone = true));
                while (!clustersDone) yield return null;
            }

            // 5. If forced row drop was triggered by misses
            if (shotResult.RowDropped)
            {
                RowsSurvivedCount++;
                _audio?.PlayRowDescent();
                _haptics?.TriggerHeavyPulse();

                bool descentDone = false;
                StartCoroutine(_boardView!.AnimateRowDescent(() => descentDone = true));
                while (!descentDone) yield return null;
            }

            // 6. Update HUD
            UpdateHUD();

            // Replenish next projectile in launcher with combo special ball rewards
            BallInfo nextBall = Engine.GenerateNextProjectile();
            _launcher.SetNextProjectile(nextBall);

            // 7. Check game over and level objectives
            EvaluateLevelObjective();

            if (Engine.Status == GameResult.Victory)
            {
                OnLevelVictory();
            }
            else if (Engine.Status == GameResult.Defeat)
            {
                _audio?.PlayDefeat();
                _resultsScreen?.Show();
            }

            Engine.Pressure.IsPaused = false;
            IsResolvingAnimation = false;
        }

        private void EvaluateLevelObjective()
        {
            if (Engine.Status == GameResult.Defeat) return;

            bool won = false;
            switch (CurrentLevel.ObjectiveType)
            {
                case LevelObjectiveType.ClearAll:
                    won = Engine.Board.GetOccupiedCoords().Count == 0;
                    break;
                case LevelObjectiveType.ClearAnchor:
                    // Check if Row 0 has 0 balls
                    int row0Cols = Engine.Board.GetColumnCount(0);
                    bool anchorEmpty = true;
                    for (int c = 0; c < row0Cols; c++)
                    {
                        if (Engine.Board.IsOccupied(new HexCoord(0, c)))
                        {
                            anchorEmpty = false;
                            break;
                        }
                    }
                    won = anchorEmpty;
                    break;
                case LevelObjectiveType.TargetScore:
                    won = Engine.Score >= CurrentLevel.TargetScore;
                    break;
                case LevelObjectiveType.SurviveRows:
                    won = RowsSurvivedCount >= CurrentLevel.TargetRowsToSurvive;
                    break;
            }

            if (won)
            {
                // Trigger victory!
                typeof(AuthoritativeEngine).GetProperty("Status")?.SetValue(Engine, GameResult.Victory);
            }
        }

        private void OnLevelVictory()
        {
            int stars = CurrentLevel.CalculateStars(Engine.Score);
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            var save = SaveSystem.LoadSafe(savePath);
            save.RecordLevelComplete(CurrentLevel.LevelNumber, Engine.Score, stars);
            SaveSystem.SaveAtomic(savePath, save);

            _audio?.PlayVictory();
            _resultsScreen?.Show();
            GameLogger.LogInfo("GameplayController", $"Level {CurrentLevel.LevelNumber} Won! Score={Engine.Score}, Stars={stars}");
        }

        private IEnumerator ExecuteRowDescentSequence()
        {
            IsResolvingAnimation = true;
            Engine.Pressure.IsPaused = true;
            RowsSurvivedCount++;

            _audio?.PlayRowDescent();
            _haptics?.TriggerHeavyPulse();

            bool descentDone = false;
            if (_boardView != null)
            {
                StartCoroutine(_boardView.AnimateRowDescent(() => descentDone = true));
                while (!descentDone) yield return null;
            }

            UpdateHUD();
            EvaluateLevelObjective();

            if (Engine.Status == GameResult.Victory)
            {
                OnLevelVictory();
            }
            else if (Engine.Status == GameResult.Defeat)
            {
                _audio?.PlayDefeat();
                _resultsScreen?.Show();
            }

            Engine.Pressure.IsPaused = false;
            IsResolvingAnimation = false;
        }

        public void SetPause(bool pause)
        {
            IsGamePaused = pause;
            if (Engine != null)
            {
                Engine.Pressure.IsPaused = pause;
            }
            GameLogger.LogInfo("GameplayController", $"Pause state set to: {pause}");
        }

        private void OnPauseRequested()
        {
            SetPause(true);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SetPause(true);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SetPause(true);
            }
        }

        private IEnumerator AnimateScreenShake(float duration, float intensity)
        {
            Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam == null) yield break;

            Vector3 originalPos = cam.transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                float x = (UnityEngine.Random.value * 2f - 1f) * intensity * damper;
                float y = (UnityEngine.Random.value * 2f - 1f) * intensity * damper;
                cam.transform.position = originalPos + new Vector3(x, y, 0f);
                yield return null;
            }

            cam.transform.position = originalPos;
        }

        private void UpdateHUD()
        {
            if (Engine == null) return;
            if (_hud != null)
            {
                _hud.UpdateScore(Engine.Score, Engine.ComboMultiplier);
                _hud.UpdateMisses(Engine.Pressure.ConsecutiveMisses);
                _hud.UpdatePressure(Engine.Pressure.RemainingTime, Engine.Pressure.MaxTime);
            }
            UpdateDangerState();
        }

        private void UpdateDangerState()
        {
            if (_dangerLine == null || Engine == null) return;
            bool inDanger = false;
            var occupied = Engine.Board.GetOccupiedCoords();
            for (int i = 0; i < occupied.Count; i++)
            {
                if (occupied[i].Row >= 9)
                {
                    inDanger = true;
                    break;
                }
            }
            _dangerLine.SetWarningPulsing(inDanger);
        }
    }
}
