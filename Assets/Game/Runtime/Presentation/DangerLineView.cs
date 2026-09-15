using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// Visual representation of the danger line separating safe rows from defeat boundary.
    /// </summary>
    public class DangerLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer? _lineRenderer;
        [SerializeField] private SpriteRenderer? _spriteRenderer;

        private bool _isPulsing;
        private float _pulseTimer;

        public void Initialize(BoardGeometry geometry)
        {
            float lineY = -(geometry.DangerRow - 0.5f) * geometry.RowHeight;
            transform.localPosition = new Vector3(geometry.BoardRight * 0.5f, lineY, 0f);

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPosition(0, new Vector3(geometry.BoardLeft, lineY, 0f));
                _lineRenderer.SetPosition(1, new Vector3(geometry.BoardRight, lineY, 0f));
                _lineRenderer.startColor = new Color(1f, 0.2f, 0.2f, 0.8f);
                _lineRenderer.endColor = new Color(1f, 0.2f, 0.2f, 0.8f);
            }
        }

        public void SetWarningPulsing(bool pulsing)
        {
            _isPulsing = pulsing;
        }

        private void Update()
        {
            if (!_isPulsing) return;

            _pulseTimer += Time.deltaTime * 4f;
            float alpha = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(_pulseTimer));

            if (_lineRenderer != null)
            {
                Color c = new Color(1f, 0.1f, 0.1f, alpha);
                _lineRenderer.startColor = c;
                _lineRenderer.endColor = c;
            }
        }
    }
}
