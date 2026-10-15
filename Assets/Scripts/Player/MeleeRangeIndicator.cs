using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Visualiza el área de un ataque melee como un círculo temporal, escalado al radio
    /// indicado. Se instancia una sola vez y se reutiliza para evitar asignaciones por golpe.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MeleeRangeIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color color = new Color(0.35f, 0.9f, 1f, 0.35f);

        private float hideTimer;
        private Transform followTarget;
        private Vector3 followOffset;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.color = color;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Coloca el círculo en la posición indicada, lo ajusta al radio dado y lo muestra
        /// durante el tiempo señalado.
        /// </summary>
        public void Show(Vector3 worldPosition, float radius, float duration, Transform target = null)
        {
            followTarget = target;
            followOffset = target != null ? target.InverseTransformPoint(worldPosition) : Vector3.zero;
            transform.position = worldPosition;
            transform.localScale = ScaleFor(radius);

            hideTimer = Mathf.Max(0f, duration);
            gameObject.SetActive(true);
        }

        private Vector3 ScaleFor(float radius)
        {
            float diameter = Mathf.Max(0f, radius) * 2f;
            Sprite sprite = spriteRenderer != null ? spriteRenderer.sprite : null;

            if (sprite == null)
            {
                return Vector3.one * diameter;
            }

            Vector3 size = sprite.bounds.size;
            float baseSize = Mathf.Max(size.x, size.y);
            return baseSize > 0f ? Vector3.one * (diameter / baseSize) : Vector3.one * diameter;
        }

        private void Update()
        {
            if (followTarget != null)
            {
                transform.position = followTarget.TransformPoint(followOffset);
            }

            if (hideTimer <= 0f)
            {
                return;
            }

            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                followTarget = null;
                gameObject.SetActive(false);
            }
        }
    }
}
