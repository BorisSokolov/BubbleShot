using UnityEngine;
using UnityEngine.UI;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Placeholder for settings modal screen.
    /// </summary>
    public class SettingsScreenPlaceholder : ScreenView
    {
        [SerializeField] private Button? _closeButton;

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        public void OnCloseClicked()
        {
            GameLogger.LogInfo("SettingsScreen", "Closing settings screen.");
            Hide();
        }
    }
}
