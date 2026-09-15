using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    public enum LevelObjectiveType
    {
        ClearAll = 0,
        ClearAnchor = 1,
        TargetScore = 2,
        SurviveRows = 3
    }

    public readonly struct HexCoordBallPair : IEquatable<HexCoordBallPair>
    {
        public readonly HexCoord Coord;
        public readonly BallInfo Ball;

        public HexCoordBallPair(HexCoord coord, BallInfo ball)
        {
            Coord = coord;
            Ball = ball;
        }

        public bool Equals(HexCoordBallPair other) => Coord.Equals(other.Coord) && Ball.Equals(other.Ball);
        public override bool Equals(object? obj) => obj is HexCoordBallPair other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Coord, Ball);
    }

    /// <summary>
    /// Pure C# data-driven level configuration.
    /// </summary>
    public class LevelDefinition
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = "Untitled Level";
        public uint Seed { get; set; } = 12345;
        public LevelObjectiveType ObjectiveType { get; set; } = LevelObjectiveType.ClearAll;
        public int TargetScore { get; set; } = 5000;
        public int TargetRowsToSurvive { get; set; } = 5;
        public int ActiveColorCount { get; set; } = 4;
        public float BasePressureTime { get; set; } = 12.0f;
        public float MissPenalty { get; set; } = 2.0f;
        public int Star1Score { get; set; } = 2000;
        public int Star2Score { get; set; } = 4000;
        public int Star3Score { get; set; } = 7000;
        public List<HexCoordBallPair> StartingBalls { get; } = new List<HexCoordBallPair>();

        public int CalculateStars(int finalScore)
        {
            if (finalScore >= Star3Score) return 3;
            if (finalScore >= Star2Score) return 2;
            if (finalScore >= Star1Score) return 1;
            return 1; // Minimum 1 star for victory
        }
    }
}
