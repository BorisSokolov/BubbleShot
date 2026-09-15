using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// BFS-based connected matching group detection for hexagonal boards.
    /// </summary>
    public static class MatchResolver
    {
        public const int MinimumMatchSize = 3;

        /// <summary>
        /// Finds all connected balls matching the color of startCoord.
        /// Returns the group if size >= MinimumMatchSize (3), otherwise returns an empty list.
        /// </summary>
        public static List<HexCoord> FindMatchingGroup(HexBoard board, HexCoord startCoord)
        {
            var matchedGroup = new List<HexCoord>();
            var startBall = board.GetBall(startCoord);
            if (!startBall.HasValue) return matchedGroup;

            BallColor targetColor = startBall.Value.Color;
            if (targetColor == BallColor.None) return matchedGroup;

            var visited = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();

            queue.Enqueue(startCoord);
            visited.Add(startCoord);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                matchedGroup.Add(current);

                var neighbors = board.GetNeighbors(current);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighborCoord = neighbors[i];
                    if (visited.Contains(neighborCoord)) continue;

                    var neighborBall = board.GetBall(neighborCoord);
                    if (neighborBall.HasValue && neighborBall.Value.MatchesColor(targetColor))
                    {
                        visited.Add(neighborCoord);
                        queue.Enqueue(neighborCoord);
                    }
                }
            }

            return matchedGroup.Count >= MinimumMatchSize ? matchedGroup : new List<HexCoord>();
        }
    }
}
