using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// Identifies floating/disconnected clusters of balls that are no longer anchored to Row 0.
    /// </summary>
    public static class ClusterResolver
    {
        /// <summary>
        /// Runs multi-source BFS from Row 0 ceiling anchors.
        /// Returns all occupied cells that have no unbroken chain of balls connecting them to Row 0.
        /// </summary>
        public static List<HexCoord> FindDisconnectedClusters(HexBoard board)
        {
            var anchored = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();

            // Seed queue with all balls currently in Row 0
            int row0Cols = board.GetColumnCount(0);
            for (int c = 0; c < row0Cols; c++)
            {
                var coord = new HexCoord(0, c);
                if (board.IsOccupied(coord))
                {
                    anchored.Add(coord);
                    queue.Enqueue(coord);
                }
            }

            // Multi-source BFS
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = board.GetNeighbors(current);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (!anchored.Contains(neighbor) && board.IsOccupied(neighbor))
                    {
                        anchored.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Any occupied ball not in anchored set is disconnected!
            var disconnected = new List<HexCoord>();
            var occupied = board.GetOccupiedCoords();
            for (int i = 0; i < occupied.Count; i++)
            {
                if (!anchored.Contains(occupied[i]))
                {
                    disconnected.Add(occupied[i]);
                }
            }

            return disconnected;
        }
    }
}
