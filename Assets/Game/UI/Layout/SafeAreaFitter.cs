using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.UI.Layout
{
    /// <summary>
    /// Adjusts RectTransform anchors to match the device screen safe area,
    /// protecting UI from notches, punch-hole cameras, and rounded corners on iOS and Android.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform? _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea ||
                _lastScreenSize.x != Screen.width ||
                _lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (_rectTransform == null) return;

            Rect safeArea = Screen.safeArea;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            var (min, max) = CalculateNormalizedAnchors(safeArea, screenWidth, screenHeight);

            _rectTransform.anchorMin = min;
            _rectTransform.anchorMax = max;

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        /// <summary>
        /// Pure mathematical calculation converting pixel safe area into normalized RectTransform anchors.
        /// Fully verifiable in unit tests.
        /// </summary>
        public static (Vector2 anchorMin, Vector2 anchorMax) CalculateNormalizedAnchors(
            Rect safeArea, float screenWidth, float screenHeight)
        {
            var (min, max) = SafeAreaEvaluator.CalculateNormalizedAnchors(
                safeArea.x, safeArea.y, safeArea.width, safeArea.height,
                screenWidth, screenHeight);
            return (new Vector2(min.X, min.Y), new Vector2(max.X, max.Y));
        }
    }
}
