using UnityEngine;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Haptics
{
    /// <summary>
    /// Placeholder haptics service triggering tactile mobile vibration feedback.
    /// </summary>
    public class HapticFeedbackPlaceholder : MonoBehaviour
    {
        public bool IsEnabled { get; set; } = true;

        public void TriggerLightPulse()
        {
            if (!IsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Light vibration (wall bounce)");
        }

        public void TriggerMediumPulse()
        {
            if (!IsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Medium vibration (match pop)");
        }

        public void TriggerHeavyPulse()
        {
            if (!IsEnabled) return;
            GameLogger.LogDebug("Haptics", "Haptic: Heavy vibration (row descent)");
        }
    }
}
