using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BubbleShot.UI.HUD
{
    /// <summary>
    /// Displays real-time pressure countdown gauge, miss indicators, score, combo multiplier, and pause button.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private Image? _pressureBarFill;
        [SerializeField] private TextMeshProUGUI? _scoreText;
        [SerializeField] private TextMeshProUGUI? _comboText;
        [SerializeField] private Image[]? _missPips;
        [SerializeField] private Button? _pauseButton;

        public event Action? PauseRequested;

        private void OnEnable()
        {
            if (_pauseButton != null) _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void OnDisable()
        {
            if (_pauseButton != null) _pauseButton.onClick.RemoveListener(OnPauseClicked);
        }

        public void UpdatePressure(float remainingTime, float maxTime)
        {
            if (_pressureBarFill != null && maxTime > 0f)
            {
                float ratio = Mathf.Clamp01(remainingTime / maxTime);
                _pressureBarFill.fillAmount = ratio;

                // Color tint: green when full, orange when half, red when critical
                _pressureBarFill.color = ratio > 0.5f
                    ? Color.Lerp(new Color(1f, 0.8f, 0f), new Color(0.2f, 0.8f, 0.3f), (ratio - 0.5f) * 2f)
                    : Color.Lerp(new Color(1f, 0.2f, 0.2f), new Color(1f, 0.8f, 0f), ratio * 2f);
            }
        }

        public void UpdateMisses(int consecutiveMisses)
        {
            if (_missPips == null) return;

            for (int i = 0; i < _missPips.Length; i++)
            {
                if (_missPips[i] != null)
                {
                    _missPips[i].color = i < consecutiveMisses
                        ? new Color(1f, 0.2f, 0.2f, 1f)
                        : new Color(0.3f, 0.3f, 0.3f, 0.4f);
                }
            }
        }

        public void UpdateScore(int score, float comboMultiplier)
        {
            if (_scoreText != null)
            {
                _scoreText.text = score.ToString("N0");
            }

            if (_comboText != null)
            {
                if (comboMultiplier > 1.0f)
                {
                    _comboText.text = $"{comboMultiplier:F1}x COMBO!";
                    _comboText.gameObject.SetActive(true);
                }
                else
                {
                    _comboText.gameObject.SetActive(false);
                }
            }
        }

        private void OnPauseClicked()
        {
            PauseRequested?.Invoke();
        }
    }
}
