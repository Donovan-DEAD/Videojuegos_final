using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Efecto greybox de explosión: un círculo que se expande y se desvanece, generado
    /// en código sin assets. Lo invoca EnemyAI en los enemigos que explotan al morir.
    /// </summary>
    public class ExplosionVFX : MonoBehaviour
    {
        [SerializeField] private float expandDuration = 0.35f;
        [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0.2f, 1f);

        private float timer;
        private float targetDiameter;
        private SpriteRenderer spriteRenderer;

        /// <summary>Crea el efecto en la posición dada, expandiéndose hasta el radio indicado.</summary>
        public static void Play(Vector3 position, float radius)
        {
            GameObject go = new GameObject("ExplosionVFX");
            go.transform.position = position;
            ExplosionVFX fx = go.AddComponent<ExplosionVFX>();
            fx.Setup(radius);
        }

        private void Setup(float radius)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.color = flashColor;
            spriteRenderer.sortingOrder = 200000;
            targetDiameter = Mathf.Max(0.2f, radius * 2f);
            transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / expandDuration);
            float scale = Mathf.Lerp(0.05f, targetDiameter, t);
            transform.localScale = new Vector3(scale, scale, 1f);

            Color c = spriteRenderer.color;
            c.a = 1f - t;
            spriteRenderer.color = c;

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite CreateCircleSprite()
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, d <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}