using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// Presentation manager that synchronizes visual ball views with authoritative HexBoard state.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private GameObject? _ballPrefab;
        [SerializeField] private Transform? _boardContainer;

        private readonly Dictionary<HexCoord, BallView> _activeViews = new Dictionary<HexCoord, BallView>();
        private HexBoard? _board;

        public void BindBoard(HexBoard board)
        {
            _board = board;
            RebuildViews();
        }

        public void RebuildViews()
        {
            ClearAllViews();
            if (_board == null) return;

            var occupied = _board.GetOccupiedCoords();
            for (int i = 0; i < occupied.Count; i++)
            {
                var coord = occupied[i];
                var ball = _board.GetBall(coord);
                if (ball.HasValue)
                {
                    SpawnBallView(coord, ball.Value);
                }
            }
        }

        public BallView? SpawnBallView(HexCoord coord, BallInfo info)
        {
            if (_board == null) return null;

            Vector2D localPos = _board.CoordToLocalPosition(coord);
            Vector3 worldPos = transform.TransformPoint(new Vector3(localPos.X, localPos.Y, 0f));

            GameObject go;
            if (_ballPrefab != null)
            {
                go = Instantiate(_ballPrefab, worldPos, Quaternion.identity, _boardContainer ?? transform);
            }
            else
            {
                // Procedural fallback if no prefab assigned in test
                go = new GameObject($"Ball_{coord.Row}_{coord.Col}");
                go.transform.SetParent(_boardContainer ?? transform);
                go.transform.position = worldPos;
                go.AddComponent<SpriteRenderer>();
            }

            var view = go.GetComponent<BallView>() ?? go.AddComponent<BallView>();
            view.Initialize(coord, info);

            _activeViews[coord] = view;
            return view;
        }

        public IEnumerator AnimateMatches(List<HexCoord> matchedCoords, Action onComplete)
        {
            int pending = matchedCoords.Count;
            if (pending == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            for (int i = 0; i < matchedCoords.Count; i++)
            {
                var coord = matchedCoords[i];
                if (_activeViews.TryGetValue(coord, out var view))
                {
                    _activeViews.Remove(coord);
                    StartCoroutine(view.AnimatePop(() =>
                    {
                        pending--;
                    }));
                }
                else
                {
                    pending--;
                }
            }

            while (pending > 0)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        public IEnumerator AnimateDetachedClusters(List<HexCoord> detachedCoords, Action onComplete)
        {
            int pending = detachedCoords.Count;
            if (pending == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            for (int i = 0; i < detachedCoords.Count; i++)
            {
                var coord = detachedCoords[i];
                if (_activeViews.TryGetValue(coord, out var view))
                {
                    _activeViews.Remove(coord);
                    StartCoroutine(view.AnimateFall(() =>
                    {
                        pending--;
                    }));
                }
                else
                {
                    pending--;
                }
            }

            while (pending > 0)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        public IEnumerator AnimateRowDescent(Action onComplete)
        {
            if (_board == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            // Remap existing views downward: (r, c) -> (r + 1, c)
            var remapped = new Dictionary<HexCoord, BallView>();
            var viewsToMove = new List<BallView>();

            foreach (var kvp in _activeViews)
            {
                var oldCoord = kvp.Key;
                var view = kvp.Value;
                var newCoord = new HexCoord(oldCoord.Row + 1, oldCoord.Col);

                view.SetGridCoord(newCoord);
                remapped[newCoord] = view;
                viewsToMove.Add(view);
            }

            _activeViews.Clear();
            foreach (var kvp in remapped)
            {
                _activeViews[kvp.Key] = kvp.Value;
            }

            // Spawn the new top row views
            int cols = _board.GetColumnCount(0);
            for (int c = 0; c < cols; c++)
            {
                var coord = new HexCoord(0, c);
                var ball = _board.GetBall(coord);
                if (ball.HasValue)
                {
                    var newView = SpawnBallView(coord, ball.Value);
                    if (newView != null)
                    {
                        // Start slightly higher and slide in
                        newView.transform.position += new Vector3(0f, _board.Geometry.RowHeight, 0f);
                        viewsToMove.Add(newView);
                    }
                }
            }

            // Smooth downward slide animation
            float elapsed = 0f;
            float duration = 0.25f;
            float dropDistance = _board.Geometry.RowHeight;

            var startPositions = new Vector3[viewsToMove.Count];
            var targetPositions = new Vector3[viewsToMove.Count];

            for (int i = 0; i < viewsToMove.Count; i++)
            {
                startPositions[i] = viewsToMove[i].transform.position;
                Vector2D targetLocal = _board.CoordToLocalPosition(viewsToMove[i].Coord);
                targetPositions[i] = transform.TransformPoint(new Vector3(targetLocal.X, targetLocal.Y, 0f));
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                for (int i = 0; i < viewsToMove.Count; i++)
                {
                    if (viewsToMove[i] != null)
                    {
                        viewsToMove[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < viewsToMove.Count; i++)
            {
                if (viewsToMove[i] != null)
                {
                    viewsToMove[i].transform.position = targetPositions[i];
                }
            }

            onComplete?.Invoke();
        }

        private void ClearAllViews()
        {
            foreach (var kvp in _activeViews)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _activeViews.Clear();
        }
    }
}
