using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// Pure C# authoritative representation of the hexagonal bubble shooter board.
    /// </summary>
    public class HexBoard
    {
        private readonly BallInfo?[,] _cells;
        public BoardGeometry Geometry { get; }

        /// <summary>
        /// Parity of row 0: 0 for 8-column row, 1 for 7-column row.
        /// Alternates on each row insertion so balls translate purely downward.
        /// </summary>
        public int TopRowParity { get; private set; }

        public HexBoard(BoardGeometry? geometry = null, int initialTopRowParity = 0)
        {
            Geometry = geometry ?? new BoardGeometry();
            TopRowParity = initialTopRowParity & 1;
            _cells = new BallInfo?[Geometry.MaxRows, Geometry.EvenWidth];
        }

        public int GetRowParity(int row) => (row + TopRowParity) & 1;
        public bool IsEvenRow(int row) => GetRowParity(row) == 0;
        public int GetColumnCount(int row) => IsEvenRow(row) ? Geometry.EvenWidth : Geometry.OddWidth;

        public bool IsValidCoord(HexCoord coord)
        {
            if (coord.Row < 0 || coord.Row >= Geometry.MaxRows) return false;
            int cols = GetColumnCount(coord.Row);
            return coord.Col >= 0 && coord.Col < cols;
        }

        public bool IsOccupied(HexCoord coord)
        {
            if (!IsValidCoord(coord)) return false;
            return _cells[coord.Row, coord.Col].HasValue;
        }

        public BallInfo? GetBall(HexCoord coord)
        {
            if (!IsValidCoord(coord)) return null;
            return _cells[coord.Row, coord.Col];
        }

        public bool SetBall(HexCoord coord, BallInfo ball)
        {
            if (!IsValidCoord(coord)) return false;
            _cells[coord.Row, coord.Col] = ball;
            return true;
        }

        public bool ClearCell(HexCoord coord)
        {
            if (!IsValidCoord(coord)) return false;
            if (!_cells[coord.Row, coord.Col].HasValue) return false;
            _cells[coord.Row, coord.Col] = null;
            return true;
        }

        public void Clear()
        {
            for (int r = 0; r < Geometry.MaxRows; r++)
            {
                for (int c = 0; c < Geometry.EvenWidth; c++)
                {
                    _cells[r, c] = null;
                }
            }
        }

        public List<HexCoord> GetOccupiedCoords()
        {
            var result = new List<HexCoord>();
            for (int r = 0; r < Geometry.MaxRows; r++)
            {
                int cols = GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    if (_cells[r, c].HasValue)
                    {
                        result.Add(new HexCoord(r, c));
                    }
                }
            }
            return result;
        }

        public List<HexCoord> GetNeighbors(HexCoord coord)
        {
            var result = new List<HexCoord>(6);
            if (!IsValidCoord(coord)) return result;

            bool isEven = IsEvenRow(coord.Row);
            HexCoord[] offsets = isEven
                ? new[]
                {
                    new HexCoord(coord.Row - 1, coord.Col - 1),
                    new HexCoord(coord.Row - 1, coord.Col),
                    new HexCoord(coord.Row, coord.Col - 1),
                    new HexCoord(coord.Row, coord.Col + 1),
                    new HexCoord(coord.Row + 1, coord.Col - 1),
                    new HexCoord(coord.Row + 1, coord.Col)
                }
                : new[]
                {
                    new HexCoord(coord.Row - 1, coord.Col),
                    new HexCoord(coord.Row - 1, coord.Col + 1),
                    new HexCoord(coord.Row, coord.Col - 1),
                    new HexCoord(coord.Row, coord.Col + 1),
                    new HexCoord(coord.Row + 1, coord.Col),
                    new HexCoord(coord.Row + 1, coord.Col + 1)
                };

            for (int i = 0; i < offsets.Length; i++)
            {
                if (IsValidCoord(offsets[i]))
                {
                    result.Add(offsets[i]);
                }
            }

            return result;
        }

        public List<HexCoord> GetEmptyNeighbors(HexCoord coord)
        {
            var neighbors = GetNeighbors(coord);
            var empty = new List<HexCoord>(neighbors.Count);
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (!IsOccupied(neighbors[i]))
                {
                    empty.Add(neighbors[i]);
                }
            }
            return empty;
        }

        public Vector2D CoordToLocalPosition(HexCoord coord)
        {
            bool isEven = IsEvenRow(coord.Row);
            float x = isEven
                ? (coord.Col * Geometry.BallDiameter) + Geometry.BallRadius
                : ((coord.Col + 1f) * Geometry.BallDiameter);
            float y = -coord.Row * Geometry.RowHeight;
            return new Vector2D(x, y);
        }

        public HexCoord FindClosestEmptyCoord(Vector2D localPos)
        {
            int approxRow = (int)MathF.Round(-localPos.Y / Geometry.RowHeight);
            int minR = Math.Clamp(approxRow - 2, 0, Geometry.MaxRows - 1);
            int maxR = Math.Clamp(approxRow + 2, 0, Geometry.MaxRows - 1);

            HexCoord bestCoord = new HexCoord(0, 0);
            float bestDistSq = float.MaxValue;

            for (int r = minR; r <= maxR; r++)
            {
                int cols = GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    var coord = new HexCoord(r, c);
                    if (IsOccupied(coord)) continue;

                    var cellPos = CoordToLocalPosition(coord);
                    float distSq = Vector2D.DistanceSquared(localPos, cellPos);

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

            if (bestDistSq == float.MaxValue)
            {
                for (int r = 0; r < Geometry.MaxRows; r++)
                {
                    int cols = GetColumnCount(r);
                    for (int c = 0; c < cols; c++)
                    {
                        var coord = new HexCoord(r, c);
                        if (!IsOccupied(coord)) return coord;
                    }
                }
            }

            return bestCoord;
        }

        /// <summary>
        /// Inserts a new procedural row at Row 0 and shifts existing rows downward.
        /// Returns true if the board is still safe, or false if any ball crossed the DangerRow.
        /// </summary>
        public bool InsertRowAtTop(BallInfo[] newRow)
        {
            int nextParity = 1 - TopRowParity;
            int newRowCols = (nextParity == 0) ? Geometry.EvenWidth : Geometry.OddWidth;

            // Shift all rows down by 1
            for (int r = Geometry.MaxRows - 1; r >= 1; r--)
            {
                int cols = (r + nextParity) % 2 == 0 ? Geometry.EvenWidth : Geometry.OddWidth;
                for (int c = 0; c < Geometry.EvenWidth; c++)
                {
                    _cells[r, c] = (c < cols) ? _cells[r - 1, c] : null;
                }
            }

            TopRowParity = nextParity;

            // Place new row at row 0
            for (int c = 0; c < Geometry.EvenWidth; c++)
            {
                if (c < newRowCols && c < newRow.Length)
                {
                    _cells[0, c] = newRow[c];
                }
                else
                {
                    _cells[0, c] = null;
                }
            }

            // Check danger breach: any ball at DangerRow or beyond
            for (int r = Geometry.DangerRow; r < Geometry.MaxRows; r++)
            {
                int cols = GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    if (_cells[r, c].HasValue)
                    {
                        return false; // Danger line breached
                    }
                }
            }

            return true;
        }

        public bool HasBreachedDangerLine()
        {
            for (int r = Geometry.DangerRow; r < Geometry.MaxRows; r++)
            {
                int cols = GetColumnCount(r);
                for (int c = 0; c < cols; c++)
                {
                    if (_cells[r, c].HasValue) return true;
                }
            }
            return false;
        }
    }
}
