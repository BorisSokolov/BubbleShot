using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    public class LevelRecord
    {
        public int LevelNumber { get; set; }
        public int StarsEarned { get; set; }
        public int HighScore { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class UserSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.8f;
        public bool HapticsEnabled { get; set; } = true;
        public bool ReducedMotionEnabled { get; set; } = false;
        public bool ColorBlindRunesEnabled { get; set; } = true;

        public bool ReducedMotion
        {
            get => ReducedMotionEnabled;
            set => ReducedMotionEnabled = value;
        }

        public bool ColorBlindRunes
        {
            get => ColorBlindRunesEnabled;
            set => ColorBlindRunesEnabled = value;
        }
    }

    public class SaveStatistics
    {
        public int TotalShotsFired { get; set; }
        public int TotalBallsPopped { get; set; }
        public int TotalClustersDropped { get; set; }
        public int TotalVictories { get; set; }
        public int TotalDefeats { get; set; }
    }

    /// <summary>
    /// Version 1 schema for local persistent game progress and settings.
    /// </summary>
    public class SaveDataV1
    {
        public int Version { get; set; } = 1;
        public int HighestUnlockedLevel { get; set; } = 1;
        public Dictionary<int, LevelRecord> Records { get; set; } = new Dictionary<int, LevelRecord>();
        public UserSettings Settings { get; set; } = new UserSettings();
        public SaveStatistics Stats { get; set; } = new SaveStatistics();

        public void RecordLevelComplete(int levelNumber, int score, int stars)
        {
            if (!Records.TryGetValue(levelNumber, out var record))
            {
                record = new LevelRecord { LevelNumber = levelNumber };
                Records[levelNumber] = record;
            }

            record.IsCompleted = true;
            if (stars > record.StarsEarned) record.StarsEarned = stars;
            if (score > record.HighScore) record.HighScore = score;

            // Unlock next level
            if (levelNumber >= HighestUnlockedLevel && levelNumber < 10)
            {
                HighestUnlockedLevel = levelNumber + 1;
            }

            Stats.TotalVictories++;
        }

        public bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= HighestUnlockedLevel;
        }
    }
}
