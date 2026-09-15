using NUnit.Framework;
using BubbleShot.Core;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class PolishAndUXTests
    {
        [Test]
        public void ObjectPool_RentAndReturn_ReusesInstancesWithoutAllocating()
        {
            int createdCount = 0;
            var pool = new ObjectPool<string>(
                createFunc: () => $"Item_{++createdCount}",
                initialCapacity: 4,
                maxCapacity: 8);

            var item1 = pool.Rent();
            var item2 = pool.Rent();

            Assert.That(item1, Is.EqualTo("Item_1"));
            Assert.That(item2, Is.EqualTo("Item_2"));
            Assert.That(pool.CountActive, Is.EqualTo(2));
            Assert.That(pool.CountInactive, Is.EqualTo(0));

            pool.Return(item1);
            Assert.That(pool.CountActive, Is.EqualTo(1));
            Assert.That(pool.CountInactive, Is.EqualTo(1));

            // Rented again should return recycled item1 without calling createFunc
            var recycled = pool.Rent();
            Assert.That(recycled, Is.EqualTo("Item_1"));
            Assert.That(createdCount, Is.EqualTo(2), "Should reuse recycled instance without invoking createFunc");
            Assert.That(pool.CountActive, Is.EqualTo(2));
        }

        [Test]
        public void ObjectPool_Prewarm_PreparesExactCapacity()
        {
            var pool = new ObjectPool<int>(
                createFunc: () => 42,
                initialCapacity: 10,
                maxCapacity: 20);

            pool.Prewarm(7);
            Assert.That(pool.CountInactive, Is.EqualTo(7));
            Assert.That(pool.CountActive, Is.EqualTo(0));

            var item = pool.Rent();
            Assert.That(item, Is.EqualTo(42));
            Assert.That(pool.CountInactive, Is.EqualTo(6));
            Assert.That(pool.CountActive, Is.EqualTo(1));
        }

        [Test]
        public void ComboCallout_ThresholdEvaluation_ReturnsExpectedRank()
        {
            Assert.That(ComboCalloutEvaluator.GetCalloutText(1.0f), Is.Empty);
            Assert.That(ComboCalloutEvaluator.GetCalloutText(1.4f), Is.Empty);
            Assert.That(ComboCalloutEvaluator.GetCalloutText(1.5f), Is.EqualTo("Good!"));
            Assert.That(ComboCalloutEvaluator.GetCalloutText(2.0f), Is.EqualTo("Great!"));
            Assert.That(ComboCalloutEvaluator.GetCalloutText(2.5f), Is.EqualTo("Super!"));
            Assert.That(ComboCalloutEvaluator.GetCalloutText(3.0f), Is.EqualTo("Mega Combo!"));
            Assert.That(ComboCalloutEvaluator.GetCalloutText(3.5f), Is.EqualTo("Unstoppable!"));
            Assert.That(ComboCalloutEvaluator.GetCalloutText(4.0f), Is.EqualTo("Unstoppable!"));
        }

        [Test]
        public void Audio_VolumeCalculation_ClampsAndMultipliesCorrectly()
        {
            // Standard volumes
            Assert.That(AudioVolumeEvaluator.CalculateEffectiveVolume(0.8f, 0.5f), Is.EqualTo(0.4f).Within(1e-5f));

            // Muted master
            Assert.That(AudioVolumeEvaluator.CalculateEffectiveVolume(0.0f, 1.0f), Is.EqualTo(0.0f));

            // Out-of-bounds clamping
            Assert.That(AudioVolumeEvaluator.CalculateEffectiveVolume(1.5f, 1.0f), Is.EqualTo(1.0f).Within(1e-5f));
            Assert.That(AudioVolumeEvaluator.CalculateEffectiveVolume(-0.2f, 0.5f), Is.EqualTo(0.0f));
        }

        [Test]
        public void SafeArea_Calculation_ClampsAnchorsWithinZeroToOne()
        {
            // Standard full screen without notches
            var (minFull, maxFull) = SafeAreaEvaluator.CalculateNormalizedAnchors(0f, 0f, 1080f, 1920f, 1080f, 1920f);
            Assert.That(minFull.X, Is.EqualTo(0f));
            Assert.That(minFull.Y, Is.EqualTo(0f));
            Assert.That(maxFull.X, Is.EqualTo(1f));
            Assert.That(maxFull.Y, Is.EqualTo(1f));

            // Notched phone: top notch 80px, bottom bar 40px (safe area height 2280 on 2400 screen)
            var (minNotch, maxNotch) = SafeAreaEvaluator.CalculateNormalizedAnchors(0f, 40f, 1080f, 2280f, 1080f, 2400f);
            Assert.That(minNotch.X, Is.EqualTo(0f));
            Assert.That(minNotch.Y, Is.EqualTo(40f / 2400f).Within(1e-5f));
            Assert.That(maxNotch.X, Is.EqualTo(1f));
            Assert.That(maxNotch.Y, Is.EqualTo((40f + 2280f) / 2400f).Within(1e-5f));
        }

        [Test]
        public void DangerThreshold_Evaluation_DetectsRowsAtOrAboveNine()
        {
            var geometry = new BoardGeometry(evenWidth: 8, maxRows: 12, dangerRow: 11);
            var board = new HexBoard(geometry);

            // Safe rows 0 to 8
            board.SetBall(new HexCoord(0, 0), BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(new HexCoord(4, 2), BallInfo.CreateNormal(BallColor.Blue));
            board.SetBall(new HexCoord(8, 3), BallInfo.CreateNormal(BallColor.Green));

            bool inDangerBefore = false;
            var occupied1 = board.GetOccupiedCoords();
            for (int i = 0; i < occupied1.Count; i++)
            {
                if (occupied1[i].Row >= 9) inDangerBefore = true;
            }
            Assert.That(inDangerBefore, Is.False, "Row 8 should not trigger danger warning pulse");

            // Danger row 9 reached
            board.SetBall(new HexCoord(9, 2), BallInfo.CreateNormal(BallColor.Yellow));

            bool inDangerAfter = false;
            var occupied2 = board.GetOccupiedCoords();
            for (int i = 0; i < occupied2.Count; i++)
            {
                if (occupied2[i].Row >= 9) inDangerAfter = true;
            }
            Assert.That(inDangerAfter, Is.True, "Row 9 reaching danger zone must trigger warning pulse");
        }

        [Test]
        public void SaveSettings_Defaults_IncludeRunesAndHaptics()
        {
            var settings = new UserSettings();

            Assert.That(settings.ColorBlindRunes, Is.True, "Color-blind runes must be enabled by default");
            Assert.That(settings.HapticsEnabled, Is.True, "Haptics must be enabled by default");
            Assert.That(settings.ReducedMotion, Is.False, "Reduced motion should default to false");
            Assert.That(settings.MasterVolume, Is.EqualTo(1.0f));
        }
    }
}
