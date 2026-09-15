using System;
using System.Collections;
using UnityEngine;
using TMPro;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// Visual representation of a single ball on the board or in flight.
    /// Includes high-contrast color glyphs for color-blind accessibility.
    /// </summary>
    public class BallView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer? _spriteRenderer;
        [SerializeField] private TextMeshPro? _glyphText;

        public HexCoord Coord { get; private set; }
        public BallInfo Info { get; private set; }

        private Vector3 _baseScale = Vector3.one;
        private bool _isAnimating;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            if (_isAnimating) return;

            if (Info.Type == BallType.Wild && _spriteRenderer != null)
            {
                // Prismatic rainbow color cycle
                _spriteRenderer.color = Color.HSVToRGB((Time.time * 0.5f) % 1.0f, 0.75f, 1.0f);
            }
            else if (Info.Type == BallType.Bomb)
            {
                // Subtle fuse pulsation
                float pulse = 1f + 0.06f * Mathf.Sin(Time.time * 8f);
                transform.localScale = _baseScale * pulse;
            }
        }

        public static Color GetColor(BallInfo info)
        {
            if (info.Type == BallType.Bomb)
            {
                return new Color(0.85f, 0.12f, 0.10f); // Deep crimson
            }

            if (info.Type == BallType.Wild)
            {
                return new Color(1.0f, 0.95f, 0.85f); // Prismatic base
            }

            return GetColor(info.Color);
        }

        public static Color GetColor(BallColor color)
        {
            return color switch
            {
                BallColor.Red => new Color(1.0f, 0.23f, 0.19f),
                BallColor.Blue => new Color(0.0f, 0.48f, 1.0f),
                BallColor.Green => new Color(0.20f, 0.78f, 0.35f),
                BallColor.Yellow => new Color(1.0f, 0.80f, 0.0f),
                BallColor.Purple => new Color(0.69f, 0.32f, 0.87f),
                BallColor.Orange => new Color(1.0f, 0.58f, 0.0f),
                _ => Color.white
            };
        }

        public static string GetGlyph(BallInfo info)
        {
            if (info.Type == BallType.Bomb)
            {
                return "\u25CE"; // Bullseye / fuse target ◎
            }

            if (info.Type == BallType.Wild)
            {
                return "\u2726"; // Sparkle / star ✦
            }

            return GetGlyph(info.Color);
        }

        public static string GetGlyph(BallColor color)
        {
            return color switch
            {
                BallColor.Red => "\u25CF",    // Circle
                BallColor.Blue => "\u25A0",   // Square
                BallColor.Green => "\u25C6",  // Diamond
                BallColor.Yellow => "\u25B2", // Triangle
                BallColor.Purple => "\u2605", // Star
                BallColor.Orange => "\u271A", // Cross
                _ => ""
            };
        }

        public void Initialize(HexCoord coord, BallInfo info)
        {
            Coord = coord;
            Info = info;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = GetColor(info);
            }

            if (_glyphText != null)
            {
                _glyphText.text = GetGlyph(info);
            }
        }

        public void SetGridCoord(HexCoord newCoord)
        {
            Coord = newCoord;
        }

        public IEnumerator AnimatePop(Action onComplete)
        {
            _isAnimating = true;
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 origScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = origScale * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = 1f - t;
                    _spriteRenderer.color = c;
                }
                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }

        public IEnumerator AnimateFall(Action onComplete)
        {
            _isAnimating = true;
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 velocity = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(2f, 4f), 0f);
            Vector3 gravity = new Vector3(0f, -15f, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                velocity += gravity * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}
