using System;

namespace BubbleShot.Core
{
    /// <summary>
    /// Pure C# mathematical evaluations for combo callout thresholds,
    /// audio volume curves, and safe area anchor normalizations.
    /// </summary>
    public static class ComboCalloutEvaluator
    {
        public static string GetCalloutText(float comboMultiplier)
        {
            if (comboMultiplier >= 3.5f) return "Unstoppable!";
            if (comboMultiplier >= 3.0f) return "Mega Combo!";
            if (comboMultiplier >= 2.5f) return "Super!";
            if (comboMultiplier >= 2.0f) return "Great!";
            if (comboMultiplier >= 1.5f) return "Good!";
            return string.Empty;
        }
    }

    public static class AudioVolumeEvaluator
    {
        public static float CalculateEffectiveVolume(float masterVolume, float channelVolume)
        {
            float m = Math.Clamp(masterVolume, 0f, 1f);
            float c = Math.Clamp(channelVolume, 0f, 1f);
            return m * c;
        }
    }

    public static class SafeAreaEvaluator
    {
        public static (Vector2D minAnchor, Vector2D maxAnchor) CalculateNormalizedAnchors(
            float safeX, float safeY, float safeWidth, float safeHeight,
            float screenWidth, float screenHeight)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                return (Vector2D.Zero, Vector2D.One);
            }

            float minX = Math.Clamp(safeX / screenWidth, 0f, 1f);
            float minY = Math.Clamp(safeY / screenHeight, 0f, 1f);
            float maxX = Math.Clamp((safeX + safeWidth) / screenWidth, 0f, 1f);
            float maxY = Math.Clamp((safeY + safeHeight) / screenHeight, 0f, 1f);

            return (new Vector2D(minX, minY), new Vector2D(maxX, maxY));
        }
    }
}
