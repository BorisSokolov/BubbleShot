using System.Collections.Generic;
using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// LineRenderer and reticle visualization for the aiming trajectory.
    /// </summary>
    public class TrajectoryPreview : MonoBehaviour
    {
        [SerializeField] private LineRenderer? _lineRenderer;
        [SerializeField] private Transform? _landingReticle;

        private void Awake()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
                _lineRenderer.startWidth = 0.1f;
                _lineRenderer.endWidth = 0.1f;
            }

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (_lineRenderer != null) _lineRenderer.enabled = visible;
            if (_landingReticle != null) _landingReticle.gameObject.SetActive(visible);
        }

        public void DrawTrajectory(List<Vector2D> pathPoints, Vector2D impactPoint, Transform boardTransform)
        {
            SetVisible(true);

            if (_lineRenderer != null && pathPoints.Count > 0)
            {
                _lineRenderer.positionCount = pathPoints.Count;
                for (int i = 0; i < pathPoints.Count; i++)
                {
                    Vector3 worldPt = boardTransform.TransformPoint(new Vector3(pathPoints[i].X, pathPoints[i].Y, 0f));
                    _lineRenderer.SetPosition(i, worldPt);
                }
            }

            if (_landingReticle != null)
            {
                Vector3 reticleWorld = boardTransform.TransformPoint(new Vector3(impactPoint.X, impactPoint.Y, 0f));
                _landingReticle.position = reticleWorld;
            }
        }
    }
}
