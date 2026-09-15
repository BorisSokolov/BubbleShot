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
        /// If startCoord is a Wild ball, delegates to ResolveWildMatches.
        /// If startCoord is a Bomb ball, delegates to ResolveBombBlast.
        /// Returns the group if size >= MinimumMatchSize (3), otherwise returns an empty list.
        /// </summary>
        public static List<HexCoord> FindMatchingGroup(HexBoard board, HexCoord startCoord)
        {
            var startBall = board.GetBall(startCoord);
            if (!startBall.HasValue) return new List<HexCoord>();

            if (startBall.Value.Type == BallType.Wild)
            {
                return ResolveWildMatches(board, startCoord);
            }

            if (startBall.Value.Type == BallType.Bomb)
            {
                return ResolveBombBlast(board, startCoord);
            }

            BallColor targetColor = startBall.Value.Color;
            if (targetColor == BallColor.None) return new List<HexCoord>();

            var matchedGroup = FindConnectedGroupForColor(board, startCoord, targetColor);
            return matchedGroup.Count >= MinimumMatchSize ? matchedGroup : new List<HexCoord>();
        }

        /// <summary>
        /// Evaluates all distinct neighbor colors of a Wild ball simultaneously per ADR 005.
        /// All colors that form a connected group >= 3 (including the Wild ball) are cleared together.
        /// Returns the union of all qualifying groups, or an empty list if none qualify.
        /// </summary>
        public static List<HexCoord> ResolveWildMatches(HexBoard board, HexCoord wildCoord)
        {
            var wildBall = board.GetBall(wildCoord);
            if (!wildBall.HasValue) return new List<HexCoord>();

            var qualifyingCoords = new HashSet<HexCoord>();
            var neighbors = board.GetNeighbors(wildCoord);

            // 1. Gather all candidate distinct colors from immediate neighbors
            var candidateColors = new HashSet<BallColor>();
            for (int i = 0; i < neighbors.Count; i++)
            {
                var nBall = board.GetBall(neighbors[i]);
                if (nBall.HasValue && nBall.Value.Color != BallColor.None)
                {
                    candidateColors.Add(nBall.Value.Color);
                }
            }

            // 2. For each candidate color, perform BFS starting from wildCoord
            foreach (var color in candidateColors)
            {
                var group = FindConnectedGroupForColor(board, wildCoord, color);
                if (group.Count >= MinimumMatchSize)
                {
                    qualifyingCoords.UnionWith(group);
                }
            }

            // 3. Edge case: If 3 or more Wild balls are directly connected with no normal colors
            if (qualifyingCoords.Count == 0)
            {
                var wildGroup = FindConnectedWildGroup(board, wildCoord);
                if (wildGroup.Count >= MinimumMatchSize)
                {
                    qualifyingCoords.UnionWith(wildGroup);
                }
            }

            var result = new List<HexCoord>(qualifyingCoords);
            result.Sort((a, b) => a.Row != b.Row ? a.Row.CompareTo(b.Row) : a.Col.CompareTo(b.Col));
            return result;
        }

        /// <summary>
        /// Resolves a Bomb ball detonation within deterministic hex radius <= 1 per ADR 006.
        /// Recursively detonates any adjacent Bomb balls caught in the blast (chain reaction).
        /// Returns all destroyed cell coordinates sorted deterministically.
        /// </summary>
        public static List<HexCoord> ResolveBombBlast(HexBoard board, HexCoord bombCoord)
        {
            var blastedCoords = new HashSet<HexCoord>();
            var explodedBombs = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();

            queue.Enqueue(bombCoord);
            explodedBombs.Add(bombCoord);
            blastedCoords.Add(bombCoord);

            while (queue.Count > 0)
            {
                var currentBomb = queue.Dequeue();
                var neighbors = board.GetNeighbors(currentBomb);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighborCoord = neighbors[i];
                    if (board.IsOccupied(neighborCoord))
                    {
                        blastedCoords.Add(neighborCoord);
                        var ball = board.GetBall(neighborCoord);
                        if (ball.HasValue && ball.Value.Type == BallType.Bomb && !explodedBombs.Contains(neighborCoord))
                        {
                            explodedBombs.Add(neighborCoord);
                            queue.Enqueue(neighborCoord);
                        }
                    }
                }
            }

            var result = new List<HexCoord>(blastedCoords);
            result.Sort((a, b) => a.Row != b.Row ? a.Row.CompareTo(b.Row) : a.Col.CompareTo(b.Col));
            return result;
        }

        private static List<HexCoord> FindConnectedGroupForColor(HexBoard board, HexCoord startCoord, BallColor targetColor)
        {
            var group = new List<HexCoord>();
            var visited = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();

            queue.Enqueue(startCoord);
            visited.Add(startCoord);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                group.Add(current);

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

            return group;
        }

        private static List<HexCoord> FindConnectedWildGroup(HexBoard board, HexCoord startCoord)
        {
            var group = new List<HexCoord>();
            var visited = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();

            queue.Enqueue(startCoord);
            visited.Add(startCoord);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                group.Add(current);

                var neighbors = board.GetNeighbors(current);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighborCoord = neighbors[i];
                    if (visited.Contains(neighborCoord)) continue;

                    var neighborBall = board.GetBall(neighborCoord);
                    if (neighborBall.HasValue && neighborBall.Value.Type == BallType.Wild)
                    {
                        visited.Add(neighborCoord);
                        queue.Enqueue(neighborCoord);
                    }
                }
            }

            return group;
        }
    }
}
