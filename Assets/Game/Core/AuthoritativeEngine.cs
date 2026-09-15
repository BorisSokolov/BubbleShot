using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    public enum GameResult
    {
        Ongoing = 0,
        Victory = 1,
        Defeat = 2
    }

    public class ShotExecutionResult
    {
        public TrajectoryResult Trajectory { get; }
        public HexCoord AttachedCoord { get; }
        public List<HexCoord> MatchedCoords { get; } = new List<HexCoord>();
        public List<HexCoord> DetachedCoords { get; } = new List<HexCoord>();
        public bool RowDropped { get; set; }
        public bool IsBombDetonation { get; set; }
        public int ScoreEarned { get; set; }
        public float ComboMultiplier { get; set; }
        public GameResult Status { get; set; }

        public ShotExecutionResult(TrajectoryResult trajectory, HexCoord attachedCoord)
        {
            Trajectory = trajectory;
            AttachedCoord = attachedCoord;
        }
    }

    /// <summary>
    /// The root pure C# deterministic game engine coordinating all authoritative state transitions.
    /// </summary>
    public class AuthoritativeEngine
    {
        public HexBoard Board { get; }
        public PressureEngine Pressure { get; }
        public DeterministicRng Rng { get; }

        public int Score { get; private set; }
        public float ComboMultiplier { get; private set; } = 1.0f;
        public int ConsecutiveMatches { get; private set; }
        public GameResult Status { get; private set; } = GameResult.Ongoing;
        public int ActiveColorCount { get; set; } = 4;

        public AuthoritativeEngine(uint seed = 12345, HexBoard? board = null, PressureEngine? pressure = null)
        {
            Board = board ?? new HexBoard();
            Pressure = pressure ?? new PressureEngine();
            Rng = new DeterministicRng(seed);
        }

        public BallInfo GenerateNextProjectile()
        {
            if (ConsecutiveMatches > 0 && ConsecutiveMatches % 3 == 0)
            {
                // Alternate between Bomb and Wild on 3-combo milestones
                return (ConsecutiveMatches / 3) % 2 == 1
                    ? BallInfo.CreateBomb()
                    : BallInfo.CreateWild();
            }

            return BallInfo.CreateNormal(Rng.NextColor(ActiveColorCount));
        }

        public ShotExecutionResult ExecuteShot(Vector2D origin, Vector2D direction, BallInfo projectile)
        {
            if (Status != GameResult.Ongoing)
            {
                throw new InvalidOperationException($"Cannot execute shot when game status is {Status}.");
            }

            // 1. Authoritative trajectory resolution
            var trajectory = TrajectorySolver.Solve(origin, direction, Board);
            var snapCoord = trajectory.SnapCoord;

            var shotResult = new ShotExecutionResult(trajectory, snapCoord);

            // 2. Attach projectile to board
            Board.SetBall(snapCoord, projectile);

            // 3. Match / Blast resolution
            bool isBomb = projectile.Type == BallType.Bomb;
            shotResult.IsBombDetonation = isBomb;

            var matched = isBomb
                ? MatchResolver.ResolveBombBlast(Board, snapCoord)
                : (projectile.Type == BallType.Wild
                    ? MatchResolver.ResolveWildMatches(Board, snapCoord)
                    : MatchResolver.FindMatchingGroup(Board, snapCoord));

            bool isSuccessful = isBomb
                ? matched.Count > 0
                : matched.Count >= MatchResolver.MinimumMatchSize;

            if (isSuccessful)
            {
                shotResult.MatchedCoords.AddRange(matched);
                for (int i = 0; i < matched.Count; i++)
                {
                    Board.ClearCell(matched[i]);
                }

                // 4. Cluster detachment resolution
                var detached = ClusterResolver.FindDisconnectedClusters(Board);
                shotResult.DetachedCoords.AddRange(detached);
                for (int i = 0; i < detached.Count; i++)
                {
                    Board.ClearCell(detached[i]);
                }

                // 5. Score and combo updates
                ConsecutiveMatches++;
                ComboMultiplier = MathF.Min(ComboMultiplier + 0.5f, 4.0f);
                int baseMatchScore = matched.Count * (isBomb ? 150 : 100);
                int detachedBonus = detached.Count * 200;
                int earned = (int)((baseMatchScore + detachedBonus) * ComboMultiplier);

                Score += earned;
                shotResult.ScoreEarned = earned;
                shotResult.ComboMultiplier = ComboMultiplier;

                // 6. Relief pressure
                Pressure.ApplyMatchRelief(matched.Count, detached.Count);
            }
            else
            {
                // Unsuccessful shot
                ConsecutiveMatches = 0;
                ComboMultiplier = 1.0f;
                shotResult.ComboMultiplier = 1.0f;

                bool forceDrop = Pressure.ApplyMiss();
                if (forceDrop)
                {
                    TriggerRowDescent(shotResult);
                }
            }

            // 7. Victory / Defeat evaluation
            EvaluateGameStatus();
            shotResult.Status = Status;

            return shotResult;
        }

        public bool Tick(float deltaTime)
        {
            if (Status != GameResult.Ongoing) return false;

            bool timerExpired = Pressure.Tick(deltaTime);
            if (timerExpired)
            {
                ConsecutiveMatches = 0;
                ComboMultiplier = 1.0f;
                var dummyResult = new ShotExecutionResult(new TrajectoryResult(), new HexCoord(0, 0));
                TriggerRowDescent(dummyResult);
                EvaluateGameStatus();
                return true;
            }

            return false;
        }

        private void TriggerRowDescent(ShotExecutionResult result)
        {
            int nextParity = 1 - Board.TopRowParity;
            int newCols = nextParity == 0 ? Board.Geometry.EvenWidth : Board.Geometry.OddWidth;
            var newRow = Rng.GenerateRow(newCols, ActiveColorCount);

            bool safe = Board.InsertRowAtTop(newRow);
            result.RowDropped = true;

            if (!safe || Board.HasBreachedDangerLine())
            {
                Status = GameResult.Defeat;
            }
        }

        private void EvaluateGameStatus()
        {
            if (Board.HasBreachedDangerLine())
            {
                Status = GameResult.Defeat;
                return;
            }

            if (Board.GetOccupiedCoords().Count == 0)
            {
                Status = GameResult.Victory;
            }
        }
    }
}
