using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// Discrete coordinate on the odd-r horizontal staggered hexagonal grid.
    /// </summary>
    public readonly struct HexCoord : IEquatable<HexCoord>, IComparable<HexCoord>
    {
        public readonly int Row;
        public readonly int Col;

        public HexCoord(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool IsEvenRow => (Row & 1) == 0;

        public int GetColumnCount(int evenWidth) => IsEvenRow ? evenWidth : evenWidth - 1;

        public bool IsValid(int maxRows, int evenWidth)
        {
            if (Row < 0 || Row >= maxRows) return false;
            int maxCols = GetColumnCount(evenWidth);
            return Col >= 0 && Col < maxCols;
        }

        /// <summary>
        /// Returns the 6 hexagonal neighbors ordered: Top-Left, Top-Right, Left, Right, Bottom-Left, Bottom-Right.
        /// </summary>
        public HexCoord[] GetRawNeighbors()
        {
            if (IsEvenRow)
            {
                return new[]
                {
                    new HexCoord(Row - 1, Col - 1), // Top-Left
                    new HexCoord(Row - 1, Col),     // Top-Right
                    new HexCoord(Row, Col - 1),     // Left
                    new HexCoord(Row, Col + 1),     // Right
                    new HexCoord(Row + 1, Col - 1), // Bottom-Left
                    new HexCoord(Row + 1, Col)      // Bottom-Right
                };
            }
            else
            {
                return new[]
                {
                    new HexCoord(Row - 1, Col),     // Top-Left
                    new HexCoord(Row - 1, Col + 1), // Top-Right
                    new HexCoord(Row, Col - 1),     // Left
                    new HexCoord(Row, Col + 1),     // Right
                    new HexCoord(Row + 1, Col),     // Bottom-Left
                    new HexCoord(Row + 1, Col + 1)  // Bottom-Right
                };
            }
        }

        public List<HexCoord> GetValidNeighbors(int maxRows, int evenWidth)
        {
            var raw = GetRawNeighbors();
            var valid = new List<HexCoord>(6);
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].IsValid(maxRows, evenWidth))
                {
                    valid.Add(raw[i]);
                }
            }
            return valid;
        }

        public bool Equals(HexCoord other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object? obj) => obj is HexCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Row, Col);
        public static bool operator ==(HexCoord left, HexCoord right) => left.Equals(right);
        public static bool operator !=(HexCoord left, HexCoord right) => !left.Equals(right);

        public int CompareTo(HexCoord other)
        {
            int rowCompare = Row.CompareTo(other.Row);
            return rowCompare != 0 ? rowCompare : Col.CompareTo(other.Col);
        }

        public override string ToString() => $"({Row}, {Col})";
    }
}
