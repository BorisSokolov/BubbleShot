using System;
using System.Collections.Generic;
using NUnit.Framework;
using BubbleShot.Core;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class HeadlessFuzzSimulationTests
    {
        public const int TotalSimulationShots = 10000;
        public const int SeedsCount = 10;
        public const int ShotsPerSeed = TotalSimulationShots / SeedsCount;

        [Test]
        [Category("Simulation")]
        public void HeadlessSimulation_TenThousandShots_MaintainsStrictInvariants()
        {
            int totalShotsExecuted = 0;
            int totalVictories = 0;
            int totalDefeats = 0;
            int totalMatchesPopped = 0;
            int totalClustersDropped = 0;
            int totalBombsDetonated = 0;

            uint[] seeds = { 10101, 20202, 30303, 40404, 50505, 60606, 70707, 80808, 90909, 12345 };

            for (int s = 0; s < seeds.Length; s++)
            {
                uint seed = seeds[s];
                var engine = CreateTestEngine(seed);
                var fuzzRng = new DeterministicRng(seed + 999);
                var launcherOrigin = new Vector2D(engine.Board.Geometry.BoardRight * 0.5f, -10.5f);

                int previousScore = 0;

                for (int shotIndex = 0; shotIndex < ShotsPerSeed; shotIndex++)
                {
                    // If game finished (Victory or Defeat), restart board for continuous fuzzing
                    if (engine.Status != GameResult.Ongoing)
                    {
                        if (engine.Status == GameResult.Victory) totalVictories++;
                        if (engine.Status == GameResult.Defeat) totalDefeats++;

                        engine = CreateTestEngine(seed + (uint)shotIndex);
                        previousScore = 0;
                    }

                    // 1. Generate randomized aim angle in valid range [15 deg, 165 deg]
                    int angleDegrees = 15 + (int)(fuzzRng.Next() % 136);
                    float rad = angleDegrees * (MathF.PI / 180.0f);
                    var dir = new Vector2D(MathF.Cos(rad), MathF.Sin(rad));

                    // 2. Obtain projectile (normal or combo special ball)
                    var projectile = engine.GenerateNextProjectile();

                    // 3. Execute authoritative shot
                    var result = engine.ExecuteShot(launcherOrigin, dir, projectile);
                    totalShotsExecuted++;

                    if (result.IsBombDetonation) totalBombsDetonated++;
                    totalMatchesPopped += result.MatchedCoords.Count;
                    totalClustersDropped += result.DetachedCoords.Count;

                    // 4. Validate strict invariants after EVERY shot

                    // Invariant A: Trajectory has valid finite path points (no NaNs or infinities)
                    Assert.That(result.Trajectory.PathPoints.Count, Is.GreaterThanOrEqualTo(2));
                    for (int p = 0; p < result.Trajectory.PathPoints.Count; p++)
                    {
                        var pt = result.Trajectory.PathPoints[p];
                        Assert.That(float.IsNaN(pt.X) || float.IsNaN(pt.Y), Is.False, "Trajectory coordinate must not be NaN");
                        Assert.That(float.IsInfinity(pt.X) || float.IsInfinity(pt.Y), Is.False, "Trajectory coordinate must not be Infinity");
                    }

                    // Invariant B: SnapCoord is within valid board coordinate boundaries at moment of attachment
                    var snap = result.AttachedCoord;
                    Assert.That(snap.Row >= 0 && snap.Row < engine.Board.Geometry.MaxRows, Is.True, $"Snap row {snap.Row} out of bounds");
                    int preDropParity = result.RowDropped ? (1 - engine.Board.TopRowParity) : engine.Board.TopRowParity;
                    int maxColsAtAttachment = ((snap.Row + preDropParity) & 1) == 0
                        ? engine.Board.Geometry.EvenWidth
                        : engine.Board.Geometry.OddWidth;
                    Assert.That(snap.Col >= 0 && snap.Col < maxColsAtAttachment, Is.True,
                        $"Snap {snap} out of bounds (max cols={maxColsAtAttachment}), RowDropped={result.RowDropped}, HitType={result.Trajectory.HitType}, HitBallCoord={result.Trajectory.HitBallCoord}, TopRowParity={engine.Board.TopRowParity}");

                    // Invariant C: All occupied cells on board are strictly valid
                    var occupied = engine.Board.GetOccupiedCoords();
                    for (int o = 0; o < occupied.Count; o++)
                    {
                        var coord = occupied[o];
                        Assert.That(engine.Board.IsValidCoord(coord), Is.True, $"Occupied cell {coord} is not a valid coordinate");
                        var ball = engine.Board.GetBall(coord);
                        Assert.That(ball.HasValue, Is.True, $"Occupied cell {coord} must contain a ball");
                        if (ball!.Value.Type == BallType.Normal)
                        {
                            Assert.That(ball.Value.Color, Is.Not.EqualTo(BallColor.None), "Normal ball must have a valid color");
                        }
                    }

                    // Invariant D: Score is monotonic non-decreasing
                    Assert.That(engine.Score, Is.GreaterThanOrEqualTo(previousScore), "Score must never decrease");
                    previousScore = engine.Score;

                    // Invariant E: Combo multiplier is clamped between 1.0 and 4.0
                    Assert.That(engine.ComboMultiplier, Is.GreaterThanOrEqualTo(1.0f).And.LessThanOrEqualTo(4.0f));

                    // Invariant F: Consecutive matches count is non-negative
                    Assert.That(engine.ConsecutiveMatches, Is.GreaterThanOrEqualTo(0));
                }
            }

            Assert.That(totalShotsExecuted, Is.EqualTo(TotalSimulationShots));
            TestContext.WriteLine($"=== 10,000-Shot Headless Fuzz Simulation Summary ===");
            TestContext.WriteLine($"Total Shots Executed:    {totalShotsExecuted:N0}");
            TestContext.WriteLine($"Total Matches Popped:    {totalMatchesPopped:N0}");
            TestContext.WriteLine($"Total Clusters Dropped:  {totalClustersDropped:N0}");
            TestContext.WriteLine($"Total Bombs Detonated:   {totalBombsDetonated:N0}");
            TestContext.WriteLine($"Total Game Victories:    {totalVictories:N0}");
            TestContext.WriteLine($"Total Game Defeats:      {totalDefeats:N0}");
            TestContext.WriteLine($"All invariants 100% verified across all seeds.");
        }

        private static AuthoritativeEngine CreateTestEngine(uint seed)
        {
            var geometry = new BoardGeometry(evenWidth: 8, maxRows: 12, dangerRow: 11);
            var board = new HexBoard(geometry);
            var pressure = new PressureEngine(maxTime: 12f, missPenalty: 2f, missThreshold: 4);

            var engine = new AuthoritativeEngine(seed, board, pressure);
            engine.ActiveColorCount = 4;

            // Populate starting 4 rows
            var rng = new DeterministicRng(seed);
            for (int r = 0; r < 4; r++)
            {
                int cols = board.GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    engine.Board.SetBall(new HexCoord(r, c), BallInfo.CreateNormal(rng.NextColor(engine.ActiveColorCount)));
                }
            }

            return engine;
        }
    }
}
