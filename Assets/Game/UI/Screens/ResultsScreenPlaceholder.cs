using UnityEngine;
using UnityEngine.UI;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Placeholder for victory/defeat game over screen.
    /// </summary>
    public class ResultsScreenPlaceholder : ScreenView
    {
        [SerializeField] private Button? _restartButton;
        [SerializeField] private Button? _mainMenuButton;

        private void OnEnable()
        {
            if (_restartButton != null) _restartButton.onClick.AddListener(OnRestartClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            if (_restartButton != null) _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void OnRestartClicked()
        {
            GameLogger.LogInfo("ResultsScreen", "Restarting Gameplay.");
            SceneNavigator.LoadScene(GameScene.Gameplay);
        }

        public void OnMainMenuClicked()
        {
            GameLogger.LogInfo("ResultsScreen", "Returning to MainMenu.");
            SceneNavigator.LoadScene(GameScene.MainMenu);
        }
    }
}
