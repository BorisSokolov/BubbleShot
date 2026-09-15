using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// Factory providing curated, data-driven levels with progressive difficulty curves.
    /// </summary>
    public static class LevelCatalog
    {
        public static LevelDefinition GetLevel(int levelNumber)
        {
            int num = Math.Clamp(levelNumber, 1, 10);
            return num switch
            {
                1 => CreateLevel1(),
                2 => CreateLevel2(),
                3 => CreateLevel3(),
                4 => CreateLevel4(),
                5 => CreateLevel5(),
                6 => CreateLevel6(),
                7 => CreateLevel7(),
                8 => CreateLevel8(),
                9 => CreateLevel9(),
                10 => CreateLevel10(),
                _ => CreateLevel1()
            };
        }

        public static List<LevelDefinition> GetAllLevels()
        {
            var list = new List<LevelDefinition>(10);
            for (int i = 1; i <= 10; i++)
            {
                list.Add(GetLevel(i));
            }
            return list;
        }

        private static LevelDefinition CreateLevel1()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 1,
                LevelName = "First Steps",
                Seed = 10001,
                ObjectiveType = LevelObjectiveType.ClearAll,
                ActiveColorCount = 3,
                BasePressureTime = 15.0f,
                Star1Score = 1500,
                Star2Score = 3000,
                Star3Score = 5000
            };
            PopulateRows(lvl, 3, 3, 10001);
            return lvl;
        }

        private static LevelDefinition CreateLevel2()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 2,
                LevelName = "Triple Trio",
                Seed = 10002,
                ObjectiveType = LevelObjectiveType.ClearAll,
                ActiveColorCount = 3,
                BasePressureTime = 14.0f,
                Star1Score = 2000,
                Star2Score = 4000,
                Star3Score = 6500
            };
            PopulateRows(lvl, 4, 3, 10002);
            return lvl;
        }

        private static LevelDefinition CreateLevel3()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 3,
                LevelName = "Ceiling Sweep",
                Seed = 10003,
                ObjectiveType = LevelObjectiveType.ClearAll,
                ActiveColorCount = 3,
                BasePressureTime = 13.0f,
                Star1Score = 2500,
                Star2Score = 5000,
                Star3Score = 8000
            };
            PopulateRows(lvl, 4, 3, 10003);
            return lvl;
        }

        private static LevelDefinition CreateLevel4()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 4,
                LevelName = "Color Splash",
                Seed = 10004,
                ObjectiveType = LevelObjectiveType.ClearAnchor,
                ActiveColorCount = 4,
                BasePressureTime = 12.0f,
                Star1Score = 3000,
                Star2Score = 6000,
                Star3Score = 9500
            };
            PopulateRows(lvl, 4, 4, 10004);
            ReplaceBall(lvl, new HexCoord(2, 3), BallInfo.CreateBomb());
            ReplaceBall(lvl, new HexCoord(1, 4), BallInfo.CreateWild());
            return lvl;
        }

        private static LevelDefinition CreateLevel5()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 5,
                LevelName = "Pockets & Angles",
                Seed = 10005,
                ObjectiveType = LevelObjectiveType.ClearAnchor,
                ActiveColorCount = 4,
                BasePressureTime = 11.5f,
                Star1Score = 3500,
                Star2Score = 7000,
                Star3Score = 11000
            };
            PopulateRows(lvl, 5, 4, 10005);
            return lvl;
        }

        private static LevelDefinition CreateLevel6()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 6,
                LevelName = "Anchor Strike",
                Seed = 10006,
                ObjectiveType = LevelObjectiveType.ClearAnchor,
                ActiveColorCount = 4,
                BasePressureTime = 11.0f,
                Star1Score = 4000,
                Star2Score = 8000,
                Star3Score = 12500
            };
            PopulateRows(lvl, 5, 4, 10006);
            return lvl;
        }

        private static LevelDefinition CreateLevel7()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 7,
                LevelName = "Penta Prism",
                Seed = 10007,
                ObjectiveType = LevelObjectiveType.TargetScore,
                TargetScore = 6000,
                ActiveColorCount = 5,
                BasePressureTime = 10.0f,
                Star1Score = 4500,
                Star2Score = 9000,
                Star3Score = 14000
            };
            PopulateRows(lvl, 5, 5, 10007);
            return lvl;
        }

        private static LevelDefinition CreateLevel8()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 8,
                LevelName = "Score Rush",
                Seed = 10008,
                ObjectiveType = LevelObjectiveType.TargetScore,
                TargetScore = 8000,
                ActiveColorCount = 5,
                BasePressureTime = 9.5f,
                Star1Score = 5000,
                Star2Score = 10000,
                Star3Score = 16000
            };
            PopulateRows(lvl, 6, 5, 10008);
            ReplaceBall(lvl, new HexCoord(3, 2), BallInfo.CreateBomb());
            ReplaceBall(lvl, new HexCoord(3, 4), BallInfo.CreateBomb());
            ReplaceBall(lvl, new HexCoord(2, 3), BallInfo.CreateWild());
            return lvl;
        }

        private static LevelDefinition CreateLevel9()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 9,
                LevelName = "Hex Rainbow",
                Seed = 10009,
                ObjectiveType = LevelObjectiveType.SurviveRows,
                TargetRowsToSurvive = 5,
                ActiveColorCount = 6,
                BasePressureTime = 9.0f,
                Star1Score = 6000,
                Star2Score = 12000,
                Star3Score = 18000
            };
            PopulateRows(lvl, 6, 6, 10009);
            return lvl;
        }

        private static LevelDefinition CreateLevel10()
        {
            var lvl = new LevelDefinition
            {
                LevelNumber = 10,
                LevelName = "Master Crucible",
                Seed = 10010,
                ObjectiveType = LevelObjectiveType.SurviveRows,
                TargetRowsToSurvive = 7,
                ActiveColorCount = 6,
                BasePressureTime = 8.5f,
                Star1Score = 7000,
                Star2Score = 14000,
                Star3Score = 22000
            };
            PopulateRows(lvl, 6, 6, 10010);
            return lvl;
        }

        private static void ReplaceBall(LevelDefinition lvl, HexCoord coord, BallInfo newBall)
        {
            for (int i = 0; i < lvl.StartingBalls.Count; i++)
            {
                if (lvl.StartingBalls[i].Coord == coord)
                {
                    lvl.StartingBalls[i] = new HexCoordBallPair(coord, newBall);
                    return;
                }
            }
            lvl.StartingBalls.Add(new HexCoordBallPair(coord, newBall));
        }

        private static void PopulateRows(LevelDefinition lvl, int rowCount, int colorCount, uint seed)
        {
            var rng = new DeterministicRng(seed);
            for (int r = 0; r < rowCount; r++)
            {
                int cols = (r & 1) == 0 ? BoardGeometry.DefaultEvenWidth : BoardGeometry.DefaultOddWidth;
                for (int c = 0; c < cols; c++)
                {
                    var color = rng.NextColor(colorCount);
                    lvl.StartingBalls.Add(new HexCoordBallPair(new HexCoord(r, c), BallInfo.CreateNormal(color)));
                }
            }
        }
    }
}
