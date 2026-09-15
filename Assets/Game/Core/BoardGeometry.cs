using System;

namespace BubbleShot.Core
{
    /// <summary>
    /// Configuration and metric transformations for the hexagonal game board.
    /// </summary>
    public class BoardGeometry
    {
        public const int DefaultEvenWidth = 8;
        public const int DefaultOddWidth = 7;
        public const int DefaultMaxRows = 12;
        public const int DefaultDangerRow = 11;
        public const float DefaultBallDiameter = 1.0f;

        public int EvenWidth { get; }
        public int OddWidth { get; }
        public int MaxRows { get; }
        public int DangerRow { get; }
        public float BallDiameter { get; }
        public float BallRadius { get; }
        public float RowHeight { get; }
        public float BoardLeft { get; }
        public float BoardRight { get; }
        public float CeilingY { get; }

        public BoardGeometry(
            int evenWidth = DefaultEvenWidth,
            int maxRows = DefaultMaxRows,
            int dangerRow = DefaultDangerRow,
            float ballDiameter = DefaultBallDiameter)
        {
            EvenWidth = evenWidth;
            OddWidth = evenWidth - 1;
            MaxRows = maxRows;
            DangerRow = dangerRow;
            BallDiameter = ballDiameter;
            BallRadius = ballDiameter * 0.5f;
            RowHeight = MathF.Sqrt(3f) * 0.5f * ballDiameter;
            BoardLeft = 0f;
            BoardRight = evenWidth * ballDiameter;
            CeilingY = 0f;
        }

        /// <summary>
        /// Transforms discrete hex coordinates into 2D local space coordinates.
        /// Origin (0,0) is top-left anchor. Y increases upward; board rows extend into negative Y.
        /// </summary>
        public Vector2D CoordToLocalPosition(HexCoord coord)
        {
            float x = coord.IsEvenRow
                ? (coord.Col * BallDiameter) + BallRadius
                : ((coord.Col + 1f) * BallDiameter);
            float y = -coord.Row * RowHeight;
            return new Vector2D(x, y);
        }

        /// <summary>
        /// Finds the nearest valid hex grid cell to a given point in local space with deterministic tie-breaking.
        /// </summary>
        public HexCoord FindClosestCoord(Vector2D localPos, int topRowParity = 0)
        {
            // Estimate approximate row
            int approxRow = (int)MathF.Round(-localPos.Y / RowHeight);
            int minR = Math.Clamp(approxRow - 2, 0, MaxRows - 1);
            int maxR = Math.Clamp(approxRow + 2, 0, MaxRows - 1);

            HexCoord bestCoord = new HexCoord(0, 0);
            float bestDistSq = float.MaxValue;

            for (int r = minR; r <= maxR; r++)
            {
                bool isEven = ((r + topRowParity) & 1) == 0;
                int cols = isEven ? EvenWidth : OddWidth;
                for (int c = 0; c < cols; c++)
                {
                    var coord = new HexCoord(r, c);
                    float x = isEven
                        ? (c * BallDiameter) + BallRadius
                        : ((c + 1f) * BallDiameter);
                    float y = -r * RowHeight;
                    var cellPos = new Vector2D(x, y);
                    float distSq = Vector2D.DistanceSquared(localPos, cellPos);

                    // Deterministic tie-breaking: prefer smaller distance, then smaller row, then smaller col
                    if (distSq < bestDistSq - Vector2D.Epsilon)
                    {
                        bestDistSq = distSq;
                        bestCoord = coord;
                    }
                    else if (MathF.Abs(distSq - bestDistSq) <= Vector2D.Epsilon)
                    {
                        if (coord.CompareTo(bestCoord) < 0)
                        {
                            bestDistSq = distSq;
                            bestCoord = coord;
                        }
                    }
                }
            }

            return bestCoord;
        }
    }
}
