using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleShot.Core;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Tutorial
{
    /// <summary>
    /// Contextual tutorial dialog introducing core gameplay, wall bounces,
    /// and special balls on key campaign levels.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject? _panel;
        [SerializeField] private TextMeshProUGUI? _titleText;
        [SerializeField] private TextMeshProUGUI? _messageText;
        [SerializeField] private Button? _dismissButton;

        private GameplayController? _controller;
        private int _currentLevelNumber;

        private void Awake()
        {
            if (_dismissButton != null)
            {
                _dismissButton.onClick.AddListener(Dismiss);
            }
            Hide();
        }

        public static string? GetTutorialMessageForLevel(int levelNumber)
        {
            return levelNumber switch
            {
                1 => "Aim & Drag to target bubbles! Connect 3 or more of the same color to pop them before the danger line approaches.",
                2 => "Bank your shots off side walls to navigate around blockers and reach high ceiling clusters!",
                4 => "Special Balls unlocked! Bombs blast all 6 surrounding bubbles; Wilds match every adjacent color simultaneously.",
                _ => null
            };
        }

        public static string GetTutorialTitleForLevel(int levelNumber)
        {
            return levelNumber switch
            {
                1 => "Welcome to BubbleShot!",
                2 => "Wall Bounces",
                4 => "Special Balls: Bomb & Wild",
                _ => "How to Play"
            };
        }

        public bool CheckAndShow(int levelNumber, GameplayController controller)
        {
            _controller = controller;
            _currentLevelNumber = levelNumber;

            string? message = GetTutorialMessageForLevel(levelNumber);
            if (string.IsNullOrEmpty(message))
            {
                Hide();
                return false;
            }

            var save = SaveSystem.CurrentSave;
            if (save != null && save.SeenTutorials.Contains(levelNumber))
            {
                Hide();
                return false;
            }

            Show(GetTutorialTitleForLevel(levelNumber), message);
            return true;
        }

        public void Show(string title, string message)
        {
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;

            if (_panel != null) _panel.SetActive(true);
            else gameObject.SetActive(true);

            _controller?.SetPause(true);
        }

        public void Dismiss()
        {
            var save = SaveSystem.CurrentSave;
            if (save != null && !save.SeenTutorials.Contains(_currentLevelNumber))
            {
                save.SeenTutorials.Add(_currentLevelNumber);
                SaveSystem.Save();
            }

            Hide();
            _controller?.SetPause(false);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
