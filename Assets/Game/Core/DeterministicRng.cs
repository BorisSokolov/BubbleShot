using System;

namespace BubbleShot.Core
{
    /// <summary>
    /// Deterministic, platform-independent 32-bit pseudo-random number generator (XorShift32).
    /// </summary>
    public class DeterministicRng
    {
        private uint _state;

        public uint Seed { get; }

        public DeterministicRng(uint seed)
        {
            Seed = seed;
            _state = seed == 0 ? 0x853c49e6 : seed;
        }

        public uint Next()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextRange(int min, int max)
        {
            if (min >= max) return min;
            uint range = (uint)(max - min);
            return min + (int)(Next() % range);
        }

        public float NextFloat()
        {
            return (Next() & 0x00FFFFFF) / (float)0x01000000;
        }

        public BallColor NextColor(int activeColorCount)
        {
            int clamped = Math.Clamp(activeColorCount, 1, 6);
            int colorIdx = NextRange(1, clamped + 1);
            return (BallColor)colorIdx;
        }

        public BallInfo[] GenerateRow(int columnCount, int activeColorCount)
        {
            var row = new BallInfo[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                row[i] = BallInfo.CreateNormal(NextColor(activeColorCount));
            }
            return row;
        }
    }
}
