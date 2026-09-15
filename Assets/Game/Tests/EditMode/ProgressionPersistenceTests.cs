using System;
using System.IO;
using NUnit.Framework;
using BubbleShot.Core;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class ProgressionPersistenceTests
    {
        private string _testDir = null!;
        private string _testSavePath = null!;

        [SetUp]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "BubbleShotTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _testSavePath = Path.Combine(_testDir, "savegame.json");
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testDir))
            {
                try { Directory.Delete(_testDir, true); } catch { }
            }
        }

        #region 1. SaveData & Progression Logic

        [Test]
        public void SaveData_DefaultInitialization_UnlocksOnlyLevel1()
        {
            var save = new SaveDataV1();

            Assert.That(save.Version, Is.EqualTo(1));
            Assert.That(save.HighestUnlockedLevel, Is.EqualTo(1));
            Assert.That(save.IsLevelUnlocked(1), Is.True);
            Assert.That(save.IsLevelUnlocked(2), Is.False);
            Assert.That(save.Settings.SfxVolume, Is.EqualTo(1.0f));
            Assert.That(save.Settings.HapticsEnabled, Is.True);
        }

        [Test]
        public void SaveData_RecordLevelComplete_UnlocksNextLevelAndUpdatesStats()
        {
            var save = new SaveDataV1();

            save.RecordLevelComplete(levelNumber: 1, score: 5500, stars: 3);

            Assert.That(save.IsLevelUnlocked(2), Is.True);
            Assert.That(save.HighestUnlockedLevel, Is.EqualTo(2));
            Assert.That(save.Records[1].HighScore, Is.EqualTo(5500));
            Assert.That(save.Records[1].StarsEarned, Is.EqualTo(3));
            Assert.That(save.Stats.TotalVictories, Is.EqualTo(1));

            // Completing with lower score does not overwrite higher score
            save.RecordLevelComplete(levelNumber: 1, score: 3000, stars: 2);
            Assert.That(save.Records[1].HighScore, Is.EqualTo(5500));
            Assert.That(save.Records[1].StarsEarned, Is.EqualTo(3));
        }

        #endregion

        #region 2. Atomic Persistence & Corrupt Recovery

        [Test]
        public void SaveSystem_SaveAtomicAndLoadSafe_PreservesDataAccurately()
        {
            var original = new SaveDataV1();
            original.RecordLevelComplete(1, 4200, 2);
            original.Settings.SfxVolume = 0.6f;
            original.Settings.ReducedMotionEnabled = true;

            bool saved = SaveSystem.SaveAtomic(_testSavePath, original);
            Assert.That(saved, Is.True);
            Assert.That(File.Exists(_testSavePath), Is.True);

            var loaded = SaveSystem.LoadSafe(_testSavePath);
            Assert.That(loaded.HighestUnlockedLevel, Is.EqualTo(2));
            Assert.That(loaded.Records[1].HighScore, Is.EqualTo(4200));
            Assert.That(loaded.Settings.SfxVolume, Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(loaded.Settings.ReducedMotionEnabled, Is.True);
        }

        [Test]
        public void SaveSystem_CorruptFile_RecoversSafelyWithBackup()
        {
            // Write corrupt invalid JSON
            File.WriteAllText(_testSavePath, "{ invalid json corrupt content !!!@#$");

            var loaded = SaveSystem.LoadSafe(_testSavePath);

            // Must return valid default save data without crashing
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.HighestUnlockedLevel, Is.EqualTo(1));

            // Must have created a .corrupt backup file
            string[] backupFiles = Directory.GetFiles(_testDir, "*.corrupt.*");
            Assert.That(backupFiles.Length, Is.GreaterThanOrEqualTo(1));
        }

        #endregion

        #region 3. Level Definitions & Star Ratings

        [Test]
        public void LevelDefinition_StarCalculation_MatchesThresholds()
        {
            var lvl = new LevelDefinition
            {
                Star1Score = 1000,
                Star2Score = 2500,
                Star3Score = 5000
            };

            Assert.That(lvl.CalculateStars(500), Is.EqualTo(1));   // Base victory
            Assert.That(lvl.CalculateStars(1500), Is.EqualTo(1));
            Assert.That(lvl.CalculateStars(2500), Is.EqualTo(2));
            Assert.That(lvl.CalculateStars(4999), Is.EqualTo(2));
            Assert.That(lvl.CalculateStars(5000), Is.EqualTo(3));
            Assert.That(lvl.CalculateStars(10000), Is.EqualTo(3));
        }

        [Test]
        public void LevelCatalog_All10Levels_AreValidAndProgressive()
        {
            var levels = LevelCatalog.GetAllLevels();
            Assert.That(levels.Count, Is.EqualTo(10));

            for (int i = 0; i < levels.Count; i++)
            {
                var lvl = levels[i];
                Assert.That(lvl.LevelNumber, Is.EqualTo(i + 1));
                Assert.That(lvl.ActiveColorCount, Is.InRange(3, 6));
                Assert.That(lvl.BasePressureTime, Is.InRange(8.0f, 15.0f));
                Assert.That(lvl.StartingBalls.Count, Is.GreaterThan(0));
                Assert.That(lvl.Star3Score, Is.GreaterThan(lvl.Star2Score));
                Assert.That(lvl.Star2Score, Is.GreaterThan(lvl.Star1Score));
            }

            // Verify difficulty progression: Level 10 has more colors than Level 1
            Assert.That(levels[9].ActiveColorCount, Is.GreaterThan(levels[0].ActiveColorCount));
            // Level 10 has stricter pressure timer than Level 1
            Assert.That(levels[9].BasePressureTime, Is.LessThan(levels[0].BasePressureTime));
        }

        [Test]
        public void SaveData_SeenTutorials_TracksAndSerializesCorrectly()
        {
            var save = new SaveDataV1();
            Assert.That(save.SeenTutorials, Is.Empty);

            save.SeenTutorials.Add(1);
            save.SeenTutorials.Add(2);
            save.SeenTutorials.Add(4);

            SaveSystem.SaveAtomic(_testSavePath, save);
            var loaded = SaveSystem.LoadSafe(_testSavePath);

            Assert.That(loaded.SeenTutorials, Has.Count.EqualTo(3));
            Assert.That(loaded.SeenTutorials, Does.Contain(1));
            Assert.That(loaded.SeenTutorials, Does.Contain(2));
            Assert.That(loaded.SeenTutorials, Does.Contain(4));
            Assert.That(loaded.SeenTutorials, Does.Not.Contain(3));
        }

        #endregion
    }
}
