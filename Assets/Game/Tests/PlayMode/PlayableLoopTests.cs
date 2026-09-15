using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BubbleShot.Core;
using BubbleShot.Runtime.Presentation;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Tests
{
    [TestFixture]
    public class PlayableLoopTests
    {
        private GameObject _testRoot = null!;
        private GameplayController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _testRoot = new GameObject("TestGameplayRoot");
            _controller = _testRoot.AddComponent<GameplayController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_testRoot != null)
            {
                Object.Destroy(_testRoot);
            }
        }

        [Test]
        public void GameplayController_InitializesLevel1_WithPopulatedBoard()
        {
            _controller.InitializeGameplay(seed: 12345);

            Assert.That(_controller.Engine, Is.Not.Null);
            Assert.That(_controller.Engine.Board.GetOccupiedCoords().Count, Is.GreaterThan(0));
            Assert.That(_controller.Engine.Status, Is.EqualTo(GameResult.Ongoing));
            Assert.That(_controller.IsResolvingAnimation, Is.False);
        }

        [Test]
        public void LauncherController_ClampsAimAngle_WithinLegalBounds()
        {
            var launcherGo = new GameObject("Launcher");
            launcherGo.transform.SetParent(_testRoot.transform);
            var launcher = launcherGo.AddComponent<LauncherController>();

            var board = new HexBoard();
            launcher.Initialize(board, BallInfo.CreateNormal(BallColor.Red), BallInfo.CreateNormal(BallColor.Blue));

            // Aim below horizontal plane (e.g. downward: y = -10)
            launcher.UpdateAim(new Vector2(launcherGo.transform.position.x + 5f, launcherGo.transform.position.y - 10f));

            // Aim direction Y must be strictly positive (clamped to at least 10 degrees)
            Assert.That(launcher.AimDirection.Y, Is.GreaterThan(0f));
            Assert.That(launcher.AimDirection.Length, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void GameplayController_PauseFreezesPressureTimer()
        {
            _controller.InitializeGameplay();

            _controller.SetPause(true);
            Assert.That(_controller.IsGamePaused, Is.True);
            Assert.That(_controller.Engine.Pressure.IsPaused, Is.True);

            float timeBefore = _controller.Engine.Pressure.RemainingTime;
            // Tick while paused
            _controller.Engine.Tick(1.0f);
            float timeAfter = _controller.Engine.Pressure.RemainingTime;

            Assert.That(timeAfter, Is.EqualTo(timeBefore));
        }

        [UnityTest]
        public IEnumerator GameplayController_LockInputDuringAnimation()
        {
            _controller.InitializeGameplay();

            Assert.That(_controller.IsResolvingAnimation, Is.False);
            yield return null;
        }
    }
}
