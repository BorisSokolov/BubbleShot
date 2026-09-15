using System.Collections.Generic;
using NUnit.Framework;
using BubbleShot.Core;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class SpecialBallTests
    {
        [Test]
        public void Bomb_DetonatesCenterAndAllSixNeighbors_WhenSurrounded()
        {
            var board = new HexBoard();
            var center = new HexCoord(2, 3);
            board.SetBall(center, BallInfo.CreateBomb());

            var neighbors = board.GetNeighbors(center);
            Assert.That(neighbors.Count, Is.EqualTo(6), "Expected 6 neighbors on open hex board");

            for (int i = 0; i < neighbors.Count; i++)
            {
                board.SetBall(neighbors[i], BallInfo.CreateNormal(BallColor.Blue));
            }

            var blasted = MatchResolver.ResolveBombBlast(board, center);

            Assert.That(blasted.Count, Is.EqualTo(7), "Bomb should blast itself + 6 neighbors");
            Assert.That(blasted.Contains(center), Is.True);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Assert.That(blasted.Contains(neighbors[i]), Is.True);
            }
        }

        [Test]
        public void Bomb_RespectsBoardBoundaries_OnCeilingAndWalls()
        {
            var board = new HexBoard();
            var corner = new HexCoord(0, 0);
            board.SetBall(corner, BallInfo.CreateBomb());

            var validNeighbors = board.GetNeighbors(corner);
            for (int i = 0; i < validNeighbors.Count; i++)
            {
                board.SetBall(validNeighbors[i], BallInfo.CreateNormal(BallColor.Red));
            }

            var blasted = MatchResolver.ResolveBombBlast(board, corner);

            Assert.That(blasted.Count, Is.EqualTo(1 + validNeighbors.Count));
            Assert.That(blasted.Contains(corner), Is.True);
            for (int i = 0; i < validNeighbors.Count; i++)
            {
                Assert.That(blasted.Contains(validNeighbors[i]), Is.True);
            }
        }

        [Test]
        public void Bomb_ChainReaction_DetonatesAdjacentBombs()
        {
            var board = new HexBoard();
            var bomb1 = new HexCoord(2, 2);
            var bomb2 = new HexCoord(2, 3); // adjacent
            var farBall = new HexCoord(2, 4); // adjacent to bomb2, not adjacent to bomb1

            board.SetBall(bomb1, BallInfo.CreateBomb());
            board.SetBall(bomb2, BallInfo.CreateBomb());
            board.SetBall(farBall, BallInfo.CreateNormal(BallColor.Green));

            var blasted = MatchResolver.ResolveBombBlast(board, bomb1);

            Assert.That(blasted.Contains(bomb1), Is.True, "Bomb1 must be destroyed");
            Assert.That(blasted.Contains(bomb2), Is.True, "Bomb2 must chain detonate");
            Assert.That(blasted.Contains(farBall), Is.True, "FarBall caught in Bomb2 blast must be destroyed");
        }

        [Test]
        public void Bomb_DetachesHangingClusters()
        {
            var board = new HexBoard();
            // Ceiling anchor
            board.SetBall(new HexCoord(0, 3), BallInfo.CreateNormal(BallColor.Yellow));
            // Support bridge ball
            board.SetBall(new HexCoord(1, 3), BallInfo.CreateNormal(BallColor.Yellow));
            // Hanging cluster below bridge
            board.SetBall(new HexCoord(2, 3), BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(new HexCoord(2, 4), BallInfo.CreateNormal(BallColor.Red));

            // Detonate bomb at (1, 2) which is adjacent to (1, 3)
            board.SetBall(new HexCoord(1, 2), BallInfo.CreateBomb());
            var blasted = MatchResolver.ResolveBombBlast(board, new HexCoord(1, 2));

            for (int i = 0; i < blasted.Count; i++)
            {
                board.ClearCell(blasted[i]);
            }

            var detached = ClusterResolver.FindDisconnectedClusters(board);

            // The hanging red cluster at (2, 3) and (2, 4) should be disconnected from ceiling
            Assert.That(detached.Contains(new HexCoord(2, 3)) || !board.IsOccupied(new HexCoord(2, 3)), Is.True);
            Assert.That(detached.Contains(new HexCoord(2, 4)) || !board.IsOccupied(new HexCoord(2, 4)), Is.True);
        }

        [Test]
        public void Wild_MultiColor_PopsTwoDifferentColorGroupsSimultaneously()
        {
            var board = new HexBoard();
            var wildCoord = new HexCoord(2, 3);
            board.SetBall(wildCoord, BallInfo.CreateWild());

            // 2 Red balls adjacent to wild
            var r1 = new HexCoord(2, 2);
            var r2 = new HexCoord(1, 2);
            board.SetBall(r1, BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(r2, BallInfo.CreateNormal(BallColor.Red));

            // 2 Blue balls adjacent to wild
            var b1 = new HexCoord(2, 4);
            var b2 = new HexCoord(1, 4);
            board.SetBall(b1, BallInfo.CreateNormal(BallColor.Blue));
            board.SetBall(b2, BallInfo.CreateNormal(BallColor.Blue));

            var matches = MatchResolver.ResolveWildMatches(board, wildCoord);

            // Both groups qualify (Wild + 2 Red = 3, Wild + 2 Blue = 3) -> 5 balls total
            Assert.That(matches.Count, Is.EqualTo(5));
            Assert.That(matches.Contains(wildCoord), Is.True);
            Assert.That(matches.Contains(r1), Is.True);
            Assert.That(matches.Contains(r2), Is.True);
            Assert.That(matches.Contains(b1), Is.True);
            Assert.That(matches.Contains(b2), Is.True);
        }

        [Test]
        public void Wild_MultiColor_OnlyPopsQualifyingGroups_LeavesIneligibleNeighbors()
        {
            var board = new HexBoard();
            var wildCoord = new HexCoord(2, 3);
            board.SetBall(wildCoord, BallInfo.CreateWild());

            // 2 Red balls adjacent to wild (qualifies: Wild + 2 Red = 3)
            var r1 = new HexCoord(2, 2);
            var r2 = new HexCoord(1, 2);
            board.SetBall(r1, BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(r2, BallInfo.CreateNormal(BallColor.Red));

            // 1 Green ball adjacent to wild (does NOT qualify: Wild + 1 Green = 2 < 3)
            var g1 = new HexCoord(3, 3);
            board.SetBall(g1, BallInfo.CreateNormal(BallColor.Green));

            var matches = MatchResolver.ResolveWildMatches(board, wildCoord);

            Assert.That(matches.Count, Is.EqualTo(3));
            Assert.That(matches.Contains(wildCoord), Is.True);
            Assert.That(matches.Contains(r1), Is.True);
            Assert.That(matches.Contains(r2), Is.True);
            Assert.That(matches.Contains(g1), Is.False, "Ineligible green ball must not be popped");
        }

        [Test]
        public void Wild_ParksOnBoard_WhenNoNeighborColorReachesThree()
        {
            var board = new HexBoard();
            var wildCoord = new HexCoord(2, 3);
            board.SetBall(wildCoord, BallInfo.CreateWild());

            // 1 Red neighbor (Wild + 1 Red = 2 < 3)
            board.SetBall(new HexCoord(2, 2), BallInfo.CreateNormal(BallColor.Red));
            // 1 Blue neighbor (Wild + 1 Blue = 2 < 3)
            board.SetBall(new HexCoord(2, 4), BallInfo.CreateNormal(BallColor.Blue));

            var matches = MatchResolver.ResolveWildMatches(board, wildCoord);

            Assert.That(matches.Count, Is.EqualTo(0), "Wild ball must not pop if no color reaches 3");
        }

        [Test]
        public void Wild_ActsAsBridge_BetweenTwoSeparateGroupsOfSameColor()
        {
            var board = new HexBoard();
            var r1 = new HexCoord(2, 2);
            var r2 = new HexCoord(2, 4); // Separated by (2, 3)
            var wildCoord = new HexCoord(2, 3);

            board.SetBall(r1, BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(r2, BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(wildCoord, BallInfo.CreateWild());

            var matches = MatchResolver.ResolveWildMatches(board, wildCoord);

            Assert.That(matches.Count, Is.EqualTo(3), "Wild must bridge the two disconnected red balls into a match of 3");
            Assert.That(matches.Contains(r1), Is.True);
            Assert.That(matches.Contains(wildCoord), Is.True);
            Assert.That(matches.Contains(r2), Is.True);
        }

        [Test]
        public void Wild_ParkedOnBoard_ParticipatesInSubsequentMatches()
        {
            var board = new HexBoard();
            // Pre-park a wild ball and a red ball
            var r1 = new HexCoord(0, 0);
            var wild = new HexCoord(0, 1);
            board.SetBall(r1, BallInfo.CreateNormal(BallColor.Red));
            board.SetBall(wild, BallInfo.CreateWild());

            // Now place a second red ball at (0, 2) which connects through the parked wild to r1
            var r2 = new HexCoord(0, 2);
            board.SetBall(r2, BallInfo.CreateNormal(BallColor.Red));

            var matches = MatchResolver.FindMatchingGroup(board, r2);

            Assert.That(matches.Count, Is.EqualTo(3), "Parked wild should bridge normal matching group");
            Assert.That(matches.Contains(r1), Is.True);
            Assert.That(matches.Contains(wild), Is.True);
            Assert.That(matches.Contains(r2), Is.True);
        }

        [Test]
        public void AuthoritativeEngine_ConsecutiveMatches_AwardsSpecialBalls()
        {
            var engine = new AuthoritativeEngine(54321);

            Assert.That(engine.ConsecutiveMatches, Is.EqualTo(0));
            Assert.That(engine.GenerateNextProjectile().Type, Is.EqualTo(BallType.Normal));

            // Place permanent background balls on Row 0 so the board is never completely empty
            engine.Board.SetBall(new HexCoord(0, 7), BallInfo.CreateNormal(BallColor.Purple));

            // Match 1: Red at (1, 0), (1, 1), (1, 2)
            engine.Board.SetBall(new HexCoord(1, 0), BallInfo.CreateNormal(BallColor.Red));
            engine.Board.SetBall(new HexCoord(1, 1), BallInfo.CreateNormal(BallColor.Red));
            var origin = new Vector2D(engine.Board.Geometry.BoardRight * 0.5f, -10f);
            var target1 = engine.Board.Geometry.CoordToLocalPosition(new HexCoord(1, 2));
            var dir1 = (target1 - origin).Normalized;

            engine.ExecuteShot(origin, dir1, BallInfo.CreateNormal(BallColor.Red));
            Assert.That(engine.ConsecutiveMatches, Is.EqualTo(1));
            Assert.That(engine.GenerateNextProjectile().Type, Is.EqualTo(BallType.Normal));

            // Match 2: Blue at (1, 3), (1, 4), (1, 5)
            engine.Board.SetBall(new HexCoord(1, 3), BallInfo.CreateNormal(BallColor.Blue));
            engine.Board.SetBall(new HexCoord(1, 4), BallInfo.CreateNormal(BallColor.Blue));
            var target2 = engine.Board.Geometry.CoordToLocalPosition(new HexCoord(1, 5));
            var dir2 = (target2 - origin).Normalized;

            engine.ExecuteShot(origin, dir2, BallInfo.CreateNormal(BallColor.Blue));
            Assert.That(engine.ConsecutiveMatches, Is.EqualTo(2));

            // Match 3: Green at (2, 0), (2, 1), (2, 2)
            engine.Board.SetBall(new HexCoord(2, 0), BallInfo.CreateNormal(BallColor.Green));
            engine.Board.SetBall(new HexCoord(2, 1), BallInfo.CreateNormal(BallColor.Green));
            var target3 = engine.Board.Geometry.CoordToLocalPosition(new HexCoord(2, 2));
            var dir3 = (target3 - origin).Normalized;

            engine.ExecuteShot(origin, dir3, BallInfo.CreateNormal(BallColor.Green));
            Assert.That(engine.ConsecutiveMatches, Is.EqualTo(3));

            // At 3 consecutive matches, next projectile must be Bomb!
            var specialBall1 = engine.GenerateNextProjectile();
            Assert.That(specialBall1.Type, Is.EqualTo(BallType.Bomb), "3-combo milestone must award Bomb ball");
        }
    }
}
