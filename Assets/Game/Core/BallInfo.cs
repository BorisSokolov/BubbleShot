using System;

namespace BubbleShot.Core
{
    public enum BallColor
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
        Orange = 6
    }

    public enum BallType
    {
        Normal = 0,
        Bomb = 1,
        Wild = 2
    }

    /// <summary>
    /// Immutable specification of a ball in the authoritative game engine.
    /// </summary>
    public readonly struct BallInfo : IEquatable<BallInfo>
    {
        public readonly BallColor Color;
        public readonly BallType Type;

        public BallInfo(BallColor color, BallType type = BallType.Normal)
        {
            Color = color;
            Type = type;
        }

        public static BallInfo CreateNormal(BallColor color) => new BallInfo(color, BallType.Normal);
        public static BallInfo CreateBomb() => new BallInfo(BallColor.None, BallType.Bomb);
        public static BallInfo CreateWild() => new BallInfo(BallColor.None, BallType.Wild);

        public bool IsSpecial => Type != BallType.Normal;

        public bool MatchesColor(BallColor otherColor)
        {
            if (Type == BallType.Wild || otherColor == BallColor.None) return true;
            return Color == otherColor;
        }

        public bool Equals(BallInfo other) => Color == other.Color && Type == other.Type;
        public override bool Equals(object? obj) => obj is BallInfo other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Color, Type);
        public static bool operator ==(BallInfo left, BallInfo right) => left.Equals(right);
        public static bool operator !=(BallInfo left, BallInfo right) => !left.Equals(right);

        public override string ToString() => Type == BallType.Normal ? $"{Color}" : $"{Type}";
    }
}
