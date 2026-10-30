using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Fondo frío tipo "lámpara de lava": degradado base + blobs suaves que derivan.</summary>
    public class ColdBackgroundController : MonoBehaviour
    {
        [SerializeField] private Image gradientImage;
        [SerializeField] private Sprite blobSprite;
        [SerializeField] private int blobCount = 4;
        [SerializeField] private float driftSpeed = 2.2f;
        [SerializeField] private float driftRadius = 320f;

        private readonly List<Blob> blobs = new List<Blob>();

        private class Blob
        {
            public RectTransform rect;
            public Vector2 center;
            public float phase;
            public float speed;
            public float baseScale;
        }

        private void Start()
        {
            if (gradientImage == null || blobSprite == null) return;

            for (int i = 0; i < blobCount; i++)
            {
                var go = new GameObject("Blob", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
                go.transform.SetParent(gradientImage.rectTransform, false);

                var img = go.GetComponent<UnityEngine.UI.Image>();
                img.sprite = blobSprite;
                img.raycastTarget = false;

                // Colores fríos (azules / púrpuras / cianes)
                img.color = new Color(
                    UnityEngine.Random.value * 0.25f + 0.15f,
                    UnityEngine.Random.value * 0.25f + 0.25f,
                    UnityEngine.Random.value * 0.35f + 0.65f,
                    0.40f);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = Vector2.one * UnityEngine.Random.Range(500f, 900f);
                rt.anchoredPosition = new Vector2(UnityEngine.Random.Range(-900f, 900f), UnityEngine.Random.Range(-500f, 500f));
                rt.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.3f);

                blobs.Add(new Blob
                {
                    rect = rt,
                    center = rt.anchoredPosition,
                    phase = UnityEngine.Random.Range(0f, 6.283f),
                    speed = UnityEngine.Random.Range(0.3f, 1f),
                    baseScale = rt.localScale.x
                });
            }
        }

        private void Update()
        {
            float t = Time.time * driftSpeed;

            for (int i = 0; i < blobs.Count; i++)
            {
                Blob b = blobs[i];
                if (b.rect == null) continue;

                float x = Mathf.Sin(t * b.speed + b.phase) * driftRadius;
                float y = Mathf.Cos(t * b.speed * 0.7f + b.phase) * driftRadius * 0.6f;
                b.rect.anchoredPosition = b.center + new Vector2(x, y);

                float s = 1f + Mathf.Sin(t * b.speed + b.phase) * 0.08f;
                b.rect.localScale = Vector3.one * (b.baseScale * s);
            }
        }
    }
}
