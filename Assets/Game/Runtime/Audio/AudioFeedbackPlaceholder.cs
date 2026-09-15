using UnityEngine;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Audio
{
    /// <summary>
    /// Placeholder audio controller providing hooks for sound effects across gameplay events.
    /// </summary>
    public class AudioFeedbackPlaceholder : MonoBehaviour
    {
        public void PlayAimClick() => GameLogger.LogDebug("Audio", "SFX: Aim tick");
        public void PlayLaunch() => GameLogger.LogDebug("Audio", "SFX: Projectile launched");
        public void PlayWallBounce() => GameLogger.LogDebug("Audio", "SFX: Wall bounce");
        public void PlayAttach() => GameLogger.LogDebug("Audio", "SFX: Ball attached");
        public void PlayMatchPop(int count) => GameLogger.LogDebug("Audio", $"SFX: Match popped {count} balls");
        public void PlayClusterDrop(int count) => GameLogger.LogDebug("Audio", $"SFX: Dropped {count} detached balls");
        public void PlayRowDescent() => GameLogger.LogDebug("Audio", "SFX: Pressure row drop");
        public void PlayVictory() => GameLogger.LogDebug("Audio", "SFX: Victory fanfare");
        public void PlayDefeat() => GameLogger.LogDebug("Audio", "SFX: Defeat sting");
    }
}
