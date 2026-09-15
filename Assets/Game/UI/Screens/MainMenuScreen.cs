using UnityEngine;
using UnityEngine.UI;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Main menu navigation screen with Play, Settings, and Exit buttons.
    /// </summary>
    public class MainMenuScreen : ScreenView
    {
        [SerializeField] private Button? _playButton;
        [SerializeField] private Button? _settingsButton;

        private void OnEnable()
        {
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDisable()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        public void OnPlayClicked()
        {
            GameLogger.LogInfo("MainMenuScreen", "Play requested -> loading Gameplay scene.");
            SceneNavigator.LoadScene(GameScene.Gameplay);
        }

        public void OnSettingsClicked()
        {
            GameLogger.LogInfo("MainMenuScreen", "Settings requested.");
        }
    }
}
