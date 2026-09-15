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

        public void InitializeGameplay(uint seed = 424242)
        {
            var geometry = new BoardGeometry(evenWidth: 8, maxRows: 12, dangerRow: 11);
            var board = new HexBoard(geometry);
            var pressure = new PressureEngine(maxTime: 12.0f, missPenalty: 2.0f, missThreshold: 4);

            Engine = new AuthoritativeEngine(seed, board, pressure);

            // Populate starting Level 1 formation (3 rows of 4 colors)
            PopulateInitialBoard(Engine);

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
            GameLogger.LogInfo("GameplayController", "Level 1 initialized successfully.");
        }

        private void PopulateInitialBoard(AuthoritativeEngine engine)
        {
            // 3 starting rows: Row 0 (8 balls), Row 1 (7 balls), Row 2 (8 balls)
            for (int r = 0; r < 3; r++)
            {
                int cols = engine.Board.GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    var color = engine.Rng.NextColor(engine.ActiveColorCount);
                    engine.Board.SetBall(new HexCoord(r, c), BallInfo.CreateNormal(color));
                }
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
                _audio?.PlayMatchPop(shotResult.MatchedCoords.Count);
                _haptics?.TriggerMediumPulse();

                bool matchesDone = false;
                StartCoroutine(_boardView!.AnimateMatches(shotResult.MatchedCoords, () => matchesDone = true));
                while (!matchesDone) yield return null;
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
                _audio?.PlayRowDescent();
                _haptics?.TriggerHeavyPulse();

                bool descentDone = false;
                StartCoroutine(_boardView!.AnimateRowDescent(() => descentDone = true));
                while (!descentDone) yield return null;
            }

            // 6. Update HUD
            UpdateHUD();

            // Replenish next projectile in launcher
            BallInfo nextBall = BallInfo.CreateNormal(Engine.Rng.NextColor(Engine.ActiveColorCount));
            _launcher.SetNextProjectile(nextBall);

            // 7. Check game over
            if (shotResult.Status == GameResult.Victory)
            {
                _audio?.PlayVictory();
                _resultsScreen?.Show();
            }
            else if (shotResult.Status == GameResult.Defeat)
            {
                _audio?.PlayDefeat();
                _resultsScreen?.Show();
            }

            Engine.Pressure.IsPaused = false;
            IsResolvingAnimation = false;
        }

        private IEnumerator ExecuteRowDescentSequence()
        {
            IsResolvingAnimation = true;
            Engine.Pressure.IsPaused = true;

            _audio?.PlayRowDescent();
            _haptics?.TriggerHeavyPulse();

            bool descentDone = false;
            if (_boardView != null)
            {
                StartCoroutine(_boardView.AnimateRowDescent(() => descentDone = true));
                while (!descentDone) yield return null;
            }

            UpdateHUD();

            if (Engine.Status == GameResult.Defeat)
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

        private void UpdateHUD()
        {
            if (_hud == null || Engine == null) return;
            _hud.UpdateScore(Engine.Score, Engine.ComboMultiplier);
            _hud.UpdateMisses(Engine.Pressure.ConsecutiveMisses);
            _hud.UpdatePressure(Engine.Pressure.RemainingTime, Engine.Pressure.MaxTime);
        }
    }
}
