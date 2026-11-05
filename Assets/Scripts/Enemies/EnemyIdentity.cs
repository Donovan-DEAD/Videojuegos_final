using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Identificación visual greybox de un enemigo: un borde del color del arquetipo (hijo, fuera del
    /// Animator) y una etiqueta flotante con su nombre. Se oculta al morir.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyIdentity : MonoBehaviour
    {
        private static Sprite whiteSprite;
        private static Font labelFont;

        private SpriteRenderer body;
        private SpriteRenderer badge;
        private TextMesh label;
        private TextMesh labelShadow;

        /// <summary>Aplica el nombre y el color del arquetipo, creando los elementos visuales si faltan.</summary>
        public void Apply(EnemyStatsSO stats)
        {
            if (stats == null)
            {
                return;
            }

            EnsureBadge(stats.IdentityColor);
            EnsureLabel(stats.DisplayName, stats.IdentityColor);
        }

        /// <summary>Muestra u oculta el borde y la etiqueta (se oculta al morir).</summary>
        public void SetVisible(bool visible)
        {
            if (badge != null)
            {
                badge.enabled = visible;
            }

            if (label != null)
            {
                label.gameObject.SetActive(visible);
            }

            if (labelShadow != null)
            {
                labelShadow.gameObject.SetActive(visible);
            }
        }

        private void EnsureBadge(Color color)
        {
            if (badge == null)
            {
                body = GetComponent<SpriteRenderer>();

                GameObject go = new GameObject("IdentityBadge");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

                badge = go.AddComponent<SpriteRenderer>();
                badge.sprite = GetWhiteSprite();
                badge.sortingLayerID = body != null ? body.sortingLayerID : 0;
                badge.sortingOrder = body != null ? body.sortingOrder - 1 : 0;
            }

            badge.color = color;
        }

        private void EnsureLabel(string text, Color color)
        {
            if (label == null)
            {
                labelShadow = CreateLabel("NameLabelShadow", new Color(0f, 0f, 0f, 0.85f), 4);
                labelShadow.transform.localPosition = new Vector3(0.02f, 0.87f, 0.01f);

                label = CreateLabel("NameLabel", color, 5);
                label.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            }

            label.text = text;
            label.color = color;
            labelShadow.text = text;
        }

        private TextMesh CreateLabel(string objectName, Color color, int orderOffset)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(transform, false);

            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.text = string.Empty;
            mesh.font = GetFont();
            mesh.fontSize = 48;
            mesh.characterSize = 0.07f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mesh.font.material;
                renderer.sortingLayerID = body != null ? body.sortingLayerID : 0;
                renderer.sortingOrder = (body != null ? body.sortingOrder : 0) + orderOffset;
            }

            return mesh;
        }

        private static Sprite GetWhiteSprite()
        {
            if (whiteSprite != null)
            {
                return whiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;

            whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }

        private static Font GetFont()
        {
            if (labelFont != null)
            {
                return labelFont;
            }

            try
            {
                labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (System.Exception)
            {
                labelFont = null;
            }

            if (labelFont == null)
            {
                try
                {
                    labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch (System.Exception)
                {
                    labelFont = null;
                }
            }

            return labelFont;
        }
    }
}
