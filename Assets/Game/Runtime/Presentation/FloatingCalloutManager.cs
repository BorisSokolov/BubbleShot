using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BubbleShot.Core;

namespace BubbleShot.Runtime.Presentation
{
    /// <summary>
    /// Manages floating combo and score callouts with object pooling to prevent runtime allocations.
    /// </summary>
    public class FloatingCalloutManager : MonoBehaviour
    {
        [SerializeField] private GameObject? _calloutPrefab;
        [SerializeField] private Transform? _calloutRoot;

        private ObjectPool<GameObject>? _pool;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            _pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject go;
                    if (_calloutPrefab != null)
                    {
                        go = Instantiate(_calloutPrefab, _calloutRoot != null ? _calloutRoot : transform);
                    }
                    else
                    {
                        go = new GameObject("FloatingCallout");
                        go.transform.SetParent(_calloutRoot != null ? _calloutRoot : transform);
                        var tmp = go.AddComponent<TextMeshProUGUI>();
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.fontSize = 28;
                    }
                    go.SetActive(false);
                    return go;
                },
                onGet: go => go.SetActive(true),
                onReturn: go => go.SetActive(false),
                initialCapacity: 8,
                maxCapacity: 24);
        }

        public static string GetComboCalloutText(float comboMultiplier) => ComboCalloutEvaluator.GetCalloutText(comboMultiplier);

        public void ShowCallout(Vector3 worldPos, string text, Color color)
        {
            if (string.IsNullOrEmpty(text) || _pool == null) return;

            var go = _pool.Rent();
            go.transform.position = worldPos;
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.color = color;
            }

            StartCoroutine(AnimateCallout(go));
        }

        private IEnumerator AnimateCallout(GameObject go)
        {
            float elapsed = 0f;
            float duration = 0.7f;
            Vector3 startPos = go.transform.position;
            Vector3 targetPos = startPos + new Vector3(0f, 1.2f, 0f);

            var tmp = go.GetComponent<TextMeshProUGUI>() ?? (TMP_Text?)go.GetComponent<TextMeshPro>();
            Color startColor = tmp != null ? tmp.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                go.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));

                if (tmp != null)
                {
                    Color c = startColor;
                    c.a = 1f - t;
                    tmp.color = c;
                }

                yield return null;
            }

            _pool?.Return(go);
        }
    }
}
