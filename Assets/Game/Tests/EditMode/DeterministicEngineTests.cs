using System;
using System.Collections.Generic;
using NUnit.Framework;
using BubbleShot.Core;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class DeterministicEngineTests
    {
        private BoardGeometry _geometry = null!;
        private HexBoard _board = null!;

        [SetUp]
        public void Setup()
        {
            _geometry = new BoardGeometry(evenWidth: 8, maxRows: 12, dangerRow: 11, ballDiameter: 1.0f);
            _board = new HexBoard(_geometry);
        }

        #region 1. Hex Coordinates & Neighbor Parities

        [Test]
        public void HexCoord_Neighbors_EvenRow_AreCorrect()
        {
            // Row 2 is even row
            var coord = new HexCoord(2, 3);
            var raw = coord.GetRawNeighbors();

            Assert.That(raw[0], Is.EqualTo(new HexCoord(1, 2))); // Top-Left
            Assert.That(raw[1], Is.EqualTo(new HexCoord(1, 3))); // Top-Right
            Assert.That(raw[2], Is.EqualTo(new HexCoord(2, 2))); // Left
            Assert.That(raw[3], Is.EqualTo(new HexCoord(2, 4))); // Right
            Assert.That(raw[4], Is.EqualTo(new HexCoord(3, 2))); // Bottom-Left
            Assert.That(raw[5], Is.EqualTo(new HexCoord(3, 3))); // Bottom-Right
        }

        [Test]
        public void HexCoord_Neighbors_OddRow_AreCorrect()
        {
            // Row 1 is odd row
            var coord = new HexCoord(1, 3);
            var raw = coord.GetRawNeighbors();

            Assert.That(raw[0], Is.EqualTo(new HexCoord(0, 3))); // Top-Left
            Assert.That(raw[1], Is.EqualTo(new HexCoord(0, 4))); // Top-Right
            Assert.That(raw[2], Is.EqualTo(new HexCoord(1, 2))); // Left
            Assert.That(raw[3], Is.EqualTo(new HexCoord(1, 4))); // Right
            Assert.That(raw[4], Is.EqualTo(new HexCoord(2, 3))); // Bottom-Left
            Assert.That(raw[5], Is.EqualTo(new HexCoord(2, 4))); // Bottom-Right
        }

        [Test]
        public void HexCoord_BoundaryCells_FilterInvalidNeighbors()
        {
            // Top-left corner (0, 0)
            var topLeft = new HexCoord(0, 0);
            var validNeighbors = _board.GetNeighbors(topLeft);

            // Cannot have row -1 or col -1
            foreach (var n in validNeighbors)
            {
                Assert.That(n.Row, Is.GreaterThanOrEqualTo(0));
                Assert.That(n.Col, Is.GreaterThanOrEqualTo(0));
            }

            Assert.That(validNeighbors, Contains.Item(new HexCoord(0, 1))); // Right
            Assert.That(validNeighbors, Contains.Item(new HexCoord(1, 0))); // Bottom-Right
        }

        #endregion

        #region 2. Coordinate Transformations

        [Test]
        public void CoordinateConversion_MatchesGeometryEquations()
        {
            // Even row (0, 0) -> x = BallRadius (0.5), y = 0
            var pos00 = _board.CoordToLocalPosition(new HexCoord(0, 0));
            Assert.That(pos00.X, Is.EqualTo(0.5f).Within(Vector2D.Epsilon));
            Assert.That(pos00.Y, Is.EqualTo(0.0f).Within(Vector2D.Epsilon));

            // Odd row (1, 0) -> x = 1.0 * BallDiameter, y = -RowHeight
            var pos10 = _board.CoordToLocalPosition(new HexCoord(1, 0));
            Assert.That(pos10.X, Is.EqualTo(1.0f).Within(Vector2D.Epsilon));
            Assert.That(pos10.Y, Is.EqualTo(-_geometry.RowHeight).Within(Vector2D.Epsilon));

            // Closest coord roundtrip
            var resolvedCoord = _geometry.FindClosestCoord(pos10);
            Assert.That(resolvedCoord, Is.EqualTo(new HexCoord(1, 0)));
        }

        #endregion

        #region 3. Trajectory, Reflections & Collision

        [Test]
        public void Trajectory_DirectCeilingShot_AttachesToCeiling()
        {
            // Launcher at bottom center: x = 4.0, y = -10.0
            var origin = new Vector2D(4.0f, -10.0f);
            var dir = Vector2D.Up;

            var result = TrajectorySolver.Solve(origin, dir, _board);

            Assert.That(result.HitType, Is.EqualTo(TrajectoryHitType.Ceiling));
            Assert.That(result.BounceCount, Is.EqualTo(0));
            Assert.That(result.SnapCoord.Row, Is.EqualTo(0));
        }

        [Test]
        public void Trajectory_SingleWallBank_ReflectsHorizontally()
        {
            // Aim angled toward right wall
            var origin = new Vector2D(4.0f, -8.0f);
            var dir = new Vector2D(1.0f, 1.0f).Normalized;

            var result = TrajectorySolver.Solve(origin, dir, _board);

            Assert.That(result.BounceCount, Is.GreaterThanOrEqualTo(1));
            // First segment hits right wall (x ~ 7.5), then reflects leftward
            Assert.That(result.PathPoints.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(result.PathPoints[1].X, Is.EqualTo(_geometry.BoardRight - _geometry.BallRadius).Within(1e-4f));
        }

        [Test]
        public void Trajectory_BallCollision_StopsAtBall()
        {
            // Place target ball at (0, 3)
            var targetCoord = new HexCoord(0, 3);
            _board.SetBall(targetCoord, BallInfo.CreateNormal(BallColor.Blue));

            var targetPos = _board.CoordToLocalPosition(targetCoord);
            var origin = new Vector2D(targetPos.X, targetPos.Y - 5.0f);
            var dir = Vector2D.Up;

            var result = TrajectorySolver.Solve(origin, dir, _board);

            Assert.That(result.HitType, Is.EqualTo(TrajectoryHitType.Ball));
            Assert.That(result.HitBallCoord, Is.EqualTo(targetCoord));
            Assert.That(result.SnapCoord, Is.Not.EqualTo(targetCoord));
        }

        [Test]
        public void Trajectory_DeterministicTieBreaking_PrefersHigherRowThenLeftCol()
        {
            // Place two balls horizontally adjacent on Row 1
            _board.SetBall(new HexCoord(1, 2), BallInfo.CreateNormal(BallColor.Red));
            _board.SetBall(new HexCoord(1, 3), BallInfo.CreateNormal(BallColor.Red));

            var pos1 = _board.CoordToLocalPosition(new HexCoord(1, 2));
            var pos2 = _board.CoordToLocalPosition(new HexCoord(1, 3));
            float midX = (pos1.X + pos2.X) * 0.5f;

            // Shoot directly between them from below
            var origin = new Vector2D(midX, -6.0f);
            var result = TrajectorySolver.Solve(origin, Vector2D.Up, _board);

            Assert.That(result.HitType, Is.EqualTo(TrajectoryHitType.Ball));
            Assert.That(result.SnapCoord.Row, Is.GreaterThanOrEqualTo(0));
        }

        #endregion

        #region 4. Match Resolution

        [Test]
        public void MatchResolver_BelowThreshold_DoesNotPop()
        {
            // 2 adjacent blue balls
            _board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Blue));
            _board.SetBall(new HexCoord(0, 1), BallInfo.CreateNormal(BallColor.Blue));

            var matched = MatchResolver.FindMatchingGroup(_board, new HexCoord(0, 0));
            Assert.That(matched.Count, Is.EqualTo(0));
        }

        [Test]
        public void MatchResolver_AtThreshold_PopsAllThree()
        {
            // 3 adjacent blue balls
            _board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Blue));
            _board.SetBall(new HexCoord(0, 1), BallInfo.CreateNormal(BallColor.Blue));
            _board.SetBall(new HexCoord(0, 2), BallInfo.CreateNormal(BallColor.Blue));

            var matched = MatchResolver.FindMatchingGroup(_board, new HexCoord(0, 0));
            Assert.That(matched.Count, Is.EqualTo(3));
            Assert.That(matched, Contains.Item(new HexCoord(0, 0)));
            Assert.That(matched, Contains.Item(new HexCoord(0, 1)));
            Assert.That(matched, Contains.Item(new HexCoord(0, 2)));
        }

        [Test]
        public void MatchResolver_AboveThreshold_PopsLargeCluster()
        {
            // 5 green balls in a cluster
            _board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Green));
            _board.SetBall(new HexCoord(0, 1), BallInfo.CreateNormal(BallColor.Green));
            _board.SetBall(new HexCoord(1, 0), BallInfo.CreateNormal(BallColor.Green));
            _board.SetBall(new HexCoord(1, 1), BallInfo.CreateNormal(BallColor.Green));
            _board.SetBall(new HexCoord(2, 0), BallInfo.CreateNormal(BallColor.Green));

            var matched = MatchResolver.FindMatchingGroup(_board, new HexCoord(2, 0));
            Assert.That(matched.Count, Is.EqualTo(5));
        }

        [Test]
        public void MatchResolver_DifferentColors_DoNotCrossMatch()
        {
            _board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Red));
            _board.SetBall(new HexCoord(0, 1), BallInfo.CreateNormal(BallColor.Red));
            _board.SetBall(new HexCoord(0, 2), BallInfo.CreateNormal(BallColor.Yellow));
            _board.SetBall(new HexCoord(0, 3), BallInfo.CreateNormal(BallColor.Yellow));

            var matchedRed = MatchResolver.FindMatchingGroup(_board, new HexCoord(0, 0));
            Assert.That(matchedRed.Count, Is.EqualTo(0)); // Only 2 reds
        }

        #endregion

        #region 5. Disconnected Clusters (Floating Balls)

        [Test]
        public void ClusterResolver_FloatingBalls_AreIdentified()
        {
            // Row 0 anchor
            _board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Red));
            _board.SetBall(new HexCoord(1, 0), BallInfo.CreateNormal(BallColor.Red));
            _board.SetBall(new HexCoord(2, 0), BallInfo.CreateNormal(BallColor.Red));

            // Detached ball isolated at (4, 4)
            _board.SetBall(new HexCoord(4, 4), BallInfo.CreateNormal(BallColor.Yellow));

            var disconnected = ClusterResolver.FindDisconnectedClusters(_board);

            Assert.That(disconnected.Count, Is.EqualTo(1));
            Assert.That(disconnected[0], Is.EqualTo(new HexCoord(4, 4)));
        }

        [Test]
        public void ClusterResolver_WhenAnchorPopped_LowerChainFalls()
        {
            // Row 0 anchor ball
            _board.SetBall(new HexCoord(0, 2), BallInfo.CreateNormal(BallColor.Green));
            // Dependent balls attached only to (0, 2)
            _board.SetBall(new HexCoord(1, 1), BallInfo.CreateNormal(BallColor.Purple));
            _board.SetBall(new HexCoord(2, 1), BallInfo.CreateNormal(BallColor.Purple));

            // Initially all anchored
            Assert.That(ClusterResolver.FindDisconnectedClusters(_board).Count, Is.EqualTo(0));

            // Remove anchor
            _board.ClearCell(new HexCoord(0, 2));

            // Now (1, 1) and (2, 1) are detached!
            var disconnected = ClusterResolver.FindDisconnectedClusters(_board);
            Assert.That(disconnected.Count, Is.EqualTo(2));
            Assert.That(disconnected, Contains.Item(new HexCoord(1, 1)));
            Assert.That(disconnected, Contains.Item(new HexCoord(2, 1)));
        }

        #endregion

        #region 6. Seed Reproducibility & RNG

        [Test]
        public void DeterministicRng_IdenticalSeed_ProducesIdenticalSequence()
        {
            var rng1 = new DeterministicRng(98765);
            var rng2 = new DeterministicRng(98765);

            for (int i = 0; i < 50; i++)
            {
                Assert.That(rng1.Next(), Is.EqualTo(rng2.Next()));
                Assert.That(rng1.NextRange(1, 6), Is.EqualTo(rng2.NextRange(1, 6)));
            }
        }

        #endregion

        #region 7. Hybrid Pressure & Row Descent

        [Test]
        public void PressureEngine_TickAndMiss_TriggerRowDescent()
        {
            var pressure = new PressureEngine(maxTime: 10.0f, missPenalty: 2.0f, missThreshold: 4);

            // Tick down
            bool triggered = pressure.Tick(4.0f);
            Assert.That(triggered, Is.False);
            Assert.That(pressure.RemainingTime, Is.EqualTo(6.0f).Within(1e-4f));

            // 3 misses: penalty of 6s -> RemainingTime reaches 0.0 -> row drop!
            pressure.ApplyMiss();
            pressure.ApplyMiss();
            bool dropTriggered = pressure.ApplyMiss();

            Assert.That(dropTriggered, Is.True);
            Assert.That(pressure.RemainingTime, Is.EqualTo(10.0f)); // Reset
        }

        [Test]
        public void HexBoard_RowDescent_PreservesBallColumnInvariance()
        {
            // Place ball at (0, 3)
            _board.SetBall(new HexCoord(0, 3), BallInfo.CreateNormal(BallColor.Blue));
            var origPos = _board.CoordToLocalPosition(new HexCoord(0, 3));

            // Drop a new row at top
            var newRow = new[] { BallInfo.CreateNormal(BallColor.Red) };
            bool safe = _board.InsertRowAtTop(newRow);

            Assert.That(safe, Is.True);

            // Original ball is now at (1, 3)
            var newBall = _board.GetBall(new HexCoord(1, 3));
            Assert.That(newBall.HasValue, Is.True);
            Assert.That(newBall!.Value.Color, Is.EqualTo(BallColor.Blue));

            // Its X position in local space is 100% IDENTICAL
            var newPos = _board.CoordToLocalPosition(new HexCoord(1, 3));
            Assert.That(newPos.X, Is.EqualTo(origPos.X).Within(Vector2D.Epsilon));
            // Its Y position translated down by exactly 1 RowHeight
            Assert.That(newPos.Y, Is.EqualTo(origPos.Y - _geometry.RowHeight).Within(Vector2D.Epsilon));
        }

        [Test]
        public void HexBoard_BreachingDangerLine_CausesDefeat()
        {
            var engine = new AuthoritativeEngine();
            // Place a ball at DangerRow (row 11)
            engine.Board.SetBall(new HexCoord(11, 2), BallInfo.CreateNormal(BallColor.Red));

            // Shooting causes game status to evaluate danger breach
            // Aim at ceiling
            var result = engine.ExecuteShot(new Vector2D(1.0f, -8.0f), Vector2D.Up, BallInfo.CreateNormal(BallColor.Yellow));

            Assert.That(result.Status, Is.EqualTo(GameResult.Defeat));
        }

        #endregion

        #region 8. End-to-End Engine Loop

        [Test]
        public void AuthoritativeEngine_ExecuteShot_MatchClearsAndAwardsPoints()
        {
            var engine = new AuthoritativeEngine();

            // Set up 2 red balls at ceiling
            engine.Board.SetBall(new HexCoord(0, 3), BallInfo.CreateNormal(BallColor.Red));
            engine.Board.SetBall(new HexCoord(0, 4), BallInfo.CreateNormal(BallColor.Red));

            // Shoot a 3rd red ball directly at (0, 3) to form a match of 3
            var pos = engine.Board.CoordToLocalPosition(new HexCoord(0, 3));
            var origin = new Vector2D(pos.X, pos.Y - 5.0f);

            var result = engine.ExecuteShot(origin, Vector2D.Up, BallInfo.CreateNormal(BallColor.Red));

            Assert.That(result.MatchedCoords.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(result.ScoreEarned, Is.GreaterThan(0));
            Assert.That(engine.Score, Is.GreaterThan(0));
            Assert.That(result.ComboMultiplier, Is.GreaterThan(1.0f));

            // Matched cells are now empty
            Assert.That(engine.Board.IsOccupied(new HexCoord(0, 3)), Is.False);
            Assert.That(engine.Board.IsOccupied(new HexCoord(0, 4)), Is.False);
        }

        [Test]
        public void AuthoritativeEngine_ClearingBoard_TriggersVictory()
        {
            var engine = new AuthoritativeEngine();

            // Place 2 red balls
            engine.Board.SetBall(new HexCoord(0, 3), BallInfo.CreateNormal(BallColor.Red));
            engine.Board.SetBall(new HexCoord(0, 4), BallInfo.CreateNormal(BallColor.Red));

            var pos = engine.Board.CoordToLocalPosition(new HexCoord(0, 3));
            var origin = new Vector2D(pos.X, pos.Y - 4.0f);

            var result = engine.ExecuteShot(origin, Vector2D.Up, BallInfo.CreateNormal(BallColor.Red));

            Assert.That(result.Status, Is.EqualTo(GameResult.Victory));
        }

        [Test]
        public void AuthoritativeEngine_InvalidCommand_ThrowsException()
        {
            var engine = new AuthoritativeEngine();
            // Clear board -> Victory
            engine.Board.Clear();
            var dummyShot = engine.ExecuteShot(new Vector2D(1f, -4f), Vector2D.Up, BallInfo.CreateNormal(BallColor.Red));
            // If game is over, subsequent shot must throw
            if (engine.Status != GameResult.Ongoing)
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    engine.ExecuteShot(new Vector2D(1f, -4f), Vector2D.Up, BallInfo.CreateNormal(BallColor.Red));
                });
            }
        }

        #endregion
    }
}
