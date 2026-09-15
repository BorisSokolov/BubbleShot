using System;

namespace BubbleShot.Core
{
    /// <summary>
    /// Governs the hybrid pressure model: real-time countdown timer, miss penalties, and match relief.
    /// </summary>
    public class PressureEngine
    {
        public const float DefaultMaxTime = 12.0f;
        public const float DefaultMissPenalty = 2.0f;
        public const int DefaultMissThreshold = 4;

        public float MaxTime { get; }
        public float RemainingTime { get; private set; }
        public float MissPenalty { get; }
        public int MissThreshold { get; }
        public int ConsecutiveMisses { get; private set; }
        public bool IsPaused { get; set; }

        public PressureEngine(
            float maxTime = DefaultMaxTime,
            float missPenalty = DefaultMissPenalty,
            int missThreshold = DefaultMissThreshold)
        {
            MaxTime = maxTime;
            RemainingTime = maxTime;
            MissPenalty = missPenalty;
            MissThreshold = missThreshold;
            ConsecutiveMisses = 0;
            IsPaused = false;
        }

        public void ResetTimer()
        {
            RemainingTime = MaxTime;
        }

        /// <summary>
        /// Updates the real-time pressure countdown. Returns true if pressure expired, triggering a row drop.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            if (IsPaused || deltaTime <= 0f) return false;

            RemainingTime -= deltaTime;
            if (RemainingTime <= 0f)
            {
                ResetTimer();
                return true; // Row descent triggered
            }

            return false;
        }

        /// <summary>
        /// Applies penalty for an unsuccessful shot (no match).
        /// Returns true if consecutive misses or timer reduction triggers an immediate row drop.
        /// </summary>
        public bool ApplyMiss()
        {
            ConsecutiveMisses++;
            RemainingTime -= MissPenalty;

            if (ConsecutiveMisses >= MissThreshold || RemainingTime <= 0f)
            {
                ResetTimer();
                ConsecutiveMisses = 0;
                return true; // Force row drop
            }

            return false;
        }

        /// <summary>
        /// Restores pressure time upon successful group match and falling detached clusters.
        /// </summary>
        public void ApplyMatchRelief(int matchedCount, int detachedCount)
        {
            ConsecutiveMisses = 0;

            if (matchedCount < 3 && detachedCount <= 0) return;

            float relief = 1.0f;
            if (matchedCount > 3)
            {
                relief += (matchedCount - 3) * 0.5f;
            }
            if (detachedCount > 0)
            {
                relief += detachedCount * 0.4f;
            }

            RemainingTime = MathF.Min(RemainingTime + relief, MaxTime);
        }
    }
}
