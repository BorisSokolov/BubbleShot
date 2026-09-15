using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleShot.Core;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Level selection grid displaying 10 levels with star ratings and locked states.
    /// </summary>
    public class LevelSelectScreen : ScreenView
    {
        [SerializeField] private Button? _backButton;
        [SerializeField] private Transform? _buttonsContainer;

        public static int SelectedLevelNumber { get; set; } = 1;

        private void OnEnable()
        {
            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);
            RefreshLevelGrid();
        }

        private void OnDisable()
        {
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackClicked);
        }

        public void RefreshLevelGrid()
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            var save = SaveSystem.LoadSafe(savePath);

            var levels = LevelCatalog.GetAllLevels();
            for (int i = 0; i < levels.Count; i++)
            {
                var lvl = levels[i];
                bool isUnlocked = save.IsLevelUnlocked(lvl.LevelNumber);
                int stars = save.Records.TryGetValue(lvl.LevelNumber, out var rec) ? rec.StarsEarned : 0;

                GameLogger.LogDebug("LevelSelect", $"Level {lvl.LevelNumber}: Unlocked={isUnlocked}, Stars={stars}");
            }
        }

        public void OnLevelSelected(int levelNumber)
        {
            SelectedLevelNumber = levelNumber;
            GameLogger.LogInfo("LevelSelect", $"Level {levelNumber} selected -> loading Gameplay scene.");
            SceneNavigator.LoadScene(GameScene.Gameplay);
        }

        private void OnBackClicked()
        {
            Hide();
        }
    }
}
