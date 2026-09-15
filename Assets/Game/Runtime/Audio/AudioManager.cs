using System;
using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Audio
{
    /// <summary>
    /// Programmatic audio synthesizer and playback manager for sound effects and ambient music.
    /// Uses procedurally synthesized audio clips, eliminating third-party asset dependencies.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager? Instance { get; private set; }

        [SerializeField] private AudioSource? _sfxSource;
        [SerializeField] private AudioSource? _musicSource;

        private const int SampleRate = 44100;

        private AudioClip? _clipAimClick;
        private AudioClip? _clipWallBounce;
        private AudioClip? _clipAttachSnap;
        private AudioClip? _clipBombExplosion;
        private AudioClip? _clipRowDescent;
        private AudioClip? _clipVictory;
        private AudioClip? _clipDefeat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSources();
            SynthesizeClips();
            StartAmbientMusic();
        }

        private void EnsureAudioSources()
        {
            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
            }

            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
            }
        }

        private void SynthesizeClips()
        {
            _clipAimClick = CreateTone(1200f, 0.025f, 0.3f);
            _clipWallBounce = CreateTone(240f, 0.05f, 0.4f);
            _clipAttachSnap = CreateTone(650f, 0.04f, 0.5f);
            _clipBombExplosion = CreateNoiseBurst(0.35f, 0.8f);
            _clipRowDescent = CreateTone(90f, 0.4f, 0.6f);
            _clipVictory = CreateFanfare();
            _clipDefeat = CreateDefeatSting();
        }

        public static float CalculateEffectiveVolume(float masterVolume, float channelVolume) =>
            AudioVolumeEvaluator.CalculateEffectiveVolume(masterVolume, channelVolume);

        public void PlayAimClick() => PlaySfx(_clipAimClick);
        public void PlayWallBounce() => PlaySfx(_clipWallBounce);
        public void PlayAttach() => PlaySfx(_clipAttachSnap);

        public void PlayMatchPop(int count)
        {
            // Melodic pentatonic pitch scaling (C5, D5, E5, G5, A5, C6...)
            float[] pentatonicFrequencies = { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f, 1046.50f };
            int idx = Mathf.Clamp(count - 3, 0, pentatonicFrequencies.Length - 1);
            float freq = pentatonicFrequencies[idx];

            var popClip = CreateTone(freq, 0.12f, 0.5f);
            PlaySfx(popClip);
        }

        public void PlayWildMatch(int count)
        {
            var wildClip = CreateTone(880f, 0.25f, 0.6f);
            PlaySfx(wildClip);
        }

        public void PlayBombExplosion(int count) => PlaySfx(_clipBombExplosion);
        public void PlayRowDescent() => PlaySfx(_clipRowDescent);
        public void PlayVictory() => PlaySfx(_clipVictory);
        public void PlayDefeat() => PlaySfx(_clipDefeat);

        private void PlaySfx(AudioClip? clip)
        {
            if (clip == null || _sfxSource == null) return;

            float master = SaveSystem.CurrentSave?.Settings.MasterVolume ?? 1.0f;
            float sfx = SaveSystem.CurrentSave?.Settings.SfxVolume ?? 1.0f;
            _sfxSource.volume = CalculateEffectiveVolume(master, sfx);
            _sfxSource.PlayOneShot(clip);
        }

        private void StartAmbientMusic()
        {
            if (_musicSource == null) return;

            var ambientClip = CreateAmbientDrone(4.0f);
            _musicSource.clip = ambientClip;

            float master = SaveSystem.CurrentSave?.Settings.MasterVolume ?? 1.0f;
            float music = SaveSystem.CurrentSave?.Settings.MusicVolume ?? 1.0f;
            _musicSource.volume = CalculateEffectiveVolume(master, music) * 0.4f;
            _musicSource.Play();
        }

        public void UpdateVolumes()
        {
            float master = SaveSystem.CurrentSave?.Settings.MasterVolume ?? 1.0f;
            float music = SaveSystem.CurrentSave?.Settings.MusicVolume ?? 1.0f;
            float sfx = SaveSystem.CurrentSave?.Settings.SfxVolume ?? 1.0f;

            if (_musicSource != null)
            {
                _musicSource.volume = CalculateEffectiveVolume(master, music) * 0.4f;
            }
            if (_sfxSource != null)
            {
                _sfxSource.volume = CalculateEffectiveVolume(master, sfx);
            }
        }

        private static AudioClip CreateTone(float frequency, float durationSeconds, float volume)
        {
            int totalSamples = (int)(SampleRate * durationSeconds);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = 1.0f - ((float)i / totalSamples); // Linear decay
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * volume;
            }

            var clip = AudioClip.Create($"Tone_{frequency}Hz", totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateNoiseBurst(float durationSeconds, float volume)
        {
            int totalSamples = (int)(SampleRate * durationSeconds);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float envelope = Mathf.Exp(-5.0f * ((float)i / totalSamples));
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float rumble = Mathf.Sin(2 * Mathf.PI * 65f * ((float)i / SampleRate));
                samples[i] = (noise * 0.6f + rumble * 0.4f) * envelope * volume;
            }

            var clip = AudioClip.Create("NoiseBurst", totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateFanfare()
        {
            float duration = 0.6f;
            int totalSamples = (int)(SampleRate * duration);
            float[] samples = new float[totalSamples];

            // C - E - G major arpeggio
            float[] chord = { 523.25f, 659.25f, 783.99f };
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Min((int)(t / 0.2f), chord.Length - 1);
                float freq = chord[noteIndex];
                float noteT = t % 0.2f;
                float env = 1f - (noteT / 0.2f);
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.5f;
            }

            var clip = AudioClip.Create("Fanfare", totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateDefeatSting()
        {
            float duration = 0.5f;
            int totalSamples = (int)(SampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float freq = Mathf.Lerp(300f, 120f, t / duration);
                float env = 1f - (t / duration);
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.5f;
            }

            var clip = AudioClip.Create("DefeatSting", totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateAmbientDrone(float durationSeconds)
        {
            int totalSamples = (int)(SampleRate * durationSeconds);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float d1 = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.4f; // A2
                float d2 = Mathf.Sin(2 * Mathf.PI * 164.81f * t) * 0.3f; // E3
                float d3 = Mathf.Sin(2 * Mathf.PI * 220f * t) * 0.2f; // A3
                samples[i] = (d1 + d2 + d3) * 0.3f;
            }

            var clip = AudioClip.Create("AmbientDrone", totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
