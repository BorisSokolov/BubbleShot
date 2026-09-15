using UnityEngine;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Audio
{
    /// <summary>
    /// Placeholder audio controller providing hooks for sound effects across gameplay events.
    /// </summary>
    public class AudioFeedbackPlaceholder : MonoBehaviour
    {
        public void PlayAimClick()
        {
            AudioManager.Instance?.PlayAimClick();
            GameLogger.LogDebug("Audio", "SFX: Aim tick");
        }

        public void PlayLaunch()
        {
            GameLogger.LogDebug("Audio", "SFX: Projectile launched");
        }

        public void PlayWallBounce()
        {
            AudioManager.Instance?.PlayWallBounce();
            GameLogger.LogDebug("Audio", "SFX: Wall bounce");
        }

        public void PlayAttach()
        {
            AudioManager.Instance?.PlayAttach();
            GameLogger.LogDebug("Audio", "SFX: Ball attached");
        }

        public void PlayMatchPop(int count)
        {
            AudioManager.Instance?.PlayMatchPop(count);
            GameLogger.LogDebug("Audio", $"SFX: Match popped {count} balls");
        }

        public void PlayBombExplosion(int count)
        {
            AudioManager.Instance?.PlayBombExplosion(count);
            GameLogger.LogDebug("Audio", $"SFX: Bomb explosion blasted {count} balls");
        }

        public void PlayWildMatch(int count)
        {
            AudioManager.Instance?.PlayWildMatch(count);
            GameLogger.LogDebug("Audio", $"SFX: Wild multi-color popped {count} balls");
        }

        public void PlayClusterDrop(int count)
        {
            GameLogger.LogDebug("Audio", $"SFX: Dropped {count} detached balls");
        }

        public void PlayRowDescent()
        {
            AudioManager.Instance?.PlayRowDescent();
            GameLogger.LogDebug("Audio", "SFX: Pressure row drop");
        }

        public void PlayVictory()
        {
            AudioManager.Instance?.PlayVictory();
            GameLogger.LogDebug("Audio", "SFX: Victory fanfare");
        }

        public void PlayDefeat()
        {
            AudioManager.Instance?.PlayDefeat();
            GameLogger.LogDebug("Audio", "SFX: Defeat sting");
        }
    }
}
