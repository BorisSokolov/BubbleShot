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
            HapticService.TriggerLightTap();
        }

        public void TriggerMediumPulse()
        {
            if (!IsEnabled) return;
            HapticService.TriggerMediumPulse();
        }

        public void TriggerHeavyPulse()
        {
            if (!IsEnabled) return;
            HapticService.TriggerHeavyPulse();
        }
    }
}
