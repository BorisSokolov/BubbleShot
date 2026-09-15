using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    public enum TrajectoryHitType
    {
        None = 0,
        Ball = 1,
        Ceiling = 2
    }

    public class TrajectoryResult
    {
        public List<Vector2D> PathPoints { get; } = new List<Vector2D>();
        public TrajectoryHitType HitType { get; set; } = TrajectoryHitType.None;
        public HexCoord? HitBallCoord { get; set; }
        public HexCoord SnapCoord { get; set; }
        public Vector2D ImpactPoint { get; set; }
        public int BounceCount { get; set; }
    }

    /// <summary>
    /// Authoritative, deterministic trajectory calculation and continuous swept-circle raycasting.
    /// </summary>
    public class TrajectorySolver
    {
        public const int MaxBounces = 2;
        public const float Epsilon = 1e-5f;

        public static TrajectoryResult Solve(
            Vector2D origin,
            Vector2D direction,
            HexBoard board,
            int maxBounces = MaxBounces)
        {
            var result = new TrajectoryResult();
            result.PathPoints.Add(origin);

            Vector2D currentOrigin = origin;
            Vector2D currentDir = direction.Normalized;
            var geometry = board.Geometry;

            float leftWallX = geometry.BoardLeft + geometry.BallRadius;
            float rightWallX = geometry.BoardRight - geometry.BallRadius;
            float ceilingY = geometry.CeilingY - geometry.BallRadius;

            int bouncesRemaining = maxBounces;

            while (true)
            {
                // Find earliest collision with balls along this segment
                float earliestBallT = float.MaxValue;
                HexCoord earliestBallCoord = new HexCoord(-1, -1);

                var occupied = board.GetOccupiedCoords();
                float collisionDistance = geometry.BallDiameter; // 2 * radius
                float colDistSq = collisionDistance * collisionDistance;

                for (int i = 0; i < occupied.Count; i++)
                {
                    var coord = occupied[i];
                    var center = board.CoordToLocalPosition(coord);
                    var d = currentOrigin - center;

                    float a = currentDir.LengthSquared; // 1.0f
                    float b = 2f * Vector2D.Dot(d, currentDir);
                    float c = d.LengthSquared - colDistSq;

                    float discriminant = b * b - 4f * a * c;
                    if (discriminant >= 0f)
                    {
                        float sqrtDisc = MathF.Sqrt(discriminant);
                        float t1 = (-b - sqrtDisc) / (2f * a);

                        if (t1 > Epsilon && t1 < earliestBallT)
                        {
                            earliestBallT = t1;
                            earliestBallCoord = coord;
                        }
                    }
                }

                // Check ceiling intersection (moving upward: Y > 0)
                float ceilingT = float.MaxValue;
                if (currentDir.Y > Epsilon)
                {
                    float t = (ceilingY - currentOrigin.Y) / currentDir.Y;
                    if (t > Epsilon)
                    {
                        ceilingT = t;
                    }
                }

                // Check side wall intersection
                float wallT = float.MaxValue;
                if (currentDir.X < -Epsilon)
                {
                    float t = (leftWallX - currentOrigin.X) / currentDir.X;
                    if (t > Epsilon) wallT = t;
                }
                else if (currentDir.X > Epsilon)
                {
                    float t = (rightWallX - currentOrigin.X) / currentDir.X;
                    if (t > Epsilon) wallT = t;
                }

                // Determine earliest event
                float minT = MathF.Min(earliestBallT, MathF.Min(ceilingT, wallT));

                if (minT == float.MaxValue)
                {
                    // No collision found (should not happen with ceiling present)
                    Vector2D finalPoint = currentOrigin + currentDir * 50f;
                    result.PathPoints.Add(finalPoint);
                    break;
                }

                if (MathF.Abs(minT - earliestBallT) <= Epsilon)
                {
                    // Ball collision occurs earliest!
                    Vector2D impact = currentOrigin + currentDir * earliestBallT;
                    result.PathPoints.Add(impact);
                    result.HitType = TrajectoryHitType.Ball;
                    result.HitBallCoord = earliestBallCoord;
                    result.ImpactPoint = impact;
                    result.SnapCoord = ResolveSnapCoordForBall(board, earliestBallCoord, impact);
                    break;
                }
                else if (MathF.Abs(minT - ceilingT) <= Epsilon)
                {
                    // Ceiling collision occurs earliest!
                    Vector2D impact = currentOrigin + currentDir * ceilingT;
                    result.PathPoints.Add(impact);
                    result.HitType = TrajectoryHitType.Ceiling;
                    result.ImpactPoint = impact;
                    result.SnapCoord = ResolveSnapCoordForCeiling(board, impact);
                    break;
                }
                else
                {
                    // Wall collision occurs earliest!
                    Vector2D wallHitPoint = currentOrigin + currentDir * wallT;
                    result.PathPoints.Add(wallHitPoint);
                    result.BounceCount++;

                    if (bouncesRemaining <= 0)
                    {
                        // Max bounces exceeded
                        break;
                    }

                    bouncesRemaining--;
                    currentOrigin = wallHitPoint;
                    currentDir = new Vector2D(-currentDir.X, currentDir.Y); // Reflect horizontally
                }
            }

            return result;
        }

        private static HexCoord ResolveSnapCoordForBall(HexBoard board, HexCoord hitCoord, Vector2D impactPoint)
        {
            var emptyNeighbors = board.GetEmptyNeighbors(hitCoord);

            if (emptyNeighbors.Count == 0)
            {
                // Fallback: check all empty cells in board with strict parity awareness
                return board.FindClosestEmptyCoord(impactPoint);
            }

            HexCoord bestCoord = emptyNeighbors[0];
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < emptyNeighbors.Count; i++)
            {
                var candidate = emptyNeighbors[i];
                var cellPos = board.CoordToLocalPosition(candidate);
                float distSq = Vector2D.DistanceSquared(impactPoint, cellPos);

                if (distSq < bestDistSq - Epsilon)
                {
                    bestDistSq = distSq;
                    bestCoord = candidate;
                }
                else if (MathF.Abs(distSq - bestDistSq) <= Epsilon)
                {
                    // Tie-break: prefer smaller row index (higher up), then smaller col index (leftmost)
                    if (candidate.CompareTo(bestCoord) < 0)
                    {
                        bestDistSq = distSq;
                        bestCoord = candidate;
                    }
                }
            }

            return bestCoord;
        }

        private static HexCoord ResolveSnapCoordForCeiling(HexBoard board, Vector2D impactPoint)
        {
            int cols = board.GetColumnCount(0);
            HexCoord bestCoord = new HexCoord(0, 0);
            float bestDistSq = float.MaxValue;

            for (int c = 0; c < cols; c++)
            {
                var candidate = new HexCoord(0, c);
                if (board.IsOccupied(candidate)) continue;

                var cellPos = board.CoordToLocalPosition(candidate);
                float distSq = Vector2D.DistanceSquared(impactPoint, cellPos);

                if (distSq < bestDistSq - Epsilon)
                {
                    bestDistSq = distSq;
                    bestCoord = candidate;
                }
                else if (MathF.Abs(distSq - bestDistSq) <= Epsilon)
                {
                    if (candidate.CompareTo(bestCoord) < 0)
                    {
                        bestDistSq = distSq;
                        bestCoord = candidate;
                    }
                }
            }

            if (bestDistSq == float.MaxValue)
            {
                return board.FindClosestEmptyCoord(impactPoint);
            }

            return bestCoord;
        }
    }
}
