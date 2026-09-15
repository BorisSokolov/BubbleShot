using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// Controls aiming, trajectory preview, and flight interpolation of launched projectiles.
    /// </summary>
    public class LauncherController : MonoBehaviour
    {
        public const float MinAimAngleDegrees = 10.0f;
        public const float MaxAimAngleDegrees = 170.0f;
        public const float ProjectileFlightSpeed = 22.0f;

        [SerializeField] private TrajectoryPreview? _trajectoryPreview;
        [SerializeField] private Transform? _boardRoot;
        [SerializeField] private Transform? _loadedBallPivot;
        [SerializeField] private Transform? _nextBallPivot;
        [SerializeField] private GameObject? _ballPrefab;

        public BallInfo CurrentProjectile { get; private set; }
        public BallInfo NextProjectile { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsFlightActive { get; private set; }
        public Vector2D AimDirection { get; private set; } = Vector2D.Up;

        private HexBoard? _board;
        private BallView? _loadedView;
        private BallView? _nextView;

        public void Initialize(HexBoard board, BallInfo initialLoaded, BallInfo initialNext)
        {
            _board = board;
            CurrentProjectile = initialLoaded;
            NextProjectile = initialNext;

            // Launcher sits at bottom center of the board
            float launcherX = board.Geometry.BoardRight * 0.5f;
            float launcherY = -(board.Geometry.DangerRow + 0.8f) * board.Geometry.RowHeight;
            transform.localPosition = new Vector3(launcherX, launcherY, 0f);

            UpdateLoadedVisuals();
        }

        public void UpdateAim(Vector2 aimTargetWorld)
        {
            if (IsFlightActive || _board == null) return;

            Vector3 originWorld = transform.position;
            Vector2 delta = aimTargetWorld - (Vector2)originWorld;

            if (delta.magnitude < 0.2f) return;

            // Compute angle in degrees from positive X axis
            float angleDeg = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            // Restrict angle between 10 and 170 degrees (upward half-plane)
            float clampedAngle = Mathf.Clamp(angleDeg, MinAimAngleDegrees, MaxAimAngleDegrees);
            float rad = clampedAngle * Mathf.Deg2Rad;

            AimDirection = new Vector2D(MathF.Cos(rad), MathF.Sin(rad));
            IsAiming = true;

            // Compute and preview trajectory
            Vector3 boardLocalOrigin = _boardRoot != null ? _boardRoot.InverseTransformPoint(originWorld) : transform.localPosition;
            var trajectory = TrajectorySolver.Solve(new Vector2D(boardLocalOrigin.x, boardLocalOrigin.y), AimDirection, _board);

            if (_trajectoryPreview != null && _boardRoot != null)
            {
                _trajectoryPreview.DrawTrajectory(trajectory.PathPoints, trajectory.ImpactPoint, _boardRoot);
            }
        }

        public void CancelAim()
        {
            IsAiming = false;
            if (_trajectoryPreview != null)
            {
                _trajectoryPreview.SetVisible(false);
            }
        }

        public IEnumerator LaunchProjectile(TrajectoryResult trajectory, BallInfo projectile, Action<ShotExecutionResult> onFlightComplete)
        {
            IsFlightActive = true;
            CancelAim();

            // Instantiate in-flight visual ball
            Vector3 startPos = transform.position;
            GameObject flightGo;
            if (_ballPrefab != null)
            {
                flightGo = Instantiate(_ballPrefab, startPos, Quaternion.identity);
            }
            else
            {
                flightGo = new GameObject("InFlightBall");
                flightGo.transform.position = startPos;
                flightGo.AddComponent<SpriteRenderer>();
            }

            var flightView = flightGo.GetComponent<BallView>() ?? flightGo.AddComponent<BallView>();
            flightView.Initialize(trajectory.SnapCoord, projectile);

            // Interpolate along trajectory path points
            var points = trajectory.PathPoints;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 pA = _boardRoot != null ? _boardRoot.TransformPoint(new Vector3(points[i].X, points[i].Y, 0f)) : new Vector3(points[i].X, points[i].Y, 0f);
                Vector3 pB = _boardRoot != null ? _boardRoot.TransformPoint(new Vector3(points[i + 1].X, points[i + 1].Y, 0f)) : new Vector3(points[i + 1].X, points[i + 1].Y, 0f);

                float segmentDistance = Vector3.Distance(pA, pB);
                float segmentDuration = segmentDistance / ProjectileFlightSpeed;
                float elapsed = 0f;

                while (elapsed < segmentDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / segmentDuration);
                    flightGo.transform.position = Vector3.Lerp(pA, pB, t);
                    yield return null;
                }

                flightGo.transform.position = pB;
            }

            // Destroy flight projectile proxy
            Destroy(flightGo);
            IsFlightActive = false;

            // Load next projectile into slot
            CurrentProjectile = NextProjectile;
            UpdateLoadedVisuals();

            var result = new ShotExecutionResult(trajectory, trajectory.SnapCoord);
            onFlightComplete?.Invoke(result);
        }

        public void SetNextProjectile(BallInfo nextBall)
        {
            NextProjectile = nextBall;
            UpdateLoadedVisuals();
        }

        private void UpdateLoadedVisuals()
        {
            if (_loadedBallPivot != null && _loadedView == null && _ballPrefab != null)
            {
                var go = Instantiate(_ballPrefab, _loadedBallPivot.position, Quaternion.identity, _loadedBallPivot);
                _loadedView = go.GetComponent<BallView>() ?? go.AddComponent<BallView>();
            }
            _loadedView?.Initialize(new HexCoord(-1, -1), CurrentProjectile);

            if (_nextBallPivot != null && _nextView == null && _ballPrefab != null)
            {
                var go = Instantiate(_ballPrefab, _nextBallPivot.position, Quaternion.identity, _nextBallPivot);
                _nextView = go.GetComponent<BallView>() ?? go.AddComponent<BallView>();
            }
            _nextView?.Initialize(new HexCoord(-2, -2), NextProjectile);
        }
    }
}
