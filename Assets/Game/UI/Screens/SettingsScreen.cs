using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BubbleShot.Core;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Interactive settings screen managing audio volume sliders and accessibility toggles.
    /// </summary>
    public class SettingsScreen : ScreenView
    {
        [SerializeField] private Slider? _sfxSlider;
        [SerializeField] private Slider? _musicSlider;
        [SerializeField] private Toggle? _hapticsToggle;
        [SerializeField] private Toggle? _reducedMotionToggle;
        [SerializeField] private Toggle? _colorBlindRunesToggle;
        [SerializeField] private Button? _closeButton;

        private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
            LoadSettings();

            if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            if (_musicSlider != null) _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (_hapticsToggle != null) _hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
            if (_reducedMotionToggle != null) _reducedMotionToggle.onValueChanged.AddListener(OnReducedMotionChanged);
            if (_colorBlindRunesToggle != null) _colorBlindRunesToggle.onValueChanged.AddListener(OnColorBlindRunesChanged);
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (_hapticsToggle != null) _hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
            if (_reducedMotionToggle != null) _reducedMotionToggle.onValueChanged.RemoveListener(OnReducedMotionChanged);
            if (_colorBlindRunesToggle != null) _colorBlindRunesToggle.onValueChanged.RemoveListener(OnColorBlindRunesChanged);
        }

        private void LoadSettings()
        {
            var save = SaveSystem.LoadSafe(SavePath);
            var s = save.Settings;

            if (_sfxSlider != null) _sfxSlider.value = s.SfxVolume;
            if (_musicSlider != null) _musicSlider.value = s.MusicVolume;
            if (_hapticsToggle != null) _hapticsToggle.isOn = s.HapticsEnabled;
            if (_reducedMotionToggle != null) _reducedMotionToggle.isOn = s.ReducedMotionEnabled;
            if (_colorBlindRunesToggle != null) _colorBlindRunesToggle.isOn = s.ColorBlindRunesEnabled;
        }

        private void ModifyAndSave(Action<UserSettings> modify)
        {
            var save = SaveSystem.LoadSafe(SavePath);
            modify(save.Settings);
            SaveSystem.SaveAtomic(SavePath, save);
            GameLogger.LogDebug("Settings", "User settings persisted.");
        }

        private void OnSfxChanged(float val) => ModifyAndSave(s => s.SfxVolume = val);
        private void OnMusicChanged(float val) => ModifyAndSave(s => s.MusicVolume = val);
        private void OnHapticsChanged(bool val) => ModifyAndSave(s => s.HapticsEnabled = val);
        private void OnReducedMotionChanged(bool val) => ModifyAndSave(s => s.ReducedMotionEnabled = val);
        private void OnColorBlindRunesChanged(bool val) => ModifyAndSave(s => s.ColorBlindRunesEnabled = val);

        private void OnCloseClicked()
        {
            Hide();
        }
    }
}
