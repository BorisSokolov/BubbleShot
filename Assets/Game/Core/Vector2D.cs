using System;

namespace BubbleShot.Core
{
    /// <summary>
    /// Pure C# deterministic 2D vector avoiding UnityEngine dependencies.
    /// </summary>
    public readonly struct Vector2D : IEquatable<Vector2D>
    {
        public const float Epsilon = 1e-5f;

        public readonly float X;
        public readonly float Y;

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D Zero => new Vector2D(0f, 0f);
        public static Vector2D Up => new Vector2D(0f, 1f);
        public static Vector2D Down => new Vector2D(0f, -1f);
        public static Vector2D Left => new Vector2D(-1f, 0f);
        public static Vector2D Right => new Vector2D(1f, 0f);

        public float Length => MathF.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;

        public Vector2D Normalized
        {
            get
            {
                float len = Length;
                if (len < Epsilon) return Zero;
                float inv = 1f / len;
                return new Vector2D(X * inv, Y * inv);
            }
        }

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator -(Vector2D a) => new Vector2D(-a.X, -a.Y);
        public static Vector2D operator *(Vector2D a, float scalar) => new Vector2D(a.X * scalar, a.Y * scalar);
        public static Vector2D operator *(float scalar, Vector2D a) => new Vector2D(a.X * scalar, a.Y * scalar);
        public static Vector2D operator /(Vector2D a, float scalar) => new Vector2D(a.X / scalar, a.Y / scalar);

        public static float Dot(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;

        public static float Distance(Vector2D a, Vector2D b) => (a - b).Length;
        public static float DistanceSquared(Vector2D a, Vector2D b) => (a - b).LengthSquared;

        public bool Equals(Vector2D other)
        {
            return MathF.Abs(X - other.X) <= Epsilon && MathF.Abs(Y - other.Y) <= Epsilon;
        }

        public override bool Equals(object? obj) => obj is Vector2D other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);
        public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

        public override string ToString() => $"({X:F3}, {Y:F3})";
    }
}
