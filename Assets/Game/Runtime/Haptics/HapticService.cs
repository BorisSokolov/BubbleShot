using UnityEngine;
using BubbleShot.Core;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Haptics
{
    /// <summary>
    /// Tactile feedback service managing device vibrations with respect to user settings.
    /// </summary>
    public static class HapticService
    {
        public static bool IsHapticsEnabled => SaveSystem.CurrentSave?.Settings.HapticsEnabled ?? true;

        public static void TriggerLightTap()
        {
            if (!IsHapticsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Light tap");
#if UNITY_ANDROID || UNITY_IOS
            if (SystemInfo.supportsVibration) Handheld.Vibrate();
#endif
        }

        public static void TriggerMediumPulse()
        {
            if (!IsHapticsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Medium pulse");
#if UNITY_ANDROID || UNITY_IOS
            if (SystemInfo.supportsVibration) Handheld.Vibrate();
#endif
        }

        public static void TriggerHeavyPulse()
        {
            if (!IsHapticsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Heavy pulse");
#if UNITY_ANDROID || UNITY_IOS
            if (SystemInfo.supportsVibration) Handheld.Vibrate();
#endif
        }
    }
}
